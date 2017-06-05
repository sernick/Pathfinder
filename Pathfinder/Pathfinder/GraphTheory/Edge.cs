using System;


namespace Pathfinder.GraphTheory
{
    internal class Edge
    {
        #region Constructors

        public Edge(Node node1, Node node2, int weight, Orientation orientation)
        {
            Node1 = node1;
            Node2 = node2;
            Weight = weight;
            Orientation = orientation;
        }

        #endregion

        #region Properties

        public Node Node1
        {
            get;
        }

        public Node Node2
        {
            get;
        }

        public Orientation Orientation
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
            return node == Node1 || node == Node2;
        }

        public Node OtherNode(Node node)
        {
            if (Node1 == node)
            {
                return Node2;
            }
            if (Node2 == node)
            {
                return Node1;
            }
            throw new ArgumentException();
        }

        #endregion
    }
}