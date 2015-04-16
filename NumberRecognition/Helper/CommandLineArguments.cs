using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace MSHC.Helper
{
	public class CommandLineArguments
	{
		private StringDictionary Parameters;

		public CommandLineArguments(string[] Args)
		{
			Parameters = new StringDictionary();

			Regex Spliter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Regex Remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			string Parameter = null;
			string[] Parts;

			foreach (string Txt in Args)
			{
				Parts = Spliter.Split(Txt, 3);

				switch (Parts.Length)
				{
					case 1:
						if (Parameter != null)
						{
							if (!Parameters.ContainsKey(Parameter))
							{
								Parts[0] = Remover.Replace(Parts[0], "$1");
								Parameters.Add(Parameter, Parts[0]);
							}
							Parameter = null;
						}
						break;

					case 2:
						if (Parameter != null)
						{
							if (!Parameters.ContainsKey(Parameter))
								Parameters.Add(Parameter, "true");
						}
						Parameter = Parts[1];
						break;

					case 3:
						if (Parameter != null)
						{
							if (!Parameters.ContainsKey(Parameter))
								Parameters.Add(Parameter, "true");
						}

						Parameter = Parts[1];

						if (!Parameters.ContainsKey(Parameter))
						{
							Parts[2] = Remover.Replace(Parts[2], "$1");
							Parameters.Add(Parameter, Parts[2]);
						}

						Parameter = null;
						break;
				}
			}
			if (Parameter != null)
			{
				if (!Parameters.ContainsKey(Parameter))
					Parameters.Add(Parameter, "true");
			}
		}

		public bool Contains(string key)
		{
			return Parameters.ContainsKey(key);
		}

		public bool IsSet(string key)
		{
			return Parameters.ContainsKey(key) && Parameters[key] != null;
		}

		public string this[string Param]
		{
			get
			{
				return (Parameters[Param]);
			}
		}

		public bool isEmpty()
		{
			return Parameters.Count == 0;
		}

		#region String

		public string GetStringDefault(string p, string def)
		{
			return Contains(p) ? this[p] : def;
		}

		public List<String> GetStringList(string p, string delimiter, StringSplitOptions options = StringSplitOptions.None)
		{
			if (Contains(p))
				return this[p].Split(new string[] { delimiter }, options).ToList();
			else
				return null;
		}

		#endregion

		#region Long

		public bool IsLong(string p)
		{
			long a;
			return IsSet(p) && long.TryParse(Parameters[p], out a);
		}

		public long GetLong(string p)
		{
			return long.Parse(this[p]);
		}

		public long GetLongDefault(string p, long def)
		{
			return IsLong(p) ? GetLong(p) : def;
		}

		public long? GetLongDefaultNull(string p)
		{
			return IsLong(p) ? GetLong(p) : (long?)null;
		}

		public long GetLongDefaultRange(string p, long def, long min, long max)
		{
			return Math.Min(max - 1, Math.Max(min, (IsLong(p) ? GetLong(p) : def)));
		}

		public List<long> GetLongList(string p, string delimiter, bool sanitize = false)
		{
			List<String> ls = GetStringList(p, delimiter, sanitize ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			long aout;
			if (ls.Any(pp => !long.TryParse(pp, out aout)))
				return null;

			return ls.Select(long.Parse).ToList();
		}

		#endregion

		#region Integer

		public bool IsInt(string p)
		{
			int a;
			return IsSet(p) && int.TryParse(Parameters[p], out a);
		}

		public int GetInt(string p)
		{
			return int.Parse(this[p]);
		}

		public int GetIntDefault(string p, int def)
		{
			return IsInt(p) ? GetInt(p) : def;
		}

		public int? GetIntDefaultNull(string p)
		{
			return IsInt(p) ? GetInt(p) : (int?)null;
		}

		public int GetIntDefaultRange(string p, int def, int min, int max)
		{
			return Math.Min(max - 1, Math.Max(min, (IsInt(p) ? GetInt(p) : def)));
		}

		public List<int> GetIntList(string p, string delimiter, bool sanitize = false)
		{
			List<String> ls = GetStringList(p, delimiter, sanitize ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			int aout;
			if (ls.Any(pp => !int.TryParse(pp, out aout)))
				return null;

			return ls.Select(int.Parse).ToList();
		}

		#endregion

		#region UInteger

		public bool IsUInt(string p)
		{
			uint a;
			return IsSet(p) && uint.TryParse(Parameters[p], out a);
		}

		public uint GetUInt(string p)
		{
			return uint.Parse(this[p]);
		}

		public uint GetUIntDefault(string p, uint def)
		{
			return IsUInt(p) ? GetUInt(p) : def;
		}

		public uint? GetUIntDefaultNull(string p)
		{
			return IsUInt(p) ? GetUInt(p) : (uint?)null;
		}

		public uint GetUIntDefaultRange(string p, uint def, uint min, uint max)
		{
			return Math.Min(max - 1, Math.Max(min, (IsUInt(p) ? GetUInt(p) : def)));
		}

		public List<uint> GetUIntList(string p, string delimiter, bool sanitize = false)
		{
			List<String> ls = GetStringList(p, delimiter, sanitize ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			uint aout;
			if (ls.Any(pp => !uint.TryParse(pp, out aout)))
				return null;

			return ls.Select(uint.Parse).ToList();
		}

		#endregion

	}
}
