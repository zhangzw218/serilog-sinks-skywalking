using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Sinks.Skywalking
{
    public class TagsFormat : IFormatProvider, ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is int intR)
            {
                return intR.ToString();
            }
            else if (arg is long longR)
            {
                return longR.ToString();
            }
            else if (arg is bool boolR)
            {
                return boolR.ToString();
            }
            else
            {
                return arg.ToString();
            }
        }

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }
    }
}
