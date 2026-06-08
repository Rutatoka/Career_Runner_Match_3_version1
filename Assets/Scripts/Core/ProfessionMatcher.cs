using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfessionMatcher
{
    private readonly List<ProfessionData> professions;
    private readonly List<float[]> professionNorms;
    private readonly int vectorLength;

    public ProfessionMatcher(IEnumerable<ProfessionData> professions)
    {
        if (professions == null) throw new ArgumentNullException(nameof(professions));

        this.professions = professions.Where(p => p != null).ToList();
        if (this.professions.Count == 0)
            throw new ArgumentException("Profession list is empty", nameof(professions));

        vectorLength = this.professions[0].GetVectorCopy().Length;

        professionNorms = new List<float[]>(this.professions.Count);
        foreach (var p in this.professions)
        {
            var v = p.GetVectorCopy();
            if (v.Length != vectorLength)
            {
                var resized = new float[vectorLength];
                for (int i = 0; i < Math.Min(v.Length, vectorLength); i++) resized[i] = v[i];
                v = resized;
            }
            professionNorms.Add(NormalizeL2(v));
        }
    }

    public (ProfessionData best, List<(ProfessionData profession, float score)> all) GetBestMatch(float[] playerVector)
    {
        if (playerVector == null) throw new ArgumentNullException(nameof(playerVector));

        var player = new float[vectorLength];
        for (int i = 0; i < Math.Min(playerVector.Length, vectorLength); i++) player[i] = playerVector[i];

        var playerNorm = NormalizeL2(player);

        ProfessionData best = null;
        float bestScore = float.NegativeInfinity;
        var all = new List<(ProfessionData, float)>(professions.Count);

        for (int i = 0; i < professions.Count; i++)
        {
            var p = professions[i];
            var pNorm = professionNorms[i];

            float score = CosineNormalized(playerNorm, pNorm);
            all.Add((p, score));

            if (score > bestScore)
            {
                bestScore = score;
                best = p;
            }
        }

        all.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        return (best, all);
    }

    public List<(ProfessionData profession, float score)> GetTopMatches(float[] playerVector, int topN = 3)
    {
        var result = GetBestMatch(playerVector).all;
        if (topN <= 0) topN = 1;
        if (topN >= result.Count) return result;
        return result.GetRange(0, topN);
    }

    private static float[] NormalizeL2(float[] v)
    {
        var norm = new float[v.Length];
        float sumSq = 0f;
        for (int i = 0; i < v.Length; i++) sumSq += v[i] * v[i];
        if (sumSq <= Mathf.Epsilon) return norm;
        float inv = 1f / Mathf.Sqrt(sumSq);
        for (int i = 0; i < v.Length; i++) norm[i] = v[i] * inv;
        return norm;
    }

    private static float CosineNormalized(float[] aNorm, float[] bNorm)
    {
        if (aNorm == null || bNorm == null) return 0f;
        int len = Math.Min(aNorm.Length, bNorm.Length);
        float dot = 0f;
        for (int i = 0; i < len; i++) dot += aNorm[i] * bNorm[i];
        return Mathf.Clamp(dot, -1f, 1f);
    }
}
