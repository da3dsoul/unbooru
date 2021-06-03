using System.Collections.Generic;

namespace ImageInfrastructure.Reimport
{
    public class ReimportSettings
    {
        public List<int[]> ImagesToImport { get; set; }
        public string Token { get; set; }
    }
}