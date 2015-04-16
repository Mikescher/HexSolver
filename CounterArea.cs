using MSHC.Helper;
using SimplePatternOCR;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace HexSolver
{
	class CounterArea
	{
		public Rectangle BoundingBox { get; private set; }
		public Rectangle InnerBox { get; private set; }

		public Bitmap OCRImage { get; private set; }
		private readonly PatternOCR patternOCR;

		private CounterValue _Value = null;
		public CounterValue Value
		{
			get
			{
				return _Value ?? (_Value = GetCounterValue());
			}
		}

		public CounterArea(Rectangle o_area, Rectangle i_area, Bitmap image, PatternOCR pocr)
		{
			this.BoundingBox = o_area;
			this.InnerBox = i_area;
			this.OCRImage = image;
			this.patternOCR = pocr;
		}

		public Bitmap GetProcessedImage(bool useTransparency, out int activePixel)
		{
			Bitmap img = OCRImage.Clone(BoundingBox, PixelFormat.Format32bppArgb);
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

						double s_value = ColorExt.GetSaturation(p[idx + 2], p[idx + 1], p[idx + 0]);
						if (s_value >= 80)
						{
							if (useTransparency)
							{
								p[idx + 0] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
								p[idx + 1] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
								p[idx + 2] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
								p[idx + 3] = 0;

							}
							else
							{
								p[idx + 0] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
								p[idx + 1] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
								p[idx + 2] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
								p[idx + 3] = 255;
							}
						}
						else
						{
							activePixel++;

							p[idx + 0] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
							p[idx + 1] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
							p[idx + 2] = (byte)(255 - (p[idx + 2] + p[idx + 1] + p[idx + 0]) / 3);
							p[idx + 3] = 255;
						}
					}
				}
			}

			img.UnlockBits(srcData);

			return img;
		}

		private CounterValue GetCounterValue()
		{
			double errDistance;
			int activepixel;

			var img = GetProcessedImage(false, out activepixel);
			var txt = patternOCR.Recognize(img, out errDistance);

			int value = int.Parse(txt);

			return new CounterValue(value, errDistance);
		}
	}
}
