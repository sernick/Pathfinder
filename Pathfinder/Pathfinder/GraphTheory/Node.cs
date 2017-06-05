using System.Collections.Generic;


namespace Pathfinder.GraphTheory
{
    internal class Node
    {
        #region Constructors

        public Node(int a, int o)
        {
            A = a;
            O = o;
        }

        #endregion

        #region Properties

        public int A
        {
            get;
        }

        public List<Edge> IncidentEdges
        {
            get;
        } = new List<Edge>();


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

        public int O
        {
            get;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return A + "," + O;
        }

        #endregion
    }
}