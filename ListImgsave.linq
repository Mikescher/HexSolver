<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
</Query>

readonly string PATH = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "imgsave");
readonly string PATH_CD = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "NumberRecognition", "counterdata");
readonly string PATH_TD = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "NumberRecognition", "testdata");

void Main()
{		
	new Hyperlinq(() => 
	{
		
		Directory
			.EnumerateFiles(PATH)
			.Select(p => new{Source = p, Target = Path.Combine(PATH_CD, Path.GetFileName(p)) , Image = Image.FromFile(p)})
			.Where(p => p.Image.Width/p.Image.Height > 2.0)
			.ToList()
			.ForEach(p => File.Copy(p.Source, p.Target));
		"SUCESS".Dump();
	
	}, "[Copy counterdata]").Dump();
	"".Dump();
	
	
	new Hyperlinq(() => 
	{
		Directory
			.EnumerateFiles(PATH_TD)
			.ToList()
			.ForEach(p => File.Delete(p));
			
		Directory
			.EnumerateFiles(PATH)
			.Select(p => new{Source = p, Target = Path.Combine(PATH_TD, Path.GetFileName(p)) , Image = Image.FromFile(p)})
			.Where(p => p.Image.Width/p.Image.Height < 2.0)
			.ToList()
			.ForEach(p => File.Copy(p.Source, p.Target));
		"SUCESS".Dump();
	
	}, "[Copy testdata]").Dump();
	"".Dump();
		
	//################################################################################
		
	Directory
		.EnumerateFiles(PATH)
		.Where(p => Path.GetFileName(p).EndsWith(".png"))
		.Select(p => Path.GetFileNameWithoutExtension(p))
		.GroupBy(p => Regex.Match(p, @"(.*)_[0-9]*").Groups[1].Value)
		.Select(p => new{Value = p.Key, Count = p.Count()})
		.OrderBy(p => GOV(p.Value))
		.Dump();
		
	Directory
		.EnumerateFiles(PATH)
		.Where(p => Path.GetFileName(p).EndsWith(".png"))
		.Select(p => new{Value = Regex.Match(Path.GetFileNameWithoutExtension(p), @"(.*)_[0-9]*").Groups[1].Value, Image = Image.FromFile(p)})
		.Where(p => p.Image.Width/p.Image.Height > 2.0)
		.OrderBy(p => GOV(p.Value))
		.Dump();
}

int GOV(string s)
{
	if (s.Length == 0) return 0;
	
	string n = Regex.Replace(s, @"[^0-9]", "");
	if (n.Length == 0) return 512;
	
	return int.Parse(n) + ((s==n) ? (0) : (s[0] * 512));
}