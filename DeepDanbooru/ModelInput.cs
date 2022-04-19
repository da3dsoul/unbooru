using Microsoft.ML.Data;

namespace unbooru.DeepDanbooru
{
    public class ModelInput
    {
        public const int Width = 512;
        public const int Height = 512;
        public const int Channels = 3;
        public const string InputString = "input_1:0";

        [ColumnName(InputString)]
        [VectorType(1, Width, Height, Channels)]
        public float[] Data { get; set; }
    }
}