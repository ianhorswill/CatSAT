﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CatSAT.SAT
{
    // todo: look into making it so that edges not in spanning tree (red ones) wouldn't get considered by greedy flip
    /// <summary>
    /// A class that represents a constraint on the graph. For now, the constraint is that the graph must be connected.
    /// </summary>
    internal class GraphConnectedConstraint : CustomConstraint
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
        /// The GraphConnectedConstraint constructor.
        /// </summary>
        /// <param name="graph">The graph corresponding to this constraint.</param>
        public GraphConnectedConstraint(Graph graph) : base(false, (ushort)short.MaxValue, graph.EdgeVariables, 1) // todo: figure this out later
        {
            Graph = graph;
            foreach (var edge in graph.SATVariableToEdge.Values)
            {
                graph.Problem.SATVariables[edge.Index].CustomConstraints.Add(this);
            }
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

        // todo: find best edge to flip while asking other constraints how they feel about flipping the particular edge
        // want to minimize return of UnsatisfiedClauseDelta (take the first thing that gives us a non-positive value)
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
                best = selectedVar;
                break;
            }

            if (best == 0) return (ushort)Math.Abs(disjuncts.RandomElement());
            return (ushort)best;
        }
        
        /// <inheritdoc />
        public override void UpdateCustomConstraint(BooleanSolver b, ushort pIndex, bool newValue)
        {
            var edgeProp = Graph.SATVariableToEdge[pIndex];
            if (newValue)
            {
                Graph.Connect(edgeProp.SourceVertex, edgeProp.DestinationVertex);
                if (Graph.Partition.ConnectedComponentCount == 1 && b.UnsatisfiedClauses.Contains(Index))
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
            b.Append("GraphConnectedConstraint");
        }
        
        #region Counting methods
        /// <inheritdoc />
        public override bool IsSatisfied(ushort satisfiedDisjuncts)
        {
            // if (Graph.Partition.ConnectedComponentCount == 1) return true; // todo: is this right?
            Graph.RebuildSpanningTree();
            return Graph.Partition.ConnectedComponentCount == 1;
            // todo: check that this is returning true
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