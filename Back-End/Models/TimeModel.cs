using System;

namespace Web_ASP.Models
{
    public class TimeModel
    {
        private TimeZoneInfo jakarta_zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public string GetTimeNow(string format)
        {
            DateTime now = DateTime.UtcNow;
            var wib_now = TimeZoneInfo.ConvertTimeFromUtc(now, jakarta_zone);
            return wib_now.ToString(format);
        }

        // method untuk melakukan pengurangan waktu dari waktu sekarang dengan parameter hari dan string formatnya
        public string GetSubstractTimeNow(int day, string format)
        {
            DateTime now = DateTime.UtcNow;
            var wib_now = TimeZoneInfo.ConvertTimeFromUtc(now, jakarta_zone);
            var oneMonth = new System.TimeSpan(day, 0, 0, 0);
            var lastMonth = wib_now.Subtract(oneMonth);
            return lastMonth.ToString(format);
        }
    }
}