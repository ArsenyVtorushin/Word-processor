using System;
using System.Collections.Generic;
using System.Text;

public class Program
{
    static void Main()
    {
        string? firstLine = Console.ReadLine();
        if (firstLine == null)
            return;

        if (!int.TryParse(firstLine.Trim(), out int K))
            return;



        var wordsInText = new List<string>();
        while (true)
        {
            string? line = Console.ReadLine();
            if (line == null || string.IsNullOrWhiteSpace(line))
                break;

            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                string norm = Normalize(token);
                if (norm.Length > 0)
                    wordsInText.Add(norm);
            }
        }

        int totalWords = wordsInText.Count;
        if (totalWords == 0)
            return;


        // НАзначаем id словам с длиной >= 2
        var wordToId = new Dictionary<string, int>();
        var idToWord = new List<string>();
        var occurrenceId = new int[totalWords]; // -1 если слово длиной 1
        for (int i = 0; i < totalWords; i++)
        {
            string w = wordsInText[i];
            if (w.Length <= 1)
            {
                occurrenceId[i] = -1;
                continue;
            }

            if (!wordToId.TryGetValue(w, out int id))
            {
                id = idToWord.Count;
                wordToId[w] = id;
                idToWord.Add(w);
            }

            occurrenceId[i] = id;
        }

        int n = idToWord.Count;
        if (n == 0) // все слова длины 1
        {
            return;
        }





        var parent = new int[n];
        var rank = new int[n];
        for (int i = 0; i < n; i++)
            parent[i] = i;

        int Find(int x)
        {
            if (parent[x] != x)
                parent[x] = Find(parent[x]);
            return parent[x];
        }

        void Union(int a, int b)
        {
            int ra = Find(a);
            int rb = Find(b);
            if (ra == rb) return;

            if (rank[ra] < rank[rb])
                parent[ra] = rb;
            else if (rank[ra] > rank[rb])
                parent[rb] = ra;
            else
            {
                parent[rb] = ra;
                rank[ra]++;
            }
        }


        // по 1му правилу:
        var patternToIndex = new Dictionary<string, int>();
        var charBuffer = new StringBuilder();

        for (int i = 0; i < n; i++)
        {
            string w = idToWord[i];
            int len = w.Length;
            if (len < 2)
                continue;

            char[] chars = w.ToCharArray();
            for (int pos = 0; pos < len; pos++)
            {
                char saved = chars[pos];
                chars[pos] = '*';
                string pattern = new string(chars);
                chars[pos] = saved;

                if (patternToIndex.TryGetValue(pattern, out int j))
                {
                    Union(i, j);
                }
                else
                {
                    patternToIndex[pattern] = i;
                }
            }
        }


        //по 2му правилу:
        for (int i = 0; i < n; i++)
        {
            string w = idToWord[i];
            int len = w.Length;
            if (len < 2)
                continue;


            string cand1 = w + 'e';
            if (wordToId.TryGetValue(cand1, out int id1))
                Union(i, id1);

            string cand2 = w + 's';
            if (wordToId.TryGetValue(cand2, out int id2))
                Union(i, id2);



            if (len >= 3)
            {
                char last = w[len - 1];
                if (last == 'e' || last == 's')
                {
                    string shorter = w.Substring(0, len - 1);
                    if (shorter.Length >= 2 && wordToId.TryGetValue(shorter, out int idShort))
                        Union(i, idShort);
                }
            }
        }

        //находим корень для каждого id и минимальное слово
        var rootOfId = new int[n];
        for (int i = 0; i < n; i++)
            rootOfId[i] = Find(i);

        var rootToRep = new Dictionary<int, string>();

        for (int i = 0; i < n; i++)
        {
            int r = rootOfId[i];
            string w = idToWord[i];
            if (!rootToRep.TryGetValue(r, out string? cur) || string.CompareOrdinal(w, cur) < 0)
            {
                rootToRep[r] = w;
            }
        }

        // контекстная частота
        var freq = new Dictionary<int, long>();

        for (int pos = 0; pos < totalWords; pos++)
        {
            int id = occurrenceId[pos];
            if (id == -1) // длины 1 - отбрасываем
                continue;

            int root = rootOfId[id];
            bool hasNeighborInGroup = false;

            int left = Math.Max(0, pos - K);
            int right = Math.Min(totalWords - 1, pos + K);

            for (int j = left; j <= right; j++)
            {
                if (j == pos) continue;

                int neighborId = occurrenceId[j];
                if (neighborId == -1)
                    continue;

                if (rootOfId[neighborId] == root)
                {
                    hasNeighborInGroup = true;
                    break;
                }
            }

            if (hasNeighborInGroup)
            {
                if (freq.TryGetValue(root, out long f))
                    freq[root] = f + 1;
                else
                    freq[root] = 1;
            }
        }



        var groups = new List<(string rep, long cnt)>();
        foreach (var kv in freq)
        {
            int root = kv.Key;
            long count = kv.Value;
            if (!rootToRep.TryGetValue(root, out string? rep))
                continue;
            groups.Add((rep, count));
        }

        if (groups.Count == 0)
            return;


        //сортировка
        groups.Sort((a, b) =>
        {
            int cmp = b.cnt.CompareTo(a.cnt);
            if (cmp != 0) return cmp;
            return string.CompareOrdinal(a.rep, b.rep);
        });

        //вывод
        foreach (var g in groups)
        {
            Console.WriteLine($"{g.rep}: {g.cnt}");
        }
    }

    private static string Normalize(string token)
    {
        if (string.IsNullOrEmpty(token))
            return string.Empty;

        var sb = new StringBuilder(token.Length);
        foreach (char c in token)
        {
            char lower = char.ToLowerInvariant(c);
            if ((lower >= 'a' && lower <= 'z') || lower == '\'')
            {
                sb.Append(lower);
            }
        }
        return sb.ToString();
    }

}










