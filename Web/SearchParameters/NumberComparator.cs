using System;

namespace ImageInfrastructure.Web.SearchParameters
{
    [Flags]
    public enum NumberComparator
    {
        NotEqual = 0,
        Equal = 1,
        GreaterThan = 1 << 2,
        LessThan = 1 << 3
    }

    public static class NumberComparatorEnum
    {
        public static NumberComparator Parse(string s)
        {
            if (s.StartsWith("<=")) return NumberComparator.LessThan | NumberComparator.Equal;
            if (s.StartsWith("<")) return NumberComparator.LessThan;
            if (s.StartsWith(">=")) return NumberComparator.GreaterThan | NumberComparator.Equal;
            if (s.StartsWith(">")) return NumberComparator.GreaterThan;
            if (s.StartsWith("!=")) return NumberComparator.NotEqual;
            return NumberComparator.Equal;
        }
    }
}