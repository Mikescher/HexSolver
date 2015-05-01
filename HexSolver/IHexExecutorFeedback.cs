
using HexSolver.Solver;
using System;
namespace HexSolver
{
	interface IHexExecutorFeedback
	{
		void OnExecutorStart();
		void OnExecutorEnd();
		void OnExecutorProgress(int current, int max);
		void OnExecutorScreenshotGrabbed();
		void OnExecutorSolutionExecuted(HexStep solution);
		void OnExecutorError(Exception e);
	}

	class DummyHexExecutorFeedback : IHexExecutorFeedback
	{
		public void OnExecutorStart() { }
		public void OnExecutorEnd() { }
		public void OnExecutorProgress(int current, int max) { }
		public void OnExecutorScreenshotGrabbed() { }
		public void OnExecutorSolutionExecuted(HexStep solution) { }
		public void OnExecutorError(Exception e) { }
	}
}
