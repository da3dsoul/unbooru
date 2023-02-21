using System;
using System.Collections.Generic;
using System.Linq;
using unbooru.Abstractions.Poco;
using unbooru.Core;
using Microsoft.Extensions.DependencyInjection;

namespace unbooru.Web.SearchParameters
{
    public record SfwSearchParameter : TagIdSearchParameter
    {
        public SfwSearchParameter(IServiceProvider provider) : base(new List<int>(), GetNsfwTagIds(provider))
        {
        }

        private static IEnumerable<int> GetNsfwTagIds(IServiceProvider provider)
        {
            var nsfw = new List<string>
            {
                "anus",
                "bar censor",
                "blur censor",
                "breast tattoo",
                "censored",
                "cunnilingus",
                "fellatio",
                "fingering",
                "hetero",
                "mosaic censoring",
                "nipples",
                "nipple piercing",
                "paizuri",
                "pee",
                "peeing",
                "penis",
                "pregnant",
                "pubic hair",
                "pubic tattoo",
                "pussy",
                "pussy juice",
                "sex",
                "vaginal",
                "yuri",
            };

            var context = provider.GetRequiredService<CoreContext>();
            return context.Set<ImageTag>().Where(a => nsfw.Contains(a.Name)).Select(a => a.ImageTagId).ToList();
        }
    }
}
