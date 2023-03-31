using System;
using System.Collections;
using System.Collections.Generic;
using static CatSAT.Language;

namespace CatSAT
{
    /// <summary>
    /// The representation of a graph.
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// The list of vertices in this graph.
        /// </summary>
        public int[] Vertices;

        /// <summary>
        /// The function that returns the proposition that the edge between two vertices exists. The integers are
        /// the indices of the vertices in the Vertices array.
        /// </summary>
        public Func<int, int, Proposition> Edges;
        
        // edge -> sat variable -> proposition -> truth assignment -> true if (a, b) edge is in graph, false otherwise
        // need to be able to answer "is it safe to flip sat variable x? need something to map sat variable to whether
        // the edges exist, and if so what they connect
        // need a table of what all the edges are, and the sat variables for them
        // need to use special proposition, where you add node numbers of the edge, maybe..? maybe not

        /// <summary>
        /// The table that maps a SAT variable index (ushort) to the edge of the form (n, m).
        /// </summary>
        public Hashtable SATVariableToEdge = new Hashtable();

        /// <summary>
        /// The current union-find partition of the graph.
        /// </summary>
        public UnionFind Partition;
        
        // make a spanning tree to keep track of what is connected and what isn't
        // spanning tree is a hash set of sat variable numbers (ushorts)
        // every time two things are union'ed which weren't connected, that edge is added to the spanning tree
        // keep track of whether an edge is part of the spanning tree
        // always safe to add an edge, always safe to remove an edge if it isn't in the spanning tree
        /// <summary>
        /// The current spanning tree in the graph. Consists of the SAT variable numbers.
        /// </summary>
        public HashSet<ushort> SpanningTree = new HashSet<ushort>();

        /// <summary>
        /// The graph constructor.
        /// </summary>
        /// <param name="problem">The problem corresponding to the graph.</param>
        /// <param name="numVertices">The number of vertices in the graph.</param>
        public Graph(Problem problem, int numVertices)
        {
            Vertices = new int[numVertices];
            for (int i = 0; i < numVertices; i++)
                Vertices[i] = i;
            Partition = new UnionFind(numVertices);
            Edges = SymmetricPredicate<int>("Edges");
            foreach (SATVariable satVariable in problem.SATVariables)
            {
                // todo: change the value to the (n, m) edge instead of just the proposition
                SATVariableToEdge.Add(satVariable.Proposition.Index, satVariable.Proposition);
            }
        }

        /// <summary>
        /// Returns the index of the given vertex. Returns -1 if the vertex is not in the graph.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns>The index of the vertex.</returns>
        /// <exception cref="ArgumentException">Throws an exception if the vertex is not found in the graph.</exception>
        public int VertexToIndex(int vertex)
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
        public int IndexToVertex(int index) => Vertices[index];

        /// <summary>
        /// Returns whether the two specified vertices are connected in the current partition.
        /// </summary>
        /// <param name="n">The first vertex.</param>
        /// <param name="m">The second vertex.</param>
        /// <returns>True if the two vertices are connected, false otherwise.</returns>
        public bool AreConnected(int n, int m) => Partition.SameClass(n, m);

        /// <summary>
        /// Adds the edge (n, m) to the spanning tree. 
        /// </summary>
        /// <param name="n">The first vertex in the edge.</param>
        /// <param name="m">The second vertex in the edge.</param>
        public void Connect(int n, int m)
        {
            int nRepresentative = Partition.Find(n);
            int mRepresentative = Partition.Find(m);

            if (nRepresentative == mRepresentative) return;
            Partition.Union(nRepresentative, mRepresentative);
            SpanningTree.Add(Edges(n, m).Index);
        }
        
        // void Disconnect(T a, T b) {
        // check if (a, b) is in the spanning tree, if not, return
        // else rebuild the spanning tree without (a, b)
        // clear spanning tree
        // reinitialize partition
        // go through all edges that are true, call connect on them
        // }
        
        /// <summary>
        /// Removes the edge (n, m) from the spanning tree, if it is present there.
        /// </summary>
        /// <param name="n">The first vertex.</param>
        /// <param name="m">The second vertex.</param>
        public void Disconnect(int n, int m)
        {
            if (!SpanningTree.Contains(Edges(n, m).Index)) return;
            
            foreach (ushort satVariable in SpanningTree)
            {
                // todo: what
            }
            SpanningTree.Clear();
            Partition = new UnionFind(Vertices.Length);
        }
    }
    
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
        /// <param name="num">The number of vertices.</param>
        public UnionFind(int num)
        {
            VerticesCount = num;
            RepresentativesAndRanks = new (int, int)[num];
        }

        /// <summary>
        /// The union-find constructor with preset representatives and ranks.
        /// </summary>
        /// <param name="num">The number of vertices.</param>
        /// <param name="tuples">The list of representatives and ranks for the specified vertices.</param>
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
            return RepresentativesAndRanks[n].Item1 == n ? n : Find(RepresentativesAndRanks[n].Item1);
        }
        
        /// <summary>
        /// Returns whether two vertices are in the same equivalence class.
        /// </summary>
        /// <param name="n">The first vertex.</param>
        /// <param name="m">The second vertex.</param>
        /// <returns>True if the vertices are in the same equivalence class, false otherwise.</returns>
        public bool SameClass(int n, int m) => Find(n) == Find(m);
        
        // todo: for directed graphs, keep track of in and out degrees of vertices
        // this is used to ensure a path between two nodes in the graph
        // source has out degree 1, in degree 0; destination has in degree 1, out degree 0
        // every other node either has in degree = out degree = 1 or isn't connected to the path
    }
}