using System.IO;

namespace InterpreterExample {
    internal static class Helper {
        public static string GetPathToExample(string fname) {
            return Path.GetFullPath(Path.Combine(
                "..", // (Debug | Release)
                "..", // bin
                "..", // [project folder]
                "..", // csharp
                "..", // src
                "..", // [repo root]
                "examples",
                fname
            ));
        }
    }
}
