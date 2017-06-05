using System.Collections.Generic;

using Pathfinder.Reading;


namespace Pathfinder.GraphTheory
{
    internal class Graph
    {
        #region Constructors

        public Graph(Vertex start,
                     Vertex end,
                     Dictionary<int, List<Side>> horizontalSides,
                     Dictionary<int, List<Side>> verticalSides,
                     List<Corridor> horizontalCorridors,
                     List<Corridor> verticalCorridors,
                     List<Segment> horizontalGaps,
                     List<Segment> verticalGaps,
                     List<int> xs,
                     List<int> ys,
                     int intersectionWeight)
        {
            int abscissaCount = xs.Count;
            int abscissaUpper = abscissaCount - 1;

            int ordinateCount = ys.Count;
            int ordinateUpper = ordinateCount - 1;

            Grid = new Node[abscissaCount, ordinateCount];
            HorizontalEdges = new Edge[ordinateCount, abscissaUpper];
            VerticalEdges = new Edge[abscissaCount, ordinateUpper];

            {
                var weights = new int[abscissaUpper];
                for (int i = 0; i < abscissaUpper; i++)
                {
                    weights[i] = xs[i + 1] - xs[i];
                }
                HorizontalWeights = weights;
            }

            {
                var weights = new int[ordinateUpper];
                for (int i = 0; i < ordinateUpper; i++)
                {
                    weights[i] = ys[i + 1] - ys[i];
                }
                VerticalWeights = weights;
            }

            var abscissaRedirects = new Dictionary<int, int>();
            for (int i = 0; i < abscissaCount; i++)
            {
                abscissaRedirects.Add(xs[i], i);
            }

            var ordinateRedirects = new Dictionary<int, int>();
            for (int i = 0; i < ordinateCount; i++)
            {
                ordinateRedirects.Add(ys[i], i);
            }

            for (int o = 0; o < ys.Count; o++)
            {
                int y = ys[o];

                sbyte crossing = 0;
                sbyte previousDirection = 0;

                for (int a = 0; a < abscissaUpper; a++)
                {
                    int x = xs[a];

                    if (verticalSides.ContainsKey(x))
                    {
                        foreach (Side side in verticalSides[x])
                        {
                            if (y >= side.Cmin && y < side.Cmax || y > side.Cmin && y <= side.Cmax)
                            {
                                if (side.Direction != previousDirection)
                                {
                                    previousDirection = side.Direction;
                                    crossing += side.Direction;
                                }
                            }
                        }
                    }

                    if (crossing == 0)
                    {
                        AddHorizontalEdge(o, a);
                    }
                }

                if (horizontalSides.ContainsKey(y))
                {
                    foreach (Side side in horizontalSides[y])
                    {
                        int amin = abscissaRedirects[side.Cmin];
                        int amax = abscissaRedirects[side.Cmax];

                        for (int a = amin; a < amax; a++)
                        {
                            AddHorizontalEdge(o, a);
                        }
                    }
                }
            }

            for (int a = 0; a < xs.Count; a++)
            {
                int x = xs[a];

                sbyte crossing = 0;
                sbyte previousDirection = 0;

                for (int o = 0; o < ordinateUpper; o++)
                {
                    int y = ys[o];

                    if (horizontalSides.ContainsKey(y))
                    {
                        foreach (Side side in horizontalSides[y])
                        {
                            if (x >= side.Cmin && x <= side.Cmax || x > side.Cmin && x <= side.Cmax)
                            {
                                if (side.Direction != previousDirection)
                                {
                                    previousDirection = side.Direction;
                                    crossing += side.Direction;
                                }
                            }
                        }
                    }

                    if (crossing == 0)
                    {
                        AddVerticalEdge(a, o);
                    }
                }

                if (verticalSides.ContainsKey(x))
                {
                    foreach (Side side in verticalSides[x])
                    {
                        int omin = ordinateRedirects[side.Cmin];
                        int omax = ordinateRedirects[side.Cmax];

                        for (int o = omin; o < omax; o++)
                        {
                            AddVerticalEdge(a, o);
                        }
                    }
                }
            }

            foreach (Corridor corridor in horizontalCorridors)
            {
                int amin = abscissaRedirects[corridor.Xmin];
                int amax = abscissaRedirects[corridor.Xmax];

                int omin = ordinateRedirects[corridor.Ymin];
                int omax = ordinateRedirects[corridor.Ymax];

                for (int o = omin + 1; o < omax; o++)
                {
                    for (int a = amin; a < amax; a++)
                    {
                        RemoveHorizontalEdge(o, a);
                    }
                }
            }

            foreach (Corridor corridor in verticalCorridors)
            {
                int amin = abscissaRedirects[corridor.Xmin];
                int amax = abscissaRedirects[corridor.Xmax];

                int omin = ordinateRedirects[corridor.Ymin];
                int omax = ordinateRedirects[corridor.Ymax];

                for (int a = amin + 1; a < amax; a++)
                {
                    for (int o = omin; o < omax; o++)
                    {
                        RemoveVerticalEdge(a, o);
                    }
                }
            }

            //foreach (Segment gap in horizontalGaps)
            //{
            //    int o = ordinateRedirects[gap.Level];

            //    int amin = abscissaRedirects[gap.Min];
            //    int amax = abscissaRedirects[gap.Max];

            //    for (int a = amin; a < amax; a++)
            //    {
            //        RemoveHorizontalEdge(o, a);
            //    }
            //}

            //foreach (Segment gap in verticalGaps)
            //{
            //    int a = abscissaRedirects[gap.Level];

            //    int omin = ordinateRedirects[gap.Min];
            //    int omax = ordinateRedirects[gap.Max];

            //    for (int o = omin; o < omax; o++)
            //    {
            //        RemoveVerticalEdge(a, o);
            //    }
            //}

            for (int a = 0; a < abscissaCount; a++)
            {
                for (int o = 0; o < ordinateCount; o++)
                {
                    Node node = Grid[a, o];
                    if (node != null && node.IncidentEdges.Count <= 0)
                    {
                        Grid[a, o] = null;
                    }
                }
            }

            foreach (Vertex vertex in new[] {start, end})
            {
                int a = abscissaRedirects[vertex.X];
                int o = ordinateRedirects[vertex.Y];

                Node currentNode = this[a, o];
                if (currentNode.IncidentEdges.Count <= 0)
                {
                    {
                        int i = a - 1;
                        while (i >= 0)
                        {
                            Node previousNode = Grid[i, o];
                            if (previousNode != null)
                            {
                                var edge = new Edge(currentNode, previousNode, 0, Orientation.Horizontal);

                                currentNode.IncidentEdges.Add(edge);
                                previousNode.IncidentEdges.Add(edge);

                                break;
                            }
                            i--;
                        }
                    }

                    {
                        int i = a + 1;
                        while (i < abscissaCount)
                        {
                            Node nextNode = Grid[i, o];
                            if (nextNode != null)
                            {
                                var edge = new Edge(currentNode, nextNode, 0, Orientation.Horizontal);

                                currentNode.IncidentEdges.Add(edge);
                                nextNode.IncidentEdges.Add(edge);

                                break;
                            }
                            i++;
                        }
                    }

                    {
                        int j = o - 1;
                        while (j >= 0)
                        {
                            Node previousNode = Grid[a, j];
                            if (previousNode != null)
                            {
                                var edge = new Edge(currentNode, previousNode, 0, Orientation.Horizontal);

                                currentNode.IncidentEdges.Add(edge);
                                previousNode.IncidentEdges.Add(edge);

                                break;
                            }
                            j--;
                        }
                    }

                    {
                        int j = o + 1;
                        while (j < ordinateCount)
                        {
                            Node nextNode = Grid[a, j];
                            if (nextNode != null)
                            {
                                var edge = new Edge(currentNode, nextNode, 0, Orientation.Horizontal);

                                currentNode.IncidentEdges.Add(edge);
                                nextNode.IncidentEdges.Add(edge);

                                break;
                            }
                            j++;
                        }
                    }
                }
            }

            //foreach (Corridor corridor in horizontalCorridors)
            //{
            //    int amin = abscissaRedirects[corridor.Xmin];
            //    int amax = abscissaRedirects[corridor.Xmax];

            //    int omin = ordinateRedirects[corridor.Ymin];
            //    int omax = ordinateRedirects[corridor.Ymax];

            //    if (omax - omin > 1)
            //    {
            //        for (int a = amin + 1; a < amax; a++)
            //        {
            //            bool replace = true;
            //            for (int o = omin; o < omax; o++)
            //            {
            //                Edge edge = VerticalEdges[a, o];
            //                if (edge == null)
            //                {
            //                    replace = false;
            //                    break;
            //                }
            //            }
            //            if (replace)
            //            {
            //                for (int o = omin + 1; o < omax; o++)
            //                {
            //                    Node node = this[a, o];
            //                    if (node.IncidentEdges.Count == 2)
            //                    {
            //                        foreach (Edge edge in node.IncidentEdges)
            //                        {
            //                            if (edge.Orientation != Orientation.Vertical)
            //                            {
            //                                replace = false;
            //                                break;
            //                            }
            //                        }
            //                        if (!replace)
            //                        {
            //                            break;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        replace = false;
            //                        break;
            //                    }
            //                }
            //                if (replace)
            //                {
            //                    int weight = intersectionWeight;
            //                    for (int o = omin; o < omax; o++)
            //                    {
            //                        weight += VerticalWeights[o];
            //                        RemoveVerticalEdge(a, o);
            //                    }

            //                    Node node1 = this[a, omin];
            //                    Node node2 = this[a, omax];

            //                    var edge = new Edge(node1, node2, weight, Orientation.Vertical);
            //                    node1.IncidentEdges.Add(edge);
            //                    node2.IncidentEdges.Add(edge);
            //                }
            //            }
            //        }
            //    }
            //}

            //foreach (Corridor corridor in verticalCorridors)
            //{
            //    int amin = abscissaRedirects[corridor.Xmin];
            //    int amax = abscissaRedirects[corridor.Xmax];

            //    int omin = ordinateRedirects[corridor.Ymin];
            //    int omax = ordinateRedirects[corridor.Ymax];

            //    if (amax - amin > 1)
            //    {
            //        for (int o = omin + 1; o < omax; o++)
            //        {
            //            bool replace = true;
            //            for (int a = amin; a < amax; a++)
            //            {
            //                Edge edge = HorizontalEdges[o, a];
            //                if (edge == null)
            //                {
            //                    replace = false;
            //                    break;
            //                }
            //            }
            //            if (replace)
            //            {
            //                for (int a = amin + 1; a < amax; a++)
            //                {
            //                    Node node = this[a, o];
            //                    if (node.IncidentEdges.Count == 2)
            //                    {
            //                        foreach (Edge edge in node.IncidentEdges)
            //                        {
            //                            if (edge.Orientation != Orientation.Horizontal)
            //                            {
            //                                replace = false;
            //                                break;
            //                            }
            //                        }
            //                        if (!replace)
            //                        {
            //                            break;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        replace = false;
            //                        break;
            //                    }
            //                }
            //                if (replace)
            //                {
            //                    int weight = intersectionWeight;
            //                    for (int a = amin; a < amax; a++)
            //                    {
            //                        weight += HorizontalWeights[a];
            //                        RemoveHorizontalEdge(o, a);
            //                    }

            //                    Node node1 = this[amin, o];
            //                    Node node2 = this[amax, o];

            //                    var edge = new Edge(node1, node2, weight, Orientation.Horizontal);
            //                    node1.IncidentEdges.Add(edge);
            //                    node2.IncidentEdges.Add(edge);
            //                }
            //            }
            //        }
            //    }
            //    for (int a = amin + 1; a < amax; a++)
            //    {
            //        for (int o = omin; o < omax; o++)
            //        {
            //            RemoveVerticalEdge(a, o);
            //        }
            //    }
            //}

            AbscissaRedirects = abscissaRedirects;
            OrdinateRedirects = ordinateRedirects;
        }

