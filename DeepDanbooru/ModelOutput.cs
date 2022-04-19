using Microsoft.ML.Data;

namespace unbooru.DeepDanbooru
{
    public class ModelOutput
    {
        public const string OutputString = "Identity:0";

        [ColumnName(OutputString)] public float[] Scores { get; set; }
    }
}