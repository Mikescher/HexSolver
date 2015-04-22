using HexSolver.Solver;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
	partial class MainWindow : Window, IHexExecutorFeedback
	{
		[DllImport("gdi32")]
		static extern int DeleteObject(IntPtr o);

		private readonly HexcellsSolver solver = null;
		private readonly HexRenderer renderer = null;

		private bool skipUpdate = false;

		public MainWindow()
		{
			InitializeComponent();

			solver = new HexcellsSolver();
			renderer = new HexRenderer();


			int shotid = 1;
			for (; File.Exists(String.Format("./example/shot{0:000}.png", shotid)); shotid++) { }
			iudExample.Maximum = shotid - 1;
		}

		private void OnCaptureClicked(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			solver.Cam.Reset();
			solver.Screenshot = null;

			imgDisplay.Source = LoadBitmap(solver.Screenshot);

			pnlExecute.IsEnabled = true;
		}

		private void OnShowPlainClicked(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			imgDisplay.Source = LoadBitmap(solver.Screenshot);
		}

		private void OnExampleLoadClicked(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			int shotid = iudExample.Value.Value;

			if (!File.Exists(String.Format("./example/shot{0:000}.png", shotid)))
				return;

			Image file = Image.FromFile(String.Format("./example/shot{0:000}.png", shotid));
			Bitmap bmp = new Bitmap(file.Width, file.Height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.DrawImageUnscaled(file, 0, 0);
			}

			imgDisplay.Source = LoadBitmap(solver.LoadScreenshot(bmp));

			pnlExecute.IsEnabled = false;
		}

		private void OnExampleSaveClicked(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			if (!solver.IsScreenshotLoaded())
				return;

			int i = 1;
			while (File.Exists(String.Format(@"..\..\example\shot{0:000}.png", i)))
				i++;

			solver.Screenshot.Save(String.Format(@"..\..\example\shot{0:000}.png", i), ImageFormat.Png);

			MessageBox.Show("Saved to " + String.Format(@"..\..\example\shot{0:000}.png", i));
		}

		private void OnSaveOCRClicked(object sender, RoutedEventArgs e)
		{
			StaticDebugSettings.SaveOCRImages = ((CheckBox)sender).IsChecked.Value;
		}

		private void OnCleanImageSave(object sender, RoutedEventArgs e)
		{
			int cleaned = StaticDebugSettings.CleanImageSave();

			MessageBox.Show(cleaned + " images due to redundancy removed");
		}

		private void HexOCRValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (solver == null || renderer == null)
				return;

			HexOCRValueSet(sender, null);
		}

		private void cbSwap_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			HexOCRValueSet(sender, null);
		}

		private void HexOCRValueAuto(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			solver.HexProperties = null;

			imgDisplay.Source = LoadBitmap(renderer.DisplayCells(solver.Screenshot, solver.HexProperties, solver.AllHexagons));

			SetUIHexGridProperties(solver.HexProperties);
		}

		private void OnShowIndizies(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayIndizies(solver.Screenshot, solver.FilteredHexagons));
		}

		private void HexOCRValueSet(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			solver.HexProperties = GetUIHexGridProperties();
			HexGrid all = solver.AllHexagons;

			imgDisplay.Source = LoadBitmap(renderer.DisplayCells(solver.Screenshot, solver.HexProperties, all));
		}

		private void OnBinPattern(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayBinPattern(solver.Screenshot, solver.OCR));
		}

		private void HexOCRValueUpdate(object sender, RoutedEventArgs e)
		{
			SetUIHexGridProperties(solver.HexProperties);
		}

		private void OnTypifyClicked(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayTypes(solver.Screenshot, solver.FilteredHexagons));
		}

		private void OnProcessClicked(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayOCRProcess(solver.Screenshot, solver.FilteredHexagons));
		}

		private void OnOCRClicked(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayOCR(solver.Screenshot, solver.FilteredHexagons));
		}

		private void OnOCRDistanceClicked(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayOCRDistance(solver.Screenshot, solver.FilteredHexagons));
		}

		private void OnShowHintGroups(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayHintGroups(solver.Screenshot, solver.FilteredHexagons, null));
		}

		private void OnShowHintGroups_1(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayHintGroups(solver.Screenshot, solver.FilteredHexagons, typeof(HexNeighborHint)));
		}

		private void OnShowHintGroups_2(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayHintGroups(solver.Screenshot, solver.FilteredHexagons, typeof(HexRowHint)));
		}

		private void OnShowHintGroups_3(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			imgDisplay.Source = LoadBitmap(renderer.DisplayHintGroups(solver.Screenshot, solver.FilteredHexagons, typeof(HexAreaHint)));
		}

		private void OnSolveSingle(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			int time = Environment.TickCount;
			imgDisplay.Source = LoadBitmap(renderer.DisplaySolveSingle(solver.Screenshot, solver.FilteredHexagons.HintList.Solutions));
			time = Environment.TickCount - time;

			Console.Out.WriteLine("Calculated Single Step in " + time + "ms");
		}

		private void OnExecuteSingle(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			HexExecutor executor = new HexExecutor(solver, this);
			executor.Start(HexEcecutionMode.Single);
		}

		private void OnExecuteMulti(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			HexExecutor executor = new HexExecutor(solver, this);
			executor.Start(HexEcecutionMode.Multi);
		}

		private void OnExecuteAll(object sender, RoutedEventArgs e)
		{
			if (solver == null || renderer == null)
				return;

			HexExecutor executor = new HexExecutor(solver, this);
			executor.Start(HexEcecutionMode.All);
		}

		void IHexExecutorFeedback.OnExecutorStart()
		{
			Dispatcher.BeginInvoke(new Action(delegate()
			{
				leftPanel.IsEnabled = false;
			}));
		}

		void IHexExecutorFeedback.OnExecutorEnd()
		{
			Dispatcher.BeginInvoke(new Action(delegate()
			{
				leftPanel.IsEnabled = true;
				pbarExecute.Maximum = 1;
				pbarExecute.Value = 0;
			}));
		}

		void IHexExecutorFeedback.OnExecutorProgress(int current, int max)
		{
			Dispatcher.BeginInvoke(new Action(delegate()
			{
				pbarExecute.Maximum = max;
				pbarExecute.Value = current;
			}));
		}

		void IHexExecutorFeedback.OnExecutorScreenshotGrabbed()
		{
			Dispatcher.BeginInvoke(new Action(delegate()
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayOCR(solver.Screenshot, solver.FilteredHexagons));
			}));
		}

		void IHexExecutorFeedback.OnExecutorSolutionExecuted(HexStep ocr)
		{
			Dispatcher.BeginInvoke(new Action(delegate()
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplaySolveSingle(solver.Screenshot, solver.FilteredHexagons.HintList.Solutions));
			}));
		}

		void IHexExecutorFeedback.OnExecutorError(Exception e)
		{
			Dispatcher.BeginInvoke(new Action(delegate()
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}));
		}

		private HexGridProperties GetUIHexGridProperties()
		{
			return new HexGridPropertiesBuilder()
				.SetCellRadius(dudRadius.Value.Value)
				.SetCellGap(dudGap.Value.Value)
				.SetCorrectionHorizontal(dudHCorr.Value.Value)
				.SetCorrectionVertical(dudVCorr.Value.Value)
				.SetPaddingX(dudPadX.Value.Value)
				.SetPaddingY(dudPadY.Value.Value)
				.SetNoCellBar_TR_X(dudBarTopRightX.Value.Value)
				.SetNoCellBar_TR_Y(dudBarTopRightY.Value.Value)
				.SetInitialSwap(cbSwap.SelectedIndex == 0)
				.SetCounter_X(dudCounterX.Value.Value)
				.SetCounter_Y(dudCounterY.Value.Value)
				.SetCounter_Width(dudCounterW.Value.Value)
				.SetCounter_Height(dudCounterH.Value.Value)
				.SetCounterInner_X(dudCounterX.Value.Value)
				.SetCounterInner_Y(dudCounterY.Value.Value)
				.SetCounterInner_Width(dudCounterW.Value.Value)
				.SetCounterInner_Height(dudCounterH.Value.Value)
				.build();
		}

		private void SetUIHexGridProperties(HexGridProperties p)
		{
			skipUpdate = true;

			dudRadius.Value = p.CellRadius;
			dudGap.Value = p.CellGap;
			dudHCorr.Value = p.CorrectionHorizontal;
			dudVCorr.Value = p.CorrectionVertical;
			dudPadX.Value = p.PaddingX;
			dudPadY.Value = p.PaddingY;
			cbSwap.SelectedIndex = p.InitialSwap ? 0 : 1;
			dudBarTopRightX.Value = p.NoCellBar_TR_X;
			dudBarTopRightY.Value = p.NoCellBar_TR_Y;
			dudCounterX.Value = p.Counter_X;
			dudCounterY.Value = p.Counter_Y;
			dudCounterW.Value = p.Counter_W;
			dudCounterH.Value = p.Counter_H;

			skipUpdate = false;
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
	}
}
