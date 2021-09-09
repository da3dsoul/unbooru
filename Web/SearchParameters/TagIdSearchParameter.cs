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
                return a => a.Tags.Any() &&
                            (!IncludedTagIds.Any() || a.Tags.Any(b => IncludedTagIds.Contains(b.ImageTagId))) &&
                            a.Tags.All(b => !ExcludedTagIds.Contains(b.ImageTagId));
            return a => a.Tags.Any() &&
                        (!IncludedTagIds.Any() || a.Tags.Count(b => IncludedTagIds.Contains(b.ImageTagId)) ==
                            IncludedTagIds.Count()) && a.Tags.All(b => !ExcludedTagIds.Contains(b.ImageTagId));
        }

        public override Type[] Types { get; } = { typeof(ImageTag) };
    }
}
