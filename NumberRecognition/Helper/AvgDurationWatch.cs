using System;

namespace MSHC.Helper
{
	public class AvgDurationWatch
	{
		private const int STANDARD_UPDATE_TIME = 500;
		private readonly int UPDATE_TIME;

		public double Duration { get; private set; }

		private long lastUpdate;

		private long lastTick;
		private long timeSum;
		private int count;

		public AvgDurationWatch()
			: this(STANDARD_UPDATE_TIME)
		{
		}

		public AvgDurationWatch(int updtime)
		{
			UPDATE_TIME = updtime;

			lastUpdate = 0;
			Duration = 1;
			timeSum = 0;
			count = 0;
			lastTick = 0;
		}

		public void Start()
		{
			lastTick = Environment.TickCount;
		}

		public void Stop()
		{
			timeSum += Environment.TickCount - lastTick;
			count++;

			if ((Environment.TickCount - lastUpdate) > UPDATE_TIME)
			{
				Duration = timeSum / (count * 1d);

				timeSum = 0;
				count = 0;
				lastTick = 0;

				lastUpdate = Environment.TickCount;
			}
		}
	}
}
