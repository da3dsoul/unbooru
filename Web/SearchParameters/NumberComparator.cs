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
}