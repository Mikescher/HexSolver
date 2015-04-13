using MSHC.Geometry;
using MSHC.Helper;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Tesseract;

namespace HexSolver
{
	enum HexagonType
	{
		HIDDEN,   // ORANGE
		ACTIVE,   // BLUE
		INACTIVE, // BLACK
		NOCELL,   // GRAY
		UNKNOWN   // NOT IDENTIFIED
	}

	class HexagonCell
	{
		public static readonly Color COLOR_CELL_HIDDEN = Color.FromArgb(250, 155, 95);
		public static readonly Color COLOR_CELL_ACTIVE = Color.FromArgb(100, 155, 230);
		public static readonly Color COLOR_CELL_INACTIVE = Color.FromArgb(125, 90, 125);
		public static readonly Color COLOR_CELL_INACTIVE_2 = Color.FromArgb(62, 62, 62);
		public static readonly Color COLOR_CELL_NOCELL = Color.FromArgb(231, 231, 231);

		public static readonly Color[] COLOR_CELLS = new Color[] { COLOR_CELL_HIDDEN, COLOR_CELL_ACTIVE, COLOR_CELL_INACTIVE, COLOR_CELL_NOCELL, Color.Transparent };

		public static readonly int MAX_DISTANCE = 96;

		public Vec2d OCRCenter { get; private set; }
		public double OCRRadius { get; private set; }
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

		public HexagonCell(Vec2d center, double radius, Bitmap image)
		{
			this.OCRCenter = center;
			this.OCRRadius = radius;
			this.OCRImage = image;
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

		private static Color GetAverageColor(Vec2d OCRCenter, double OCRRadius, Bitmap OCRImage, Rectangle BoundingBox)
		{
			BitmapData srcData = OCRImage.LockBits(new Rectangle(0, 0, OCRImage.Width, OCRImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			int acR = 0;
			int acG = 0;
			int acB = 0;
			double acS = 0;

			double hexheight = OCRRadius * (Math.Sin(MathExt.ToRadians(60)) / Math.Sin(MathExt.ToRadians(90)));

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

						if (py < hexheight && py + 2 * (px - OCRRadius) < 0)
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

			double[] distance = COLOR_CELLS.Select(p => GetColorDistance(p.R, p.G, p.B, avg)).ToArray();

			double min_distance = distance.Min();

			if (min_distance >= MAX_DISTANCE)
				return HexagonType.UNKNOWN;

			return (HexagonType)Enum.GetValues(typeof(HexagonType)).GetValue(distance.ToList().IndexOf(min_distance));
		}

		private static int GetHue(int red, int green, int blue)
		{
			float min = Math.Min(Math.Min(red, green), blue);
			float max = Math.Max(Math.Max(red, green), blue);

			float hue = 0f;
			if (max == red)
			{
				hue = (green - blue) / (max - min);

			}
			else if (max == green)
			{
				hue = 2f + (blue - red) / (max - min);

			}
			else
			{
				hue = 4f + (red - green) / (max - min);
			}

			hue = hue * 60;
			if (hue < 0)
				hue = hue + 360;

			return (int)Math.Round(hue);
		}

		private static double GetColorDistance(byte aR, byte aG, byte aB, Color b)
		{
			return Math.Sqrt(Math.Pow(aR - b.R, 2) + Math.Pow(aG - b.G, 2) + Math.Pow(aB - b.B, 2));
		}

		private static double GetHueDistance(byte aR, byte aG, byte aB, Color b)
		{
			return Math.Abs(GetHue(aR, aG, aB) - b.GetHue());
		}

		private static Color InvertColor(Color a)
		{
			return Color.FromArgb(255 - a.R, 255 - a.G, 255 - a.B);
		}

		private static Color GrayscaleColor(Color a)
		{
			int c = (a.R + a.G + a.B) / 3;
			return Color.FromArgb(c, c, c);
		}

		public Bitmap GetOCRImage()
		{
			Bitmap img = OCRImage.Clone(GetBoundingBox(), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Color refColor = COLOR_CELLS[(int)Type];
			double hexheight = OCRRadius * (Math.Sin(MathExt.ToRadians(60)) / Math.Sin(MathExt.ToRadians(90)));

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

						if (py < hexheight && py + 2 * (px - OCRRadius) < 0)
						{

							byte repl_R;
							byte repl_G;
							byte repl_B;
							byte repl_A;

							switch (Type)
							{
								case HexagonType.HIDDEN:
									repl_R = 0;
									repl_G = 0;
									repl_B = 0;
									repl_A = 0;
									break;

								case HexagonType.ACTIVE:
									{
										double h_distance = GetHueDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_CELL_ACTIVE);
										if (h_distance < 32)
										{
											repl_R = 0;
											repl_G = 0;
											repl_B = 0;
											repl_A = 0;
										}
										else
										{
											repl_R = (byte)(255 - ((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_G = (byte)(255 - ((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_B = (byte)(255 - ((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_A = 255;
										}
									}
									break;

								case HexagonType.INACTIVE:
									{
										int grayness = ((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3;

										if (grayness < 128)
										{
											repl_R = 0;
											repl_G = 0;
											repl_B = 0;
											repl_A = 0;
										}
										else
										{
											repl_R = (byte)(255 - ((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_G = (byte)(255 - ((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_B = (byte)(255 - ((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_A = 255;
										}
									}
									break;

								case HexagonType.NOCELL:
									{
										double c_distance = GetColorDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_CELL_NOCELL);
										if (c_distance < 32)
										{
											repl_R = 0;
											repl_G = 0;
											repl_B = 0;
											repl_A = 0;
										}
										else
										{
											repl_R = (byte)(((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_G = (byte)(((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_B = (byte)(((int)p[idx + 2] + (int)p[idx + 1] + (int)p[idx + 0]) / 3);
											repl_A = 255;
										}
									}
									break;

								default:
								case HexagonType.UNKNOWN:
									repl_R = 0;
									repl_G = 0;
									repl_B = 0;
									repl_A = 255;
									break;
							}

							p[idx + 0] = repl_B;
							p[idx + 1] = repl_G;
							p[idx + 2] = repl_R;
							p[idx + 3] = repl_A;

						}
						else
						{
							p[idx + 0] = 0;
							p[idx + 1] = 0;
							p[idx + 2] = 0;
							p[idx + 3] = 0;
						}
					}
				}
			}

			img.UnlockBits(srcData);

			return img;
		}

		public string GetOCRString(TesseractEngine engine)
		{
			Bitmap img = GetOCRImage();

			using (var page = engine.Process(img))
			{
				Console.Out.WriteLine(page.GetText() + ": " + page.GetMeanConfidence());
				return page.GetText();
			}
		}
	}
}
