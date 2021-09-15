using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Semgus.Enumerator {
    public class BoolArrayComparer : IEqualityComparer<bool[]> {
        public static BoolArrayComparer Instance { get; } = new BoolArrayComparer();

        public bool Equals(bool[] x, bool[] y) {
            if (x is null) return y is null;
            if (y is null) return false;
            if (x.Length != y.Length) return false;

            for(int i = 0; i < x.Length; i++) {
                if (x[i] != y[i]) return false;
            }

            return true;
        }

        public int GetHashCode([DisallowNull] bool[] obj) {
            var hash = new HashCode();
            hash.Add(obj.Length);
            for(int i =0; i < obj.Length; i++) {
                hash.Add(obj[i]);
            }
            return hash.ToHashCode();
        }
    }
}