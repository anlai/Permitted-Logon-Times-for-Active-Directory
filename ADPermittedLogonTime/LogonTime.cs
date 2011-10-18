using System;

namespace ADPermittedLogonTime
{
    public class LogonTime
    {
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public int TimeZoneOffSet { get; set; }

        public LogonTime(DayOfWeek dayOfWeek, DateTime beginTime, DateTime endTime)
        {
            DayOfWeek = dayOfWeek;
            BeginTime = beginTime;
            EndTime = endTime;

            SetOffset(TimeZoneInfo.FindSystemTimeZoneById(TimeZone.CurrentTimeZone.StandardName));
            ValidateTimes();
        }

        public LogonTime(DayOfWeek dayOfWeek, TimeSpan begin, TimeSpan end)
        {
            DayOfWeek = dayOfWeek;
            BeginTime = new DateTime(begin.Ticks);
            EndTime = new DateTime(end.Ticks);

            SetOffset(TimeZoneInfo.FindSystemTimeZoneById(TimeZone.CurrentTimeZone.StandardName));
            ValidateTimes();
        }

        public LogonTime(DayOfWeek dayOfWeek, DateTime beginTime, DateTime endTime, TimeZoneInfo timeZone)
        {
            DayOfWeek = dayOfWeek;
            BeginTime = beginTime;
            EndTime = endTime;

            SetOffset(timeZone);
            ValidateTimes();
        }

        public LogonTime(DayOfWeek dayOfWeek, TimeSpan begin, TimeSpan end, TimeZoneInfo timeZone)
        {
            DayOfWeek = dayOfWeek;
            BeginTime = new DateTime(begin.Ticks);
            EndTime = new DateTime(end.Ticks);

            SetOffset(timeZone);
            ValidateTimes();
        }

        private void SetOffset(TimeZoneInfo timeZone)
        {
            TimeZoneOffSet = (-1) * (timeZone.BaseUtcOffset.Hours);
            //TimeZoneOffSet = timeZone.IsDaylightSavingTime(DateTime.Now) ? (-1) * (timeZone.GetUtcOffset(DateTime.Now).Hours - 1) : (-1)*(timeZone.GetUtcOffset(DateTime.Now).Hours);
        }

        private void ValidateTimes()
        {
            if (EndTime.Hour < BeginTime.Hour)
            {
                throw new ArgumentException("Begin time cannot be after End time.");
            }
        }
    }
}
