namespace Pathfinder.Reading
{
    public struct Area
    {
        #region Constructors

        public Area(double x1, double y1, double x2, double y2)
        {
            if (x1 < x2)
            {
                Xmin = x1;
                Xmax = x2;
            }
            else
            {
                Xmin = x2;
                Xmax = x1;
            }

            if (y1 < y2)
            {
                Ymin = y1;
                Ymax = y2;
            }
            else
            {
                Ymin = y2;
                Ymax = y1;
            }
        }

        #endregion

        #region Properties

        public double Xmax
        {
            get;
        }

        public double Xmin
        {
            get;
        }

        public double Ymax
        {
            get;
        }

        public double Ymin
        {
            get;
        }

        #endregion
    }
}