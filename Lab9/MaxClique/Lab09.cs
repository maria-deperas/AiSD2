
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;

/// <summary>
/// Klasa rozszerzająca klasę Graph o rozwiązania problemów największej kliki i izomorfizmu grafów metodą pełnego przeglądu (backtracking)
/// </summary>
public static class Lab10GraphExtender
{
    /// <summary>
    /// Wyznacza największą klikę w grafie i jej rozmiar metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Badany graf</param>
    /// <param name="clique">Wierzchołki znalezionej największej kliki - parametr wyjściowy</param>
    /// <returns>Rozmiar największej kliki</returns>
    /// <remarks>
    /// Nie wolno modyfikować badanego grafu.
    /// </remarks>
    public static int MaxClique(this Graph g, out int[] clique)
    {
        List<int> bestS = new List<int>();
        List<int> S = new List<int>();

        int n = g.VertexCount;

        void MaxCliqueRec(int k)
        {
            // Budujemy C - zbiór wierzchołków z zakresu [k, n-1], które są połączone krawędzią z każdym wierzchołkiem ze zbioru S
            List<int> C = new List<int>();
            for (int i = k; i < n; i++)
            {
                bool isConnectedToAll = true;
                foreach (int v in S)
                {
                    if (!g.HasEdge(i, v))
                    {
                        isConnectedToAll = false;
                        break; // skoro nie łączy się z jednym z wierzchołków, to i tak nie będzie kliki, więc możemy przerwać sprawdzanie dla tego wierzchołka i przejść do następnego kandydata
                    }
                }
                if (isConnectedToAll)
                {
                    C.Add(i); // i jest połączony z każdym wierzchołkiem ze zbioru S, więc może być kandydatem do dodania do kliki
                }
            }

            // Sprawdzamy, czy jest sens rozpatrywać zbiór C
            if (S.Count + C.Count <= bestS.Count)
            {
                return;
            }

            if (S.Count > bestS.Count)
            {
                // Znaleźliśmy większą klikę, więc aktualizujemy najlepszy wynik
                bestS = new List<int>(S);
            }

            foreach (int m in C)
            {
                S.Add(m);
                MaxCliqueRec(m + 1); // rekurencyjnie sprawdzamy, czy dodanie m do zbioru S pozwala znaleźć większą klikę, ale już bez rozpatrywania wierzchołków o indeksach m i niższych (bo już je rozpatrzyliśmy w poprzednich krokach rekurencji)
                S.RemoveAt(S.Count - 1); // Backtracking - usuwamy m z S, żeby sprawdzić kolejne kandydatury z C, które mogą być dodane do S i pozwolić znaleźć większą klikę
            }
        }

        // uruchamiamy rekurencję od pierwszego wierzchołka
        MaxCliqueRec(0);

        clique = bestS.ToArray();
        return bestS.Count;
    }

