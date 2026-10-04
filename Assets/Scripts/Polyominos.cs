using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Polyominos
{
    private const int BoardSize = 8;

    // Hình gốc + trọng số (đã gộp mọi hướng xoay/gương). Hàng cách nhau bằng '/'
    private static readonly (string pattern, float weight)[] baseShapes =
    {
        ("1", 1f), ("11", 2f), ("111", 3f), ("1111", 3f),
        ("11/11", 3f), ("111/111", 2f), ("111/111/111", 1f),
        ("10/11", 3f), ("10/10/11", 3f), ("100/100/111", 2f),
        ("111/010", 3f), ("011/110", 3f),
    };

    private static readonly List<int[,]> polyominos = new List<int[,]>();
    private static readonly List<float> weights = new List<float>();
    private static readonly float totalWeight;

    static Polyominos()
    {
        foreach (var (pattern, weight) in baseShapes)
        {
            var shape = Parse(pattern);

            if (Mathf.Max(shape.GetLength(0), shape.GetLength(1)) > BoardSize)
            {
                Debug.LogWarning($"Khối '{pattern}' lớn hơn board {BoardSize}x{BoardSize}, đã bỏ qua.");
                continue;
            }

            // 8 hướng (4 xoay x 2 gương), khóa chuỗi giúp tự loại bản trùng
            var variants = new Dictionary<string, int[,]>();
            for (var m = 0; m < 2; m++, shape = Mirror(shape))
                for (var i = 0; i < 4; i++, shape = Rotate(shape))
                    variants[Key(shape)] = shape;

            foreach (var v in variants.Values)
            {
                polyominos.Add(v);
                weights.Add(weight / variants.Count); // chia đều trọng số cho các hướng
            }
        }

        totalWeight = weights.Sum();
    }

    public static int[,] Get(int index) => polyominos[index];

    public static int Length => polyominos.Count;

    // Bốc ngẫu nhiên 1 index theo trọng số
    public static int RandomIndex()
    {
        var roll = Random.value * totalWeight;

        for (var i = 0; i < weights.Count; i++)
            if ((roll -= weights[i]) <= 0f)
                return i;

        return weights.Count - 1;
    }

    private static int[,] Build(int h, int w, System.Func<int, int, int> cell)
    {
        var result = new int[h, w];
        for (var r = 0; r < h; r++)
            for (var c = 0; c < w; c++)
                result[r, c] = cell(r, c);
        return result;
    }

    private static int[,] Parse(string p)
    {
        var rows = p.Split('/');
        return Build(rows.Length, rows[0].Length, (r, c) => rows[r][c] == '1' ? 1 : 0);
    }

    // Xoay 90° theo chiều kim đồng hồ
    private static int[,] Rotate(int[,] s)
    {
        var h = s.GetLength(0);
        return Build(s.GetLength(1), h, (r, c) => s[h - 1 - c, r]);
    }

    // Lật ngang
    private static int[,] Mirror(int[,] s)
    {
        var w = s.GetLength(1);
        return Build(s.GetLength(0), w, (r, c) => s[r, w - 1 - c]);
    }

    private static string Key(int[,] s) =>
        s.GetLength(0) + ":" + string.Concat(s.Cast<int>());
}