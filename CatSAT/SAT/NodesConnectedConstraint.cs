using System.Collections.Generic;
using System.Text;

namespace CatSAT.SAT
{
    /// <summary>
    /// A class that represents a constraint on the graph. For now, the constraint is that two nodes must be connected.
    /// </summary>
    public class NodesConnectedConstraint : CustomConstraint
    {
        /// <summary>
        /// The graph corresponding to this constraint.
        /// </summary>
        public Graph Graph;

        /// <summary>
        /// The first node to be connected.
        /// </summary>
        public int SourceNode;

        /// <summary>
        /// The second node to be connected.
        /// </summary>
        public int DestinationNode;

        /// <summary>
        /// The spanning forest of the graph.
        /// </summary>
        private List<HashSet<ushort>> SpanningForest => Graph.SpanningForest;

        // todo: fix this description
        /// <summary>
        /// The risk associated with removing an edge.
        /// </summary>
        private const int EdgeRemovalRisk = 1;
        
        // todo: fix this description
        /// <summary>
        /// The risk associated with adding an edge.
        /// </summary>
        private const int EdgeAdditionRisk = -1;

        /// <summary>
        /// The NodesConnectedConstraint constructor.
        /// </summary>
        /// <param name="graph">The graph corresponding to this constraint.</param>
        /// <param name="sourceNode">The first node to be connected.</param>
        /// <param name="destinationNode">The second node to be connected.</param>
        public NodesConnectedConstraint(Graph graph, int sourceNode, int destinationNode) : base(false, (ushort)short.MaxValue, graph.EdgeVariables, 1) // todo: figure this out later
        {
            Graph = graph;
            SourceNode = sourceNode;
            DestinationNode = destinationNode;
            foreach (var edge in graph.SATVariableToEdge.Values)
            {
                graph.Problem.SATVariables[edge.Index].CustomConstraints.Add(this);
            }
        }
        
        /// <inheritdoc />
        public override int CustomFlipRisk(ushort index, bool newValue)
        {
            var edge = Graph.SATVariableToEdge[index];
            bool previouslyConnected = Graph.AreConnected(edge.SourceVertex, edge.DestinationVertex);
            if (previouslyConnected && newValue) return 0;
            return newValue ? AddingRisk(edge) : RemovingRisk(edge);
        }

        // todo: fix
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private int AddingRisk(EdgeProposition edge) => EdgeAdditionRisk;
        
        // todo: fix
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private int RemovingRisk(EdgeProposition edge) => EdgeRemovalRisk;
        
        /// <inheritdoc />
        public override void UpdateCustomConstraint(BooleanSolver b, ushort pIndex, bool newValue)
        {
            var edgeProp = Graph.SATVariableToEdge[pIndex];
            bool previouslyConnected = Graph.AreConnected(edgeProp.SourceVertex, edgeProp.DestinationVertex);
            if (newValue)
            {
                // todo: need to change Connect to work with spanning forest?
                Graph.Connect(edgeProp.SourceVertex, edgeProp.DestinationVertex);
                if (!previouslyConnected && Graph.AreConnected(edgeProp.SourceVertex, edgeProp.DestinationVertex) &&
                    b.UnsatisfiedClauses.Contains(Index))
                {
                    b.UnsatisfiedClauses.Remove(Index);
                }
            }
            else
            {
                // todo: need to change Disconnect to work with spanning forest?
                Graph.Disconnect(edgeProp.SourceVertex, edgeProp.DestinationVertex);
                if (previouslyConnected && !Graph.AreConnected(edgeProp.SourceVertex, edgeProp.DestinationVertex))
                {
                    b.UnsatisfiedClauses.Add(Index);
                }
            }
        }
        
        /// <inheritdoc />
        internal override bool EquivalentTo(Constraint c) => false;

        /// <inheritdoc />
        internal override void Decompile(Problem p, StringBuilder b)
        {
            b.Append("NodesConnectedConstraint");
        }

        #region Counting methods
        /// <inheritdoc />
        public override bool IsSatisfied(ushort satisfiedDisjuncts)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override int ThreatCountDeltaIncreasing(ushort count)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override int ThreatCountDeltaDecreasing(ushort count)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void UpdateTruePositiveAndFalseNegative(BooleanSolver b)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void UpdateTrueNegativeAndFalsePositive(BooleanSolver b)
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
        #endregion
    }
}