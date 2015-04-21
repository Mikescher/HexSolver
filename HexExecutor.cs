using System;
using System.Linq;
using System.Threading;

namespace HexSolver
{
	enum HexEcecutionMode
	{
		Single,
		Multi,
		All
	}

	class HexExecutor
	{
		private const int MOUSE_MOVEMENT_TIME = 200;
		private const int CLICK_SLEEP_TIME = 250;
		private const int INTER_SOLUTION_SLEEP_TIME = 500;
		private const int RECAPTURE_SLEEP = 2000;

		private readonly IHexExecutorFeedback feedback;
		private readonly HexcellsSolver Solver;
		private Thread runThread;

		public HexExecutor(HexcellsSolver svr, IHexExecutorFeedback fb)
		{
			Solver = svr;
			feedback = fb ?? new DummyHexExecutorFeedback();
		}

		public void Start(HexEcecutionMode mode)
		{
			switch (mode)
			{
				case HexEcecutionMode.Single:
					runThread = new Thread(new ThreadStart(Run_Single));
					break;
				case HexEcecutionMode.Multi:
					runThread = new Thread(new ThreadStart(Run_Multi));
					break;
				case HexEcecutionMode.All:
					runThread = new Thread(new ThreadStart(Run_All));
					break;
			}

			runThread.Start();
		}

		public void Run_Single()
		{
			feedback.OnExecutorStart();
			try
			{
				feedback.OnExecutorProgress(0, 1);

				Solver.Cam.ForceWindowToForeground();

				var solution = Solver.FilteredHexagons.HintList.Solutions.First();
				var solution_x = (int)solution.Cell.Image.OCRCenter.X;
				var solution_y = (int)solution.Cell.Image.OCRCenter.Y;

				Solver.Cam.MoveMouseContinoous(solution_x, solution_y, MOUSE_MOVEMENT_TIME);
				Solver.Cam.ClickMouseSimple(solution_x, solution_y, solution.Action == HexSolver.Solver.CellAction.ACTIVATE);
				Thread.Sleep(CLICK_SLEEP_TIME);

				feedback.OnExecutorSolutionExecuted(solution);
				feedback.OnExecutorProgress(1, 1);
			}
			catch (Exception e)
			{
				feedback.OnExecutorError(e);
			}
			feedback.OnExecutorEnd();
		}

		public void Run_Multi()
		{
			feedback.OnExecutorStart();
			try
			{
				ExecuteSingle(0, Solver.FilteredHexagons.HintList.Solutions.Count);
			}
			catch (Exception e)
			{
				feedback.OnExecutorError(e);
			}
			feedback.OnExecutorEnd();
		}

		public void Run_All()
		{
			feedback.OnExecutorStart();
			try
			{
				int progress = 0;
				int progressmax = Solver.FilteredHexagons.Count(p => p.Value.Type == HexagonType.HIDDEN);

				for (; ; )
				{
					var currentProgress = Solver.FilteredHexagons.HintList.Solutions.ToList();
					if (currentProgress.Count == 0)
						break;

					ExecuteSingle(progress, progressmax);
					progress += currentProgress.Count;
				}
			}
			catch (Exception e)
			{
				feedback.OnExecutorError(e);
			}
			feedback.OnExecutorEnd();
		}

		private void ExecuteSingle(int progress, int progmax)
		{
			var solutions = Solver.FilteredHexagons.HintList.Solutions.ToList();
			feedback.OnExecutorProgress(progress, progmax);

			Solver.Cam.ForceWindowToForeground();

			for (int i = 0; i < solutions.Count; i++)
			{
				var solution = solutions[i];
				var solution_x = (int)solution.Cell.Image.OCRCenter.X;
				var solution_y = (int)solution.Cell.Image.OCRCenter.Y;
				Solver.FilteredHexagons.HintList.RemoveSolution(solution);

				Solver.Cam.MoveMouseContinoous(solution_x, solution_y, MOUSE_MOVEMENT_TIME);
				Solver.Cam.ClickMouseSimple(solution_x, solution_y, solution.Action == HexSolver.Solver.CellAction.ACTIVATE);
				Thread.Sleep(CLICK_SLEEP_TIME);

				feedback.OnExecutorSolutionExecuted(solution);
				feedback.OnExecutorProgress(progress + i + 1, progmax);

				Thread.Sleep(INTER_SOLUTION_SLEEP_TIME);
			}

			Thread.Sleep(RECAPTURE_SLEEP);

			Solver.Update(solutions.Select(p => p.Cell).ToList());
			feedback.OnExecutorScreenshotGrabbed();
		}
	}
}
