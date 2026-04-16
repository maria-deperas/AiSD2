using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    public static class FlowExtender
    {

        /// <summary>
        /// Metod wylicza minimalny s-t-przekrój.
        /// </summary>
        /// <param name="undirectedGraph">Nieskierowany graf</param>
        /// <param name="s">wierzchołek źródłowy</param>
        /// <param name="t">wierzchołek docelowy</param>
        /// <param name="minCut">minimalny przekrój</param>
        /// <returns>wartość przekroju</returns>
        public static double MinCut(this Graph<double> undirectedGraph, int s, int t, out Edge<double>[] minCut)
        {
            int n = undirectedGraph.VertexCount;
            var graph = new DiGraph<double>(n);

            // Zamieniamy nieskierowany graf na skierowany
            // Każdą krawędź zamieniamy na dwie skierowane o tej samej wadze
            for (int u = 0; u < n; u++)
            {
                foreach (var e in undirectedGraph.OutEdges(u))
                {
                    if (e.From < e.To)
                    {
                        graph.AddEdge(e.From, e.To, e.Weight);
                        graph.AddEdge(e.To, e.From, e.Weight);
                    }
                }
            }

            // Wyznaczamy maksymalny przepływ = minimalny przekrój
            var (flowValue, flowGraph) = Flows.FordFulkerson(graph, s, t);

            // Szukamy składowej S
            minCut = GetMinCutFromFlow(undirectedGraph, graph, flowGraph, s);

            return flowValue;
        }

        // Metoda pomocnicza wyznaczająca przekrój
        private static Edge<double>[] GetMinCutFromFlow(Graph<double> originalGraph, DiGraph<double> directedGraph, DiGraph<double> flowGraph, int s)
        {
            int n = originalGraph.VertexCount;
            bool[] inS = new bool[n];
            Queue<int> queue = new Queue<int>();

            inS[s] = true;
            queue.Enqueue(s);

            // Szukamy składowej S
            while (queue.Count > 0)
            {
                int u = queue.Dequeue();

                foreach (var edge in directedGraph.OutEdges(u))
                {
                    int v = edge.To;
                    if (inS[v]) continue;

                    double capacityUV = edge.Weight;

                    double flowUV = 0;
                    foreach (var flowEdge in flowGraph.OutEdges(u))
                    {
                        if (flowEdge.To == v)
                        {
                            flowUV = flowEdge.Weight;
                            break;
                        }
                    }

                    double flowVU = 0;
                    foreach (var flowEdge in flowGraph.OutEdges(v))
                    {
                        if (flowEdge.To == u)
                        {
                            flowVU = flowEdge.Weight;
                            break;
                        }
                    }

                    if (capacityUV - flowUV > 0 || flowVU > 0)
                    {
                        inS[v] = true;
                        queue.Enqueue(v);
                    }
                }
            }

            // Wyznaczamy krawędzie przekroju z oryginalnego grafu
            List<Edge<double>> cutList = new List<Edge<double>>();
            for (int u = 0; u < n; u++)
            {
                if (inS[u])
                {
                    foreach (var edge in originalGraph.OutEdges(u))
                    {
                        if (!inS[edge.To])
                        {
                            cutList.Add(edge);
                        }
                    }
                }
            }

            return cutList.ToArray();
        }

        /// <summary>
        /// Metada liczy spójność krawędziową grafu oraz minimalny zbiór rozcinający.
        /// </summary>
        /// <param name="undirectedGraph">nieskierowany graf</param>
        /// <param name="cutingSet">zbiór krawędzi rozcinających</param>
        /// <returns>spójność krawędziowa</returns>
        public static int EdgeConnectivity(this Graph<double> undirectedGraph, out Edge<double>[] cutingSet)
        {
            // Zmiana na int i int.MaxValue
            int minConnectivity = int.MaxValue;
            cutingSet = null;

            int n = undirectedGraph.VertexCount;

            if (n < 2)
            {
                cutingSet = new Edge<double>[0];
                return 0; // Zwracamy int
            }

            int s = 0;

            for (int t = 1; t < n; t++)
            {
                // Rzutujemy wynik MinCut z double na int
                int currentFlow = (int)undirectedGraph.MinCut(s, t, out Edge<double>[] currentCut);

                if (currentFlow < minConnectivity)
                {
                    minConnectivity = currentFlow;
                    cutingSet = currentCut;
                }
            }

            return minConnectivity;
        }
        
    }
}
