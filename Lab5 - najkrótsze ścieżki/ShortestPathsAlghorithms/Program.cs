using System;
using ASD.Graphs;
using ShortestPathsAlghorithms;

class Program
{
    static void Main()
    {
        DiGraph<int> graph = new DiGraph<int>(5);

        graph.AddEdge(0, 1, 10);
        graph.AddEdge(0, 2, 3);
        graph.AddEdge(1, 3, 2);
        graph.AddEdge(2, 1, 4);
        graph.AddEdge(2, 3, 8);
        graph.AddEdge(2, 4, 2);
        graph.AddEdge(3, 4, 5);

        Console.WriteLine("Dijkstra's algorithm:");
        int[] distDijkstra = Dijkstra.dijkstra(graph, 0);

        for (int v = 0; v < distDijkstra.Length; v++)
            Console.WriteLine($"Distance from 0 to {v}: {distDijkstra[v]}");

        Console.WriteLine("\nBellman-Ford algorithm:");
        int[] distBf = BellmanFord.bellmanFord(graph, 0);

        for (int v = 0; v < distBf.Length; v++)
            Console.WriteLine($"Distance from 0 to {v}: {distBf[v]}");

        Console.WriteLine("\nFloyd-Warshall algorithm:");
        int[,] distFW = FloydWarshall.floydWarshall(graph);

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if(distFW[i, j] == int.MaxValue / 2)
                    Console.Write("INF\t");
                else
                    Console.Write($"{distFW[i,j]}\t");
            }
            Console.WriteLine();
        }

        Console.WriteLine("\nJohnson's algorithm:");
        int[,] distJ = Johnson.johnson(graph);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (distJ[i, j] == int.MaxValue)
                    Console.Write("INF\t");
                else
                    Console.Write($"{distJ[i, j]}\t");
            }
            Console.WriteLine();
        }
    }
}
