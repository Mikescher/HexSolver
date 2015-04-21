using System.Collections.Generic;

namespace SimplePatternOCR
{
	public struct OCRResult
	{
		public string Value;

		public double Distance;

		public List<OCRCharacterResult> Characters;
	}

	public struct OCRCharacterResult
	{
		public string Character;

		public int OffsetX;
		public int OffsetY;
		public double Distance;

		public int EulerNumber;

		public Dictionary<string, double> AllDistances;
	}
}
