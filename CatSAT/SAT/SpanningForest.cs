using System.Collections.Generic;

namespace CatSAT.SAT
{
    /// <summary>
    /// The Union-Find data structure. Currently only works for integer-valued vertices.
    /// </summary>
    public class SpanningForest
    {
        /// <summary>
        /// The number of connected components in this partition.
        /// </summary>
        public int ConnectedComponentCount;

        /// <summary>
        /// 
        /// </summary>
        private Graph _graph;
        
        /// <summary>
        /// 
        /// </summary>
        private HashSet<ushort> _edges;
        
        /// <summary>
        /// The number of vertices in this union-find data structure.
        /// </summary>
        private readonly int verticesCount;

        /// <summary>
        /// The list of representatives and ranks for this union-find data structure, indexed by vertex number.
        /// </summary>
        private readonly (int representative, int rank)[] representativesAndRanks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        public SpanningForest(Graph graph)
        {
            _graph = graph;
            ConnectedComponentCount = graph.NumVertices;
            verticesCount = graph.NumVertices;
            representativesAndRanks = new (int representative, int rank)[verticesCount];
            _edges = new HashSet<ushort>();
            for (int i = 0; i < verticesCount; i++)
            {
                representativesAndRanks[i].representative = i;
                representativesAndRanks[i].rank = 0;
            }
        }

        /// <summary>
        /// Merges two vertices to have the same representative. Vertex n merges into vertex m. Uses union by rank.
        /// </summary>
        /// <param name="n">The vertex to be merged with m.</param>
        /// <param name="m">The vertex with the representative that will become the representative for n.</param>
        public void Union(int n, int m)
        {
            int nRepresentative = Find(n);
            int mRepresentative = Find(m);
            
            if (nRepresentative == mRepresentative) return;

            _edges.Add(_graph.EdgeToSATVariable[_graph.Edges(n, m)]);
            if (representativesAndRanks[nRepresentative].rank < representativesAndRanks[mRepresentative].rank)
            {
                representativesAndRanks[nRepresentative].representative = mRepresentative;
            }
            else if (representativesAndRanks[nRepresentative].rank > representativesAndRanks[mRepresentative].rank)
            {
                representativesAndRanks[mRepresentative].representative = nRepresentative;
            }
            else
            {
                representativesAndRanks[mRepresentative].representative = nRepresentative;
                representativesAndRanks[nRepresentative].rank++;
            }
            
            ConnectedComponentCount--;
        }
        
        /// <summary>
        /// Finds the representative of the specified vertex. Uses path compression.
        /// </summary>
        /// <param name="n">The vertex for which we return the representative.</param>
        /// <returns>The vertex's representative.</returns>
        public int Find(int n)
        {
            return representativesAndRanks[n].representative == n ? n : Find(representativesAndRanks[n].representative);
        }
        
        /// <summary>
        /// Returns whether two vertices are in the same equivalence class.
        /// </summary>
        /// <param name="n">The first vertex.</param>
        /// <param name="m">The second vertex.</param>
        /// <returns>True if the vertices are in the same equivalence class, false otherwise.</returns>
        public bool SameClass(int n, int m) => Find(n) == Find(m);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool WouldConnect(int n, int m, EdgeProposition edge)
        {
            var nRep = Find(n);
            var mRep = Find(m);
            var sourceRep = Find(edge.SourceVertex);
            var destRep = Find(edge.DestinationVertex);
            return (nRep == sourceRep && mRep == destRep) || (nRep == destRep && mRep == sourceRep);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool MightDisconnect(EdgeProposition edge) => _edges.Contains(edge.Index);

        /// <summary>
        /// Resets the union-find data structure. All nodes become their own representatives and all ranks become 0.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < representativesAndRanks.Length; i++)
            {
                representativesAndRanks[i] = (i, 0);
            }
            ConnectedComponentCount = verticesCount;
            _edges.Clear();
        }
    }
}