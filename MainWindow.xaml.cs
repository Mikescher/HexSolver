using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
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

		public MainWindow()
		{
			InitializeComponent();
		}

		private void onCaptureClicked(object sender, RoutedEventArgs e)
		{
			HexCam cam = new HexCam();
			HexOCR ocr = new HexOCR();

			Bitmap image = cam.GetScreenShot();
			image = ocr.FindCells(image);

			imgDisplay.Source = LoadBitmap(image);

		}

		public static BitmapSource LoadBitmap(Bitmap source)
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
	}
}
