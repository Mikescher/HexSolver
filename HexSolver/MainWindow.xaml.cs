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

			try
			{
				solver = new HexcellsSolver();
				renderer = new HexRenderer();

				int shotid = 1;
				for (; File.Exists(String.Format("./example/shot{0:000}.png", shotid)); shotid++) { }
				iudExample.Maximum = shotid - 1;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnCaptureClicked(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				solver.Cam.Reset();
				solver.Screenshot = null;

				imgDisplay.Source = LoadBitmap(solver.Screenshot);

				pnlExecute.IsEnabled = true;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnShowPlainClicked(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(solver.Screenshot);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnExampleLoadClicked(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
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
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnExampleSaveClicked(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			if (!solver.IsScreenshotLoaded())
				return;

			try
			{
				int i = 1;
				while (File.Exists(String.Format(@"..\..\example\shot{0:000}.png", i)))
					i++;

				solver.Screenshot.Save(String.Format(@"..\..\example\shot{0:000}.png", i), ImageFormat.Png);

				MessageBox.Show("Saved to " + String.Format(@"..\..\example\shot{0:000}.png", i));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnSaveOCRClicked(object sender, RoutedEventArgs eargs)
		{
			try
			{
				StaticDebugSettings.SaveOCRImages = ((CheckBox)sender).IsChecked.Value;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnCleanImageSave(object sender, RoutedEventArgs eargs)
		{
			try
			{
				int cleaned = StaticDebugSettings.CleanImageSave();

				MessageBox.Show(cleaned + " images due to redundancy removed");
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void HexOCRValueChanged(object sender, RoutedPropertyChangedEventArgs<object> eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				HexOCRValueSet(sender, null);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void cbSwap_SelectionChanged(object sender, SelectionChangedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				HexOCRValueSet(sender, null);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void HexOCRValueAuto(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				solver.HexProperties = null;

				imgDisplay.Source = LoadBitmap(renderer.DisplayCells(solver.Screenshot, solver.HexProperties, solver.AllHexagons));

				SetUIHexGridProperties(solver.HexProperties);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnShowIndizies(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayIndizies(solver.Screenshot, solver.FilteredHexagons));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void HexOCRValueSet(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			try
			{
				solver.HexProperties = GetUIHexGridProperties();
				HexGrid all = solver.AllHexagons;

				imgDisplay.Source = LoadBitmap(renderer.DisplayCells(solver.Screenshot, solver.HexProperties, all));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnBinPattern(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayBinPattern(solver.Screenshot, solver.OCR));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void HexOCRValueUpdate(object sender, RoutedEventArgs eargs)
		{
			try
			{
				SetUIHexGridProperties(solver.HexProperties);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnTypifyClicked(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayTypes(solver.Screenshot, solver.FilteredHexagons));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnProcessClicked(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayOCRProcess(solver.Screenshot, solver.FilteredHexagons));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnOCRClicked(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayOCR(solver.Screenshot, solver.FilteredHexagons));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnOCRDistanceClicked(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;
			if (skipUpdate)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayOCRDistance(solver.Screenshot, solver.FilteredHexagons));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnShowHintGroups(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayHintGroups(solver.Screenshot, solver.FilteredHexagons, null));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnShowHintGroups_1(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayHintGroups(solver.Screenshot, solver.FilteredHexagons, typeof(HexNeighborHint)));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnShowHintGroups_2(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayHintGroups(solver.Screenshot, solver.FilteredHexagons, typeof(HexRowHint)));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnShowHintGroups_3(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayHintGroups(solver.Screenshot, solver.FilteredHexagons, typeof(HexAreaHint)));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnSolveSingle(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				int time = Environment.TickCount;
				imgDisplay.Source = LoadBitmap(renderer.DisplaySolveSingle(solver.Screenshot, solver.FilteredHexagons.HintList.Solutions));
				time = Environment.TickCount - time;

				Console.Out.WriteLine("Calculated Single Step in " + time + "ms");
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnSolveTSP(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				int time = Environment.TickCount;
				imgDisplay.Source = LoadBitmap(renderer.DisplaySolveSingleOrdered(solver.Screenshot, solver.FilteredHexagons.HintList.Solutions, false));
				time = Environment.TickCount - time;

				Console.Out.WriteLine("Calculated Single Step in " + time + "ms");
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnSolveTSPBezier(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			try
			{
				int time = Environment.TickCount;
				imgDisplay.Source = LoadBitmap(renderer.DisplaySolveSingleOrdered(solver.Screenshot, solver.FilteredHexagons.HintList.Solutions, true));
				time = Environment.TickCount - time;

				Console.Out.WriteLine("Calculated Single Step in " + time + "ms");
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), "Execption while executing", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnExecuteSingle(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			HexExecutor executor = new HexExecutor(solver, this);
			executor.Start(HexEcecutionMode.Single);
		}

		private void OnExecuteMulti(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			HexExecutor executor = new HexExecutor(solver, this);
			executor.Start(HexEcecutionMode.Multi);
		}

		private void OnExecuteAll(object sender, RoutedEventArgs eargs)
		{
			if (solver == null || renderer == null)
				return;

			HexExecutor executor = new HexExecutor(solver, this);
			executor.Start(HexEcecutionMode.All);
		}

		void IHexExecutorFeedback.OnExecutorStart()
		{
			Dispatcher.Invoke(new Action(delegate()
			{
				leftPanel.IsEnabled = false;
			}));
		}

		void IHexExecutorFeedback.OnExecutorEnd()
		{
			Dispatcher.Invoke(new Action(delegate()
			{
				leftPanel.IsEnabled = true;
				pbarExecute.Maximum = 1;
				pbarExecute.Value = 0;
			}));
		}

		void IHexExecutorFeedback.OnExecutorProgress(int current, int max)
		{
			Dispatcher.Invoke(new Action(delegate()
			{
				pbarExecute.Maximum = max;
				pbarExecute.Value = current;
			}));
		}

		void IHexExecutorFeedback.OnExecutorScreenshotGrabbed()
		{
			Dispatcher.Invoke(new Action(delegate()
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplayOCR(solver.Screenshot, solver.FilteredHexagons));
			}));
		}

		void IHexExecutorFeedback.OnExecutorSolutionExecuted(HexStep ocr)
		{
			Dispatcher.Invoke(new Action(delegate()
			{
				imgDisplay.Source = LoadBitmap(renderer.DisplaySolveSingle(solver.Screenshot, solver.FilteredHexagons.HintList.Solutions));
			}));
		}

		void IHexExecutorFeedback.OnExecutorError(Exception e)
		{
			Dispatcher.Invoke(new Action(delegate()
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
