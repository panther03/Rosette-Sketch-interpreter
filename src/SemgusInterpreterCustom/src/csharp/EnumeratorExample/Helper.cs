using System.IO;

namespace EnumeratorExample {
    internal static class Helper {
        public static string GetPathToExample(string fname) {
            return Path.GetFullPath(Path.Combine(

                "..", // csharp
                "..", // src
                "..", // [repo root]
                "examples",
                fname
            ));
        }
    }
}


/*

                "..", // (Debug | Release)
                "..", // bin
                "..", // [project folder]

*/