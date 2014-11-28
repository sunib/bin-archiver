using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinArchiver
{
    public class Helpers
    {
        /// <summary>
        /// Converts a C# Datetime to the number of milliseconds since 1-1-1970.
        /// Inspired by: http://stackoverflow.com/a/17632585
        /// </summary>
        /// <param name="utc">The DateTime to be converted, in UTC!</param>
        /// <returns>Unix timestamp in milliseconds.</returns>
        public static ulong convertToUnixTimeMilliseconds(DateTime timeUtc)
        {
            return (ulong)(timeUtc.Subtract(new DateTime(1970, 1, 1))).TotalSeconds * 1000;
        }

        /// <summary>
        /// Converts a unix timestamp in milliseconds to a .NET DateTime object.
        /// Inspired by: http://stackoverflow.com/a/250400
        /// </summary>
        /// <param name="unixTimeMilliseconds">The number of milliseconds from 1970-1-1.</param>
        /// <returns>A filled .NET DateTime object</returns>
        public static DateTime convertUnixTimeMillisecondsToDateTime(ulong unixTimeMilliseconds)
        {
            DateTime result = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            result = result.AddMilliseconds( unixTimeMilliseconds );
            return result;
        }
    }
}
