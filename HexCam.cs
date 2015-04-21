using HexSolver.Solver;
using System;
using System.Collections.Generic;
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
		[DllImport("user32.dll")]
		private static extern long SetCursorPos(int x, int y);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetClientRect(HandleRef hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		private static extern bool ClientToScreen(IntPtr hwnd, ref Point lpPoint);

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out MousePoint lpMousePoint);

		private const int MOUSEEVENTF_LEFTDOWN = 0x02;
		private const int MOUSEEVENTF_LEFTUP = 0x04;
		private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
		private const int MOUSEEVENTF_RIGHTUP = 0x10;

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MousePoint
		{
			public int X;
			public int Y;
		}

		//###################################

		private const int MOUSE_MOVEMENT_TIME = 600;

		private Process _HexProcess;
		private Process HexProcess
		{
			get { return _HexProcess ?? (_HexProcess = GetHexCellsProcess()); }
			set { _HexProcess = value; HexProcessBounds = null; }
		}

		private IntPtr? _HexProcessHandle;
		private IntPtr? HexProcessHandle
		{
			get { return _HexProcessHandle ?? (_HexProcessHandle = HexProcess.MainWindowHandle); }
			set { _HexProcessHandle = value; HexProcessBounds = null; }
		}

		private RECT? _HexProcessBounds;
		private RECT? HexProcessBounds
		{
			get { return _HexProcessBounds ?? (_HexProcessBounds = GetClientRect(HexProcessHandle.Value)); }
			set { _HexProcessBounds = value; }
		}


		public Bitmap GetScreenShot()
		{
			try
			{
				SetForegroundWindow(HexProcessHandle.Value);
				Thread.Sleep(0);
				Bitmap shot = GrabScreen(HexProcessBounds.Value);
				Thread.Sleep(0);

				SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

				return shot;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.ToString());
				return null;
			}
		}

		private Process GetHexCellsProcess()
		{
			return Process
				.GetProcesses()
				.Where(p => !String.IsNullOrEmpty(p.MainWindowTitle))
				.FirstOrDefault(p => p.MainWindowTitle.ToLower().Contains("hexcells"));
		}

		private RECT? GetClientRect(IntPtr hWnd)
		{
			if (hWnd == null)
				return null;

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

		private void MoveMouseContinoous(int tx, int ty, double mtime)
		{
			MousePoint pos;
			GetCursorPos(out pos);

			int sx = pos.X;
			int sy = pos.Y;

			int starttime = Environment.TickCount;

			while ((Environment.TickCount - starttime) <= mtime)
			{
				int cx = sx + (int)((tx - sx) * Math.Min(1.0, (Environment.TickCount - starttime) / mtime));
				int cy = sy + (int)((ty - sy) * Math.Min(1.0, (Environment.TickCount - starttime) / mtime));

				SetCursorPos(cx, cy);
			}

			SetCursorPos(tx, ty);
		}

		private void ClickMouseSimple(int x, int y, bool left)
		{
			if (left)
				mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
			else
				mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)x, (uint)y, 0, 0);
		}

		public void Execute(HexStep solution)
		{
			SetForegroundWindow(HexProcessHandle.Value);

			int x = (int)(HexProcessBounds.Value.Left + solution.Cell.Image.OCRCenter.X);
			int y = (int)(HexProcessBounds.Value.Top + solution.Cell.Image.OCRCenter.Y);

			MoveMouseContinoous(x, y, MOUSE_MOVEMENT_TIME);
			ClickMouseSimple(x, y, solution.Action == CellAction.ACTIVATE);
		}

		public void Execute(IEnumerable<HexStep> solutions)
		{
			foreach (var solution in solutions)
			{
				Execute(solution);
				Thread.Sleep(350);
			}
		}

		public void Reset()
		{
			HexProcess = null;
		}
	}
}