    /// <summary>
    /// Bada izomorfizm grafów metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Pierwszy badany graf</param>
    /// <param name="h">Drugi badany graf</param>
    /// <param name="map">Mapowanie wierzchołków grafu h na wierzchołki grafu g (jeśli grafy nie są izomorficzne to null) - parametr wyjściowy</param>
    /// <returns>Informacja, czy grafy g i h są izomorficzne</returns>
    /// <remarks>
    /// 1) Uwzględniamy wagi krawędzi
    /// 3) Nie wolno modyfikować badanych grafów.
    /// </remarks>
    public static bool IsomorphismTest(this Graph<int> g, Graph<int> h, out int[] map)
    {
        map = null;
        if (g.VertexCount != h.VertexCount) return false;
        if (g.EdgeCount != h.EdgeCount) return false;

        // Zakładam, że masz właściwość zwracającą liczbę wierzchołków
        int n = g.VertexCount;

        // 1. Szybkie odcięcie: grafy o różnej liczbie wierzchołków nie mogą być izomorficzne
        if (n != h.VertexCount)
        {
            return false;
        }

        // Edge case: grafy puste
        if (n == 0)
        {
            map = new int[0];
            return true;
        }

        // Zmienne stanu do nawrotów
        int[] gToH = new int[n];   // gToH[u] = v oznacza, że wierzchołek 'u' z G mapuje na 'v' z H
        bool[] usedH = new bool[n]; // Flagi blokujące użycie dwa razy tego samego wierzchołka z H

        // FUNKCJA LOKALNA (Rekurencja)
        bool IsomorphismRec(int u)
        {
            // Warunek stopu: przypisaliśmy poprawnie wszystkie n wierzchołków (Sukces!)
            if (u == n)
            {
                return true;
            }

            // Pętla po wierzchołkach z grafu H
            for (int v = 0; v < n; v++)
            {
                if (usedH[v]) continue; // Pomijamy użyte wierzchołki

                // --- PRUNING (Odcinanie gałęzi) ---
                // Opcjonalna optymalizacja: Możesz tu najpierw sprawdzić stopnie wierzchołków
                // if (g.Degree(u) != h.Degree(v)) continue;

                bool isValid = true;

                // Sprawdzamy krawędzie POMIĘDZY naszym aktualnym kandydatem 'u', 
                // a WSZYSTKIMI wcześniej przypisanymi wierzchołkami (indeksy od 0 do u-1)
                for (int i = 0; i < u; i++)
                {
                    int mappedI = gToH[i]; // Jakiemu wierzchołkowi w H odpowiada wierzchołek i w G?

                    // Test kierunku 1: od 'i' do 'u' (oraz ich odpowiedników w H)
                    // TUTAJ ZMIEŃ: Użyj odpowiednich metod Twojej klasy grafu
                    bool hasEdgeG1 = g.HasEdge(i, u);
                    bool hasEdgeH1 = h.HasEdge(mappedI, v);

                    if (hasEdgeG1 != hasEdgeH1)
                    {
                        isValid = false; break;
                    }

                    // UWAGA Z ZADANIA: Sprawdzamy wagi krawędzi
                    if (hasEdgeG1 && g.GetEdgeWeight(i, u) != h.GetEdgeWeight(mappedI, v))
                    {
                        isValid = false; break;
                    }

                    // Test kierunku 2: od 'u' do 'i' (Wymagane, jeśli graf jest skierowany)
                    bool hasEdgeG2 = g.HasEdge(u, i);
                    bool hasEdgeH2 = h.HasEdge(v, mappedI);

                    if (hasEdgeG2 != hasEdgeH2)
                    {
                        isValid = false; break;
                    }

                    if (hasEdgeG2 && g.GetEdgeWeight(u, i) != h.GetEdgeWeight(v, mappedI))
                    {
                        isValid = false; break;
                    }
                }

                // Jeśli krawędzie lub wagi się nie zgadzają, ucinamy tę gałąź i bierzemy kolejne 'v'
                if (!isValid) continue;

                // --- KROK W PRZÓD ---
                usedH[v] = true;
                gToH[u] = v;

                // Wchodzimy poziom głębiej
                if (IsomorphismRec(u + 1))
                {
                    return true; // Jeśli wewnątrz znaleziono kompletne rozwiązanie, "przekazujemy" true do góry
                }

                // --- NAWRÓT (BACKTRACKING) ---
                // Rozwiązanie w tej gałęzi nie wypaliło, odblokowujemy wierzchołek
                usedH[v] = false;
            }

            // Przejrzeliśmy wszystkie opcje dla 'v' na poziomie 'u' i nic nie pasowało
            return false;
        }

        // URUCHOMIENIE ALGORYTMU
        if (IsomorphismRec(0))
        {
            // Budowanie wyniku zgodnego z dokumentacją.
            // Komentarz XML mówi: "Mapowanie wierzchołków grafu h na wierzchołki grafu g"
            // Czyli: map[v_z_H] = u_z_G
            map = new int[n];
            for (int u = 0; u < n; u++)
            {
                int v = gToH[u];
                map[v] = u;
            }
            return true;
        }

        // Algorytm przejrzał wszystkie permutacje, nie znalazł izomorfizmu
        return false;
    }

}

