using System;
using Tensorflow;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;
using Tensorflow.Operations.Initializers;

namespace unbooru.DeepDanbooru.Training;

public class ResNetCustomV3
{
    private static readonly int[] FilterSizes = { 256, 512, 1024, 1024, 2048, 4096 };
    private static readonly int[] RepeatSizes = {2, 7, 19, 19, 2, 2};
    private static readonly LayersApi Layers = new();

    public static Tensors CreateOutputs(Tensors inputs, int length)
    {
        inputs = OriginalBottleneckModel(inputs, finalPool: false);
        inputs = inputs.ConvGap(length, (1, 1));
        // need to find the ML.Net equivalent for tf.keras.activations.sigmoid
        inputs = Layers.HardSigmoid().Apply(inputs);

        return inputs;
    }
    

    /// <summary>
    /// https://github.com/Microsoft/CNTK/blob/master/Examples/Image/Classification/ResNet/Python/resnet_models.py
    /// </summary>
    /// <param name="inputs"></param>
    /// <param name="finalPool"></param>
    /// <param name="se"></param>
    /// <returns></returns>
    private static Tensors OriginalBottleneckModel(Tensors inputs, bool finalPool = true, bool se = false)
    {
        if (FilterSizes.Length != RepeatSizes.Length)
            throw new ArgumentException("FilterSizes and RepeatSizes must be the same length");

        inputs = inputs.ConvBnRelu((int)Math.Floor(FilterSizes[0] / 4D), (7, 7), strides: new[] {2, 2}, new Ones());
        inputs = Layers.MaxPooling2D((3, 3), strides: (2, 2), padding: "same").Apply(inputs);

        for (int i = 0; i < RepeatSizes.Length; i++)
        {
            inputs = BottleneckIncBlock(inputs, outputFilters: FilterSizes[i],
                interFilters: (int)Math.Floor(FilterSizes[i] / 4D), strides1x1: new []{1, 1}, strides2x2: i > 0 ? new[] { 2, 2 } : new[] { 1, 1 },
                se: se);
            inputs = inputs.RepeatBlocks(i, RepeatSizes[i],
                (x, index) =>
                    BottleneckBlock(x, FilterSizes[index], (int)Math.Floor(FilterSizes[index] / 4D), true, se));
        }

        if (finalPool) inputs = Layers.AveragePooling2D((7, 7)).Apply(inputs);

        return inputs;
    }

    private static Tensors BottleneckIncBlock(Tensors input, int outputFilters, int interFilters, Shape strides1x1, Shape strides2x2, bool se = false)
    {
        var c1 = input.ConvBnRelu(interFilters, (1, 1), strides1x1, new Ones());
        c1 = c1.ConvBnRelu(interFilters, (3, 3), strides2x2, new Ones());
        c1 = c1.ConvBnRelu(outputFilters, (1, 1), new[] { 1, 1 }, new Zeros());

        if (se) c1 = c1.SqueezeExcitation();

        var strides = np.multiply((int[])strides1x1, (int[])strides2x2);
        var s = input.ConvBn(outputFilters, (1, 1), strides.ToArray<int>(), new Ones());

        s = Binding.tf.add(s, c1);

        return Binding.tf.nn.relu().Activate(s);
    }

    public static Tensors BottleneckBlock(Tensors input, int outputFilters, int interFilters, bool activation = true,
        bool se = false)
    {
        var c1 = input.ConvBnRelu(interFilters, (1, 1));
        c1 = c1.ConvBnRelu(interFilters, (3, 3));
        c1 = c1.ConvBn(outputFilters, (1, 1), (1, 1), new Zeros());

        if (se)
            c1 = c1.SqueezeExcitation();

        var p = Binding.tf.add(c1, input);

        if (activation)
            return Binding.tf.nn.relu().Activate(p);

        return p;
    }
}