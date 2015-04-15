using System.Drawing;

namespace HexSolver
{
	class HexcellsSolver
	{
		public HexCam Cam { get; private set; }
		public HexOCR OCR { get; private set; }

		private Bitmap _Screenshot = null;
		public Bitmap Screenshot
		{
			get { return _Screenshot ?? (_Screenshot = Cam.GetScreenShot()); }
			set
			{
				_Screenshot = value;
				HexProperties = null;
			}
		}

		private HexGridProperties _HexProperties = null;
		public HexGridProperties HexProperties
		{
			get { return _HexProperties ?? (_HexProperties = OCR.FindHexPattern(Screenshot)); }
			set
			{
				if (value != null && Screenshot == null)
					_Screenshot = Cam.GetScreenShot();

				_HexProperties = value;
				AllHexagons = null;
			}
		}

		private HexGrid _AllHexagons = null;
		public HexGrid AllHexagons
		{
			get { return _AllHexagons ?? (_AllHexagons = OCR.GetAllHexagons(Screenshot, HexProperties)); }
			private set
			{
				_AllHexagons = value;
				FilteredHexagons = null;
			}
		}

		private HexGrid _FilteredHexagons = null;
		public HexGrid FilteredHexagons
		{
			get { return _FilteredHexagons ?? (_FilteredHexagons = OCR.GetHexagons(AllHexagons)); }
			private set
			{
				_FilteredHexagons = value;
			}
		}

		public HexcellsSolver()
		{
			Cam = new HexCam();
			OCR = new HexOCR();
		}

		public Bitmap GrabScreenshot()
		{
			Screenshot = Cam.GetScreenShot();

			return Screenshot;
		}

		public Bitmap LoadScreenshot(Bitmap bmp)
		{
			Screenshot = bmp;

			return Screenshot;
		}

		public bool IsScreenshotLoaded()
		{
			return _Screenshot != null;
		}
	}
}
