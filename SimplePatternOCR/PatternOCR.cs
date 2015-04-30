using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace SimplePatternOCR
{
	public enum OCRCoupling
	{
		LOW_COUPLED_SEGMENTS = 0,
		NORMAL_COUPLED_SEGMENTS = 1,
		HIGH_COUPLED_SEGMENTS = 2,
	}

	public class PatternOCR
	{
		private struct PointI
		{
			public int X, Y;
		}

		public struct RectangleI
		{
			public readonly int X, Y;
			public readonly int Width, Height;

			public int X_Max { get { return X + Width; } }
			public int Y_Max { get { return Y + Height; } }

			public int X_Mid { get { return X + Width / 2; } }
			public int Y_Mid { get { return Y + Height / 2; } }

			public RectangleI(int x, int y, int w, int h)
			{
				X = x;
				Y = y;
				Width = w;
				Height = h;
			}
		}

		public OCRCoupling Coupling;

		private readonly double[] _FCB_TRESHOLD_INIT = { 160, 180, 225 };
		private readonly double[] _FCB_TRESHOLD_FLOOD = { 150, 170, 220 };
		private readonly double[] _FCB_TRESHOLD_X = { 65, 35, 2 };
		private readonly double[] _FCB_TRESHOLD_Y = { 210, 200, 190 };
		private readonly int[] _FCB_MIN_SIZE = { 4, 4, 16 };

		private double FCB_TRESHOLD_INIT { get { return _FCB_TRESHOLD_INIT[(int)Coupling]; } }
		private double FCB_TRESHOLD_FLOOD { get { return _FCB_TRESHOLD_FLOOD[(int)Coupling]; } }
		private double FCB_TRESHOLD_X { get { return _FCB_TRESHOLD_X[(int)Coupling]; } }
		private double FCB_TRESHOLD_Y { get { return _FCB_TRESHOLD_Y[(int)Coupling]; } }
		private int FCB_MIN_SIZE { get { return _FCB_MIN_SIZE[(int)Coupling]; } }

		private const int PATTERN_WIDTH = 32;
		private const int PATTERN_HEIGHT = 32;

		private const int MAX_IMGDISTANCE_OFFSET = 4;
		private const double EULER_DISTANCE_CORRECTION = 3.5;


		private readonly string[] OCR_CHARACTERS =
		{
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", 
			"{", "}", "-", "?", "10", "11", "12", "13", "14", "15", 
			"16", "17", "18", "19", "20", "21", "22", "23", "24"
		};

		private static readonly Dictionary<string, int> EULER_DICTIONARY = new Dictionary<string, int>
		{
			{ "0", 1 },
			{ "1", 0 },
			{ "2", 0 },
			{ "3", 0 },
			{ "4", 1 },
			{ "5", 0 },
			{ "6", 1 },
			{ "7", 0 },
			{ "8", 2 },
			{ "9", 1 },
			{ "{", 0 },
			{ "}", 0 },
			{ "-", 0 },
			{ "?", 0 },
		};

		private readonly PointI[] DIRECTIONS_E4 =
		{
			new PointI() {X = -1, Y = 0},
			new PointI() {X = +1, Y = 0},
			new PointI() {X = 0, Y = -1},
			new PointI() {X = 0, Y = +1},
		};

		//#####################################################################

		private Dictionary<string, byte[,]> references = new Dictionary<string, byte[,]>();

		public PatternOCR(OCRCoupling coupling)
		{
			Coupling = coupling;
			references = new Dictionary<string, byte[,]>();
		}

		public PatternOCR(Dictionary<string, Bitmap> refdic, OCRCoupling coupling)
		{
			Coupling = coupling;
			LoadReferencePatterns(refdic);
		}

		public void LoadReferencePatterns(Dictionary<string, Bitmap> refdic)
		{
			references.Clear();

			foreach (var element in refdic)
			{
				if (element.Value.Width != PATTERN_WIDTH || element.Value.Height != PATTERN_HEIGHT)
					throw new Exception("Pattern has wrong dimensions");

				references.Add(element.Key, GrayscaleBitmapToByteArr(element.Value));
			}
		}

		private Bitmap ConvertBitmapTo32bppArgb(Image b)
		{
			Bitmap result = new Bitmap(b.Width, b.Height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(result))
			{
				g.DrawImageUnscaled(b, 0, 0);
			}
			return result;
		}

		private Bitmap Force32bppArgb(Bitmap b)
		{
			if (b.PixelFormat != PixelFormat.Format32bppArgb)
				return ConvertBitmapTo32bppArgb(b);

			return b;
		}

		private byte[,] GrayscaleBitmapToByteArr(Bitmap img)
		{
			img = Force32bppArgb(img);

			BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			byte[,] result = new byte[img.Width, img.Height];

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				for (int x = 0; x < img.Width; x++)
				{
					for (int y = 0; y < img.Height; y++)
					{
						result[x, y] = p[(y * stride) + x * 4];
					}
				}
			}

			img.UnlockBits(srcData);

			return result;
		}

		private Bitmap ByteArrToGrayscaleBitmap(byte[,] arr)
		{
			Bitmap img = new Bitmap(arr.GetLength(0), arr.GetLength(1), PixelFormat.Format32bppArgb);

			BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				for (int x = 0; x < img.Width; x++)
				{
					for (int y = 0; y < img.Height; y++)
					{
						p[(y * stride) + x * 4 + 0] = arr[x, y];
						p[(y * stride) + x * 4 + 1] = arr[x, y];
						p[(y * stride) + x * 4 + 2] = arr[x, y];
						p[(y * stride) + x * 4 + 3] = 255;
					}
				}
			}

			img.UnlockBits(srcData);

			return img;
		}

		private Tuple<int, int, double, int> GetImageDistance(bool[,] imgA, byte[,] imgPattern, int imgAEuler, string pattern)
		{
			var result = Enumerable.Range(-MAX_IMGDISTANCE_OFFSET, 2 * MAX_IMGDISTANCE_OFFSET + 1)
				.SelectMany(
					ox =>
						Enumerable
							.Range(-MAX_IMGDISTANCE_OFFSET, 2 * MAX_IMGDISTANCE_OFFSET + 1)
							.Select(oy => new { ofx = ox, ofy = oy, val = GetImageDistance(imgA, imgPattern, ox, oy, imgAEuler, pattern) })
						)
				.OrderBy(p => p.val)
				.First();

			return Tuple.Create(result.ofx, result.ofy, result.val, imgAEuler);
		}

		private double GetImageDistance(bool[,] imgA, byte[,] imgPattern, int offsetX, int offsetY, int imgAEuler, string pattern)
		{
			double d = 0;
			int c = 0;

			double euler_correction = 0;
			if (EULER_DICTIONARY.ContainsKey(pattern))
			{
				euler_correction = (EULER_DICTIONARY[pattern] == imgAEuler) ? -EULER_DISTANCE_CORRECTION : +EULER_DISTANCE_CORRECTION;
			}

			for (int x = 0; x < PATTERN_WIDTH; x++)
			{
				for (int y = 0; y < PATTERN_HEIGHT; y++)
				{
					int patternValue;
					if (x + offsetX < 0 || x + offsetX >= PATTERN_WIDTH || y + offsetY < 0 || y + offsetY >= PATTERN_HEIGHT)
						patternValue = 255;
					else
						patternValue = imgPattern[x + offsetX, y + offsetY];

					int diff = (Math.Abs((imgA[x, y] ? 0 : 255) - patternValue));

					d += (diff / 255.0) * (diff / 255.0);
					c += imgA[x, y] ? 1 : 0;
				}
			}

			return Math.Max(0, ((d / c) * 100) + euler_correction);
		}

		public List<bool[,]> RecognizeSingleCharacter(Bitmap img)
		{
			var wfresult = FindCharacterBoxes(img);
			var grid = wfresult.Item1;
			var boxes = wfresult.Item2;

			List<bool[,]> result = new List<bool[,]>();

			foreach (var box in boxes)
			{
				bool[,] arr = new bool[PATTERN_WIDTH, PATTERN_HEIGHT];

				double scale = Math.Min((PATTERN_WIDTH * 1.0) / box.Item2.Width, (PATTERN_HEIGHT * 1.0) / box.Item2.Height);


				for (int x = 0; x < PATTERN_WIDTH; x++)
				{
					for (int y = 0; y < PATTERN_HEIGHT; y++)
					{
						double rx = box.Item2.X + x / scale;
						double ry = box.Item2.Y + y / scale;

						if (rx >= box.Item2.X_Max || ry >= box.Item2.Y_Max)
							continue;


						arr[x, y] = grid[(int)rx, (int)ry] == box.Item1;
					}
				}

				result.Add(arr);
			}

			return result;
		}

		public int GetEulerNumber(bool[,] image)
		{
			bool[,] finished = new bool[PATTERN_WIDTH, PATTERN_HEIGHT];

			for (int x = 0; x < PATTERN_WIDTH; x++)
			{
				for (int y = 0; y < PATTERN_HEIGHT; y++)
				{
					finished[x, y] = image[x, y];
				}
			}

			{
				Stack<PointI> outside = new Stack<PointI>();

				for (int ix = 0; ix < PATTERN_WIDTH; ix++)
				{
					if (!finished[ix, 0])
						outside.Push(new PointI() { X = ix, Y = 0 });
					if (!finished[ix, PATTERN_HEIGHT - 1])
						outside.Push(new PointI() { X = ix, Y = PATTERN_HEIGHT - 1 });
				}
				for (int iy = 0; iy < PATTERN_HEIGHT; iy++)
				{
					if (!finished[0, iy])
						outside.Push(new PointI() { X = 0, Y = iy });
					if (!finished[PATTERN_WIDTH - 1, iy])
						outside.Push(new PointI() { X = PATTERN_WIDTH - 1, Y = iy });
				}

				while (outside.Count > 0)
				{
					PointI point = outside.Pop();
					if (finished[point.X, point.Y])
						continue;

					finished[point.X, point.Y] = true;

					if (point.X - 1 >= 0 && !finished[point.X - 1, point.Y])
						outside.Push(new PointI() { X = point.X - 1, Y = point.Y });
					if (point.Y - 1 >= 0 && !finished[point.X, point.Y - 1])
						outside.Push(new PointI() { X = point.X, Y = point.Y - 1 });
					if (point.X + 1 < PATTERN_WIDTH && !finished[point.X + 1, point.Y])
						outside.Push(new PointI() { X = point.X + 1, Y = point.Y });
					if (point.Y + 1 < PATTERN_HEIGHT && !finished[point.X, point.Y + 1])
						outside.Push(new PointI() { X = point.X, Y = point.Y + 1 });
					if (point.X + 1 < PATTERN_WIDTH && point.Y + 1 < PATTERN_HEIGHT && !finished[point.X + 1, point.Y + 1])
						outside.Push(new PointI() { X = point.X + 1, Y = point.Y + 1 });
					if (point.X + 1 < PATTERN_WIDTH && point.Y - 1 >= 0 && !finished[point.X + 1, point.Y - 1])
						outside.Push(new PointI() { X = point.X + 1, Y = point.Y - 1 });
					if (point.X - 1 >= 0 && point.Y + 1 < PATTERN_HEIGHT && !finished[point.X - 1, point.Y + 1])
						outside.Push(new PointI() { X = point.X - 1, Y = point.Y + 1 });
					if (point.X - 1 >= 0 && point.Y - 1 >= 0 && !finished[point.X - 1, point.Y - 1])
						outside.Push(new PointI() { X = point.X - 1, Y = point.Y - 1 });
				}
			}

			int euler = 0;
			Stack<PointI> inside = new Stack<PointI>();

			for (int x = 0; x < PATTERN_WIDTH; x++)
			{
				for (int y = 0; y < PATTERN_HEIGHT; y++)
				{
					if (!finished[x, y])
					{
						euler++;
						inside.Push(new PointI() { X = x, Y = y });

						int size = 0;
						while (inside.Count > 0)
						{

							PointI point = inside.Pop();
							if (finished[point.X, point.Y])
								continue;

							size++;

							finished[point.X, point.Y] = true;

							if (point.X - 1 >= 0 && !finished[point.X - 1, point.Y])
								inside.Push(new PointI() { X = point.X - 1, Y = point.Y });
							if (point.Y - 1 >= 0 && !finished[point.X, point.Y - 1])
								inside.Push(new PointI() { X = point.X, Y = point.Y - 1 });
							if (point.X + 1 < PATTERN_WIDTH && !finished[point.X + 1, point.Y])
								inside.Push(new PointI() { X = point.X + 1, Y = point.Y });
							if (point.Y + 1 < PATTERN_HEIGHT && !finished[point.X, point.Y + 1])
								inside.Push(new PointI() { X = point.X, Y = point.Y + 1 });
							if (point.X + 1 < PATTERN_WIDTH && point.Y + 1 < PATTERN_HEIGHT && !finished[point.X + 1, point.Y + 1])
								inside.Push(new PointI() { X = point.X + 1, Y = point.Y + 1 });
							if (point.X + 1 < PATTERN_WIDTH && point.Y - 1 >= 0 && !finished[point.X + 1, point.Y - 1])
								inside.Push(new PointI() { X = point.X + 1, Y = point.Y - 1 });
							if (point.X - 1 >= 0 && point.Y + 1 < PATTERN_HEIGHT && !finished[point.X - 1, point.Y + 1])
								inside.Push(new PointI() { X = point.X - 1, Y = point.Y + 1 });
							if (point.X - 1 >= 0 && point.Y - 1 >= 0 && !finished[point.X - 1, point.Y - 1])
								inside.Push(new PointI() { X = point.X - 1, Y = point.Y - 1 });

						}

						if (size <= 12)
							euler--;
					}
				}
			}

			return euler;
		}

		public OCRResult RecognizeOCR(Bitmap img, OCRCoupling coupling)
		{
			Coupling = coupling;
			var ochars = RecognizeSingleCharacter(img);

			var distances = new List<Tuple<int, int, double>>();
			StringBuilder result = new StringBuilder();

			OCRResult ocrResult = new OCRResult()
			{
				Characters = new List<OCRCharacterResult>(),
			};

			foreach (var ochar in ochars)
			{
				var euler = GetEulerNumber(ochar);

				var matches = references
					.Select(p => new { Reference = p, Distance = GetImageDistance(ochar, p.Value, euler, p.Key) })
					.OrderBy(p => p.Distance.Item3)
					.ToList();

				if (matches.Count == 0)
				{
					throw new Exception("No References");
				}
				else
				{
					result.Append(matches.First().Reference.Key);

					ocrResult.Characters.Add(new OCRCharacterResult()
					{
						Character = matches.First().Reference.Key,

						OffsetX = matches.First().Distance.Item1,
						OffsetY = matches.First().Distance.Item2,
						Distance = matches.First().Distance.Item3,

						EulerNumber = matches.First().Distance.Item4,

						AllDistances = matches.ToDictionary(p => p.Reference.Key, p => p.Distance.Item3),
					});
				}
			}

			ocrResult.Value = result.ToString();
			ocrResult.Distance = (ocrResult.Characters.Count == 0) ? 0 : ocrResult.Characters.Max(p => p.Distance);

			return ocrResult;
		}

		public Tuple<int[,], List<Tuple<int, RectangleI>>> FindCharacterBoxes(Bitmap img)
		{
			img = Force32bppArgb(img);
			BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			int[,] grid = new int[img.Width, img.Height];
			var boxes = new List<Tuple<int, RectangleI>>();
			int shapecount = 0;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				Func<int, int, byte> pget_r = (x, y) => p[(y * stride) + x * 4];

				double col_black = Enumerable
					.Range(0, img.Width)
					.Select(px => Enumerable.Range(0, img.Height).Select(py => pget_r(px, py)).Min())
					.Min();

				Func<int, int, double> pget = (x, y) => (255 - pget_r(x, y)) / ((255 - col_black) / 255.0);

				for (int x = 1; x < img.Width - 1; x++)
				{
					for (int y = 1; y < img.Height - 1; y++)
					{
						if (grid[x, y] == 0 && pget(x, y) > FCB_TRESHOLD_INIT)
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

								foreach (var off in DIRECTIONS_E4)
								{
									int nx = point.X + off.X;
									int ny = point.Y + off.Y;

									bool valid = nx < img.Width - 1 && nx > 0 && ny > 0 && ny < img.Height - 1;

									double treshold = (nx == 0) ? FCB_TRESHOLD_Y : FCB_TRESHOLD_X;

									if (valid && grid[nx, ny] == 0 && ((pget(nx, ny) - cme) < treshold || allPoints.Count == 1) && pget(nx, ny) > FCB_TRESHOLD_FLOOD)
									{
										PointI nPoint = new PointI() { X = nx, Y = ny };

										grid[nPoint.X, nPoint.Y] = shapecount;
										walkStack.Push(nPoint);
									}
								}
							}

							var boxrect = new RectangleI(minX, minY, maxX - minX + 1, maxY - minY + 1);

							var box = boxes.FirstOrDefault(q =>
								boxrect.X < q.Item2.X_Max && boxrect.X > q.Item2.X ||
								boxrect.X_Max < q.Item2.X_Max && boxrect.X_Max > q.Item2.X ||
								boxrect.X_Mid < q.Item2.X_Max && boxrect.X_Mid > q.Item2.X ||

								q.Item2.X < boxrect.X_Max && q.Item2.X > boxrect.X ||
								q.Item2.X_Max < boxrect.X_Max && q.Item2.X_Max > boxrect.X ||
								q.Item2.X_Mid < boxrect.X_Max && q.Item2.X_Mid > boxrect.X
								);

							if (box != null)
							{
								allPoints.ForEach(pp => grid[pp.X, pp.Y] = box.Item1);

								minX = Math.Min(minX, box.Item2.X);
								maxX = Math.Max(maxX, box.Item2.X_Max - 1);
								minY = Math.Min(minY, box.Item2.Y);
								maxY = Math.Max(maxY, box.Item2.Y_Max - 1);

								boxes.Remove(box);

								boxrect = new RectangleI(minX, minY, maxX - minX + 1, maxY - minY + 1);

								boxes.Add(Tuple.Create(box.Item1, boxrect));
							}
							else if (allPoints.Count < FCB_MIN_SIZE)
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

		public Bitmap GetPatternDiff(bool[,] imgA, byte[,] imgPattern, int offsetX, int offsetY)
		{
			Bitmap diff = new Bitmap(PATTERN_WIDTH, PATTERN_HEIGHT, PixelFormat.Format32bppArgb);

			BitmapData srcData = diff.LockBits(new Rectangle(0, 0, diff.Width, diff.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				for (int x = 0; x < PATTERN_WIDTH; x++)
				{
					for (int y = 0; y < PATTERN_HEIGHT; y++)
					{
						int patternValue;
						if (x + offsetX < 0 || x + offsetX >= PATTERN_WIDTH || y + offsetY < 0 || y + offsetY >= PATTERN_HEIGHT)
							patternValue = 255;
						else
							patternValue = imgPattern[x + offsetX, y + offsetY];

						int diffv = 255 - (Math.Abs((imgA[x, y] ? 0 : 255) - patternValue));

						p[((y) * stride) + (x) * 4 + 0] = (byte)diffv;
						p[((y) * stride) + (x) * 4 + 1] = (byte)diffv;
						p[((y) * stride) + (x) * 4 + 2] = (byte)diffv;
						p[((y) * stride) + (x) * 4 + 3] = 255;
					}
				}

			}
			diff.UnlockBits(srcData);

			return diff;
		}

		public byte[,] GetPattern(string text)
		{
			return references[text];
		}

		private Dictionary<string, byte[,]> TrainPatterns(List<Tuple<string, bool[,]>> trainingdata)
		{
			int[] count = new int[OCR_CHARACTERS.Length];
			int[][,] characterdata = new int[OCR_CHARACTERS.Length][,];
			for (int i = 0; i < OCR_CHARACTERS.Length; i++)
				characterdata[i] = new int[32, 32];


			foreach (var data in trainingdata)
			{
				int pindex = OCR_CHARACTERS.ToList().IndexOf(data.Item1);

				count[pindex]++;

				for (int x = 0; x < 32; x++)
				{
					for (int y = 0; y < 32; y++)
					{
						characterdata[pindex][x, y] += data.Item2[x, y] ? 1 : 0;
					}
				}
			}

			Dictionary<string, byte[,]> result = new Dictionary<string, byte[,]>();

			for (int i = 0; i < OCR_CHARACTERS.Length; i++)
			{
				if (count[i] == 0)
					continue;

				byte[,] pattern = new byte[PATTERN_WIDTH, PATTERN_HEIGHT];

				for (int x = 0; x < 32; x++)
				{
					for (int y = 0; y < 32; y++)
					{
						double bytevalue = characterdata[i][x, y] * 255.0 / count[i];

						pattern[x, y] = (byte)(255 - bytevalue);
					}
				}

				result.Add(OCR_CHARACTERS[i], pattern);
			}

			return result;
		}

		public Dictionary<string, Bitmap> TrainPatternsToImage(List<Tuple<string, bool[,]>> trainingdata)
		{
			return TrainPatterns(trainingdata).ToDictionary(p => p.Key, p => ByteArrToGrayscaleBitmap(p.Value));
		}

		public string Recognize(Bitmap bmp, OCRCoupling coupling, out double errorDistance)
		{
			var ocr = RecognizeOCR(bmp, coupling);

			errorDistance = ocr.Distance;

			return ocr.Value;
		}
	}
}
