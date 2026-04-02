using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASD.Graphs;

namespace ShortestPathsAlghorithms
{
    public class Dijkstra
    {
        public static int[] dijkstra(DiGraph<int> graph, int source)
        {
            int n = graph.VertexCount;
            int[] dist = new int[n];
            bool[] visited = new bool[n];

            for (int i = 0; i < n; i++)
                dist[i] = int.MaxValue;
            dist[source] = 0;

            for (int i = 0; i < n; i++)
            {
                int u = -1;
                for (int v = 0; v < n; v++)
                    if (!visited[v] && (u == -1 || dist[v] < dist[u]))
                        u = v;

                if (dist[u] == int.MaxValue) break;
                visited[u] = true;

                foreach (Edge<int> edge in graph.OutEdges(u))
                {
                    if (dist[u] + edge.Weight < dist[edge.To])
                        dist[edge.To] = dist[u] + edge.Weight;
                }
            }

            return dist;
        }
    }
}
