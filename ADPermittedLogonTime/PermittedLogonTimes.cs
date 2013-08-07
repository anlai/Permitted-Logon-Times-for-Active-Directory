using System;
using System.Collections.Generic;
using System.Linq;

namespace ADPermittedLogonTime
{
    public class PermittedLogonTimes
    {
        /// <summary>
        /// Calculates the byte mask for Active Directory Logon Times
        /// </summary>
        /// <param name="logonTimes">List of LogonTime objects to signify allows times</param>
        /// <returns>Active directory byte mask</returns>
        public static byte[] GetByteMask (List<LogonTime> logonTimes )
        {
            var hours = InitializeTimeArray();

            foreach (var time in logonTimes)
            {
                // skip a block to go to the 24 block set that represents the targetted day
                var dayOffset = (int) time.DayOfWeek*24;

                // mark the hours
                MarkHours(hours, time);
            }

            // generate a byte [] for AD
            return ConvertToAD(hours);
        }

        /// <summary>
        /// Calculate the logon times based on an Active Directory byte mask
        /// </summary>
        /// <param name="byteMask">Active Directory byte mask</param>
        /// <returns>List of LogonTime objects to signify allows times</returns>
        public static List<LogonTime> GetLogonTimes(byte[] byteMask)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(TimeZone.CurrentTimeZone.StandardName);
            return GetLogonTimes(byteMask, zone);
        }

        /// <summary>
        /// Calculate the logon times based on an Active Directory byte mask
        /// </summary>
        /// <param name="byteMask">Active Directory byte mask</param>
        /// <param name="timeZone">Time zone to convert to</param>
        /// <returns>List of LogonTime objects to signify allows times</returns>
        public static List<LogonTime> GetLogonTimes(byte[] byteMask, TimeZoneInfo timeZone)
        {
            var hours = MarkHours(byteMask);

            return ConvertToLogonTime(hours, (timeZone.BaseUtcOffset.Hours));
        }
        
        /// <summary>
        /// Initialize an array for every hour of everyday for a week
        /// </summary>
        /// <remarks>
        /// Each slot represents an hour of a day.  Ex. [0]=sunday 12am GMT, [1]=sunday 1am GMT ...
        /// 
        /// During calculations based on time offset, hours will shift, Ex. [0]=sunday 1am GMT-1, [1]=sunday 2am GMT-1 ...
        /// 
        /// PST Calcuation (GMT -8): [9]=sunday 8am, [1]=sunday 9am
        /// 
        /// All values will be stored with an offset relative to GMT
        /// </remarks>
        /// <returns></returns>
        private static bool[] InitializeTimeArray()
        {
            var array = new bool[168];

            for (var i = 0; i < array.Count(); i++)
            {
                array[i] = false;
            }

            return array;
        }

        /// <summary>
        /// Mark the hours that have been selected
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="logonTime"></param>
        private static void MarkHours(bool[] hours, LogonTime logonTime)
        {
            // hour offset, from GMT
            var offset = logonTime.TimeZoneOffSet;
            // day offset in the array
            var dayOffset = (int) logonTime.DayOfWeek*24;

            // iterate through each of the hours of the day
            for (int i = 0; i < 24; i++)
            {
                // is the hour between what we're looking for
                if (logonTime.BeginTime.Hour <= i && i < logonTime.EndTime.Hour)
                {
                    // figure out which location to mark
                    var index = dayOffset + i + offset;

                    if (index < 0)
                    {
                        index = hours.Count() + index;
                    }
                    else if (index > hours.Count())
                    {
                        index = index - hours.Count();
                    }

                    hours[index] = true;
                }
            }

        }

