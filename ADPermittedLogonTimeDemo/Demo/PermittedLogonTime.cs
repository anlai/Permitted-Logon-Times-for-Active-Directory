using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADPermittedLogonTimeDemo.Demo
{
    public class PermittedLogonTime
    {
        public static byte[] Calculate(List<LogonTime> logonTimes)
        {
            var permittedLogonHours = new byte[21];

            // calculate the byte values for each individual days
            foreach (var lt in logonTimes)
            {
                CalculateByteValue(permittedLogonHours, lt.DayOfWeek, lt.BeginTime, lt.EndTime);
            }

            return permittedLogonHours;
        }

        private static void CalculateByteValue(byte[] permittedLogonHours, DayOfWeek dayOfWeek, DateTime begin, DateTime end)
        {
            var dayHours = new byte[3];

            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dayHours = CalculateDayHours(begin, end);
                    permittedLogonHours[1] = dayHours[0];
                    permittedLogonHours[2] = dayHours[1];
                    permittedLogonHours[3] = dayHours[2];
                    break;
                case DayOfWeek.Monday:
                    dayHours = CalculateDayHours(begin, end);
                    permittedLogonHours[4] = dayHours[0];
                    permittedLogonHours[5] = dayHours[1];
                    permittedLogonHours[6] = dayHours[2];
                    break;
                case DayOfWeek.Tuesday:
                    dayHours = CalculateDayHours(begin, end);
                    permittedLogonHours[7] = dayHours[0];
                    permittedLogonHours[8] = dayHours[1];
                    permittedLogonHours[9] = dayHours[2];
                    break;
                case DayOfWeek.Wednesday:
                    dayHours = CalculateDayHours(begin, end);
                    permittedLogonHours[10] = dayHours[0];
                    permittedLogonHours[11] = dayHours[1];
                    permittedLogonHours[12] = dayHours[2];
                    break;
                case DayOfWeek.Thursday:
                    dayHours = CalculateDayHours(begin, end);
                    permittedLogonHours[13] = dayHours[0];
                    permittedLogonHours[14] = dayHours[1];
                    permittedLogonHours[15] = dayHours[2];
                    break;
                case DayOfWeek.Friday:
                    dayHours = CalculateDayHours(begin, end);
                    permittedLogonHours[16] = dayHours[0];
                    permittedLogonHours[17] = dayHours[1];
                    permittedLogonHours[18] = dayHours[2];
                    break;
                case DayOfWeek.Saturday:
                    dayHours = CalculateDayHours(begin, end);
                    permittedLogonHours[19] = dayHours[0];
                    permittedLogonHours[20] = dayHours[1];
                    permittedLogonHours[0] = dayHours[2];
                    break;
            }
        }

        /// <summary>
        /// Takes the hours that user is allowed to login and calculates the byte mask for it
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static byte[] CalculateDayHours(DateTime begin, DateTime end)
        {
            var result = new int[3];

            for (int i = 0; i < 24; i++)
            {
                if (begin.Hour <= i && i < end.Hour) // if the hour is in the allowable range
                {
                    var multiplier = (int)Math.Floor((decimal)(i) / 8);

                    result[multiplier] += CalculateHourValue(i);

                }
            }

            return new byte[3] { Convert.ToByte(result[0]), Convert.ToByte(result[1]), Convert.ToByte(result[2]) };
        }

        /// <summary>
        /// Returns the byte mask for the specific hours
        /// </summary>
        /// <param name="hour"></param>
        /// <returns></returns>
        private static int CalculateHourValue(int hour)
        {
            hour = hour % 8;
            switch (hour)
            {
                case 0: return 1;
                case 1: return 2;
                case 2: return 4;
                case 3: return 8;
                case 4: return 16;
                case 5: return 32;
                case 6: return 64;
                case 7: return 128;
            }

            return 0;
        }
    }
}
