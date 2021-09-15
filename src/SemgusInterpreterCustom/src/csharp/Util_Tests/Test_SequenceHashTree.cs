using Semgus.Util;
using System;
using System.Linq;
using Xunit;

namespace Semgus.Util_Tests {

    public class Test_SequenceHashTree {

        [Fact]
        public void Test2() {
            const string STR = "abc";
            var ht = new SequenceHashTree<char>();
            Assert.True(ht.TryAdd(STR));
            Assert.False(ht.TryAdd(STR));

            var list = ht.EnumerateSequences().Select(l => l.ToArray()).ToList();
            Assert.Single(list);

            Assert.Equal(STR, new string(list[0]));
        }

        [Fact]
        public void Test3() {
            var ht = new SequenceHashTree<object>();
            var a1 = new object[] { 1, true, "a" };
            var a2 = new object[] { 5, false, "b" };

            var k = new object();

            var a3 = new object[] { k, 1, 2 };
            var a4 = new object[] { k, 1, 2 };

            foreach (var m in new[] { a1, a2, a3 }) {
                Assert.True(ht.TryAdd(m));
            }

            foreach (var m in new[] { a1, a2, a3 }) {
                Assert.False(ht.TryAdd(m));
            }

            Assert.False(ht.TryAdd(a4));
        }

        [Fact]
        public void Test1() {
            var ht = new SequenceHashTree<int>();

            var a1 = new[] { 1, 2, 3 };
            var a2 = new[] { 1, 2, 3, 4 };
            var a3 = new int[] { };
            var a4 = new[] { 2, 3, };
            var a5 = new[] { 5 };

            int[][] inputs = new[] { a1, a2, a3, a4, a5 };
            foreach (var m in inputs) {
                Assert.True(ht.TryAdd(m));
                Assert.False(ht.TryAdd(m));
            }

            var inputs_sorted = inputs.OrderBy(l => l.Length).ToList();
            var sequences_sorted = ht.EnumerateSequences().Select(seq => seq.ToArray()).OrderBy(l => l.Length).ToList();

            Assert.True(
                sequences_sorted.Zip(inputs_sorted, (a, b) => a.SequenceEqual(b)).All(b => b)
            );

        }
    }
}