using System.Linq;

namespace ASD
{
    using ASD.Graphs;
    using System;
    using System.Collections.Generic;

    public class Lab06 : System.MarshalByRefObject
    {
        /// <summary>
        /// Znajduje ścieżkę o maksymalnej szerokości między dwoma wierzchołkami w grafie skierowanym. Szerokość ścieżki to minimalna waga krawędzi na tej ścieżce.
        /// </summary>
        /// <param name="G">graf skierowany, ważony</param> 
        /// <param name="start">wierzchołek początkowy</param>
        /// <param name="end">wierzchołek końcowy</param>
        /// <returns></returns>
        public List<int> WidePath(DiGraph<int> G, int start, int end)
        {
            int n = G.VertexCount;
            int[] width = new int[n];
            int[] prev = new int[n];
            bool[] visited = new bool[n];

            for (int i = 0; i < n; i++)
            {
                width[i] = 0;
                prev[i] = -1;
            }
            width[start] = int.MaxValue;

            var pq = new PriorityQueue<int, int>(); // <priorytet, wierzchołek>
            pq.Insert(start, -int.MaxValue);

            while (pq.Count > 0)
            {
                int u = pq.Extract();
                if (visited[u]) continue;
                visited[u] = true;

                foreach (Edge<int> edge in G.OutEdges(u))
                {
                    int newWidth = Math.Min(width[u], edge.Weight);
                    if (newWidth > width[edge.To])
                    {
                        width[edge.To] = newWidth;
                        prev[edge.To] = u;
                        pq.Insert(edge.To, -newWidth);
                    }
                }
            }

            List<int> path = new List<int>();
            if (width[end] == 0) return path;

            for (int v = end; v != -1; v = prev[v])
                path.Add(v);
            path.Reverse();
            return path;
        }
        public List<int> WeightedWidePath(DiGraph<int> G, int start, int end, int[] weights, int maxWeight)
        {
            int n = G.VertexCount;

            // zbierz unikalne wagi krawędzi
            var uniqueWidths = new List<int>();
            for (int v = 0; v < n; v++)
                foreach (Edge<int> edge in G.OutEdges(v))
                    if (!uniqueWidths.Contains(edge.Weight))
                        uniqueWidths.Add(edge.Weight);

            List<int> bestPath = new List<int>();
            int bestScore = int.MinValue;

            foreach (int W in uniqueWidths)
            {
                // Dijkstra minimalizujaca sumy wag wierzcholkow, ignorujac krawedzie < W
                int[] dist = new int[n];
                int[] prev = new int[n];
                bool[] visited = new bool[n];

                for (int i = 0; i < n; i++)
                {
                    dist[i] = int.MaxValue;
                    prev[i] = -1;
                }
                dist[start] = weights[start];

                var pq = new PriorityQueue<int, int>(); // <priorytet, wierzchołek>
                pq.Insert(start, weights[start]);

                while (pq.Count > 0)
                {
                    int u = pq.Extract();
                    if (visited[u]) continue;
                    visited[u] = true;
                    foreach (Edge<int> edge in G.OutEdges(u))
                    {
                        if (edge.Weight < W) continue;
                        int newDist = dist[u] + weights[edge.To];
                        if (newDist < dist[edge.To])
                        {
                            dist[edge.To] = newDist;
                            prev[edge.To] = u;
                            pq.Insert(edge.To, newDist);
                        }
                    }
                }

                if (dist[end] == int.MaxValue) continue; // brak ścieżki dla tego W

                int score = W - dist[end];
                if (score > bestScore)
                {
                    bestScore = score;
                    // rekonstruuj sciezke
                    bestPath = new List<int>();
                    for (int v = end; v != -1; v = prev[v])
                        bestPath.Add(v);
                    bestPath.Reverse();
                }
            }

            return bestPath;
        }


    }
}