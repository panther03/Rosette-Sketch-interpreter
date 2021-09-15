using Semgus.Util;
using System.Linq;
using Xunit;

namespace Semgus.Util_Tests {
    public class Test_SequenceHashTreeMap {
        [Fact]
        public void CharArrays() {
            var a1 = new object[] { 1, true, "a" };
            var p1 = "abc";
            var p2 = "def";

            var ht = new SequenceHashTreeMap<object, string>();

            Assert.True(ht.TryAdd(a1, p1));
            Assert.False(ht.TryAdd(a1, p1));
            Assert.False(ht.TryAdd(a1, p2));

            var list = ht.EnumerateSequences().ToList();
            Assert.Single(list);

            Assert.Equal(list[0].payload, p1);

        }
    }
}