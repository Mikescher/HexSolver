<Query Kind="Expression">
  <Namespace>System.Drawing</Namespace>
</Query>

Directory
	.EnumerateFiles(Path.GetDirectoryName(Util.CurrentQueryPath) + @"\ocr_pattern\")
	.Select(p => new { Path = p, File = Path.GetFileName(p) })
	.Where(p => Regex.IsMatch(p.File, @"^pattern_(.*)\.png$"))
	.Select(p => new {Path = p.Path, File = p.File, Character = Regex.Match(p.File, @"^pattern_(.*)\.png$").Groups[1].Value.Replace("Q", "?")})
	.OrderBy(p => Regex.IsMatch(p.Character, @"^[0-9]+$") ? int.Parse(p.Character) : (99 + (int)p.Character[0]))
	.ToDictionary(p => p.Character, q => new Bitmap(System.Drawing.Image.FromFile(q.Path)))