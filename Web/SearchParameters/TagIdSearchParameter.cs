using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SearchParameters
{
    public record TagIdSearchParameter(IEnumerable<int> IncludedTagIds, IEnumerable<int> ExcludedTagIds,
        bool AnyTag = false, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            if (AnyTag)
            {
                if (IncludedTagIds.Any() && ExcludedTagIds.Any())
                    return a => a.TagSources.Any() && a.TagSources.Any(b =>
                        IncludedTagIds.Contains(b.TagsImageTagId)) && a.TagSources.All(b => !ExcludedTagIds.Contains(b.TagsImageTagId));
                if (IncludedTagIds.Any())
                    return a => a.TagSources.Any() && a.TagSources.Any(b => IncludedTagIds.Contains(b.TagsImageTagId));
                if (ExcludedTagIds.Any())
                    return a => a.TagSources.Any() && a.TagSources.All(b => !ExcludedTagIds.Contains(b.TagsImageTagId));
            }
            else
            {
                if (IncludedTagIds.Any() && ExcludedTagIds.Any())
                    return a => a.TagSources.Any() && a.TagSources.Select(b => b.TagsImageTagId).Distinct().Count(b =>
                        IncludedTagIds.Contains(b)) == IncludedTagIds.Count() && a.TagSources.All(b => !ExcludedTagIds.Contains(b.TagsImageTagId));
                if (IncludedTagIds.Any())
                    return a => a.TagSources.Any() && a.TagSources.Select(b => b.TagsImageTagId).Distinct().Count(b =>
                        IncludedTagIds.Contains(b)) == IncludedTagIds.Count();
                if (ExcludedTagIds.Any())
                    return a => a.TagSources.Any() && a.TagSources.All(b => !ExcludedTagIds.Contains(b.TagsImageTagId));
            }

            return a => a.TagSources.Any();
        }

        public override Type[] Types { get; } = { typeof(ImageTag) };
    }
}
