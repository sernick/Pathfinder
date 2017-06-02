using System;

using Pathfinder.Reading;


namespace Pathfinder.Analysis
{
    internal struct Rectangle
    {
        #region Constructors

        public Rectangle(Area area, int offset)
        {
            Xmin = (int) Math.Floor(area.Xmin) - offset;
            Ymin = (int) Math.Floor(area.Ymin) - offset;
            Xmax = (int) Math.Ceiling(area.Xmax) + offset;
            Ymax = (int) Math.Ceiling(area.Ymax) + offset;
        }

        #endregion

        #region Properties

        public int Xmax
        {
            get;
        }

        public int Xmin
        {
            get;
        }

        public int Ymax
        {
            get;
        }

        public int Ymin
        {
            get;
        }

        #endregion
    }
}