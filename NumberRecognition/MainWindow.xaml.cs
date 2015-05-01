using SimplePatternOCR;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace NumberRecognition
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly Tuple<String, Bitmap>[] standard_trainingdata =
		{
			Tuple.Create("-3-", Load32bppArgbBitmap(Properties.Resources.img_nocell_3_dist_err_1)),
			Tuple.Create("-3-", Load32bppArgbBitmap(Properties.Resources.img_nocell_3_dist_err_2)),
			Tuple.Create("-2-", Load32bppArgbBitmap(Properties.Resources.img_nocell_2_dist_err_1)),

			Tuple.Create("10", Load32bppArgbBitmap(Properties.Resources.img_nocell_10)),
			Tuple.Create("{5}", Load32bppArgbBitmap(Properties.Resources.img_inactive_5_cons)),
			Tuple.Create("15", Load32bppArgbBitmap(Properties.Resources.img_active_15)),
			Tuple.Create("?", Load32bppArgbBitmap(Properties.Resources.img_inactive_Q)),
			Tuple.Create("2", Load32bppArgbBitmap(Properties.Resources.img_active_2)),
			Tuple.Create("3", Load32bppArgbBitmap(Properties.Resources.img_active_3)),
			Tuple.Create("4", Load32bppArgbBitmap(Properties.Resources.img_active_4)),
			Tuple.Create("5", Load32bppArgbBitmap(Properties.Resources.img_active_5)),
			Tuple.Create("7", Load32bppArgbBitmap(Properties.Resources.img_active_7)),
			Tuple.Create("0", Load32bppArgbBitmap(Properties.Resources.img_inactive_0)),
			Tuple.Create("1", Load32bppArgbBitmap(Properties.Resources.img_inactive_1)),
			Tuple.Create("2", Load32bppArgbBitmap(Properties.Resources.img_inactive_2)),
			Tuple.Create("{2}", Load32bppArgbBitmap(Properties.Resources.img_inactive_2_cons)),
			Tuple.Create("-2-", Load32bppArgbBitmap(Properties.Resources.img_inactive_2_dist)),
			Tuple.Create("3", Load32bppArgbBitmap(Properties.Resources.img_inactive_3)),
			Tuple.Create("{3}", Load32bppArgbBitmap(Properties.Resources.img_inactive_3_cons)),
			Tuple.Create("-3-", Load32bppArgbBitmap(Properties.Resources.img_inactive_3_dist)),
			Tuple.Create("4", Load32bppArgbBitmap(Properties.Resources.img_inactive_4)),
			Tuple.Create("-4-", Load32bppArgbBitmap(Properties.Resources.img_inactive_4_dist)),
			Tuple.Create("0", Load32bppArgbBitmap(Properties.Resources.img_nocell_0)),
			Tuple.Create("1", Load32bppArgbBitmap(Properties.Resources.img_nocell_1)),
			Tuple.Create("{2}", Load32bppArgbBitmap(Properties.Resources.img_nocell_2_cons)),
			Tuple.Create("-2-", Load32bppArgbBitmap(Properties.Resources.img_nocell_2_dist)),
			Tuple.Create("3", Load32bppArgbBitmap(Properties.Resources.img_nocell_3)),
			Tuple.Create("-3-", Load32bppArgbBitmap(Properties.Resources.img_nocell_3_dist)),
			Tuple.Create("4", Load32bppArgbBitmap(Properties.Resources.img_nocell_4)),
			Tuple.Create("{4}", Load32bppArgbBitmap(Properties.Resources.img_nocell_4_cons)),
			Tuple.Create("5", Load32bppArgbBitmap(Properties.Resources.img_nocell_5)),
			Tuple.Create("6", Load32bppArgbBitmap(Properties.Resources.img_nocell_6)),
			Tuple.Create("7", Load32bppArgbBitmap(Properties.Resources.img_nocell_7)),
			Tuple.Create("8", Load32bppArgbBitmap(Properties.Resources.img_nocell_8)),
			Tuple.Create("9", Load32bppArgbBitmap(Properties.Resources.img_nocell_9)),
		};

		private readonly List<Tuple<String, Bitmap>> data = new List<Tuple<String, Bitmap>>();

		[DllImport("gdi32")]
		private static extern int DeleteObject(IntPtr o);

		public MainWindow()
		{
			InitializeComponent();
		}

		private static Bitmap Load32bppArgbBitmap(Bitmap b)
		{
			Bitmap result = new Bitmap(b.Width, b.Height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(result))
			{
				g.DrawImageUnscaled(b, 0, 0);
			}
			return result;
		}

		private static BitmapSource LoadBitmap(Bitmap source)
		{
			IntPtr ip = source.GetHbitmap();
			BitmapSource bs = null;
			try
			{
				bs = Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(ip);
			}

			return bs;
		}

		private void Load_Click(object sender, RoutedEventArgs e)
		{
			ContentGrid.Children.Clear();
			ContentGrid.RowDefinitions.Clear();
			data.Clear();

			int pos = 1;
			foreach (var tdata in standard_trainingdata)
			{
				ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
				ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

				SetContentGridCell(tdata.Item2, 1, pos);

				pos += 2;

				data.Add(tdata);
			}
		}

		private void LoadAll_Click(object sender, RoutedEventArgs e)
		{
			ContentGrid.Children.Clear();
			ContentGrid.RowDefinitions.Clear();
			data.Clear();

			int pos = 1;
			foreach (var file in Directory.EnumerateFileSystemEntries(@"..\..\testdata", "*.png", SearchOption.AllDirectories))
			{
				System.Drawing.Image ifile = System.Drawing.Image.FromFile(file);
				Bitmap bmp = new Bitmap(ifile.Width, ifile.Height, PixelFormat.Format32bppArgb);
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.DrawImageUnscaled(ifile, 0, 0);
				}

				ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
				ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

				SetContentGridCell(bmp, 1, pos);

				pos += 2;

				string fn = Path.GetFileNameWithoutExtension(file);

				data.Add(Tuple.Create(fn.Substring(0, fn.Contains('_') ? fn.IndexOf('_') : fn.Length).Replace("Q", "?"), bmp));
			}
		}

		private void LoadError_Click(object sender, RoutedEventArgs e)
		{
			ContentGrid.Children.Clear();
			ContentGrid.RowDefinitions.Clear();
			data.Clear();

			int pos = 1;
			foreach (var file in Directory.EnumerateFileSystemEntries(@"..\..\errordata", "*.png", SearchOption.AllDirectories))
			{
				System.Drawing.Image ifile = System.Drawing.Image.FromFile(file);
				Bitmap bmp = new Bitmap(ifile.Width, ifile.Height, PixelFormat.Format32bppArgb);
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.DrawImageUnscaled(ifile, 0, 0);
				}

				ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
				ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

				SetContentGridCell(bmp, 1, pos);

				pos += 2;

				string fn = Path.GetFileNameWithoutExtension(file);

				data.Add(Tuple.Create(fn.Substring(0, fn.Contains('_') ? fn.IndexOf('_') : fn.Length).Replace("Q", "?"), bmp));
			}
		}

		private void LoadCounter_Click(object sender, RoutedEventArgs e)
		{
			ContentGrid.Children.Clear();
			ContentGrid.RowDefinitions.Clear();
			data.Clear();

			int pos = 1;
			foreach (var file in Directory.EnumerateFileSystemEntries(@"..\..\counterdata", "*.png", SearchOption.AllDirectories))
			{
				System.Drawing.Image ifile = System.Drawing.Image.FromFile(file);
				Bitmap bmp = new Bitmap(ifile.Width, ifile.Height, PixelFormat.Format32bppArgb);
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.DrawImageUnscaled(ifile, 0, 0);
				}

				ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
				ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

				SetContentGridCell(bmp, 1, pos);

				pos += 2;

				string fn = Path.GetFileNameWithoutExtension(file);

				data.Add(Tuple.Create(fn.Substring(0, fn.Contains('_') ? fn.IndexOf('_') : fn.Length).Replace("Q", "?"), bmp));
			}
		}

		private void SetContentGridCell(Bitmap dataimg, int col, int row)
		{
			int factor = 4;

			Bitmap b4x = new Bitmap(dataimg.Width * factor, dataimg.Height * factor, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(b4x))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.DrawImage(dataimg, 0, 0, dataimg.Width * factor, dataimg.Height * factor);
			}

			Image img = new Image
			{
				Source = LoadBitmap(b4x),
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			Grid.SetRow(img, row);
			Grid.SetColumn(img, col);

			ContentGrid.Children.Add(img);
		}

		private void SetContentGridCell(string datastr, int col, int row, bool valid)
		{
			Label lbl = new Label
			{
				Content = datastr,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				FontSize = 48,
				FontWeight = valid ? FontWeights.Normal : FontWeights.Bold,
				Foreground = valid ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Red),
			};

			Grid.SetRow(lbl, row);
			Grid.SetColumn(lbl, col);

			ContentGrid.Children.Add(lbl);
		}

		//private static int picid = 0;

		private void Boxing_Click(object sender, RoutedEventArgs e)
		{
			Color[] CLRS = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Fuchsia, Color.SaddleBrown, Color.Purple, Color.Orange, Color.LightGray };

			PatternOCR pocr = new PatternOCR((OCRCoupling)cbxCoupling.SelectedIndex);

			{
				int pos = 1;
				foreach (var tdata in data)
				{
					var result = pocr.FindCharacterBoxes(tdata.Item2);
					var grid = result.Item1;

					Bitmap img = new Bitmap(tdata.Item2.Width, tdata.Item2.Height);
					using (Graphics g = Graphics.FromImage(img))
					{
						g.Clear(Color.White);

						for (int x = 1; x < img.Width - 1; x++)
						{
							for (int y = 1; y < img.Height - 1; y++)
							{
								if (grid[x, y] > 0)
									img.SetPixel(x, y, CLRS[grid[x, y] % CLRS.Length]);
							}
						}
					}

					SetContentGridCell(img, 3, pos);
					pos += 2;
				}
			}

			{
				int pos = 1;
				foreach (var tdata in data)
				{
					var characters = pocr.RecognizeSingleCharacter(tdata.Item2);

					int cpos = 0;
					foreach (var ochar in characters)
					{
						Bitmap img = new Bitmap(ochar.GetLength(0), ochar.GetLength(1));
						using (Graphics g = Graphics.FromImage(img))
						{
							g.Clear(Color.White);

							for (int x = 0; x < img.Width; x++)
							{
								for (int y = 0; y < img.Height; y++)
								{
									img.SetPixel(x, y, Color.FromArgb(ochar[x, y] ? 0 : 255, ochar[x, y] ? 0 : 255, ochar[x, y] ? 0 : 255));
								}
							}
						}

						SetContentGridCell(img, 5 + 2 * cpos++, pos);
						//img.Save(@"..\..\imgsave\_" + picid++ + ".png");
					}

					pos += 2;
				}
			}
		}

		private void Train_Click(object sender, RoutedEventArgs e)
		{
			PatternOCR pocr = new PatternOCR((OCRCoupling)cbxCoupling.SelectedIndex);

			List<Tuple<string, bool[,]>> trainingdata = new List<Tuple<string, bool[,]>>();

			foreach (var tdata in data)
			{
				var characters = pocr.RecognizeSingleCharacter(tdata.Item2);

				List<String> characters_final = (from Match match in Regex.Matches(tdata.Item1, "[0-9]+|[^0-9]+") select match.Value).ToList();

				int cpos = 0;
				foreach (var ochar in characters)
				{
					string pchar = null;
					if (characters.Count == characters_final.Count)
						pchar = characters_final[cpos];
					if (characters.Count == tdata.Item1.Length)
						pchar = tdata.Item1[cpos] + "";

					trainingdata.Add(Tuple.Create(pchar, ochar));

					cpos++;
				}
			}

			foreach (var pattern in pocr.TrainPatternsToImage(trainingdata))
			{
				pattern.Value.Save(@"..\..\pattern\pattern_" + pattern.Key.Replace("?", "Q") + ".png");
			}
		}

		private void Recognize_Click(object sender, RoutedEventArgs e)
		{
			var references = Directory
				.EnumerateFiles(@"..\..\pattern\")
				.Select(p => new { Path = p, File = Path.GetFileName(p) })
				.Where(p => Regex.IsMatch(p.File, @"^pattern_(.*)\.png$"))
				.ToDictionary(p => Regex.Match(p.File, @"^pattern_(.*)\.png$").Groups[1].Value.Replace("Q", "?"), q => new Bitmap(System.Drawing.Image.FromFile(q.Path)));

			PatternOCR pocr = new PatternOCR(references, (OCRCoupling)cbxCoupling.SelectedIndex);

			int errors = 0;

			List<int> distanceTable = new List<int>();

			int pos = 1;
			foreach (var tdata in data)
			{
				var ocr = pocr.RecognizeOCR(tdata.Item2, (OCRCoupling)cbxCoupling.SelectedIndex);

				SetContentGridCell(ocr.Value + "        {" + string.Join(", ", ocr.Characters.Select(p => p.Distance.ToString("F0"))) + "}" + "\r\n           {" + string.Join(", ", ocr.Characters.Select(p => p.EulerNumber)) + "}", 13, pos, ocr.Value == tdata.Item1);

				errors += ocr.Value == tdata.Item1 ? 0 : 1;

				//############################################
				{
					var ochars = pocr.RecognizeSingleCharacter(tdata.Item2);
					List<String> characters_final = (from Match match in Regex.Matches(ocr.Value, "[0-9]+|[^0-9]+") select match.Value).ToList();

					int opos = 0;
					foreach (var ochar in ochars)
					{
						string pchar = null;
						if (ochars.Count == characters_final.Count)
							pchar = characters_final[opos];
						else if (ochars.Count == ocr.Value.Length)
							pchar = (ocr.Value[opos] + "");
						else
							continue;

						var pattern = pocr.GetPattern(pchar);
						var diff = pocr.GetPatternDiff(ochar, pattern, ocr.Characters[opos].OffsetX, ocr.Characters[opos].OffsetY);

						SetContentGridCell(diff, 15 + 2 * opos, pos);

						opos++;
					}
				}
				//############################################

				SetContentGridCell(string.Join("\r\n", ocr.Characters.Select(p => string.Join(" | ", p.AllDistances.OrderBy(q => Regex.IsMatch(q.Key, @"^[0-9+]+$") ? int.Parse(q.Key) : ((q.Key + "@")[0] + 255)).Select(q => String.Format("{0}: {1:X}", q.Key, (int)q.Value))))), 23, pos, true);

				if (ocr.Value != tdata.Item1)
				{
					if (cbSaveError.IsChecked.Value)
					{
						int sidx = 0;
						while (File.Exists(string.Format(@"..\..\errordata\{0}_{1}.png", tdata.Item1, sidx)))
							sidx++;
						tdata.Item2.Save(string.Format(@"..\..\errordata\{0}_{1}.png", tdata.Item1, sidx));
						Console.Out.WriteLine("Saved errordata to " + Path.GetFullPath(string.Format(@"..\..\errordata\{0}_{1}.png", tdata.Item1, sidx)));
					}

					var ochars = pocr.RecognizeSingleCharacter(tdata.Item2);
					List<String> characters_final = (from Match match in Regex.Matches(tdata.Item1, "[0-9]+|[^0-9]+") select match.Value).ToList();

					int opos = 0;
					foreach (var ochar in ochars)
					{
						string pchar = null;
						if (ochars.Count == characters_final.Count)
							pchar = characters_final[opos];
						else if (opos < tdata.Item1.Length)
							pchar = (tdata.Item1[opos] + "");
						else
							continue;

						var pattern = pocr.GetPattern(pchar);
						var diff = pocr.GetPatternDiff(ochar, pattern, ocr.Characters[opos].OffsetX, ocr.Characters[opos].OffsetY);

						SetContentGridCell(diff, 25 + 2 * opos, pos);

						opos++;
					}
				}
				else
				{
					foreach (var chr in ocr.Characters)
					{
						if (chr.Character != "-")
							distanceTable.Add((int)chr.Distance);
					}
				}

				pos += 2;
			}

			MessageBox.Show(errors + "/" + data.Count + "  false detections");

			MessageBox.Show("Distances: " + Environment.NewLine + string.Join(Environment.NewLine, distanceTable.GroupBy(p => p).OrderBy(p => p.Key).Select(p => string.Format("{0:00} = {1}", p.Key, p.Count()))));
		}
	}
}
