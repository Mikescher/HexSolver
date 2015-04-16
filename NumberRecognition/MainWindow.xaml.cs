using MSHC.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
		private struct PointI
		{
			public int X, Y;
		}

		private readonly string[] chars =
		{
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", 
			"{", "}", "-", "?", "10", "11", "12", "13", "14", "15", 
			"16", "17", "18", "19", "20", "21", "22", "23", "24"
		};

		private readonly string[] chars_fn =
		{
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", 
			"{", "}", "-", "Q", "10", "11", "12", "13", "14", "15", 
			"16", "17", "18", "19", "20", "21", "22", "23", "24"
		};

		private readonly Tuple<String, Bitmap>[] standard_trainingdata =
		{
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

		private List<Tuple<String, Bitmap>> data = new List<Tuple<String, Bitmap>>();

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

				data.Add(Tuple.Create(fn.Substring(0, fn.IndexOf('_')).Replace("Q", "?"), bmp));
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
			int factor = 4;

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

		private static int picid = 0;

		private void Boxing_Click(object sender, RoutedEventArgs e)
		{
			{
				int pos = 1;
				foreach (var tdata in data)
				{
					var result = FindBoxes(tdata.Item2);
					var grid = result.Item1;

					Bitmap img = new Bitmap(tdata.Item2.Width, tdata.Item2.Height);
					using (Graphics g = Graphics.FromImage(img))
					{
						g.Clear(Color.White);

						for (int x = 1; x < img.Width - 1; x++)
						{
							for (int y = 1; y < img.Height - 1; y++)
							{
								if (grid[x, y] > 0 && grid[x, y] % 3 == 0)
									img.SetPixel(x, y, Color.Red);
								if (grid[x, y] > 0 && grid[x, y] % 3 == 1)
									img.SetPixel(x, y, Color.Green);
								if (grid[x, y] > 0 && grid[x, y] % 3 == 2)
									img.SetPixel(x, y, Color.Blue);
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
					var characters = GetSingleCharacter(tdata.Item2);

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
			int[] count = new int[chars.Length];
			int[][,] pattern = new int[chars.Length][,];
			for (int i = 0; i < chars.Length; i++)
				pattern[i] = new int[32, 32];

			foreach (var tdata in data)
			{
				var characters = GetSingleCharacter(tdata.Item2);

				List<String> characters_final = (from Match match in Regex.Matches(tdata.Item1, "[0-9]+|[^0-9]+") select match.Value).ToList();

				int cpos = 0;
				foreach (var ochar in characters)
				{
					string pchar = null;
					if (characters.Count == characters_final.Count)
						pchar = characters_final[cpos];
					if (characters.Count == tdata.Item1.Length)
						pchar = tdata.Item1[cpos] + "";

					int pindex = chars.ToList().IndexOf(pchar);

					count[pindex]++;

					for (int x = 0; x < 32; x++)
					{
						for (int y = 0; y < 32; y++)
						{
							pattern[pindex][x, y] += ochar[x, y] ? 1 : 0;
						}
					}

					cpos++;
				}
			}

			for (int i = 0; i < chars.Length; i++)
			{
				if (count[i] == 0)
					continue;

				Bitmap patternimage = new Bitmap(32, 32, PixelFormat.Format32bppArgb);

				for (int x = 0; x < 32; x++)
				{
					for (int y = 0; y < 32; y++)
					{
						double bytevalue = pattern[i][x, y] * 255.0 / count[i];

						byte bv = (byte)(255 - bytevalue);

						patternimage.SetPixel(x, y, Color.FromArgb(bv, bv, bv));
					}
				}

				patternimage.Save(@"..\..\pattern\pattern_" + chars_fn[i] + ".png");
			}
		}

		private void Recognize_Click(object sender, RoutedEventArgs e)
		{
			int errors = 0;

			int pos = 1;
			foreach (var tdata in data)
			{
				var ocr = RecognizeOCR(tdata.Item2);

				SetContentGridCell(ocr.Item1 + "        {" + string.Join(", ", ocr.Item2.Select(p => p.Item3.ToString("F0"))) + "}", 13, pos, ocr.Item1 == tdata.Item1);

				errors += ocr.Item1 == tdata.Item1 ? 0 : 1;

				//############################################

				var ochars = GetSingleCharacter(tdata.Item2);
				List<String> characters_final = (from Match match in Regex.Matches(tdata.Item1, "[0-9]+|[^0-9]+") select match.Value).ToList();

				int opos = 0;
				foreach (var ochar in ochars)
				{
					Bitmap diff = new Bitmap(32, 32, PixelFormat.Format32bppArgb);

					BitmapData srcData = diff.LockBits(new Rectangle(0, 0, diff.Width, diff.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
					IntPtr Scan0 = srcData.Scan0;
					int stride = srcData.Stride;

					string pchar = null;
					if (ochars.Count == characters_final.Count)
						pchar = characters_final[opos].Replace("?", "Q");
					if (ochars.Count == tdata.Item1.Length)
						pchar = (tdata.Item1[opos] + "").Replace("?", "Q");

					var imgPattern =
						new Bitmap(
							System.Drawing.Image.FromFile(@"..\..\pattern\pattern_" + pchar + ".png"));

					int offsetX = ocr.Item2[opos].Item1;
					int offsetY = ocr.Item2[opos].Item2;

					unsafe
					{
						byte* p = (byte*)(void*)Scan0;

						for (int x = 0; x < 32; x++)
						{
							for (int y = 0; y < 32; y++)
							{
								int patternValue;
								if (x + offsetX < 0 || x + offsetX > 31 || y + offsetY < 0 || y + offsetY > 31)
									patternValue = 255;
								else
									patternValue = imgPattern.GetPixel(x + offsetX, y + offsetY).R;

								int diffv = 255 - (Math.Abs((ochar[x, y] ? 0 : 255) - patternValue));

								p[((y) * stride) + (x) * 4 + 0] = (byte)diffv;
								p[((y) * stride) + (x) * 4 + 1] = (byte)diffv;
								p[((y) * stride) + (x) * 4 + 2] = (byte)diffv;
								p[((y) * stride) + (x) * 4 + 3] = 255;
							}
						}

					}
					diff.UnlockBits(srcData);

					SetContentGridCell(diff, 15 + 2 * opos, pos);

					//diff.Save("c:/asd.png");

					opos++;
				}

				pos += 2;
			}

			MessageBox.Show(errors + "false detections");
		}

		private Tuple<string, List<Tuple<int, int, double>>> RecognizeOCR(Bitmap img)
		{
			var ochars = GetSingleCharacter(img);

			var references = chars
				.Select(p => new { Char = p, File = (@"..\..\pattern\pattern_" + p + ".png").Replace("?", "Q") })
				.Where(p => File.Exists(p.File))
				.Select(p => new { Char = p.Char, File = p.File, Image = new Bitmap(System.Drawing.Image.FromFile(p.File)) });

			var distances = new List<Tuple<int, int, double>>();
			string result = "";

			foreach (var ochar in ochars)
			{
				var match = references.Select(p => new { Reference = p, Value = GetImageDistance(ochar, p.Image) }).OrderBy(p => p.Value.Item3).First();

				result += match.Reference.Char;
				distances.Add(match.Value);
			}

			return Tuple.Create(result, distances);
		}

		private Tuple<int, int, double> GetImageDistance(bool[,] imgA, Bitmap imgPattern)
		{
			var result = Enumerable.Range(-4, 9)
				.SelectMany(
					ox =>
						Enumerable.Range(-4, 9).Select(oy => new { ofx = ox, ofy = oy, val = GetImageDistance(imgA, imgPattern, ox, oy) }))
				.OrderBy(p => p.val)
				.First();

			return Tuple.Create(result.ofx, result.ofy, result.val);
		}

		private double GetImageDistance(bool[,] imgA, Bitmap imgPattern, int offsetX, int offsetY)
		{
			BitmapData srcData = imgPattern.LockBits(new Rectangle(0, 0, imgPattern.Width, imgPattern.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			double d = 0;
			int c = 0;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;


				for (int x = 0; x < 32; x++)
				{
					for (int y = 0; y < 32; y++)
					{
						int patternValue;
						if (x + offsetX < 0 || x + offsetX > 31 || y + offsetY < 0 || y + offsetY > 31)
							patternValue = 255;
						else
							patternValue = p[((y + offsetY) * stride) + (x + offsetX) * 4];

						int diff = (Math.Abs((imgA[x, y] ? 0 : 255) - patternValue));

						d += (diff / 255.0) * (diff / 255.0);
						c += imgA[x, y] ? 1 : 0;
					}
				}
			}

			imgPattern.UnlockBits(srcData);

			return (d / c) * 100;
		}

		private List<bool[,]> GetSingleCharacter(Bitmap img)
		{
			var wfresult = FindBoxes(img);
			var grid = wfresult.Item1;
			var boxes = wfresult.Item2;

			List<bool[,]> result = new List<bool[,]>();

			foreach (var box in boxes)
			{
				bool[,] arr = new bool[32, 32];

				double scale = Math.Min(32.0 / box.Item2.Width, 32.0 / box.Item2.Height);

				var bmin = box.Item2.bl;
				var bmax = box.Item2.tr;

				for (int x = 0; x < 32; x++)
				{
					for (int y = 0; y < 32; y++)
					{
						double rx = bmin.X + x / scale;
						double ry = bmin.Y + y / scale;

						if (rx >= bmax.X || ry >= bmax.Y)
							continue;


						arr[x, y] = grid[(int)rx, (int)ry] == box.Item1;
					}
				}

				result.Add(arr);
			}

			return result;
		}

		// Waterflooding
		private Tuple<int[,], List<Tuple<int, Rect2i>>> FindBoxes(Bitmap img)
		{
			const double TRESHOLD_INIT = 180;
			const double TRESHOLD_X = 35;
			const double TRESHOLD_Y = 200;

			PointI[] directions =
			{
				new PointI() {X = -1, Y = 0},
				new PointI() {X = +1, Y = 0},
				new PointI() {X = 0, Y = -1},
				new PointI() {X = 0, Y = +1},
			};

			BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			int[,] grid = new int[img.Width, img.Height];
			var boxes = new List<Tuple<int, Rect2i>>();
			int shapecount = 0;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				Func<int, int, byte> pget_r = (x, y) => p[(y * stride) + x * 4];

				double col_black = Enumerable.Range(0, img.Width).Select(px => Enumerable.Range(0, img.Height).Select(py => pget_r(px, py)).Min()).Min();

				Func<int, int, double> pget = (x, y) => (255 - pget_r(x, y)) / ((255 - col_black) / 255.0);

				for (int x = 1; x < img.Width - 1; x++)
				{
					for (int y = 1; y < img.Height - 1; y++)
					{
						if (grid[x, y] == 0 && pget(x, y) > TRESHOLD_INIT)
						{
							shapecount++;

							List<PointI> allPoints = new List<PointI>();
							Stack<PointI> walkStack = new Stack<PointI>();

							walkStack.Push(new PointI() { X = x, Y = y });
							grid[x, y] = shapecount;

							int minX = x;
							int maxX = x;
							int minY = y;
							int maxY = y;

							int maxCVal = 0;
							while (walkStack.Count > 0)
							{
								PointI point = walkStack.Pop();
								allPoints.Add(point);

								double cme = pget(point.X, point.Y);
								maxCVal = Math.Max(maxCVal, (int)cme);

								minX = Math.Min(minX, point.X);
								maxX = Math.Max(maxX, point.X);
								minY = Math.Min(minY, point.Y);
								maxY = Math.Max(maxY, point.Y);

								foreach (var off in directions)
								{
									int nx = point.X + off.X;
									int ny = point.Y + off.Y;

									bool valid = nx < img.Width - 1 && nx > 0 && ny > 0 && ny < img.Height - 1;

									double treshold = (nx == 0) ? TRESHOLD_Y : TRESHOLD_X;

									if (valid && grid[nx, ny] == 0 && ((pget(nx, ny) - cme) < treshold || allPoints.Count == 1) && pget(nx, ny) > TRESHOLD_INIT)
									{
										PointI nPoint = new PointI() { X = nx, Y = ny };

										grid[nPoint.X, nPoint.Y] = shapecount;
										walkStack.Push(nPoint);
									}
								}
							}

							var boxrect = new Rect2i(minX, minY, maxX - minX + 1, maxY - minY + 1);

							var box = boxes.FirstOrDefault(q =>
								boxrect.tl.X < q.Item2.tr.X && boxrect.tl.X > q.Item2.tl.X ||
								boxrect.tr.X < q.Item2.tr.X && boxrect.tr.X > q.Item2.tl.X ||
								q.Item2.tl.X < boxrect.tr.X && q.Item2.tl.X > boxrect.tl.X ||
								q.Item2.tr.X < boxrect.tr.X && q.Item2.tr.X > boxrect.tl.X
								);

							if (box != null)
							{
								allPoints.ForEach(pp => grid[pp.X, pp.Y] = box.Item1);

								minX = Math.Min(minX, box.Item2.bl.X);
								maxX = Math.Max(maxX, box.Item2.tr.X - 1);
								minY = Math.Min(minY, box.Item2.bl.Y);
								maxY = Math.Max(maxY, box.Item2.tr.Y - 1);

								boxes.Remove(box);

								boxrect = new Rect2i(minX, minY, maxX - minX + 1, maxY - minY + 1);

								boxes.Add(Tuple.Create(box.Item1, boxrect));
							}
							else if (allPoints.Count < 4)
							{
								shapecount--;
								allPoints.ForEach(pp => grid[pp.X, pp.Y] = 0);
							}
							else
							{
								boxes.Add(Tuple.Create(shapecount, boxrect));
							}
						}
					}
				}
			}

			img.UnlockBits(srcData);

			return Tuple.Create(grid, boxes);
		}
	}
}
