namespace Pathfinder.GraphTheory
{
	internal class DijkstraData
	{
		#region Properties

		public Edge PreviousEdge
		{
			get;
			set;
		}

		public Node PreviousNode
		{
			get;
			set;
		}

		public int Price
		{
			get;
			set;
		}

		#endregion
	}
}