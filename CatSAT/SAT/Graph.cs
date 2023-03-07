using System;
using static CatSAT.Language;

namespace CatSAT
{
    /// <summary>
    /// The representation of a vertex in a graph.
    /// </summary>
    /// <typeparam name="T">The type of the vertex. Most likely will be int.</typeparam>
    public class Vertex<T>
    {
        /// <summary>
        /// The value stored in this vertex. Most likely will be int.
        /// </summary>
        public T Value;
        
        /// <summary>
        /// The representative of this vertex. This is used for the union-find algorithm.
        /// </summary>
        public Vertex<T> Representative;

        /// <summary>
        /// The rank of this vertex. This is used for the union-find algorithm.
        /// </summary>
        public int Rank;
        
        /// <summary>
        /// The vertex constructor without a specified representative.
        /// </summary>
        /// <param name="value">The value of the vertex.</param>
        public Vertex(T value)
        {
            Value = value;
            Representative = this;
            Rank = 0;
        }

        /// <summary>
        /// The vertex constructor with a specified representative.
        /// </summary>
        /// <param name="value">The value of the vertex.</param>
        /// <param name="representative">The vertex's representative.</param>
        public Vertex(T value, Vertex<T> representative)
        {
            Value = value;
            Representative = representative;
            Rank = 0;
        }
        
        /// <summary>
        /// The vertex constructor with a specified representative and rank.
        /// </summary>
        /// <param name="value">The value of the vertex.</param>
        /// <param name="representative">The vertex's representative.</param>
        /// <param name="rank">The rank (height) of the vertex in the tree.</param>
        public Vertex(T value, Vertex<T> representative, int rank)
        {
            Value = value;
            Representative = representative;
            Rank = rank;
        }
    }
    
    /// <summary>
    /// The representation of a graph.
    /// </summary>
    /// <typeparam name="T">The type of the vertices. Most likely will be int.</typeparam>
    public class Graph<T>
    {
        /// <summary>
        /// The list of vertices in this graph.
        /// </summary>
        public Vertex<T>[] Vertices;

        /// <summary>
        /// The function that returns the proposition that the edge between two vertices exists. The integers are
        /// the indices of the vertices in the Vertices array.
        /// </summary>
        public Func<int, int, Proposition> Edges = SymmetricPredicate<int>("Edges");

        /// <summary>
        /// The graph constructor.
        /// </summary>
        /// <param name="vertices">The list of vertices for this graph.</param>
        public Graph(Vertex<T>[] vertices)
        {
            Vertices = vertices;
        }

        /// <summary>
        /// Returns the index of the given vertex. Returns -1 if the vertex is not in the graph.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns>The index of the vertex.</returns>
        /// <exception cref="ArgumentException">Throws an exception if the vertex is not found in the graph.</exception>
        public int VertexToIndex(Vertex<T> vertex)
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
        public Vertex<T> IndexToVertex(int index) => Vertices[index];
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

        // todo: change <int> to <T>
        /// <summary>
        /// The union-find constructor.
        /// </summary>
        /// <param name="graph">The graph corresponding to this union-find data structure.</param>
        public UnionFind(Graph<int> graph)
        {
            Graph = graph;
        }
        
        /// <summary>
        /// Merges two vertices to have the same representative. Vertex n merges into vertex m. Uses union by rank.
        /// </summary>
        /// <param name="n">The vertex to be merged with m.</param>
        /// <param name="m">The vertex with the representative that will become the representative for n.</param>
        public void Union(Vertex<int> n, Vertex<int> m)
        {
            Vertex<int> nRepresentative = Find(n);
            Vertex<int> mRepresentative = Find(m);
            
            if (nRepresentative == mRepresentative) return;

            if (nRepresentative.Rank < mRepresentative.Rank)
            {
                nRepresentative.Representative = mRepresentative;
            }
            else if (nRepresentative.Rank > mRepresentative.Rank)
            {
                mRepresentative.Representative = nRepresentative;
            }
            else
            {
                mRepresentative.Representative = nRepresentative;
                nRepresentative.Rank++;
            }
        }
        
        /// <summary>
        /// Finds the representative of the specified vertex. Uses path compression.
        /// </summary>
        /// <param name="n">The vertex for which we return the representative.</param>
        /// <returns>The vertex's representative.</returns>
        public Vertex<int> Find(Vertex<int> n)
        {
            if (n.Representative == n) return n;
            return Find(n.Representative);
        }
        
        /// <summary>
        /// Returns whether two vertices are in the same equivalence class.
        /// </summary>
        /// <param name="n">The first vertex.</param>
        /// <param name="m">The second vertex.</param>
        /// <returns>True if the vertices are in the same equivalence class, false otherwise.</returns>
        public bool SameClass(Vertex<int> n, Vertex<int> m) => Find(n) == Find(m);
    }
}