using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

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
		private Bitmap screenshot = null;

		private bool skipUpdate = false;

		public MainWindow()
		{
			InitializeComponent();

			cam = new HexCam();
			ocr = new HexOCR();
		}

		private void onCaptureClicked(object sender, RoutedEventArgs e)
		{
			if (cam == null)
				return;
			if (ocr == null)
				return;
			if (screenshot == null)
				screenshot = cam.GetScreenShot();

			imgDisplay.Source = LoadBitmap(ocr.FindCells(screenshot));
		}

		private void HexOCRValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			HexOCRValueSet(sender, null);
		}

		private void cbSwap_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			HexOCRValueSet(sender, null);
		}

		private void HexOCRValueSet(object sender, RoutedEventArgs e)
		{
			if (cam == null)
				return;
			if (ocr == null)
				return;
			if (skipUpdate)
				return;
			if (screenshot == null)
				screenshot = cam.GetScreenShot();

			updateOCRProperties();

			imgDisplay.Source = LoadBitmap(ocr.FindCells(screenshot));
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

		private void updateOCRProperties()
		{
			ocr.CellRadius = dudRadius.Value.Value;
			ocr.CellGap = dudGap.Value.Value;
			ocr.CorrectionHorizontal = dudHCorr.Value.Value;
			ocr.CorrectionVertical = dudVCorr.Value.Value;
			ocr.PaddingX = dudPadX.Value.Value;
			ocr.PaddingY = dudPadY.Value.Value;
			ocr.InitialSwap = cbSwap.SelectedIndex == 0;
			ocr.NoCellBarRight = dudBarRight.Value.Value;
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
			dudBarRight.Value = ocr.NoCellBarRight;

			skipUpdate = false;
		}
	}
}
