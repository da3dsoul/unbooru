using System.Collections.Generic;

namespace unbooru.ImageSaveHandler
{
    public class ImageSaveHandlerSettings
    {
        public string ImagePath { get; set; }
        public bool UseFilesystemFriendlyTree { get; set; } = true;

        public HashSet<string> ExcludeTags { get; set; } = new()
        {
            "penis",
            "sex",
            "nipples",
            "pussy",
            "vaginal",
            "fellatio",
            "paizuri",
            "cunnilingus"
        };
        public bool ExcludeMissingInfo { get; set; } = true;
        public bool EnableAspectRatioSplitting { get; set; } = true;

    }
}