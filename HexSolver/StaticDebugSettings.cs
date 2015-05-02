using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace HexSolver
{
	class StaticDebugSettings
	{
		public static bool SaveOCRImages = false;

		private static Dictionary<string, int> ImageSaveDict = new Dictionary<string, int>();
		public static void ImageSave(Bitmap img, string text)
		{
			if (!SaveOCRImages)
				return;

			text = text.Replace("?", "Q");

			if (text.EndsWith("-") && !text.StartsWith("-"))
				text = "-" + text;
			if (!text.EndsWith("-") && text.StartsWith("-"))
				text = text + "-";
			if (text.EndsWith("{") && !text.StartsWith("}"))
				text = "{" + text;
			if (!text.EndsWith("{") && text.StartsWith("}"))
				text = text + "}";

			int shotid = ImageSaveDict.ContainsKey(text) ? ImageSaveDict[text] : 0;
			while (File.Exists(String.Format(@"..\..\imgsave\{0}_{1}.png", text, shotid)))
				shotid++;

			img.Save(String.Format(@"..\..\imgsave\{0}_{1}.png", text, shotid));
			ImageSaveDict[text] = shotid + 1;
		}

		public static int CleanImageSave()
		{
			Dictionary<string, List<string>> files = Directory
				.EnumerateFiles(@"..\..\imgsave\")
				.Where(p => p.EndsWith(".png"))
				.Where(p => Regex.IsMatch(Path.GetFileName(p), @"^(.*)_[0-9]+.png$"))
				.GroupBy(p => Regex.Match(Path.GetFileNameWithoutExtension(p), @"(.*)_[0-9]+").Groups[1].Value)
				.ToDictionary(p => p.Key, p => p.ToList());

			List<string> duplicates = new List<string>();

			foreach (var fileslist in files.Select(p => p.Value))
			{
				HashSet<string> hashList = new HashSet<string>();

				foreach (var file in fileslist)
				{
					byte[] hash;
					using (FileStream stream = File.OpenRead(file))
					{
						SHA256Managed sha = new SHA256Managed();
						hash = sha.ComputeHash(stream);
					}
					string shash = BitConverter.ToString(hash);

					if (hashList.Contains(shash))
						duplicates.Add(file);
					else
						hashList.Add(shash);
				}
			}

			foreach (var file in duplicates)
			{
				File.Delete(file);
			}

			return duplicates.Count;
		}
	}
}
