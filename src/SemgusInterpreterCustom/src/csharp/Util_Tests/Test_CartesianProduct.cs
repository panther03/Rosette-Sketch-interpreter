using Semgus.Util;
using System.Collections.Generic;
using Xunit;

namespace Semgus.Util_Tests {
    public class Test_CartesianProduct {
        [Fact]
        public void CharArrays() {
            var c0 = "abcd";
            var c1 = "efg";
            var c2 = "hijkl";

            var strs = new HashSet<string>();

            foreach (var cc0 in c0)
                foreach (var cc1 in c1)
                    foreach (var cc2 in c2)
                        strs.Add(new string(new[] { cc0, cc1, cc2 }));
            
            int k = 0;

            foreach(var q in IterationUtil.CartesianProduct(new[] { c0, c1, c2 })) {
                var s = new string(q);
                Assert.True(strs.Remove(s));
                k++;
            }

            Assert.Empty(strs);
            Assert.Equal(c0.Length*c1.Length*c2.Length, k);
        }

        [Fact]
        public void Empty() {
            var a = new[] { 1, 2, 3 };
            var b = new int[0];
            var c = new[] { 1 };

            Assert.Empty(IterationUtil.CartesianProduct(new[] { a, b, c }));
        }
    }
}