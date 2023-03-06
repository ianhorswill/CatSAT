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
    
    // todo: expand to type UnionFind<T>
    /// <summary>
    /// The Union-Find data structure. Currently only works for integer-valued vertices.
    /// </summary>
    public class UnionFind
    {
        /// <summary>
        /// The graph corresponding to this union-find data structure.
        /// </summary>
        public Graph<int> Graph;
        
        /// <summary>
        /// The list of representatives, indexed by the vertex.
        /// </summary>
        private int[] representatives;
        
        // todo: change <int> to <T>
        /// <summary>
        /// The union-find constructor.
        /// </summary>
        /// <param name="graph">The graph corresponding to this union-find data structure.</param>
        public UnionFind(Graph<int> graph)
        {
            Graph = graph;
            representatives = new int[graph.Vertices.Length];
            graph.Vertices.CopyTo(representatives, 0);
        }
        
        /// <summary>
        /// Merges two vertices to have the same representative. Vertex n merges into vertex m.
        /// </summary>
        /// <param name="n">The vertex to be merged with m.</param>
        /// <param name="m">The vertex with the representative that will become the representative for n.</param>
        public void Union(int n, int m)
        {
            representatives[n] = m;
        }
        
        /// <summary>
        /// Recursively finds the representative of the specified vertex.
        /// </summary>
        /// <param name="n">The vertex for which we return the representative.</param>
        /// <returns></returns>
        public int Find(int n)
        {
            if (representatives[n] == n) return n;
            return Find(representatives[n]);
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