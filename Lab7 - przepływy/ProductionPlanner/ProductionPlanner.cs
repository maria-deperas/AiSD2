using System;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class ProductionPlanner : MarshalByRefObject
    {
        /// <summary>
        /// Flaga pozwalająca na włączenie wypisywania szczegółów skonstruowanego planu na konsolę.
        /// Wartość <code>true</code> spoeoduje wypisanie planu.
        /// </summary>
        public bool ShowDebug { get; } = false;

        // Pomocnicza metoda do odczytania przepływu między dwoma wierzchołkami
        private int GetFlow(DiGraph<int> flowGraph, int u, int v)
        {
            if (flowGraph == null) return 0;

            foreach (var edge in flowGraph.OutEdges(u))
            {
                if (edge.To == v) return edge.Weight;
            }
            return 0;
        }

        /// <summary>
        /// Część 1. zadania - zaplanowanie produkcji telewizorów dla pojedynczego kontrahenta.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających maksymalną produkcję i zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się maksymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateSimplePlan(PlanData[] production, PlanData[] sales, PlanData storageInfo,
            out SimpleWeeklyPlan[] weeklyPlan)
        {
            if (production == null || sales == null) throw new ArgumentException();

            int weeks = production.Length;
            if (weeks <= 0 || sales.Length != weeks) throw new ArgumentException();
            if (storageInfo.Quantity < 0 || storageInfo.Value < 0) throw new ArgumentException();

            for (int i = 0; i < weeks; i++)
            {
                if (production[i].Quantity < 0 || production[i].Value < 0) throw new ArgumentException();
                if (sales[i].Quantity < 0 || sales[i].Value < 0) throw new ArgumentException();
            }

            // Tworzymy graf z kosztami
            int s = 0; 
            int t = 1; 
            int nodeCount = weeks + 2;
            var G = new NetworkWithCosts<int, double>(nodeCount);

            for (int i = 0; i < weeks; i++)
            {
                int weekNode = i + 2;

                // Krawędź ze źródła - produkcja w danym tygodniu
                G.AddEdge(s, weekNode, production[i].Quantity, production[i].Value);

                // Krawędź do ujścia - sprzedaż w danym tygodniu (ujemny koszt jako zysk)
                G.AddEdge(weekNode, t, sales[i].Quantity, -sales[i].Value);

                // Krawędź magazynowania - przejście do następnego tygodnia
                if (i < weeks - 1)
                {
                    G.AddEdge(weekNode, weekNode + 1, storageInfo.Quantity, storageInfo.Value);
                }
            }

            // Wyznaczenie min-cost max-flow
            var (flowValue, flowCost, flowGraph) = Flows.MinCostMaxFlow(G, s, t);

            // Odtworzenie planu z przepływu
            weeklyPlan = new SimpleWeeklyPlan[weeks];
            for (int i = 0; i < weeks; i++)
            {
                int weekNode = i + 2;
                int nextWeekNode = i + 3;

                weeklyPlan[i] = new SimpleWeeklyPlan
                {
                    UnitsProduced = GetFlow(flowGraph, s, weekNode),
                    UnitsSold = GetFlow(flowGraph, weekNode, t),
                    UnitsStored = (i < weeks - 1) ? GetFlow(flowGraph, weekNode, nextWeekNode) : 0
                };
            }

            // Zwracamy wyznaczony plan
            return new PlanData
            {
                Quantity = flowValue,
                Value = -flowCost
            };
        }

        /// <summary>
        /// Część 2. zadania - zaplanowanie produkcji telewizorów dla wielu kontrahentów.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających produkcję dającą maksymalny zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Dwuwymiarowa tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Pierwszy wymiar tablicy jest równy liczbie kontrahentów, zaś drugi - liczbie tygodni w planie.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// Każdy wiersz tablicy odpowiada jednemu kontrachentowi.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się optymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateComplexPlan(PlanData[] production, PlanData[,] sales, PlanData storageInfo,
            out WeeklyPlan[] weeklyPlan)
        {
            if (production == null || sales == null) throw new ArgumentException();

            int weeks = production.Length;
            int customers = sales.GetLength(0);

            if (weeks <= 0 || customers <= 0) throw new ArgumentException();
            if (sales.GetLength(1) != weeks) throw new ArgumentException();
            if (storageInfo.Quantity < 0 || storageInfo.Value < 0) throw new ArgumentException();

            for (int i = 0; i < weeks; i++)
            {
                if (production[i].Quantity < 0 || production[i].Value < 0) throw new ArgumentException();

                for (int j = 0; j < customers; j++)
                {
                    if (sales[j, i].Quantity < 0 || sales[j, i].Value < 0) throw new ArgumentException();
                }
            }

            // Tworzenie grafu z kosztami
            int s = 0;
            int t = 1;

            int nodeCount = 2 + (2 * weeks) + (weeks * customers);
            var G = new NetworkWithCosts<int, double>(nodeCount);

            for (int i = 0; i < weeks; i++)
            {
                int pNode = 2 + i; 
                int wNode = 2 + weeks + i; 

                G.AddEdge(s, pNode, production[i].Quantity, 0);

                // Produkujemy (płacimy za koszt produkcji i towar trafia do fabryki)
                G.AddEdge(pNode, wNode, production[i].Quantity, production[i].Value);

                // Ignorujemy (towar "wyparowuje" ze statusem kosztu 0, obchodzimy system)
                G.AddEdge(pNode, t, production[i].Quantity, 0);

                // Magazyn: przesyłamy niesprzedany towar do fabryki w następnym tygodniu
                if (i < weeks - 1)
                {
                    int nextWNode = 2 + weeks + i + 1;
                    G.AddEdge(wNode, nextWNode, storageInfo.Quantity, storageInfo.Value);
                }

                // Sprzedaż do poszczególnych klientów
                for (int j = 0; j < customers; j++)
                {
                    int cNode = 2 + (2 * weeks) + (i * customers) + j;

                    // Od fabryki do klienta -> przepustowość to popyt klienta, zysk to ujemny koszt
                    G.AddEdge(wNode, cNode, sales[j, i].Quantity, -sales[j, i].Value);

                    // Od klienta do ujścia -> zamykamy przepływ
                    G.AddEdge(cNode, t, sales[j, i].Quantity, 0);
                }
            }

            // Wyznaczenie min-cost max-flow 
            var (_, flowCost, flowGraph) = Flows.MinCostMaxFlow(G, s, t);

            // Odtworzenie planu z przepływu 
            weeklyPlan = new WeeklyPlan[weeks];
            int actualTotalProduced = 0;

            for (int i = 0; i < weeks; i++)
            {
                int pNode = 2 + i;
                int wNode = 2 + weeks + i;
                int nextWNode = 2 + weeks + i + 1;

                int produced = GetFlow(flowGraph, pNode, wNode);
                actualTotalProduced += produced;

                int[] unitsSold = new int[customers];
                for (int j = 0; j < customers; j++)
                {
                    int cNode = 2 + (2 * weeks) + (i * customers) + j;
                    unitsSold[j] = GetFlow(flowGraph, wNode, cNode);
                }

                weeklyPlan[i] = new WeeklyPlan
                {
                    UnitsProduced = produced,
                    UnitsStored = (i < weeks - 1) ? GetFlow(flowGraph, wNode, nextWNode) : 0,
                    UnitsSold = unitsSold
                };
            }

            return new PlanData
            {
                Quantity = actualTotalProduced,
                Value = -flowCost
            };
        }
    }
}