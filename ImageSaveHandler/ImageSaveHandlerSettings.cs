using System.Collections.Generic;

namespace unbooru.ImageSaveHandler
{
    public class ImageSaveHandlerSettings
    {
        public string ImagePath { get; set; }
        public bool UseFilesystemFriendlyTree { get; set; } = true;

        public HashSet<string> ExcludeTags { get; set; } = new()
        {
            "anus",
            "blur censor",
            "blurry",
            "blurry foreground",
            "censored",
            "cunnilingus",
            "fellatio",
            "hetero",
            "mosaic censoring",
            "nipples",
            "paizuri",
            "pee",
            "peeing",
            "penis",
            "pubic hair",
            "pussy",
            "sex",
            "vaginal",
        };
        public bool ExcludeMissingInfo { get; set; } = true;
        public bool EnableAspectRatioSplitting { get; set; } = true;

    }
}