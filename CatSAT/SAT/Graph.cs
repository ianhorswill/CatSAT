using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static CatSAT.Language;

namespace CatSAT.SAT
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
        public Func<int, int, EdgeProposition> Edges;

        /// <summary>
        /// The table that maps a SAT variable index (ushort) to the edge proposition.
        /// </summary>
        public Dictionary<ushort, EdgeProposition> SATVariableToEdge = new Dictionary<ushort, EdgeProposition>();

        /// <summary>
        /// The current union-find partition of the graph.
        /// </summary>
        public UnionFind Partition;
        
        /// <summary>
        /// The current spanning tree in the graph. Consists of the SAT variable numbers.
        /// </summary>
        public HashSet<ushort> SpanningTree = new HashSet<ushort>();

        /// <summary>
        /// The problem corresponding to this graph.
        /// </summary>
        public readonly Problem Problem;
        
        /// <summary>
        /// The BooleanSolver for the problem corresponding to this graph.
        /// </summary>
        private BooleanSolver Solver => Problem.BooleanSolver;

        /// <summary>
        /// True if the spanning tree has been built, false otherwise.
        /// </summary>
        private bool _spanningTreeBuilt = false;

        /// <summary>
        /// The graph constructor.
        /// </summary>
        /// <param name="p">The problem corresponding to the graph.</param>
        /// <param name="numVertices">The number of vertices in the graph.</param>
        public Graph(Problem p, int numVertices, float initialDensity = 0.5f)
        {
            Problem = p;
            Vertices = new int[numVertices];
            for (int i = 0; i < numVertices; i++)
                Vertices[i] = i;
            Partition = new UnionFind(numVertices);
            Edges = SymmetricPredicateOfType<int, EdgeProposition>("Edges");
            for (int i = 0; i < numVertices; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    EdgeProposition edgeProposition = Edges(i, j);
                    edgeProposition.InitialProbability = initialDensity;
                    SATVariableToEdge.Add(edgeProposition.Index, edgeProposition);
                }
            }
        }
        
        /// <summary>
        /// The list of the shorts corresponding to the SAT variables of the edges in the graph.
        /// </summary>
        public short[] EdgeVariables => SATVariableToEdge.Select(pair => (short)pair.Key).ToArray();

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
        /// Returns all of the edges connected to the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex of interest.</param>
        /// <returns>The EdgePropositions including that vertex.</returns>
        private IEnumerable<EdgeProposition> EdgesIncidentTo(int vertex)
        {
            return from v in Vertices where v != vertex select Edges(v, vertex);
        }

        /// <summary>
        /// Asserts that the specified vertex has degree between min and max.
        /// </summary>
        /// <param name="vertex">The vertex of interest.</param>
        /// <param name="min">The minimum bound on the degree.</param>
        /// <param name="max">The maximum bound on the degree.</param>
        public void VertexDegree(int vertex, int min, int max)
        {
            Problem.Quantify(min, max, EdgesIncidentTo(vertex));
        }

        /// <summary>
        /// Asserts that the graph has density (percentage of edges present in the graph) between min and max.
        /// </summary>
        /// <param name="min">The minimum bound on the graph's density.</param>
        /// <param name="max">The maximum bound on the graph's density.</param>
        public void Density(float min, float max)
        {
            var edgeCount = SATVariableToEdge.Count;
            Problem.Quantify((int)Math.Round(min * edgeCount), (int)Math.Round(max * edgeCount),
                SATVariableToEdge.Values);
        }

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
            // Partition.Union(n, m); // todo: is this correct instead of above statement?
            SpanningTree.Add(Edges(n, m).Index);
            Console.WriteLine($"Connected {n} and {m}");
        }

        /// <summary>
        /// Removes the edge (n, m) from the spanning tree, if it is present there.
        /// </summary>
        /// <param name="n">The first vertex.</param>
        /// <param name="m">The second vertex.</param>
        public void Disconnect(int n, int m)
        {
            if (!SpanningTree.Contains(Edges(n, m).Index)) return;
            SpanningTree.Clear();
            _spanningTreeBuilt = false; // todo: remove this later for cleanup
            Console.WriteLine($"Disconnected {n} and {m}");
            RebuildSpanningTree();
        }
        
        /// <summary>
        /// Rebuilds the spanning tree with the current edge propositions which are true. Called after removing an edge.
        /// </summary>
        private void RebuildSpanningTree()
        {
            Partition.Clear();
            // todo: down the road, keep a list/hashset of all the edges that are true, and only iterate over those
            foreach (EdgeProposition edgeProposition in SATVariableToEdge.Values.Where(edgeProposition =>
                         Solver.Propositions[edgeProposition.Index]))
            {
                Connect(edgeProposition.SourceVertex, edgeProposition.DestinationVertex);
            }
            _spanningTreeBuilt = true;
        }

        /// <summary>
        /// Writes the Dot file to visualize the graph.
        /// </summary>
        /// <param name="solution">The graph's solution.</param>
        /// <param name="path">The file path for the outputted Dot file.</param>
        public void WriteDot(Solution solution, string path)
        {
            using var file = File.CreateText(path);
            {
                file.WriteLine("graph G {");
                file.WriteLine("   layout = fdp;");
                foreach (var vertex in Vertices)
                    file.WriteLine($"   {vertex};");
                foreach (var edge in SATVariableToEdge.Select(pair => pair.Value).Where(edge => solution[edge]))
                    file.WriteLine(
                        $"   {edge.SourceVertex} -- {edge.DestinationVertex} [color={EdgeColor(edge.Index)}];");
                file.WriteLine("}");
            }
        }

        /// <summary>
        /// Sets the color of the edge ot be green if it is in the spanning tree, red otherwise.
        /// </summary>
        /// <param name="index">The index corresponding to the edge.</param>
        /// <returns>The color of the edge as a string.</returns>
        private string EdgeColor(ushort index) => SpanningTree.Contains(index) ? "green" : "red";

        /// <summary>
        /// Determines whether the spanning tree has been correctly constructed.
        /// </summary>
        /// <returns>True if the spanning tree contains all the vertices in the graph, false otherwise.</returns>
        public bool IsSpanningTree()
        {
            HashSet<int> visited = new HashSet<int>();
            foreach (ushort index in SpanningTree)
            {
                visited.Add(SATVariableToEdge[index].SourceVertex);
                visited.Add(SATVariableToEdge[index].DestinationVertex);
            }
            Console.WriteLine(string.Join(", ", visited));
            return visited.Count == Vertices.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public void EnsureSpanningTreeBuilt()
        {
            if (_spanningTreeBuilt) return;
            RebuildSpanningTree();
        }

        public void Reset()
        {
            _spanningTreeBuilt = false;
        }
    }
    
    /// <summary>
    /// The Union-Find data structure. Currently only works for integer-valued vertices.
    /// </summary>
    public class UnionFind
    {
        /// <summary>
        /// The number of connected components in this partition.
        /// </summary>
        public int ConnectedComponentCount;
        
        /// <summary>
        /// The number of vertices in this union-find data structure.
        /// </summary>
        private readonly int verticesCount;

        /// <summary>
        /// The list of representatives and ranks for this union-find data structure, indexed by vertex number.
        /// </summary>
        private readonly (int representative, int rank)[] representativesAndRanks;

        /// <summary>
        /// The union-find constructor.
        /// </summary>
        /// <param name="count">The number of vertices.</param>
        public UnionFind(int count)
        {
            ConnectedComponentCount = count;
            verticesCount = count;
            representativesAndRanks = new (int representative, int rank)[verticesCount];
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
        /// Resets the union-find data structure. All nodes become their own representatives and all ranks become 0.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < representativesAndRanks.Length; i++)
            {
                representativesAndRanks[i] = (i, 0);
            }
            ConnectedComponentCount = verticesCount;
        }

        // todo: for directed graphs, keep track of in and out degrees of vertices
        // this is used to ensure a path between two nodes in the graph
        // source has out degree 1, in degree 0; destination has in degree 1, out degree 0
        // every other node either has in degree = out degree = 1 or isn't connected to the path
    }
}