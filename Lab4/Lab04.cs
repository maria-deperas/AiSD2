using System;
using ASD.Graphs;
using ASD;
using System.Collections.Generic;
using System.Collections;

namespace ASD
{

    public class Lab04 : System.MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - szukanie trasy z miasta start_v do miasta end_v, startując w dniu day
        /// </summary>
        /// <param name="g">Ważony graf skierowany będący mapą</param>
        /// <param name="start_v">Indeks wierzchołka odpowiadającego miastu startowemu</param>
        /// <param name="end_v">Indeks wierzchołka odpowiadającego miastu docelowemu</param>
        /// <param name="day">Dzień startu (w tym dniu należy wyruszyć z miasta startowego)</param>
        /// <param name="days_number">Liczba dni uwzględnionych w rozkładzie (tzn. wagi krawędzi są z przedziału [0, days_number-1])</param>
        /// <returns>(result, route) - result ma wartość true gdy podróż jest możliwa, wpp. false, 
        /// route to tablica z indeksami kolejno odwiedzanych miast (pierwszy indeks to indeks miasta startowego, ostatni to indeks miasta docelowego),
        /// jeżeli result == false to route ustawiamy na null</returns>
        public (bool result, int[] route) Lab04_FindRoute(DiGraph<int> g, int start_v, int end_v, int day, int days_number)
        {
            int n = g.VertexCount;

            // graf pomocniczy
            DiGraph graph = new DiGraph(days_number * n);
            List<int> route = new List<int>();

            // krawędź u -> v o wadze m w grafie g odpowiada krawędzi (u, m) -> (v, m + 1) w grafie graph
            // w graph wierzchołek (u, d) odpowiada wierchołkowi u + (d * n)
            for(int u = 0; u < n; u++)
            {
                foreach (Edge<int> e in g.OutEdges(u))
                {
                    int v = e.To;
                    int weight = e.Weight;
                    graph.AddEdge(u + (weight * n), v + ((weight + 1) % days_number) * n);
                }
            }

            // tablica poprzedników - przechowuje z jakiego wierchołka przyszliśmy
            int[] parent = new int[days_number * n];
            // parent[i] = -1, jeśli jeszcze nie odwiedziliśmy danego wierzchołka
            for (int i = 0; i < parent.Length; i++)
            {
                parent[i] = -1;
            }

            int last = -1;

            // DFS zaczynając od wierzchołka startowego (z odpowiednim dniem)
            // 
            foreach (Edge e in graph.DFS().SearchFrom(start_v + (day * n)))
            {
                // jeśli nie odwiedziliśmy jeszcze danego wierzchołka, zapisujemy jego poprzednika
                if (parent[e.To] == -1)
                {
                    parent[e.To] = e.From;
                }
                // kończymy, gdy trafimy do końcowego miasta (w dowolnym dniu)
                if (e.To % n == end_v) {
                    last = e.To;
                    break;
                }
            }

            // jeśli dotarliśmy do docelowego wierzchołka - odtwarzamy trasę od tyłu
            if (last != -1)
            {
                int curr = last;

                while (curr != -1)
                {
                    // dodajemy do trasy wierzchołek z pierwotnego grafu
                    route.Add(curr % n);
                    // jeśli wróciliśmy do wierzchołka startowego - koniec
                    if (curr == start_v + (day * n)) break;
                    // cofamy się do poprzednika
                    curr = parent[curr];
                }

                route.Reverse();
                return (true, route.ToArray());
            }

            return (false, null);
        }

        /// <summary>
        /// Etap 2 - szukanie trasy z jednego z miast z tablicy start_v do jednego z miast z tablicy end_v (startować można w dowolnym dniu)
        /// </summary>
        /// <param name="g">Ważony graf skierowany będący mapą</param>
        /// <param name="start_v">Tablica z indeksami wierzchołków startowych (trasę trzeba zacząć w jednym z nich)</param>
        /// <param name="end_v">Tablica z indeksami wierzchołków docelowych (trasę trzeba zakończyć w jednym z nich)</param>
        /// <param name="days_number">Liczba dni uwzględnionych w rozkładzie (tzn. wagi krawędzi są z przedziału [0, days_number-1])</param>
        /// <returns>(result, route) - result ma wartość true gdy podróż jest możliwa, wpp. false, 
        /// route to tablica z indeksami kolejno odwiedzanych miast (pierwszy indeks to indeks miasta startowego, ostatni to indeks miasta docelowego),
        /// jeżeli result == false to route ustawiamy na null</returns>
        public (bool result, int[] route) Lab04_FindRouteSets(DiGraph<int> g, int[] start_v, int[] end_v, int days_number)
        {
            int n = g.VertexCount;
            int start = days_number * n;
            int end = days_number * n + 1;
            DiGraph graph = new DiGraph(days_number * n + 2);
            List<int> route = new List<int>();

            // graf pomocniczy budujemy analogicznie do etapu 1, ale dodajemy dodatkowe 2 wierzchołki - startowy i końcowy
            // wierzchołek startowy łączy się krawędziami ze wszystkimi wierzchołkami odpowiadającymi wierzchołkom z start_v
            // analogicznie wierzchołek końcowy
            for (int u = 0; u < n; u++)
            {
                foreach (Edge<int> e in g.OutEdges(u))
                {
                    int v = e.To;
                    int weight = e.Weight;
                    graph.AddEdge(u + (weight * n), v + ((weight + 1) % days_number) * n);
                }
            }

            foreach (int v in start_v)
            {
                for (int i = 0; i < days_number; i++)
                {
                    graph.AddEdge(start, v + (i * n));
                }
            }

            foreach (int v in end_v)
            {
                for (int i = 0; i < days_number; i++)
                {
                    graph.AddEdge(v + (i * n), end);
                }
            }

            int[] parent = new int[days_number * n + 2];
            for (int i = 0; i < parent.Length; i++)
            {
                parent[i] = -1;
            }

            int last = -1;

            // szukamy ścieżki od start do end - od dowolnego wierzchołka ze start_v do dowolnego z end_v
            foreach (Edge e in graph.DFS().SearchFrom(start))
            {
                if (parent[e.To] == -1)
                {
                    parent[e.To] = e.From;
                }
                if (e.To == end)
                {
                    last = e.To;
                    break;
                }
            }

            // odczytujemy ścieżkę od tyłu, pomijamy wierzchołek start i end
            if (last != -1)
            {
                int curr = parent[last];

                while (curr != -1)
                {
                    if (curr == start) break;
                    route.Add(curr % n);
                    curr = parent[curr];
                }

                route.Reverse();
                return (true, route.ToArray());
            }

            return (false, null);
        }
    }
}
