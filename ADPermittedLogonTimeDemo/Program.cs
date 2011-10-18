using System;
using System.Collections.Generic;
using ADPermittedLogonTime;
using ADPermittedLogonTimeDemo.Demo;

namespace ADPermittedLogonTimeDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AD Permitted Logon Times Demo");

            var oldTest = new List<ADPermittedLogonTimeDemo.Demo.LogonTime>();
            oldTest.Add(new ADPermittedLogonTimeDemo.Demo.LogonTime(DayOfWeek.Monday, new DateTime(2011, 1, 1, 8, 0, 0), new DateTime(2011, 1, 1, 10, 0, 0)));
            oldTest.Add(new ADPermittedLogonTimeDemo.Demo.LogonTime(DayOfWeek.Wednesday, new DateTime(2011, 1, 1, 13, 0, 0), new DateTime(2011, 1, 1, 16, 0, 0)));

            var oldResult = PermittedLogonTime.Calculate(oldTest);

            var newTest = new List<ADPermittedLogonTime.LogonTime>();
            var zone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            newTest.Add(new ADPermittedLogonTime.LogonTime(DayOfWeek.Monday, new DateTime(2011, 1, 1, 8, 0, 0), new DateTime(2011, 1, 1, 10, 0, 0), zone));
            newTest.Add(new ADPermittedLogonTime.LogonTime(DayOfWeek.Wednesday, new DateTime(2011, 1, 1, 13, 0, 0), new DateTime(2011, 1, 1, 16, 0, 0), zone));

            var newResult = PermittedLogonTimes.GetByteMask(newTest);

            for (var i = 0; i < newResult.Length; i++)
            {
                if (oldResult[i] != newResult[i])
                {
                    Console.WriteLine(oldResult[i]);
                    Console.WriteLine(newResult[i]);
                    Console.WriteLine("Results do not match.");
                    return;
                }
            }

            Console.WriteLine("Results match for generating AD byte mask.");

            PermittedLogonTimes.GetLogonTimes(newResult);
        }
    }
}
