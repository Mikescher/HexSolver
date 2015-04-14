using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;

namespace HexSolver
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		[DllImport("gdi32")]
		static extern int DeleteObject(IntPtr o);

		private HexCam cam = null;
		private HexOCR ocr = null;
		private HexRenderer renderer = null;
		private Bitmap screenshot = null;

		private bool skipUpdate = false;

		public MainWindow()
		{
			InitializeComponent();

			cam = new HexCam();
			ocr = new HexOCR();
			renderer = new HexRenderer();
		}

		private void OnCaptureClicked(object sender, RoutedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;

			screenshot = cam.GetScreenShot();

			imgDisplay.Source = LoadBitmap(screenshot);
		}

		private void OnExampleClicked(object sender, RoutedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;

			Image file = Image.FromFile("./example/shot001.png");
			screenshot = new Bitmap(file.Width, file.Height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(screenshot))
			{
				g.DrawImageUnscaled(file, 0, 0);
			}

			imgDisplay.Source = LoadBitmap(screenshot);
		}

		private void HexOCRValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;

			HexOCRValueSet(sender, null);
		}

		private void cbSwap_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;

			HexOCRValueSet(sender, null);
		}

		private void HexOCRValueAuto(object sender, RoutedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;
			if (screenshot == null)
				screenshot = cam.GetScreenShot();

			ocr.FindHexPattern(screenshot);

			updateOCRValues();

			imgDisplay.Source = LoadBitmap(renderer.DisplayCells(screenshot, ocr));
		}

		private void HexOCRValueSet(object sender, RoutedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;
			if (skipUpdate)
				return;
			if (screenshot == null)
				screenshot = cam.GetScreenShot();

			updateOCRProperties();

			imgDisplay.Source = LoadBitmap(renderer.DisplayCells(screenshot, ocr));
		}

		private void OnBinPattern(object sender, RoutedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;
			if (skipUpdate)
				return;
			if (screenshot == null)
				screenshot = cam.GetScreenShot();

			imgDisplay.Source = LoadBitmap(renderer.DisplayBinPattern(screenshot, ocr));
		}

		private void HexOCRValueUpdate(object sender, RoutedEventArgs e)
		{
			updateOCRValues();
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

		private void OnTypifyClicked(object sender, RoutedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;
			if (skipUpdate)
				return;
			if (screenshot == null)
				screenshot = cam.GetScreenShot();

			imgDisplay.Source = LoadBitmap(renderer.DisplayTypes(screenshot, ocr.GetHexagons(screenshot)));
		}

		private void OnProcessClicked(object sender, RoutedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;
			if (skipUpdate)
				return;
			if (screenshot == null)
				screenshot = cam.GetScreenShot();

			imgDisplay.Source = LoadBitmap(renderer.DisplayOCRProcess(screenshot, ocr));
		}

		private void OnOCRClicked(object sender, RoutedEventArgs e)
		{
			if (cam == null || ocr == null || renderer == null)
				return;
			if (skipUpdate)
				return;
			if (screenshot == null)
				screenshot = cam.GetScreenShot();

			imgDisplay.Source = LoadBitmap(renderer.DisplayOCR(screenshot, ocr));
		}

		private void updateOCRProperties()
		{
			ocr.CellRadius = dudRadius.Value.Value;
			ocr.CellGap = dudGap.Value.Value;
			ocr.CorrectionHorizontal = dudHCorr.Value.Value;
			ocr.CorrectionVertical = dudVCorr.Value.Value;
			ocr.PaddingX = dudPadX.Value.Value;
			ocr.PaddingY = dudPadY.Value.Value;
			ocr.InitialSwap = cbSwap.SelectedIndex == 0;
			ocr.NoCellBar_TR_X = dudBarTopRightX.Value.Value;
			ocr.NoCellBar_TR_Y = dudBarTopRightY.Value.Value;
		}

		private void updateOCRValues()
		{
			skipUpdate = true;

			dudRadius.Value = ocr.CellRadius;
			dudGap.Value = ocr.CellGap;
			dudHCorr.Value = ocr.CorrectionHorizontal;
			dudVCorr.Value = ocr.CorrectionVertical;
			dudPadX.Value = ocr.PaddingX;
			dudPadY.Value = ocr.PaddingY;
			cbSwap.SelectedIndex = ocr.InitialSwap ? 0 : 1;
			dudBarTopRightX.Value = ocr.NoCellBar_TR_X;
			dudBarTopRightY.Value = ocr.NoCellBar_TR_Y;

			skipUpdate = false;
		}
	}
}
