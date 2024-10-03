using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyTest.Constants
{
    public static class ColorByResponseCodeHelper
    {
        public static string GetColorNameByCode(int? code)
        {
            if (code == null)
            {
                return "Grey";
            }
            if (Enum.IsDefined(typeof(ColorByResponseCode), code))
            {
                return Enum.GetName(typeof(ColorByResponseCode), code);
            }
            else
            {
                return "Grey";
            }
        }
    }
}
