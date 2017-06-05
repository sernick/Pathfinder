namespace Pathfinder.Reading
{
    internal class Segment
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

        public int Level
        {
            get;
        }

        public int Max
        {
            get;
            set;
        }

        public int Min
        {
            get;
            set;
        }

        #endregion
    }
}