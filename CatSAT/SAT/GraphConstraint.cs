using System.Collections.Generic;
using System.Text;

namespace CatSAT.SAT
{
    /// <summary>
    /// A class that assesses the risk associated with adding/removing edges
    /// </summary>
    internal class GraphConstraint : CustomConstraint
    {
        /// <summary>
        /// The graph corresponding to this constraint.
        /// </summary>
        public Graph Graph;

        /// <summary>
        /// The spanning tree of the graph.
        /// </summary>
        private HashSet<ushort> SpanningTree => Graph.SpanningTree;
        
        /// <summary>
        /// The risk associated with removing an edge which is in the spanning tree.
        /// </summary>
        private const int EdgeRemovalRisk = 1;

        /// <summary>
        /// The risk associated with adding an edge which connects two previously unconnected components.
        /// </summary>
        private const int EdgeAdditionRisk = -1;

        /// <summary>
        /// The GraphConstraint constructor.
        /// </summary>
        /// <param name="graph">The graph corresponding to this constraint.</param>
        public GraphConstraint(Graph graph) : base(false, 0, graph.EdgeVariables, 1) // todo: figure this out later
        {
            Graph = graph;
        }

        /// <inheritdoc />
        public override int CustomFlipRisk(ushort index, bool newValue)
        {
            var componentCount = Graph.Partition.ConnectedComponentCount;
            if (componentCount == 1 && newValue) return 0;
            var edge = Graph.SATVariableToEdge[index];
            return newValue ? AddingRisk(edge) : RemovingRisk(edge);
        }

        /// <summary>
        /// Returns the associated cost with adding this edge to the spanning tree of the graph.
        /// </summary>
        /// <param name="edge">The edge proposition to be flipped to true.</param>
        /// <returns>The cost of adding this edge. Positive cost is unfavorable, negative cost is favorable.</returns>
        private int AddingRisk(EdgeProposition edge) =>
            Graph.AreConnected(edge.SourceVertex, edge.DestinationVertex) ? 0 : EdgeAdditionRisk;

        /// <summary>
        /// Returns the associated cost with removing this edge from the spanning tree of the graph.
        /// </summary>
        /// <param name="edge">The edge proposition to be flipped to false.</param>
        /// <returns>The cost of removing this edge. Positive cost is unfavorable, negative cost is favorable.</returns>
        private int RemovingRisk(EdgeProposition edge) => !SpanningTree.Contains(edge.Index) ? 0 : EdgeRemovalRisk;

        /// <inheritdoc />
        public override void UpdateCustomConstraint(BooleanSolver b, ushort pIndex, bool newValue)
        {
            var edgeProp = Graph.SATVariableToEdge[pIndex];
            if (newValue)
            {
                Graph.Connect(edgeProp.SourceVertex, edgeProp.DestinationVertex);
                if (Graph.Partition.ConnectedComponentCount == 1)
                    b.UnsatisfiedClauses.Remove(Index);
            }
            else
            {
                var previousComponentCount = Graph.Partition.ConnectedComponentCount;
                Graph.Disconnect(edgeProp.SourceVertex, edgeProp.DestinationVertex);
                if (Graph.Partition.ConnectedComponentCount > 1 && previousComponentCount == 1)
                    b.UnsatisfiedClauses.Add(Index);
            }
        }

        /// <inheritdoc />
        internal override bool EquivalentTo(Constraint c) => false;

        /// <inheritdoc />
        internal override void Decompile(Problem p, StringBuilder b)
        {
            b.Append("GraphConstraint");
        }
        
        #region Counting methods
        /// <inheritdoc />
        public override bool IsSatisfied(ushort satisfiedDisjuncts)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override bool MaxFalseLiterals(int falseLiterals)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override bool MaxTrueLiterals(int trueLiterals)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override int ThreatCountDeltaDecreasing(ushort count)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override int ThreatCountDeltaIncreasing(ushort count)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void UpdateTrueNegativeAndFalsePositive(BooleanSolver b)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void UpdateTruePositiveAndFalseNegative(BooleanSolver b)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}