using System.Collections.Generic;


namespace Pathfinder.GraphTheory
{
	internal class Graph
	{
		#region Constructors

		public Graph(int x, int y)
		{
			_grid = new Node[x, y];
		}

		#endregion

		#region Fields

		private readonly Node[,] _grid;
		private readonly List<Node> _nodes = new List<Node>();

		#endregion

		#region Properties

		public IEnumerable<Edge> Edges
		{
			get
			{
				var edges = new HashSet<Edge>();
				foreach (Node node in _nodes)
				{
					foreach (Edge edge in node.IncidentEdges)
					{
						edges.Add(edge);
					}
				}
				return edges;
			}
		}

		public IEnumerable<Node> Nodes
		{
			get
			{
				foreach (Node node in _nodes)
				{
					yield return node;
				}
			}
		}

		#endregion

		#region Methods

		public Node AddNode(int i, int j)
		{
			var node = new Node(i, j);

			_nodes.Add(node);
			_grid[i, j] = node;

			return node;
		}

		public void Delete(Edge edge)
		{
			Node.Disconnect(edge);
		}

		#endregion

		public Node this[int i, int j] => _grid[i, j];
	}
}