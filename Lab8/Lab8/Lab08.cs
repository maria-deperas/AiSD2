using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ASD2
{
    public class TreasureTrackers : MarshalByRefObject
    {
        /// <summary>
        /// Etap I: Wybór dnia ekspedycji.
        /// Wyznaczenie pierwszego dnia, w którym cała ekspedycja będzie w stanie
        /// przejść przez podziemia.
        /// </summary>
        /// <param name="map">Graf skierowany reprezentujący połączenia pomiędzy komnatami w podziemiach.</param>
        /// <param name="startChamber">Wierzchołek będący wejściem do podziemi.</param>
        /// <param name="endChamber">Wierzchołek będący wyjściem z podziemi.</param>
        /// <param name="durability">Tablica utrzymująca wytrzymałość każdej komnaty.</param>
        /// <param name="opensOn">Tablica informująca, którego dnia otwiera się dana komnata.</param>
        /// <param name="expeditionSize">Rozmiar ekspedycji, chcącej przejść przez podziemia.</param>
        public int? Stage1(DiGraph map, int startChamber, int endChamber, int[] durability, int[] opensOn, int expeditionSize)
        {
            int n = map.VertexCount;
            int l = opensOn.Length;
            int maks = 0;

            for(int i = 0; i < l; i++)
            {
                if (opensOn[i] > maks) maks = opensOn[i];
            }

            int[] openChambers = new int[maks];
            
            for(int i = 0; i <= maks; i++)
            {
                if (opensOn[startChamber] > i || opensOn[endChamber] > i) continue;
                var graph = new DiGraph<int>(n + 1);
                for (int u = 0; u < n; u++)
                {
                    if (opensOn[u] <= i)
                    {
                        if (u == endChamber)
                        {
                            graph.AddEdge(u, n, durability[u]);
                        }
                        else
                        {
                            foreach (var v in map.OutNeighbors(u))
                            {
                                if (opensOn[v] <= i)
                                {
                                    graph.AddEdge(u, v, durability[u]);
                                }
                            }
                        }
                    }
                }
                var (flow, f) = Flows.FordFulkerson(graph, startChamber, n);
                if (flow >= expeditionSize)
                {
                    return i;
                }
            }
            return null;
        }

        public bool IsReachable(DiGraph<int> g, int start, int end)
        {
            foreach(var e in g.DFS().SearchFrom(start))
            {
                if(e.To == end) return true;
            }
            return false;
        }

        /// <summary>
        /// Etap II: 
        /// Wyznaczenie minimalnej liczby poszukiwaczy skarbów,
        /// która będzie w stanie zebrać wszystkie skarby.
        /// </summary>
        /// <param name="map">Acykliczny graf skierowany reprezentujący połączenia pomiędzy komnatami w podziemiach.</param>
        /// <param name="startChamber">Wierzchołek będący wejściem do podziemi.</param>
        /// <param name="endChamber">Wierzchołek będący wyjściem z podziemi.</param>
        /// <param name="durability">Tablica utrzymująca wytrzymałość każdej komnaty.</param>
        public int? Stage2(DiGraph map, int startChamber, int endChamber, int[] durability)
        {
            return null;
        }
    }
}
