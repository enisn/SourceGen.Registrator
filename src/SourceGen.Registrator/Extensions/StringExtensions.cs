using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGen.Registrator.Extensions;
public static class StringExtensions
{
    public static string EnsureEndsWith(this string source, string suffix)
    {
        if (source.EndsWith(suffix))
        {
            return source;
        }

        return source + suffix;
    }
}
