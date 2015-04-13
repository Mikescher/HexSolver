using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace HexSolver
{
	class HexCam
	{
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetClientRect(HandleRef hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		static extern bool ClientToScreen(IntPtr hwnd, ref Point lpPoint);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
		}

		public Bitmap GetScreenShot()
		{
			Process hcprocess = GetHexCellsProcess();
			if (hcprocess == null)
				return null;

			SetForegroundWindow(hcprocess.MainWindowHandle);
			Thread.Sleep(0);

			RECT hcbounds = GetClientRect(hcprocess.MainWindowHandle);
			Bitmap shot = GrabScreen(hcbounds);
			Thread.Sleep(0);

			SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

			return shot;
		}

		private Process GetHexCellsProcess()
		{
			return Process
				.GetProcesses()
				.Where(p => !String.IsNullOrEmpty(p.MainWindowTitle))
				.FirstOrDefault(p => p.MainWindowTitle.ToLower().Contains("hexcells"));
		}

		private RECT GetClientRect(IntPtr hWnd)
		{
			RECT bounds;
			GetClientRect(new HandleRef(this, hWnd), out bounds);

			Point pTL = new Point(bounds.Left, bounds.Top);
			Point pBR = new Point(bounds.Right, bounds.Bottom);

			ClientToScreen(hWnd, ref pTL);
			ClientToScreen(hWnd, ref pBR);

			return new RECT() { Top = pTL.Y, Left = pTL.X, Bottom = pBR.Y, Right = pBR.X };
		}

		private Bitmap GrabScreen(RECT hcbounds)
		{
			var bmpScreenshot = new Bitmap(hcbounds.Right - hcbounds.Left, hcbounds.Bottom - hcbounds.Top, PixelFormat.Format32bppArgb);

			using (var gfxScreenshot = Graphics.FromImage(bmpScreenshot))
			{
				gfxScreenshot.CopyFromScreen(hcbounds.Left, hcbounds.Top, 0, 0, bmpScreenshot.Size, CopyPixelOperation.SourceCopy);
			}

			return bmpScreenshot;
		}
	}
}
