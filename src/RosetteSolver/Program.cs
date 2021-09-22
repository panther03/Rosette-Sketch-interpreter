using System;
using System.IO;
using System.Runtime.ExceptionServices;
using Antlr4.Runtime;
using Semgus.Parser.Internal;
using Semgus.Syntax;

namespace Semgus.Solver.Rosette {
    class Program {

        public static readonly string HEADER = @"#lang rosette

(require
  rosette/lib/match
  rosette/lib/destruct
  rosette/lib/angelic
  rosette/lib/synthax)

(current-bitwidth #f)

; hack because i can't get the interpreter to generate #t and #f
(define True #t)
(define False #f)";

        static void Main(string[] args) {
            if (args.Length != 1) {
                Console.Error.WriteLine("Expects one argument: a Semgus file to parse");
            }
            
            var filename = args[0];

            SemgusLexer lexer = new SemgusLexer(new AntlrFileStream(filename));
            SemgusParser parser = new SemgusParser(new CommonTokenStream(lexer));

            var cst = parser.start();
            var normalizer = new SyntaxNormalizer();
            try {
                (var ast, var env) = normalizer.Normalize(cst);

                var printer = HEADER + "\n" + AdtBuilder.BuildAdtRepresentation(ast) + "\n" + SyntaxGenPass.BuildSyntaxGenFns(ast) + "\n" + SemGenPass.BuildSemGenFns(ast);


                // Print the AST
                System.Console.WriteLine(env.PrettyPrint());
                System.Console.WriteLine(printer);
            } catch (SemgusSyntaxException e) {
                using (var file = new StreamReader(filename)) {
                    PrintExceptionAndItsLocationInFile(e, file);
                }
            }
            System.Console.WriteLine("Done");
        }

        private static void PrintExceptionAndItsLocationInFile(SemgusSyntaxException e, StreamReader file) {
            System.Console.WriteLine(e.Message);
            var l0 = e.ParserContext.Start.Line;
            var l1 = e.ParserContext.Stop.Line;
            int k = 0;
            string line;
            while ((line = file.ReadLine()) != null) {
                if (k == (l0 - 1)) {
                    Console.WriteLine("--- BEGIN ERROR SECTION ---");
                } else if (k == l1) {
                    Console.WriteLine("---  END ERROR SECTION  ---");
                }
                Console.WriteLine(line);
                k++;
            }
        }
    }
}
