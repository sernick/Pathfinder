namespace Pathfinder.Reading
{
    internal class Side
    {
        #region Constructors

        public Side(int cmin, int cmax, sbyte direction)
        {
            Cmin = cmin;
            Cmax = cmax;
            Direction = direction;
        }

        #endregion

        #region Properties

        public int Cmax
        {
            get;
        }

        public int Cmin
        {
            get;
        }

        public sbyte Direction
        {
            get;
        }

        #endregion
    }
}