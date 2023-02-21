using System.Collections.Generic;

namespace unbooru.FoldSaveHandler
{
    public class FoldSaveHandlerSettings
    {
        public string ImagePath { get; set; }
        public bool UseFilesystemFriendlyTree { get; set; } = true;

        public HashSet<string> ExcludeTags { get; set; } = new()
        {
            "anus",
            "bar censor",
            "blur censor",
            "blurry foreground",
            "blurry",
            "breast tattoo",
            "censored",
            "cover",
            "cunnilingus",
            "fellatio",
            "fingering",
            "hetero",
            "mosaic censoring",
            "nipple piercing",
            "nipples",
            "paizuri",
            "pee",
            "peeing",
            "penis",
            "pregnant",
            "pubic hair",
            "pubic tattoo",
            "pussy juice",
            "pussy",
            "sex",
            "vaginal",
            "yuri",
        };
        public bool ExcludeMissingInfo { get; set; } = true;
        public bool EnableAspectRatioSplitting { get; set; } = true;

    }
}
