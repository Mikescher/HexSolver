using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MSHC.Helper
{
	public static class TimeFormatHelper
	{
		public static string FormatTimespan(TimeSpan ts)
		{
			var parts = string
							.Format("{0}d:{1}h:{2}m:{3}s:{4}ms", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds)
							.Split(':')
							.SkipWhile(s => Regex.Match(s, @"0\w").Success); // skip zero-valued components

			var join = string.Join(" ", parts);

			if (join == "")
				join = "0ms";

			return join;
		}

		public static string FormatMilliseconds(long ms)
		{
			return FormatTimespan(TimeSpan.FromMilliseconds(ms));
		}
	}
}
