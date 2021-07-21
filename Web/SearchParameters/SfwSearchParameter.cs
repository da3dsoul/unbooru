using System;
using System.Collections.Generic;
using System.Linq;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Web.SearchParameters
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
                "penis",
                "sex",
                "nipples",
                "pussy",
                "vaginal",
                "fellatio",
                "paizuri",
                "cunnilingus",
            };

            var context = provider.GetRequiredService<CoreContext>();
            return context.Set<ImageTag>().Where(a => nsfw.Contains(a.Name)).Select(a => a.ImageTagId).ToList();
        }
    }
}