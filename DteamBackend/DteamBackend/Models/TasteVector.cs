using DteamBackend.Models.Enums;

namespace DteamBackend.Models
{
    public static class TasteCategories
    {
        private static readonly GameGenre[] Genres = Enum.GetValues<GameGenre>();

        public static readonly int Length = Genres.Length;

        private static readonly Dictionary<string, int> IndexOf = Genres
            .Select((genre, idx) => (Name: genre.ToString(), Index: idx))
            .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

        public static int? TryGetIndex(string category)
            => IndexOf.TryGetValue(category, out var idx) ? idx : null;

        public static float[] Empty() => new float[Length];
        private const float BaselineValue = 0.05f;

        public static float[] Baseline()
        {
            var vector = new float[Length];
            for (var i = 0; i < Length; i++) vector[i] = BaselineValue;
            return Normalize(vector);
        }

        public static float[] BuildGameVector(IEnumerable<string> genres, IEnumerable<string> tags, IEnumerable<string> features)
        {
            var vector = Empty();

            void FillVector(IEnumerable<string> items)
            {
                if (items == null) return;
                foreach (var item in items)
                {
                    if (TryGetIndex(item) is { } idx)
                    {
                        vector[idx] = 1f;
                    }
                }
            }

            FillVector(genres);
            FillVector(tags);
            FillVector(features);

            return Normalize(vector);
        }

        public static float[] Normalize(float[] vector)
        {
            var sumOfSquares = 0f;
            for (var i = 0; i < vector.Length; i++)
            {
                sumOfSquares += vector[i] * vector[i];
            }

            var norm = MathF.Sqrt(sumOfSquares);
            if (norm < 1e-6f) return vector;

            var result = new float[vector.Length];
            for (var i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] / norm;
            }

            return result;
        }
    }
}