using System;
using Tensorflow;
using Tensorflow.Keras.Layers;
using Tensorflow.Operations.Initializers;

namespace unbooru.DeepDanbooru.Training;

public static class LayerExtensions
{
    private static readonly int[] DefaultStrides = { 1, 1 };
    private static readonly LayersApi Layers = new();

    public static Tensors Conv(this Tensors inputs, int filters, Shape kernelSize,
        string padding = "same", string initializer="he_normal")
    {
        return Conv(inputs, filters, kernelSize, DefaultStrides, padding, initializer);
    }

    public static Tensors Conv(this Tensors inputs, int filters, Shape kernelSize, Shape strides,
        string padding = "same", string initializer="he_normal")
    {
        return Layers.Conv2D(filters, kernelSize, strides, padding, kernel_initializer: initializer).Apply(inputs);
    }

    public static Tensors ConvBn(this Tensors inputs, int filters, Shape kernelSize, string padding = "same",
        string initializer = "he_normal")
    {
        return ConvBn(inputs, filters, kernelSize, DefaultStrides, new Ones(), padding, initializer);
    }

    public static Tensors ConvBn(this Tensors inputs, int filters, Shape kernelSize, Shape strides, IInitializer bnGammaInitializer, 
        string padding = "same",
        string initializer = "he_normal")
    {
        var layer = inputs.Conv(filters, kernelSize, strides, padding, initializer);
        return Layers.BatchNormalization(gamma_initializer: bnGammaInitializer).Apply(layer);
    }

    public static Tensors ConvGap(this Tensors inputs, int length, Shape kernelSize)
    {
        var layer = Conv(inputs, length, kernelSize);
        return Layers.GlobalAveragePooling2D().Apply(layer);
    }

    public static Tensors ConvBnRelu(this Tensors inputs, int filters, Shape kernelSize, string padding = "same",
        string initializer = "he_normal")
    {
        return ConvBnRelu(inputs, filters, kernelSize, DefaultStrides, new Ones(), padding, initializer);
    }

    public static Tensors ConvBnRelu(this Tensors inputs, int filters, Shape kernelSize, Shape strides,
        IInitializer bnGammaInitializer, string padding = "same", string initializer = "he_normal")
    {
        var layer = Layers.Conv2D(filters, kernelSize, strides, padding, kernel_initializer: initializer, activation: "relu").Apply(inputs);
        return Layers.BatchNormalization(gamma_initializer: bnGammaInitializer).Apply(layer);
    }

    /// <summary>
    /// Squeeze - Excitation layer from https: //arxiv.org/abs/1709.01507
    /// </summary>
    /// <param name="input"></param>
    /// <param name="reduction"></param>
    /// <returns></returns>
    public static Tensors SqueezeExcitation(this Tensors input, int reduction = 16)
    {
        var outputFilters = input.shape[^1];

        if (Math.Floor(outputFilters / (double)reduction) <= 0) throw new Exception();
        //var s = Layers.GlobalAveragePooling2D().Apply(input);
        //s = Layers.Dense((int)Math.Floor(outputFilters / (double)reduction), activation: "relu").Apply(input);
        var s = Layers.Dense((int)outputFilters, activation: "sigmoid").Apply(input);
        input = math_ops.multiply(input, s);

        return input;
    }

    public static Tensors RepeatBlocks(this Tensors input, int index, int count, Func<Tensors,int,Tensors> blockDelegate)
    {
        if (count < 0) throw new ArgumentException("Count must be 0 or more");

        for (var i = 0; i < count; i++)
        {
            input = blockDelegate.Invoke(input, index);
        }

        return input;
    }
}