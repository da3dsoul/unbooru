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
                return a => a.TagSources.Any() &&
                            (!IncludedTagIds.Any() || a.TagSources.Any(b => IncludedTagIds.Contains(b.TagsImageTagId))) &&
                            a.TagSources.All(b => !ExcludedTagIds.Contains(b.TagsImageTagId));
            return a => a.TagSources.Any() &&
                        (!IncludedTagIds.Any() || a.TagSources.Count(b => IncludedTagIds.Contains(b.TagsImageTagId)) ==
                            IncludedTagIds.Count()) && a.TagSources.All(b => !ExcludedTagIds.Contains(b.TagsImageTagId));
        }

        public override Type[] Types { get; } = { typeof(ImageTag) };
    }
}
