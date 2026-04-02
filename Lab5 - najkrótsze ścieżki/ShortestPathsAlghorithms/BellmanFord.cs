using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASD.Graphs;

namespace ShortestPathsAlghorithms
{
    public class BellmanFord
    {
        public static int[] bellmanFord(DiGraph<int> graph, int source)
        {
            int n = graph.VertexCount;
            int[] dist = new int[n];

            for (int i = 0; i < n; i++)
                dist[i] = int.MaxValue;
            dist[source] = 0;

            // Pętla wykonująca n-1 relaksacji
            for (int i = 0; i < n - 1; i++)
            {
                // W każdym kroku musimy przejść przez WSZYSTKIE wierzchołki (u)...
                for (int u = 0; u < n; u++)
                {
                    // ...i zrelaksować wszystkie wychodzące z nich krawędzie
                    foreach (Edge<int> edge in graph.OutEdges(u))
                    {
                        // Dodany warunek "dist[edge.From] != int.MaxValue" chroni przed Integer Overflow
                        if (dist[edge.From] != int.MaxValue && dist[edge.From] + edge.Weight < dist[edge.To])
                        {
                            dist[edge.To] = dist[edge.From] + edge.Weight;
                        }
                    }
                }
            }
            return dist;
        }
    }
}
