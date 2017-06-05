using System.Collections.Generic;
using System.Linq;


namespace Pathfinder.GraphTheory
{
    internal class Dijkstra
    {
        #region Methods

        public static void FindShortPath(Graph graph,
                                         Node start,
                                         Node end,
                                         out List<Node> nodePath,
                                         out List<Edge> edgePath)
        {
            nodePath = new List<Node>();
            edgePath = new List<Edge>();

            List<Node> notVisited = graph.Nodes.ToList();
            var track = new Dictionary<Node, DijkstraData>
                        {
                            [start] = new DijkstraData {Price = 0, PreviousNode = null, PreviousEdge = null}
                        };

            while (true)
            {
                Node toOpen = null;
                double bestPrice = double.PositiveInfinity;

                foreach (Node node in notVisited)
                {
                    if (track.ContainsKey(node))
                    {
                        double price = track[node].Price;
                        if (price < bestPrice)
                        {
                            bestPrice = price;
                            toOpen = node;
                        }
                    }
                }

                if (toOpen == null)
                {
                    return;
                }

                if (toOpen == end)
                {
                    break;
                }

                foreach (Edge edge in toOpen.IncidentEdges)
                {
                    if (edge.IsIncident(toOpen))
                    {
                        int currentPrice = track[toOpen].Price + edge.Weight;
                        Node nextNode = edge.OtherNode(toOpen);
                        if (!track.ContainsKey(nextNode) || track[nextNode].Price > currentPrice)
                        {
                            track[nextNode] = new DijkstraData
                                              {
                                                  Price = currentPrice,
                                                  PreviousNode = toOpen,
                                                  PreviousEdge = edge
                                              };
                        }
                    }
                }

                notVisited.Remove(toOpen);
            }

            Node lastNode = end;

            while (true)
            {
                nodePath.Add(lastNode);

                DijkstraData data = track[lastNode];

                lastNode = data.PreviousNode;
                if (lastNode == null)
                {
                    break;
                }

                edgePath.Add(data.PreviousEdge);
            }

            nodePath.Reverse();
            edgePath.Reverse();
        }

        #endregion
    }
}