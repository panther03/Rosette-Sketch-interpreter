using Semgus.Interpreter;
using Semgus.Parser;
using Semgus.Syntax;
using System;
using System.Linq;
using Semgus.Util;
using System.IO;

// normal to have to include this for InductiveConstraintAnalyzer?
using Semgus.Solvers.Enumerative;


namespace Semgus.Solver.Rosette {
    class Program {

        public static readonly string HEADER = @"#lang rosette

(require
  rosette/lib/match
  rosette/lib/destruct
  rosette/lib/angelic
  rosette/lib/synthax)

(current-bitwidth #f)

(define (ite c x y) (if c x y))

; hardcoded for now, this will only work for mul-impv (see README.md)
(define-syntax-rule (set-vals-list __depth x y z val)
     (define-values (__depth x y z) (let ([l val]) (values (first l) (second l) (third l) (fourth l)))))";

        static void Main(string[] args) {
            if (args.Length != 2) {
                Console.Error.WriteLine("Expects two arguments: a Semgus file to parse, and an output racket file.");
            }
            
            var infile = args[0];
            var outfile = args[1];

            var theory = BasicLibrary.Instance; // default theory
            var constraintAnalyzer = new InductiveConstraintAnalyzer(theory);

            var ast = Parse(infile);
            var grammar = InterpretationGrammar.FromAst(ast, theory);
            var spec = constraintAnalyzer.Analyze(ast);

            var printer = HEADER + "\n" + AdtBuilder.BuildAdtRepr(grammar) + "\n"
                                        + SyntaxBuilder.BuildSyntaxGenFns(grammar) + "\n"
                                        + SemanticsBuilder.BuildSemGenFns(grammar) + "\n"
                                        + ConstraintsBuilder.BuildConstraintFn(spec);

            File.WriteAllText(outfile, printer);
        }
        public static SemgusProblem Parse(string filePath) {
            using var writer = new StringWriter();

            if (new SemgusParser(filePath).TryParse(out var ast, writer)) {
                return ast;
            } else {
                throw new Exception(writer.ToString());
            }
        }
    }
}
