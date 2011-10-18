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
            var zone = TimeZone.CurrentTimeZone;
            return GetLogonTimes(byteMask, zone);
        }

        /// <summary>
        /// Calculate the logon times based on an Active Directory byte mask
        /// </summary>
        /// <param name="byteMask">Active Directory byte mask</param>
        /// <param name="timeZone">Time zone to convert to</param>
        /// <returns>List of LogonTime objects to signify allows times</returns>
        public static List<LogonTime> GetLogonTimes(byte[] byteMask, TimeZone timeZone)
        {
            throw new NotImplementedException();
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
            int index = 0, index2 = 0;

            while (index < hours.Count())
            {
                var result = ConvertBlockToAD(hours.Skip(index).Take(24).ToArray());

                permittedHours[index2] = result[0];
                permittedHours[index2+1] = result[1];
                permittedHours[index2+2] = result[2];

                index += 24;
                index += 3;
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
                var index = (int) Math.Floor((decimal) (i)/8);
                set[index] += CalculateLocationValue(i);
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
    }
}
