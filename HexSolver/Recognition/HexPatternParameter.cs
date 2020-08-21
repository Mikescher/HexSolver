namespace HexSolver
{
    class HexPatternParameter
    {
        public bool UseActiveCellsInBinaryGridRecognition;
        public double  ActiveCellHueThreshold;
        public double  ActiveCellSatThreshold;
        public double  ActiveCellValThreshold;

        public bool UseInactiveCellsInBinaryGridRecognition;
        public double  InactiveCellHueThreshold;
        public double  InactiveCellSatThreshold;
        public double  InactiveCellValThreshold;

        public bool UseHiddenCellsInBinaryGridRecognition;
        public double  HiddenCellHueThreshold;
        public double  HiddenCellSatThreshold;
        public double  HiddenCellValThreshold;

        public HexPatternParameter(bool useActive,   double thldActiveH,   double thldActiveS,   double thldActiveV, 
                                   bool useInactive, double thldInactiveH, double thldInactiveS, double thldInactiveV, 
                                   bool useHidden,   double thldHiddenH,   double thldHiddenS,   double thldHiddenV)
        {
            UseActiveCellsInBinaryGridRecognition   = useActive;
            ActiveCellHueThreshold                  = thldActiveH;
            ActiveCellSatThreshold                  = thldActiveS;
            ActiveCellValThreshold                  = thldActiveV;

            UseInactiveCellsInBinaryGridRecognition = useInactive;
            InactiveCellHueThreshold                = thldInactiveH;
            InactiveCellSatThreshold                = thldInactiveS;
            InactiveCellValThreshold                = thldInactiveV;

            UseHiddenCellsInBinaryGridRecognition   = useHidden;
            HiddenCellHueThreshold                  = thldHiddenH;
            HiddenCellSatThreshold                  = thldHiddenS;
            HiddenCellValThreshold                  = thldHiddenV;
        }
    }
}
