using System;
using static CatSAT.Language;

namespace CatSAT
{
    /// <summary>
    /// The representation of a graph.
    /// </summary>
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
        /// <returns></returns>
        public T IndexToVertex(int index) => Vertices[index];
    }
}