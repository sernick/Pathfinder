namespace Pathfinder.Reading
{
    internal struct Segment
    {
        #region Constructors

        public Segment(int level, int min, int max)
        {
            Level = level;
            Min = min;
            Max = max;
        }

        #endregion

        #region Properties

        public int Min
        {
            get;
            set;
        }

        public int Max
        {
            get;
            set;
        }

        public int Level
        {
            get;
        }

        #endregion
    }
}