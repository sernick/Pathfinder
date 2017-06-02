using System;


namespace Pathfinder.Reading
{
    internal class Corridor
    {
        #region Constructors

        private Corridor(int xmin, int xmax, int ymin, int ymax)
        {
            Xmin = xmin;
            Ymin = ymin;
            Xmax = xmax;
            Ymax = ymax;
        }

        #endregion

        #region Properties

        public int Xmax
        {
            get;
            set;
        }

        public int Xmin
        {
            get;
            set;
        }

        public int Ymax
        {
            get;
            set;
        }

        public int Ymin
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public static Corridor GetHorizontalCorridor(Section section, int offset)
        {
            int xmin = (int) Math.Floor(section.Cmin) - offset;
            int xmax = (int) Math.Ceiling(section.Cmax) + offset;
            int ymin = (int) Math.Floor(section.Level) - offset;
            int ymax = (int) Math.Ceiling(section.Level) + offset;

            return new Corridor(xmin, xmax, ymin, ymax);
        }

        public static Corridor GetVerticalCorridor(Section section, int offset)
        {
            int xmin = (int) Math.Floor(section.Level) - offset;
            int xmax = (int) Math.Ceiling(section.Level) + offset;
            int ymin = (int) Math.Floor(section.Cmin) - offset;
            int ymax = (int) Math.Ceiling(section.Cmax) + offset;

            return new Corridor(xmin, xmax, ymin, ymax);
        }

        #endregion
    }
}