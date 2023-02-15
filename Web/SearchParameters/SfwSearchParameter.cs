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

            var context = provider.GetRequiredService<CoreContext>();
            return context.Set<ImageTag>().Where(a => nsfw.Contains(a.Name)).Select(a => a.ImageTagId).ToList();
        }
    }
}