        #endregion

        #region Fields

        private readonly List<Node> _nodes = new List<Node>();

        #endregion

        #region Properties

        public Dictionary<int, int> AbscissaRedirects
        {
            get;
        }

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

        public Node[,] Grid
        {
            get;
        }

        public Edge[,] HorizontalEdges
        {
            get;
        }

        public int[] HorizontalWeights
        {
            get;
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

        public Dictionary<int, int> OrdinateRedirects
        {
            get;
        }

        public Edge[,] VerticalEdges
        {
            get;
        }

        public int[] VerticalWeights
        {
            get;
        }

        #endregion

        #region Methods

        private void AddHorizontalEdge(int o, int a)
        {
            Node node1 = this[a, o];
            Node node2 = this[a + 1, o];

            int weight = HorizontalWeights[a];
            const Orientation orientation = Orientation.Horizontal;

            var edge = new Edge(node1, node2, weight, orientation);
            node1.IncidentEdges.Add(edge);
            node2.IncidentEdges.Add(edge);

            HorizontalEdges[o, a] = edge;
        }

        private void AddVerticalEdge(int a, int o)
        {
            Node node1 = this[a, o];
            Node node2 = this[a, o + 1];

            int weight = VerticalWeights[o];
            const Orientation orientation = Orientation.Vertical;

            var edge = new Edge(node1, node2, weight, orientation);
            node1.IncidentEdges.Add(edge);
            node2.IncidentEdges.Add(edge);

            VerticalEdges[a, o] = edge;
        }

        private void RemoveHorizontalEdge(int o, int a)
        {
            Edge edge = HorizontalEdges[o, a];
            if (edge != null)
            {
                edge.Node1.IncidentEdges.Remove(edge);
                edge.Node2.IncidentEdges.Remove(edge);

                HorizontalEdges[o, a] = null;
            }
        }

        private void RemoveVerticalEdge(int a, int o)
        {
            Edge edge = VerticalEdges[a, o];
            if (edge != null)
            {
                edge.Node1.IncidentEdges.Remove(edge);
                edge.Node2.IncidentEdges.Remove(edge);

                VerticalEdges[a, o] = null;
            }
        }

        #endregion

        public Node this[int a, int o]
        {
            get
            {
                Node node = Grid[a, o];
                if (node == null)
                {
                    node = new Node(a, o);

                    _nodes.Add(node);
                    Grid[a, o] = node;
                }
                return node;
            }
        }
    }
}