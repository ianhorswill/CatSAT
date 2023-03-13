using System;
using static CatSAT.Language;

namespace CatSAT
{
    /// <summary>
    /// The representation of a graph.
    /// </summary>
    /// <typeparam name="T">The type of the vertices. Most likely will be int.</typeparam>
    public class Graph<T>
    {
        /// <summary>
        /// The list of vertices in this graph.
        /// </summary>
        public T[] Vertices;

        /// <summary>
        /// The function that returns the proposition that the edge between two vertices exists. The integers are
        /// the indices of the vertices in the Vertices array.
        /// </summary>
        public Func<int, int, Proposition> Edges = SymmetricPredicate<int>("Edges");

        /// <summary>
        /// The graph constructor.
        /// </summary>
        /// <param name="vertices">The list of vertices for this graph.</param>
        public Graph(T[] vertices)
        {
            Vertices = vertices;
        }

        /// <summary>
        /// Returns the index of the given vertex. Returns -1 if the vertex is not in the graph.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns>The index of the vertex.</returns>
        /// <exception cref="ArgumentException">Throws an exception if the vertex is not found in the graph.</exception>
        public int VertexToIndex(T vertex)
        {
            int index = Array.IndexOf(Vertices, vertex);
            if (index == -1)
                throw new ArgumentException("Vertex not found in graph.");
            return index;
        }
        
        /// <summary>
        /// Returns the vertex at the given index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The vertex at the specified index.</returns>
        public T IndexToVertex(int index) => Vertices[index];
    }
    
    // has the array of representatives and ranks, indexed by vertex number
    
    // todo: expand to type UnionFind<T>
    /// <summary>
    /// The Union-Find data structure. Currently only works for integer-valued vertices.
    /// </summary>
    public class UnionFind
    {
        /// <summary>
        /// The number of vertices in this union-find data structure.
        /// </summary>
        public int VerticesCount;
        
        /// <summary>
        /// The list of representatives and ranks for this union-find data structure, indexed by vertex number.
        /// </summary>
        public (int, int)[] RepresentativesAndRanks;

        /// <summary>
        /// The union-find constructor.
        /// </summary>
        /// <param name="num"></param>
        public UnionFind(int num)
        {
            VerticesCount = num;
            RepresentativesAndRanks = new (int, int)[num];
        }

        /// <summary>
        /// The union-find constructor with preset representatives and ranks..
        /// </summary>
        /// <param name="tuples">The list of representatives and ranks for the specified vertices.</param>
        /// <param name="num"></param>
        public UnionFind(int num, (int, int)[] tuples)
        {
            VerticesCount = num;
            RepresentativesAndRanks = tuples;
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

            if (RepresentativesAndRanks[nRepresentative].Item2 < RepresentativesAndRanks[mRepresentative].Item2)
            {
                RepresentativesAndRanks[nRepresentative].Item1 = mRepresentative;
            }
            else if (RepresentativesAndRanks[nRepresentative].Item2 > RepresentativesAndRanks[mRepresentative].Item2)
            {
                RepresentativesAndRanks[mRepresentative].Item1 = nRepresentative;
            }
            else
            {
                RepresentativesAndRanks[mRepresentative].Item1 = nRepresentative;
                RepresentativesAndRanks[nRepresentative].Item2++;
            }
        }
        
        /// <summary>
        /// Finds the representative of the specified vertex. Uses path compression.
        /// </summary>
        /// <param name="n">The vertex for which we return the representative.</param>
        /// <returns>The vertex's representative.</returns>
        public int Find(int n)
        {
            if (RepresentativesAndRanks[n].Item1 == n) return n;
            return Find(RepresentativesAndRanks[n].Item1);
        }
        
        /// <summary>
        /// Returns whether two vertices are in the same equivalence class.
        /// </summary>
        /// <param name="n">The first vertex.</param>
        /// <param name="m">The second vertex.</param>
        /// <returns>True if the vertices are in the same equivalence class, false otherwise.</returns>
        public bool SameClass(int n, int m) => Find(n) == Find(m);
    }
}