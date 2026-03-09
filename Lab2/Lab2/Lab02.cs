using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Lab02 : MarshalByRefObject
    {
        /// <summary>
        /// Optymalne rozmieszczenie parasolek w wariancie, w którym każda parasolka ma taki sam promień
        /// oraz mamy do dyspozycji tylko zadaną liczbę parasolek (rozmieszczenie parasolek nie wiąże się z żadnym kosztem)
        /// </summary>
        /// <param name="Z">Tablica zysków, Z[i] to zysk za pokrycie punktu o numerze i</param>
        /// <param name="umbrellaCount">Liczba dostępnych parasolek</param>
        /// <param name="umbrellaRadius">Promień parasolki (parasolka o promieniu r umieszczona w punkcie i pokrywa punkty i-r, i-r+1, ..., i+r)</param>
        /// <returns></returns>
        public (int profit, int[] umbrellaPosition) Stage1(int[] Z, int umbrellaCount, int umbrellaRadius)
        {
            int r = 1 + 2 * umbrellaRadius;
            int n = Z.Length;

            // tablica sum skumulowanych 
            int[] sum = new int[n + 1];
            sum[0] = 0;

            for (int i = 1; i <= n; i++)
            {
                sum[i] = sum[i - 1] + Z[i - 1];
            }

            // tab[i, j] - maksymalny zysk dla i parasolek na pierwszych j punktach
            int[,] tab = new int [umbrellaCount + 1, n + 1];

            // umb[i, j] - czy dla i parasolek i j punktów stawiamy parasolkę kończącą się w punkcie j
            bool[,] umb = new bool[umbrellaCount + 1, n + 1];   

            for (int i = 1; i <= umbrellaCount; i++)
            {
                for(int j = 1;  j <= n; j++)
                {
                    int left = j - r;
                    if (left < 0) left = 0;

                    // przypadek 1: nie stawiamy parasolki kończącej się w j, bierzemy najlepszy wynik dla j-1 punktów
                    tab[i, j] = tab[i, j - 1];

                    int prof = sum[j] - sum[left] + tab[i - 1, left];

                    // przypadek 2: stawiamy parasolkę kończącą się w j, jeśli otrzymamy większy zysk
                    if (prof > tab[i, j])
                    {
                        umb[i, j] = true;
                        tab[i, j] = prof;
                    }

                }
            }

            // lista punktów, w których należy umieścić parasolki
            List<int> umbrellaPosition = new List<int>();
            int cnt = umbrellaCount;
            int m = n;

            while (cnt > 0 && m > 0)
            {
                // jeśli w danym punkcie kończy się parasolka: dodajemy jej środek i przesuwamy m o szerokość parasolki w lewo
                if (umb[cnt, m] == true)
                {
                    int center = m - umbrellaRadius - 1;
                    if (center < 0) center = 0;

                    // dodajemy środek parasolki do listy
                    umbrellaPosition.Add(center);
                    m = m - r;
                    if (m < 0) m = 0;
                    cnt--;
                }
                else
                {
                    m--;
                }
            }
            return (tab[umbrellaCount, n], umbrellaPosition.ToArray());
        }


        /// <summary>
        /// Optymalne rozmieszczenie parasolek w wariancie, w którym mamy dostępne modele parasolek o różnych promieniach.
        /// Każdego modelu możemy użyć dowolną liczbę razy, jednak za każdym razem musimy ponieść jego koszt.
        /// </summary>
        /// <param name="Z">Tablica zysków, Z[i] to zysk za pokrycie punktu o numerze i</param>
        /// <param name="umbrellaType">Tablice dostępnych modeli parasolek, gdzie i-ty model ma promień umbrellaType[i].radius i koszt umbrellaType[i].cost</param>
        /// <returns></returns>
        public (int profit, (int position, int model)[] umbrellas) Stage2(int[] Z, (int radius, int cost)[] umbrellaType)
        {
            int n = Z.Length;
            int m = umbrellaType.Length;

            int[] sum = new int[n + 1];
            sum[0] = 0;

            for (int i = 1; i <= n; i++)
            {
                sum[i] = sum[i - 1] + Z[i - 1];
            }

            int[] tab = new int[n + 1];
            int[] model = new int[n + 1];
            int[] idx = new int[n + 1];

            for(int i = 1; i <= n; i++)
            {
                tab[i] = tab[i - 1];
                model[i] = -1;
                idx[i] = i - 1;

                for(int j = 0; j < m; j++)
                {
                    int w = 1 + 2 * umbrellaType[j].radius;
                    int left = i - w;
                    if (left < 0) left = 0;

                    int profit = sum[i] - sum[left] - umbrellaType[j].cost + tab[left];

                    if(profit > tab[i])
                    {
                        tab[i] = profit;
                        model[i] = j;
                        idx[i] = left;
                    }
                }
            }

            List<(int position, int model)> result = new List<(int position, int model)>();

            int pos = n;
            while (pos > 0)
            {
                int currmodel = model[pos];
                if (currmodel != -1)
                {
                    int position = pos - umbrellaType[currmodel].radius - 1;
                    if(position < 0) position = 0;
                    result.Add((position, currmodel));
                    pos = idx[pos];
                }
                else
                {
                    pos--;
                }
            }

            return (tab[n], result.ToArray());
        }
    }
}
