using System;

namespace EnumeratorExample {
    class Program {
        static void Main(string[] args) {
            var fpath = Helper.GetPathToExample("arith0.sem");
            Test1.Run(fpath);
        }
    }
}
