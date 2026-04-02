using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASD.Graphs;

namespace ShortestPathsAlghorithms
{
    public class Johnson
    {
        public static int[,] johnson(DiGraph<int> graph)
        {
            int n = graph.VertexCount;
            int[,] dist = new int[n, n];

            // Krok 1: Skonstruuj G' z nowym wierzchołkiem q
            DiGraph<int> gPrime = new DiGraph<int>(n + 1);
            int q = n; // nowy wierzchołek q

            // przepisz krawędzie z oryginalnego grafu
            for (int v = 0; v < n; v++)
                foreach (Edge<int> edge in graph.OutEdges(v))
                    gPrime.AddEdge(edge.From, edge.To, edge.Weight);

            // dodaj krawędzie q -> v o wadze 0 dla każdego v
            for (int v = 0; v < n; v++)
                gPrime.AddEdge(q, v, 0);

            // Krok 2: Bellman-Ford z q jako źródłem
            int[] h = BellmanFord.bellmanFord(gPrime, q);

            // Krok 3: Przekształć wagi krawędzi
            DiGraph<int> gReweighted = new DiGraph<int>(n);
            for (int v = 0; v < n; v++)
                foreach (Edge<int> edge in graph.OutEdges(v))
                    gReweighted.AddEdge(edge.From, edge.To, edge.Weight + h[edge.From] - h[edge.To]);

            // Krok 4: Dijkstra z każdego wierzchołka
            for (int u = 0; u < n; u++)
            {
                int[] du = Dijkstra.dijkstra(gReweighted, u);
                for (int v = 0; v < n; v++)
                    dist[u, v] = du[v] == int.MaxValue / 2
                        ? int.MaxValue / 2
                        : du[v] - h[u] + h[v];
            }

            return dist;
        }
    }
}
