using System.Collections.Generic;


namespace Pathfinder.GraphTheory
{
	internal class Node
	{
		#region Constructors

		public Node(int x, int y)
		{
			X = x;
			Y = y;
		}

		#endregion

		#region Fields

		private readonly List<Edge> _incidentEdges = new List<Edge>();

		#endregion

		#region Properties

		public IEnumerable<Edge> IncidentEdges
		{
			get
			{
				foreach (Edge edge in _incidentEdges)
				{
					yield return edge;
				}
			}
		}


		public IEnumerable<Node> IncidentNodes
		{
			get
			{
				foreach (Edge edge in IncidentEdges)
				{
					yield return edge.OtherNode(this);
				}
			}
		}

		public int X
		{
			get;
		}

		public int Y
		{
			get;
		}

		#endregion

		#region Methods

		public static Edge Connect(Node first, Node second, int weight, Orientation orientation, Graph graph)
		{
			var edge = new Edge(first, second, weight, orientation);
			first._incidentEdges.Add(edge);
			second._incidentEdges.Add(edge);
			return edge;
		}

		public static void Disconnect(Edge edge)
		{
			edge.First._incidentEdges.Remove(edge);
			edge.Second._incidentEdges.Remove(edge);
		}

		public override string ToString()
		{
			return X + "," + Y;
		}

		#endregion
	}
}