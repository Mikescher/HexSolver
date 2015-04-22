using SimplePatternOCR;
using System.Collections.Generic;
using System.Drawing;

namespace HexSolver
{
	//TODO: Can't solve 3-1

	class HexcellsSolver
	{
		public HexCam Cam { get; private set; }
		public HexOCR OCR { get; private set; }
		public PatternOCR POCR { get; private set; }

		private Bitmap _Screenshot = null;
		public Bitmap Screenshot
		{
			get { return _Screenshot ?? (_Screenshot = Cam.GetScreenShot(true)); }
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
					_Screenshot = Cam.GetScreenShot(true);

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
			Dictionary<string, Bitmap> refdic = new Dictionary<string, Bitmap>();
			refdic.Add("-", Properties.Resources.pattern_dash);
			refdic.Add("{", Properties.Resources.pattern_Open);
			refdic.Add("}", Properties.Resources.pattern_Close);
			refdic.Add("?", Properties.Resources.pattern_QMark);
			refdic.Add("0", Properties.Resources.pattern_0);
			refdic.Add("1", Properties.Resources.pattern_1);
			refdic.Add("2", Properties.Resources.pattern_2);
			refdic.Add("3", Properties.Resources.pattern_3);
			refdic.Add("4", Properties.Resources.pattern_4);
			refdic.Add("5", Properties.Resources.pattern_5);
			refdic.Add("6", Properties.Resources.pattern_6);
			refdic.Add("7", Properties.Resources.pattern_7);
			refdic.Add("8", Properties.Resources.pattern_8);
			refdic.Add("9", Properties.Resources.pattern_9);
			refdic.Add("10", Properties.Resources.pattern_10);
			refdic.Add("12", Properties.Resources.pattern_12);
			refdic.Add("13", Properties.Resources.pattern_13);
			refdic.Add("14", Properties.Resources.pattern_14);
			refdic.Add("15", Properties.Resources.pattern_15);
			refdic.Add("16", Properties.Resources.pattern_16);
			refdic.Add("17", Properties.Resources.pattern_17);
			refdic.Add("18", Properties.Resources.pattern_18);
			refdic.Add("19", Properties.Resources.pattern_19);


			POCR = new PatternOCR(refdic, OCRCoupling.NORMAL_COUPLED_SEGMENTS);
			Cam = new HexCam();
			OCR = new HexOCR(POCR);
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

		public void Update(List<HexagonCell> updates)
		{
			Bitmap shot = Cam.GetScreenShot(false);

			_Screenshot = shot;

			foreach (var cell in updates)
			{
				cell.Update(shot);
			}

			AllHexagons.HintList = null;
			AllHexagons.CounterArea.Update(shot);

			FilteredHexagons.HintList = null;
			FilteredHexagons.CounterArea.Update(shot);
		}
	}
}
