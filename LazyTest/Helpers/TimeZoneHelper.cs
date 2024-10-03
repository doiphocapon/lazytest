namespace LazyTest.Helpers
{
    public  static class TimeZoneHelper
    {
        public static DateTime ConvertGmtToSydney(DateTime gmtDateTime)
        {
            TimeZoneInfo sydneyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(gmtDateTime, sydneyTimeZone);
        }
    }
}
