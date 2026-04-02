using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASD.Graphs;

namespace ShortestPathsAlghorithms
{
    public class FloydWarshall
    {
        public static int[,] floydWarshall(DiGraph<int> graph)
        {
            int n = graph.VertexCount;
            int[,] dist = new int[n, n];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    dist[i, j] = int.MaxValue / 2;

            // wagi krawędzi
            for (int v = 0; v < n; v++)
                foreach (Edge<int> edge in graph.OutEdges(v))
                    dist[edge.From, edge.To] = edge.Weight;

            // odległość do samego siebie = 0
            for (int v = 0; v < n; v++)
                dist[v, v] = 0;

            // główna pętla algorytmu
            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (dist[i, j] > dist[i, k] + dist[k, j])
                            dist[i, j] = dist[i, k] + dist[k, j];

            return dist;
        }
    }
}
