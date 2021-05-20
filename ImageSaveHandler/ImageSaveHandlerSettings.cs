using System.Collections.Generic;

namespace ImageInfrastructure.ImageSaveHandler
{
    public class ImageSaveHandlerSettings
    {
        public bool UseFilesystemFriendlyTree { get; set; } = true;
        public HashSet<string> ExcludeTags { get; set; } = new() { "penis", "vaginal", "sex" };
        public bool ExcludeMissingInfo { get; set; } = true;
        public bool EnableAspectRatioSplitting { get; set; } = true;

    }
}