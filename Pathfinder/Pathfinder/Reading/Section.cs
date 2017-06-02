namespace Pathfinder.Reading
{
    public struct Section
    {
        #region Constructors

        public Section(double level, double c1, double c2)
        {
            Level = level;

            if (c1 < c2)
            {
                Cmin = c1;
                Cmax = c2;
            }
            else
            {
                Cmin = c2;
                Cmax = c1;
            }
        }

        #endregion

        #region Properties

        public double Cmax
        {
            get;
        }

        public double Cmin
        {
            get;
        }

        public double Level
        {
            get;
        }

        #endregion
    }
}