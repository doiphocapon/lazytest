namespace LazyTest.Extentions
{
    public static class LongExtentions
    {
        public static string ToKB(this long? value)
        {
            if (value == null)
            {
                return "0";
            }
            if (value == 0 )
            {
                return "0";
            }
            return Math.Floor((decimal)(value / 1024)).ToString("0 Kb");
        }
    }
}
