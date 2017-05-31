using System;


namespace Pathfinder.GraphTheory
{
	internal class Edge
	{
		#region Constructors

		public Edge(Node first, Node second, int weight, Orientation orientation)
		{
			First = first;
			Second = second;
			Weight = weight;
			Orientation = orientation;
		}

		#endregion

		#region Properties

		public Node First
		{
			get;
		}

		public Orientation Orientation
		{
			get;
		}

		public Node Second
		{
			get;
		}

		public int Weight
		{
			get;
		}

		#endregion

		#region Methods

		public bool IsIncident(Node node)
		{
			return node == First || node == Second;
		}

		public Node OtherNode(Node node)
		{
			if (First == node)
			{
				return Second;
			}
			if (Second == node)
			{
				return First;
			}
			throw new ArgumentException();
		}

		#endregion
	}
}