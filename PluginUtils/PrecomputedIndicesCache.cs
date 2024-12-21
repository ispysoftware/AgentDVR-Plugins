using System;
using Microsoft.Extensions.Caching.Memory;
namespace PluginUtils
{
    public class PrecomputedIndicesCache
    {
        // Define a key structure for caching X and Y computations
        private struct CacheKey : IEquatable<CacheKey>
        {
            public int RecWidth { get; }
            public float ScaleX { get; }
            public int RecHeight { get; }
            public float ScaleY { get; }

            public CacheKey(int recWidth, float scaleX, int recHeight, float scaleY)
            {
                RecWidth = recWidth;
                ScaleX = scaleX;
                RecHeight = recHeight;
                ScaleY = scaleY;
            }

            public bool Equals(CacheKey other)
            {
                return RecWidth == other.RecWidth &&
                       ScaleX.Equals(other.ScaleX) &&
                       RecHeight == other.RecHeight &&
                       ScaleY.Equals(other.ScaleY);
            }

            public override bool Equals(object obj)
            {
                return obj is CacheKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + RecWidth.GetHashCode();
                    hash = hash * 23 + ScaleX.GetHashCode();
                    hash = hash * 23 + RecHeight.GetHashCode();
                    hash = hash * 23 + ScaleY.GetHashCode();
                    return hash;
                }
            }
        }

        // The MemoryCache to store precomputed indices and weights
        private readonly MemoryCache cache;

        // Maximum number of cache entries
        private const long MaxCacheSize = 1000; // Adjust based on your requirements

        // Structure to hold the cached arrays
        public class CachedData
        {
            public int[] XIndices { get; }
            public int[] XWeights { get; }
            public int[] YIndices { get; }
            public int[] YWeights { get; }

            public CachedData(int[] xIndices, int[] xWeights, int[] yIndices, int[] yWeights)
            {
                XIndices = xIndices;
                XWeights = xWeights;
                YIndices = yIndices;
                YWeights = yWeights;
            }
        }

        public PrecomputedIndicesCache()
        {
            var cacheOptions = new MemoryCacheOptions
            {
                SizeLimit = MaxCacheSize, // Set the size limit
            };
            cache = new MemoryCache(cacheOptions);
        }

        /// <summary>
        /// Retrieves the precomputed indices and weights from the cache.
        /// If not present, computes them, stores them in the cache, and then returns.
        /// </summary>
        /// <param name="recWidth">Width for X plane computation.</param>
        /// <param name="scaleX">Scale factor for X plane.</param>
        /// <param name="recHeight">Height for Y plane computation.</param>
        /// <param name="scaleY">Scale factor for Y plane.</param>
        /// <returns>A CachedData object containing the indices and weights.</returns>
        public CachedData GetOrAddIndices(int recWidth, float scaleX, int recHeight, float scaleY)
        {
            var key = new CacheKey(recWidth, scaleX, recHeight, scaleY);

            // Try to get the cached data
            if (!cache.TryGetValue(key, out CachedData cachedData))
            {
                // Compute the data if not present in cache
                cachedData = ComputeCachedData(key);

                // Define cache entry options with size and eviction policy
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1) // Each entry counts as '1' towards the size limit
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30)) // Optional: Adjust as needed
                    .SetPriority(CacheItemPriority.High); // Optional: Adjust based on importance

                // Add the computed data to the cache
                cache.Set(key, cachedData, cacheEntryOptions);
            }

            return cachedData;
        }

        /// <summary>
        /// Computes the CachedData based on the provided CacheKey.
        /// </summary>
        /// <param name="key">The cache key containing parameters for computation.</param>
        /// <returns>A new instance of CachedData.</returns>
        private CachedData ComputeCachedData(CacheKey key)
        {
            // Choose a scale factor of 256 for Q8.8 fixed-point format
            const float scale = 256.0f;

            // Compute X indices and fixed-point weights
            int[] xIndices = new int[key.RecWidth];
            int[] xWeights = new int[key.RecWidth];
            for (int x = 0; x < key.RecWidth; x++)
            {
                float sx = x * key.ScaleX;
                xIndices[x] = (int)sx;
                float frac = sx - xIndices[x];  // Fractional part
                int w = Math.Clamp((int)(frac * scale + 0.5f), 0, 256);
                xWeights[x] = w;
            }

            // Compute Y indices and fixed-point weights
            int[] yIndices = new int[key.RecHeight];
            int[] yWeights = new int[key.RecHeight];
            for (int y = 0; y < key.RecHeight; y++)
            {
                float sy = y * key.ScaleY;
                yIndices[y] = (int)sy;
                float frac = sy - yIndices[y];
                int w = Math.Clamp((int)(frac * scale + 0.5f), 0, 256);
                yWeights[y] = w;
            }

            return new CachedData(xIndices, xWeights, yIndices, yWeights);
        }
    }
}
