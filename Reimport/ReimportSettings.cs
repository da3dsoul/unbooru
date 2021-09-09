using System.Collections.Generic;

namespace unbooru.Reimport
{
    public class ReimportSettings
    {
        public List<int[]> ImagesToImport { get; set; }
        public string Token { get; set; }
    }
}