using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADPermittedLogonTimeDemo.Demo
{
    public class LogonTime
    {
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public LogonTime(DayOfWeek dayOfWeek, DateTime beginTime, DateTime endTime)
        {
            DayOfWeek = dayOfWeek;
            BeginTime = beginTime;
            EndTime = endTime;
        }

        public LogonTime(DayOfWeek dayOfWeek, TimeSpan begin, TimeSpan end)
        {
            DayOfWeek = dayOfWeek;
            BeginTime = new DateTime(begin.Ticks);
            EndTime = new DateTime(end.Ticks);
        }
    }
}
