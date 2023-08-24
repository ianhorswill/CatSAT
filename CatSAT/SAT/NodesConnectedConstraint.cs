using System;
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
        private UnionFind SpanningForest => Graph.Partition;
        
        /// <summary>
        /// The default risk associated with removing an edge.
        /// </summary>
        private const int EdgeRemovalRisk = 1;
        
        /// <summary>
        /// The default risk associated with adding an edge.
        /// </summary>
        private const int EdgeAdditionRisk = -1;

        /// <summary>
        /// True if the source node and destination node are connected, false otherwise.
        /// </summary>
        private bool _connected = false;
        
        // todo: binary heap implemented using an array for dijkstra's priority queue
        // complete binary tree has nice property
        // put bfs order of nodes in an array
        // parent of node i is i / 2, children are at 2i and 2i + 1

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
        public override int CustomFlipRisk(ushort index, bool adding)
        {
            var edge = Graph.SATVariableToEdge[index];
            bool previouslyConnected = Graph.AreConnected(edge.SourceVertex, edge.DestinationVertex);
            if (previouslyConnected && adding) return 0;
            return adding ? AddingRisk(edge) : RemovingRisk(edge);
        }
        
        /// <summary>
        /// Returns the associated cost with adding this edge to the graph.
        /// </summary>
        /// <param name="edge">The edge proposition to be flipped to true.</param>
        /// <returns>The cost of adding this edge. Positive cost is unfavorable, negative cost is favorable.</returns>
        private int AddingRisk(EdgeProposition edge)
        {
            if (SpanningForest.WouldConnect(SourceNode, DestinationNode, edge)) return EdgeAdditionRisk * 2;
            return Graph.AreConnected(edge.SourceVertex, edge.DestinationVertex) ? 0 : EdgeAdditionRisk;
        }
        
        // todo: this is version 0. make it better later
        /// <summary>
        /// Returns the associated cost with removing this edge from the graph.
        /// </summary>
        /// <param name="edge">The edge proposition to be flipped to false.</param>
        /// <returns>The cost of removing this edge. Positive cost is unfavorable, negative cost is favorable.</returns>
        private int RemovingRisk(EdgeProposition edge) => Graph.SpanningTree.Contains(edge.Index) ? EdgeRemovalRisk : 0;
        
        /// <summary>
        /// Find the edge (proposition) to flip that will lead to the lowest cost.
        /// </summary>
        /// <param name="b">The current BooleanSolver.</param>
        /// <returns>The index of the edge (proposition) to flip.</returns>
        public override ushort GreedyFlip(BooleanSolver b)
        {
            List<short> disjuncts = UnPredeterminedDisjuncts;
            ushort lastFlipOfThisClause = b.LastFlip[Index];

            var best = 0;
            var bestDelta = int.MaxValue;

            var dCount = (uint)disjuncts.Count;
            var index = Random.InRange(dCount);
            uint prime;
            do prime = Random.Prime(); while (prime <= dCount);
            for (var i = 0; i < dCount; i++)
            {
                var literal = disjuncts[(int)index];
                index = (index + prime) % dCount;
                var selectedVar = (ushort)Math.Abs(literal);
                if (selectedVar == lastFlipOfThisClause) continue;
                EdgeProposition edge = Graph.SATVariableToEdge[selectedVar];
                if (Graph.AreConnected(edge.SourceVertex, edge.DestinationVertex)) continue;
                var delta = b.UnsatisfiedClauseDelta(selectedVar);
                if (delta <= 0)
                {
                    best = selectedVar;
                    break;
                }

                if (delta >= bestDelta) continue;
                best = selectedVar;
                bestDelta = delta;
            }

            if (best == 0) return (ushort)Math.Abs(disjuncts.RandomElement());
            return (ushort)best;
        }

        /// <inheritdoc />
        public override void UpdateCustomConstraint(BooleanSolver b, ushort pIndex, bool adding)
        {
            var edgeProp = Graph.SATVariableToEdge[pIndex];
            bool previouslyConnected = Graph.AreConnected(SourceNode, DestinationNode);
            if (adding)
            {
                Graph.Connect(edgeProp.SourceVertex, edgeProp.DestinationVertex);
                if (!previouslyConnected && Graph.AreConnected(SourceNode, DestinationNode) &&
                    b.UnsatisfiedClauses.Contains(Index))
                {
                    b.UnsatisfiedClauses.Remove(Index);
                }
            }
            else
            {
                Graph.Disconnect(edgeProp.SourceVertex, edgeProp.DestinationVertex);
                if (previouslyConnected && !Graph.AreConnected(SourceNode, DestinationNode))
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
        
        /// <inheritdoc />
        public override void Reset()
        {
            _connected = false;
            Graph.Reset();
        }

        #region Counting methods
        /// <inheritdoc />
        public override bool IsSatisfied(ushort satisfiedDisjuncts)
        {
            Graph.EnsureSpanningTreeBuilt();
            return Graph.AreConnected(SourceNode, DestinationNode);
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