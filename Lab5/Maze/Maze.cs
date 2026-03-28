using ASD.Graphs;
using System;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Maze : MarshalByRefObject
    {

        /// <summary>
        /// Wersje zadania I oraz II
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt bez dynamitów lub z dowolną ich liczbą
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="withDynamite">informacja, czy dostępne są dynamity 
        /// Wersja I zadania -> withDynamites = false, Wersja II zadania -> withDynamites = true</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany (dotyczy tylko wersji II)</param> 
        public int FindShortestPath(char[,] maze, bool withDynamite, out string path, int t = 0)
        {
            path = "";
            int rows = maze.GetLength(0);
            int cols = maze.GetLength(1);
            int vertexCount = rows * cols;

            DiGraph<int> graph = new DiGraph<int>(vertexCount, new ListGraphRepresentation());

            int start = -1;
            int end = -1;

            (int dr, int dc)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (maze[r, c] == 'X' && !withDynamite) continue;

                    int u = r * cols + c;

                    if (maze[r, c] == 'S') start = u;
                    if (maze[r, c] == 'E') end = u;

                    foreach(var dir in directions)
                    {
                        int nr = r + dir.dr;
                        int nc = c + dir.dc;
                        int v = nr * cols + nc;

                        if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                        {
                            if (!withDynamite && maze[nr, nc] == 'X') continue;

                            int weight;

                            if (maze[nr, nc] == 'X')
                            {
                                weight = t;
                            }
                            else
                            {
                                weight = 1;
                            }

                            graph.AddEdge(u, v, weight);
                        }
                    }
                }
            }

            if (start == -1 || end == -1) return -1;

            PathsInfo<int> pathsInfo = Paths.Dijkstra<int>(graph, start);

            if (pathsInfo.Reachable(start, end))
            {
                int distance = pathsInfo.GetDistance(start, end);
                int[] pathVertices = pathsInfo.GetPath(start, end);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < pathVertices.Length - 1; i++)
                {
                    int curr = pathVertices[i];
                    int next = pathVertices[i + 1];

                    int y1 = curr / cols;
                    int x1 = curr % cols;

                    int y2 = next / cols;
                    int x2 = next % cols;

                    if (y2 == y1 - 1) sb.Append('N');
                    else if (y2 == y1 + 1) sb.Append('S');
                    else if (x2 == x1 + 1) sb.Append('E');
                    else if (x2 == x1 - 1) sb.Append('W');
                }
                path = sb.ToString();

                return distance;
            }
            return -1;
        }

        /// <summary>
        /// Wersja III i IV zadania
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt z użyciem co najwyżej k lasek dynamitu
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="k">liczba dostępnych lasek dynamitu, dla wersji III k=1</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany</param>
        public int FindShortestPathWithKDynamites(char[,] maze, int k, out string path, int t)
        {
            path = "";
            int rows = maze.GetLength(0);
            int cols = maze.GetLength(1);
            int n = rows * cols;

            int vertexCount = (k + 1) * n + 1;
            DiGraph<int> graph = new DiGraph<int>(vertexCount, new ListGraphRepresentation());

            int end_v = vertexCount - 1; // wierzchołek końcowy łączymy go ze wszystkimi ends w każdej warstwie grafu

            int start = -1;

            // możliwe kierunki ruchu: góra, dół, lewo, prawo
            (int dr, int dc)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };

            // przechodzimy przez każdą komórkę labiryntu i dodajemy odpowiednie krawędzie do grafu
            for(int i = 0; i < k + 1; i++)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int u = r * cols + c + (i * n);

                        // start i koniec labiryntu
                        if (maze[r, c] == 'S' && i == 0) start = u;
                        if (maze[r, c] == 'E')
                        {
                            graph.AddEdge(u, end_v, 0);
                        }

                        // dla każdej komórki dodajemy krawędzie do sąsiednich komórek
                        foreach (var dir in directions)
                        {
                            int nr = r + dir.dr;
                            int nc = c + dir.dc;
                            if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                            {
                                int v = nr * cols + nc + (i * n);

                                // jeśli ściana dodajemy krawędź do kolejnej warstwy grafu
                                if (maze[nr, nc] == 'X')
                                {
                                    if (i < k)
                                    {
                                        graph.AddEdge(u, v + n, t);
                                    }
                                }
                                else
                                {
                                    graph.AddEdge(u, v, 1);
                                }
                            }
                        }
                    }
                }

            }

            if (start == -1) return -1;

            PathsInfo<int> pathsInfo = Paths.Dijkstra<int>(graph, start);

            if (pathsInfo.Reachable(start, end_v))
            {
                int distance = pathsInfo.GetDistance(start, end_v);
                int[] pathVertices = pathsInfo.GetPath(start, end_v);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < pathVertices.Length - 2; i++)
                {
                    // znajdujemy odpowiadający wierzchołek w warstwie 0
                    int curr = pathVertices[i] % n;
                    int next = pathVertices[i + 1] % n;

                    int y1 = curr / cols;
                    int x1 = curr % cols;

                    int y2 = next / cols;
                    int x2 = next % cols;

                    if (y2 == y1 - 1) sb.Append('N');
                    else if (y2 == y1 + 1) sb.Append('S');
                    else if (x2 == x1 + 1) sb.Append('E');
                    else if (x2 == x1 - 1) sb.Append('W');
                }
                path = sb.ToString();

                return distance;
            }
            return -1;
        }
    }
}