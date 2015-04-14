using MSHC.Geometry;
using MSHC.Helper;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace HexSolver
{
	class HexagonCellImage
	{
		public static readonly Color COLOR_CELL_HIDDEN = Color.FromArgb(250, 155, 95);
		public static readonly Color COLOR_CELL_ACTIVE = Color.FromArgb(100, 155, 230);
		public static readonly Color COLOR_CELL_INACTIVE = Color.FromArgb(125, 90, 125);
		public static readonly Color COLOR_CELL_NOCELL = Color.FromArgb(231, 231, 231);

		public static readonly Color[] COLOR_CELLS = new Color[] { COLOR_CELL_HIDDEN, COLOR_CELL_ACTIVE, COLOR_CELL_INACTIVE, COLOR_CELL_NOCELL, Color.Transparent };

		public static readonly int MAX_TYPE_COLOR_DISTANCE = 96;

		public Vec2d OCRCenter { get; private set; }
		public double OCRRadius { get; private set; }
		public double OCRHeight { get; private set; }
		public Bitmap OCRImage { get; private set; }
		public Rectangle BoundingBox { get; private set; }

		public HexagonType? _Type = null;
		public HexagonType Type
		{
			get
			{
				if (_Type == null)
					_Type = GetHexagonType();

				return _Type.Value;
			}
		}

		public CellHint _Hint = null;
		public CellHint Hint
		{
			get
			{
				if (_Hint == null)
					_Hint = GetHexagonHint();

				return _Hint;
			}
		}

		public HexagonCellImage(Vec2d center, double radius, Bitmap image)
		{
			this.OCRCenter = center;
			this.OCRRadius = radius;
			this.OCRImage = image;
			this.OCRHeight = OCRRadius * (Math.Sin(MathExt.ToRadians(60)) / Math.Sin(MathExt.ToRadians(90)));
			this.BoundingBox = GetBoundingBox(center, radius);
		}

		private Rectangle GetBoundingBox()
		{
			return GetBoundingBox(OCRCenter, OCRRadius);
		}

		private static Rectangle GetBoundingBox(Vec2d center, double radius)
		{
			Vec2d top = new Vec2d(0, radius);
			top.Rotate(MathExt.ToRadians(30 + 3 * 60));

			Vec2d right = new Vec2d(0, radius);
			right.Rotate(MathExt.ToRadians(30 + 4 * 60));

			Vec2d bottom = new Vec2d(0, radius);
			bottom.Rotate(MathExt.ToRadians(30 + 5 * 60));

			Vec2d left = new Vec2d(0, radius);
			left.Rotate(MathExt.ToRadians(30 + 1 * 60));

			return new Rectangle((int)(center.X + left.X), (int)(center.Y + top.Y), (int)(right.X - left.X), (int)(bottom.Y - top.Y));
		}

		public Vec2d GetEdge(int edge)
		{
			Vec2d result = new Vec2d(0, OCRRadius);
			result.Rotate(MathExt.ToRadians(30 + edge * 60));

			result += OCRCenter;

			return result;
		}

		private Color GetAverageColor(Vec2d OCRCenter, double OCRRadius, Bitmap OCRImage, Rectangle BoundingBox)
		{
			BitmapData srcData = OCRImage.LockBits(new Rectangle(0, 0, OCRImage.Width, OCRImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			int acR = 0;
			int acG = 0;
			int acB = 0;
			double acS = 0;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				for (int x = BoundingBox.Left; x < BoundingBox.Right; x++)
				{
					for (int y = BoundingBox.Top; y < BoundingBox.Bottom; y++)
					{
						int idx = (y * stride) + x * 4;

						double px = Math.Abs(x - OCRCenter.X);
						double py = Math.Abs(y - OCRCenter.Y);

						if (py < OCRHeight && py + 2 * (px - OCRRadius) < 0)
						{
							acB += p[idx + 0];
							acG += p[idx + 1];
							acR += p[idx + 2];
							acS += 255;
						}
					}
				}
			}

			OCRImage.UnlockBits(srcData);

			return Color.FromArgb((int)(255 * acR / acS), (int)(255 * acG / acS), (int)(255 * acB / acS));
		}

		private HexagonType GetHexagonType()
		{
			Color avg = GetAverageColor(OCRCenter, OCRRadius, OCRImage, BoundingBox);

			double[] distance = COLOR_CELLS.Select(p => RGBHelper.GetColorDistance(p.R, p.G, p.B, avg)).ToArray();

			double min_distance = distance.Min();

			if (min_distance >= MAX_TYPE_COLOR_DISTANCE)
				return HexagonType.UNKNOWN;

			return (HexagonType)Enum.GetValues(typeof(HexagonType)).GetValue(distance.ToList().IndexOf(min_distance));
		}

		public Bitmap GetOCRImage(bool useTransparency)
		{
			int temp;
			return GetOCRImage(useTransparency, out temp);
		}

		public Bitmap GetOCRImage(bool useTransparency, out int activePixel)
		{
			Bitmap img = OCRImage.Clone(GetBoundingBox(), PixelFormat.Format32bppArgb);
			double hexheight = OCRRadius * (Math.Sin(MathExt.ToRadians(60)) / Math.Sin(MathExt.ToRadians(90)));
			activePixel = 0;

			BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				for (int x = 0; x < img.Width; x++)
				{
					for (int y = 0; y < img.Height; y++)
					{
						int idx = (y * stride) + x * 4;

						double px = Math.Abs(x - img.Width / 2);
						double py = Math.Abs(y - img.Height / 2);
						double pd = Math.Sqrt(px * px + py * py);

						if (py < hexheight && py + 2 * (px - OCRRadius) < 0)
						{
							byte repl_R = 255;
							byte repl_G = 255;
							byte repl_B = 255;
							byte repl_A = (byte)(useTransparency ? 0 : 255);

							switch (Type)
							{
								case HexagonType.HIDDEN:
									// TRANSPARENT
									break;

								case HexagonType.ACTIVE:
									{
										double h_distance = RGBHelper.GetHueDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_CELL_ACTIVE);
										if (h_distance < 32 || pd > (OCRHeight - 1))
										{
											// TRANSPARENT
										}
										else
										{
											activePixel++;

											repl_R = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_G = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_B = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_A = 255;
										}
									}
									break;

								case HexagonType.INACTIVE:
									{
										int grayness = (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3;

										if (grayness < 128 || pd > (OCRHeight - 1))
										{
											// TRANSPARENT
										}
										else
										{
											activePixel++;

											repl_R = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_G = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_B = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_A = 255;
										}
									}
									break;

								case HexagonType.NOCELL:
									{
										double c_distance = RGBHelper.GetColorDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_CELL_NOCELL);
										if (c_distance < 32)
										{
											// TRANSPARENT
										}
										else
										{
											activePixel++;

											repl_R = (byte)((p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_G = (byte)((p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_B = (byte)((p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
											repl_A = 255;
										}
									}
									break;

								default:
								case HexagonType.UNKNOWN:
									// TRANSPARENT
									break;
							}

							p[idx + 0] = repl_B;
							p[idx + 1] = repl_G;
							p[idx + 2] = repl_R;
							p[idx + 3] = repl_A;

						}
						else
						{
							p[idx + 0] = 255;
							p[idx + 1] = 255;
							p[idx + 2] = 255;
							p[idx + 3] = (byte)(useTransparency ? 0 : 255);
						}
					}
				}

				//

			}

			img.UnlockBits(srcData);

			return img;
		}

		private CellHintArea GetHintColumn(Bitmap img)
		{
			int[] pixelcount = { 0, 0, 0 };

			BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			Vec2d p0 = GetEdge(0) - OCRCenter;
			Vec2d p1 = GetEdge(1) - OCRCenter;
			Vec2d p2 = GetEdge(2) - OCRCenter;
			Vec2d p3 = GetEdge(3) - OCRCenter;
			Vec2d p4 = GetEdge(4) - OCRCenter;
			Vec2d p5 = GetEdge(5) - OCRCenter;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				for (int x = 0; x < img.Width; x++)
				{
					for (int y = 0; y < img.Height; y++)
					{
						int idx = (y * stride) + x * 4;

						if (p[idx + 0] + p[idx + 1] + p[idx + 2] == 255 * 3)
							continue;


						Vec2d rpos = new Vec2d((x - img.Width / 2), (y - img.Height / 2));

						if (isRight(p2, p5, rpos) && (isRight(p1, p3, rpos) && isRight(p4, p0, rpos)))
							pixelcount[0]++;

						if (isRight(p1, p4, rpos) && (isRight(p0, p2, rpos) && isRight(p3, p5, rpos)))
							pixelcount[1]++;

						if (isRight(p0, p3, rpos) && (isRight(p5, p1, rpos) && isRight(p2, p4, rpos)))
							pixelcount[2]++;
					}
				}
			}

			img.UnlockBits(srcData);

			if (pixelcount[0] > 16 && pixelcount[0] == pixelcount.Max())
				return CellHintArea.COLUMN_LEFT;
			if (pixelcount[1] > 16 && pixelcount[1] == pixelcount.Max())
				return CellHintArea.COLUMN_DOWN;
			if (pixelcount[2] > 16 && pixelcount[2] == pixelcount.Max())
				return CellHintArea.COLUMN_RIGHT;

			return CellHintArea.NONE;
		}

		private bool isRight(Vec2d a, Vec2d b, Vec2d c)
		{
			return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0;
		}

		private Bitmap RotateImage(Bitmap bitmap, float angle, Color bg)
		{
			Bitmap returnBitmap = new Bitmap(bitmap.Width, bitmap.Height);
			Graphics graphics = Graphics.FromImage(returnBitmap);
			graphics.TranslateTransform((float)bitmap.Width / 2, (float)bitmap.Height / 2);
			graphics.RotateTransform(angle);
			graphics.TranslateTransform(-(float)bitmap.Width / 2, -(float)bitmap.Height / 2);
			graphics.Clear(bg);
			graphics.DrawImage(bitmap, new Point(0, 0));
			return returnBitmap;
		}

		private static int ocrctr;
		private CellHint GetHexagonHint()
		{
			if (Type == HexagonType.HIDDEN || Type == HexagonType.UNKNOWN)
				return new CellHint();

			if (Type == HexagonType.INACTIVE)
			{
				using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
				{
					engine.SetVariable("tessedit_char_whitelist", "0123456789-{}?");

					int activePixel;
					Bitmap img = GetOCRImage(false, out activePixel);
					if (activePixel == 0)
						return new CellHint();

					img.Save(@"..\..\imgsave\img_inactive" + (ocrctr++) + ".png", ImageFormat.Png);

					using (var page = engine.Process(img))
					{
						string txt = page.GetText();
						txt = Regex.Replace(txt, @"[^0123456789-\{\}\?]", "");
						Console.Out.WriteLine(page.GetText() + ": " + page.GetMeanConfidence());

						if (Regex.IsMatch(txt, @"^\{[0-9]+\}$"))
							return new CellHint(CellHintType.CONSECUTIVE, CellHintArea.DIRECT, int.Parse(txt.Substring(1, txt.Length - 2)));
						if (Regex.IsMatch(txt, @"^-[0-9]+-$"))
							return new CellHint(CellHintType.NONCONSECUTIVE, CellHintArea.DIRECT, int.Parse(txt.Substring(1, txt.Length - 2)));
						if (Regex.IsMatch(txt, @"^[0-9]+$"))
							return new CellHint(CellHintType.COUNT, CellHintArea.DIRECT, int.Parse(txt));
						if (txt == "?")
							return new CellHint();


						Console.Out.WriteLine("THROW EXCEPTION PLS"); //TODO THROW EXCEPTION PLS
						return new CellHint(CellHintType.COUNT, CellHintArea.DIRECT, 0);
					}
				}
			}

			if (Type == HexagonType.ACTIVE)
			{
				using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
				{
					engine.SetVariable("tessedit_char_whitelist", "0123456789-{}?");

					int activePixel;
					Bitmap img = GetOCRImage(false, out activePixel);
					if (activePixel == 0)
						return new CellHint();

					img.Save(@"..\..\imgsave\img_active" + (ocrctr++) + ".png", ImageFormat.Png);

					using (var page = engine.Process(img))
					{
						string txt = page.GetText();
						txt = Regex.Replace(txt, @"[^0123456789-\{\}\?]", "");
						Console.Out.WriteLine(page.GetText() + ": " + page.GetMeanConfidence());

						if (Regex.IsMatch(txt, @"^[0-9]+$"))
							return new CellHint(CellHintType.COUNT, CellHintArea.CIRCLE, int.Parse(txt));


						Console.Out.WriteLine("THROW EXCEPTION PLS"); //TODO THROW EXCEPTION PLS
						return new CellHint(CellHintType.COUNT, CellHintArea.CIRCLE, 0);
					}
				}
			}

			if (Type == HexagonType.NOCELL)
			{
				int activePixel;
				Bitmap img = GetOCRImage(false, out activePixel);
				if (activePixel == 0)
					return new CellHint();

				CellHintArea col = GetHintColumn(img);

				if (col == CellHintArea.NONE)
				{
					Console.Out.WriteLine("THROW EXCEPTION PLS"); //TODO THROW EXCEPTION PLS
					return new CellHint(CellHintType.COUNT, col, 0);
				}

				if (col == CellHintArea.COLUMN_LEFT)
					img = RotateImage(img, -60, Color.White);
				else if (col == CellHintArea.COLUMN_RIGHT)
					img = RotateImage(img, +60, Color.White);

				img.Save(@"..\..\imgsave\img_nocell_" + (int)col + "_" + (ocrctr++) + ".png", ImageFormat.Png);

				using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
				{
					engine.SetVariable("tessedit_char_whitelist", "0123456789-{}?");

					using (var page = engine.Process(img))
					{
						string txt = page.GetText();
						txt = Regex.Replace(txt, @"[^0123456789-\{\}\?]", "");
						Console.Out.WriteLine(page.GetText() + ": " + page.GetMeanConfidence());

						if (Regex.IsMatch(txt, @"^\{[0-9]+\}$"))
							return new CellHint(CellHintType.CONSECUTIVE, col, int.Parse(txt.Substring(1, txt.Length - 2)));
						if (Regex.IsMatch(txt, @"^-[0-9]+-$"))
							return new CellHint(CellHintType.NONCONSECUTIVE, col, int.Parse(txt.Substring(1, txt.Length - 2)));
						if (Regex.IsMatch(txt, @"^[0-9]+$"))
							return new CellHint(CellHintType.COUNT, col, int.Parse(txt));


						Console.Out.WriteLine("THROW EXCEPTION PLS"); //TODO THROW EXCEPTION PLS
						return new CellHint(CellHintType.COUNT, col, 0);
					}
				}
			}

			throw new Exception("WTF - Type ==" + Type);
		}
	}
}
