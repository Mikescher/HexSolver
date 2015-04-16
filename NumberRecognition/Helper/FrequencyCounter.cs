using System;

namespace MSHC.Helper
{
	public class FrequencyCounter
	{
		private const int UPDATE_TIME = 500;

		public double Frequency { get; private set; }

		private long lastUpdate;
		private int count;

		public FrequencyCounter()
		{
			Frequency = 1;
			lastUpdate = 0;
			count = 0;
		}

		public void Inc()
		{
			count++;

			calc();
		}

		private void calc()
		{
			long delta = Environment.TickCount - lastUpdate;
			if (delta > UPDATE_TIME)
			{
				Frequency = Math.Max(1, (count * 1d) / (delta / 1000.0));

				count = 0;
				lastUpdate = Environment.TickCount;
			}
		}
	}
}