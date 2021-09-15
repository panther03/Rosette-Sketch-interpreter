using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Util {
    public static class IterationUtil {
        /// <summary>
        /// Return an array of ints [0, 1, ... n-1].
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int[] IntRange(int n) {
            if (n < 0) throw new ArgumentOutOfRangeException();
            var result = new int[n];
            for (int i = 0; i < n; i++) result[i] = i;
            return result;
        }

        /// <summary>
        /// Enumerate all distinct sequences of integers, of length <paramref name="choiceLength"/>,
        /// of which at least one is equal to <paramref name="max"/> and the others are in the range [0, <paramref name="max"/>] (inclusive).
        /// 
        /// Note that this is a "shallow" operation - the yielded array will be overwritten by subsequent iterations.
        /// </summary>
        /// <param name="choiceLength"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static IEnumerable<int[]> EnumerateChoicesWithMax(int choiceLength, int max) {
            if(max < 0) {
                yield break;
            }

            var low = IntRange(max);
            var high = IntRange(max+1);
            var middle = new[] { max };

            int[][] specifier = new int[choiceLength][];

            int i, j;
            for (i = 0; i < choiceLength; i++) {
                specifier[i] = middle;

                for (j = 0; j < i; j++) { specifier[j] = low; }
                for (j = i + 1; j < choiceLength; j++) { specifier[j] = high; }

                foreach (var a in IterationUtil.CartesianProduct(specifier)) {
                    yield return a;
                }
            }
        }

        /// <summary>
        /// Given a list <paramref name="sources"/> of integer sets, enumerate every choice containing one integer from each set
        /// such that the sum of entries in the choice is equal to the expected value <paramref name="sum"/>.
        /// 
        /// This is NP-complete (see https://en.wikipedia.org/wiki/Subset_sum_problem), so we perform exhaustive search.
        /// 
        /// Note that this is a "shallow" operation - the yielded array will be overwritten by subsequent iterations.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static IEnumerable<int[]> EnumerateChoicesWithSum(IEnumerable<IEnumerable<int>> sources, int sum) {
            int sigma;

            foreach(var a in IterationUtil.CartesianProduct(sources)) {
                sigma = 0;
                for(int i = 0; i < a.Length; i++) {
                    sigma += a[i];
                }
                if(sigma == sum) {
                    yield return a;
                }
            }

        }

        /// <summary>
        /// Given a list <paramref name="sources"/> of integer sets, enumerate every choice containing one integer from each set
        /// such that the maximum of the choice is equal to the expected value <paramref name="max"/>.
        /// 
        /// Currently we perform exhaustive search - this could be optimized further.
        /// 
        /// Note that this is a "shallow" operation - the yielded array will be overwritten by subsequent iterations.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static IEnumerable<int[]> EnumerateChoicesWithMax(IEnumerable<IEnumerable<int>> sources, int max) {
            bool contains_max, exceeds_max;

            foreach (var a in IterationUtil.CartesianProduct(sources)) {
                contains_max = false;
                exceeds_max = false;
                for (int i = 0; i < a.Length; i++) {
                    if (a[i] > max) {
                        exceeds_max = true;
                        break;
                    }
                    if (a[i] == max) {
                        contains_max = true;
                    }
                }
                if(exceeds_max) {
                    continue;
                }
                if (contains_max) {
                    yield return a;
                }
            }
        }

        /// <summary>       
        /// Enumerates the cartesian product of its input sets.
        /// Note that this is a "shallow" operation - the yielded array will be overwritten by subsequent iterations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sets"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sets) {
            // https://codereview.stackexchange.com/a/140428


            var enumerators = sets.Select(s => s.GetEnumerator()).ToArray();
            var n = enumerators.Length;

            try {
                // Move enumerators to first position
                for (int i = 0; i < n; i++) {
                    if (!enumerators[i].MoveNext()) yield break; // If one of the enumerables is empty, exit early
                }

                T[] array = new T[n];
                while (true) {
                    for (int i = 0; i < n; i++) {
                        array[i] = enumerators[i].Current;
                    }

                    yield return array;

                    // Move to the next position
                    for (int k = n - 1; k >= 0; k--) {
                        if (enumerators[k].MoveNext()) {
                            break;
                        } else {
                            if (k == 0) yield break;

                            // Reset enumerator for this collection
                            enumerators[k].Reset();
                            enumerators[k].MoveNext();

                            // Try to advance the next enumerator
                            continue;
                        }
                    }
                }
            } finally {
                for (int i = 0; i < n; i++) {
                    enumerators[i].Dispose();
                }
            }
        }
    }
}
