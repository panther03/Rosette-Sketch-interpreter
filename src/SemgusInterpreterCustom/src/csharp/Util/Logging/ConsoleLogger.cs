namespace Semgus.Util {
    public class ConsoleLogger : ILogger {
        public static ConsoleLogger Instance { get; } = new ConsoleLogger();

        public void Log(string s) => System.Console.WriteLine(s);
    }
}