        /// <summary>
        /// Convert the bool array to an AD byte mask
        /// </summary>
        /// <param name="hours"></param>
        /// <returns></returns>
        private static byte[] ConvertToAD(bool[] hours)
        {
            var permittedHours = new byte[21];
            var index = 0;  // index through the hours array
            var index2 = 0; // index through the permitted hours array

            while (index < hours.Length)
            {
                var result = ConvertBlockToAD(hours.Skip(index).Take(24).ToArray());

                permittedHours[index2] = result[0];
                permittedHours[index2+1] = result[1];
                permittedHours[index2+2] = result[2];

                index += 24;
                index2 += 3;
            }
            
            return permittedHours;
        }

        /// <summary>
        /// Convert individual blocks into AD
        /// </summary>
        /// <param name="hours"></param>
        /// <returns></returns>
        private static byte[] ConvertBlockToAD(bool[] hours)
        {
            var set = new int[3];

            for (var i = 0; i < 24; i++)
            {
                if (hours[i])
                {
                    var index = (int)Math.Floor((decimal)(i) / 8);
                    set[index] += CalculateLocationValue(i);    
                }
                
            }

            return new byte[3] { Convert.ToByte(set[0]), Convert.ToByte(set[1]), Convert.ToByte(set[2]) };
        }

        /// <summary>
        /// Calculate individual byte mask locations
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private static int CalculateLocationValue(int location)
        {
            location = location%8;

            switch(location)
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

        /// <summary>
        /// Fills in an hour array based on bytemask
        /// </summary>
        /// <param name="byteMask"></param>
        private static bool[] MarkHours(byte[] byteMask)
        {
            var hours = InitializeTimeArray();

            for (var i = 0; i < byteMask.Length; i++ )
            {
                ParseBlock(byteMask[i], hours, i * 8);
            }

            return hours;
        }

        /// <summary>
        /// Convert the byte block back into the array
        /// </summary>
        /// <param name="block"></param>
        /// <param name="hours"></param>
        /// <param name="index"></param>
        private static void ParseBlock(byte block, bool[] hours, int index)
        {
            var value = (int) block;

            if (value >= 128)
            {
                hours[index + 7] = true;
                value -= 128;
            }
            if (value >= 64)
            {
                hours[index + 6] = true;
                value -= 64;
            }
            if (value >= 32)
            {
                hours[index + 5] = true;
                value -= 32;
            }
            if (value >= 16)
            {
                hours[index + 4] = true;
                value -= 16;
            }
            if (value >= 8)
            {
                hours[index + 3] = true;
                value -= 8;
            }
            if (value >= 4)
            {
                hours[index + 2] = true;
                value -= 4;
            }
            if (value >= 2)
            {
                hours[index + 1] = true;
                value -= 2;
            }
            if (value >= 1)
            {
                hours[index] = true;
                value -= 1;
            }
        }

        private static List<LogonTime> ConvertToLogonTime(bool[] hours, int offset)
        {
            var ltimes = new List<LogonTime>();

            int? begin = null, end = null;

            for (var i = 0; i < hours.Length; i++)
            {
                var index = i + (-1)*offset;

                // shifts over begging, loop back to the end
                if (index < 0)
                {
                    index = hours.Length + index;
                }
                // shifts over end, start back from beginning of array
                else if (index >= hours.Length)
                {
                    index = index - hours.Length;
                }

                if (!begin.HasValue && hours[index])
                {
                    begin = CalculateHour(index, offset);
                }
                else if (begin.HasValue && !hours[index])
                {
                    end = CalculateHour(index, offset);

                    // save the day
                    ltimes.Add(new LogonTime(CalculateDay(index,offset), new DateTime(2011, 1, 1, begin.Value, 0, 0), new DateTime(2011, 1, 1, end.Value, 0,0)));

                    begin = null;
                    end = null;
                }

            }

            return ltimes;
        }

        private static int CalculateHour(int index, int offset)
        {
            var hour = index + offset;
            hour = hour%24;

            return hour;
        }

        private static DayOfWeek CalculateDay(int index, int offset)
        {
            var day = Math.Floor((decimal)(index + offset)/24);
            
            return (DayOfWeek) day;
        }
    }
}
