using Semgus.Syntax;

using Dec = Semgus.Syntax.VariableDeclaration.Context;
namespace InterpreterExample {
    public class ExampleSemgus {
        /*
            Start : (Start.Sem(Term Int Int Int)) : t      // NT name : relation-def : term name
            [() (Start.Sem t x y result)]                  // CHC conclusion
            (
                x [() (and (= t (Leaf 'x')) (= result x))]          // Leaf (x), with semantics for t and result.
                y [() (and (= t (Leaf 'y')) (= result y))]          // Using Leaf('y') to represent y as a term
                Zero [() (and (= t (Leaf '0')) (= result 0))]       // Maybe we can automate the syntax part instead of
                1 [() (and (= t (Leaf '1')) (= result 1))]          // having users write about terms
                n [() (and (= t (Leaf 'n.value')) (= result Int(n.value)))]  // Symbolic integer n - Here, we use n.value to show that n and n.value are different constructs

                (+ Start:t1 Start:t2)
                    [((v1 Int) (v2 Int)) (and (= t (Plus t1 t2)) (Start.Sem t1 x y v1) (Start.Sem t2 x y v2) (= result (+ v1 v2)))]
                // Premise for Plus: premise contains Start.Sems and computes v1 + v2 for result

                (ite B:tb Start:t1 Start:t2)
                [((vb Bool) (v1 Int) (v2 Int)) (and (= t (ITE tb t1 t2)) (B.Sem tb x y vb) (Start.Sem t1 x y v1) (Start.Sem t2 x y v2) (= result (ite vb v1 v2)))]
                // Premise for If-Then-Else: We can use NTs from other LHSes as well (scope should follow this NTs rules)
            )
        */


        public readonly LibraryFunction l_and;
        public readonly LibraryFunction l_eq;
        public readonly LibraryFunction l_plus;
        public readonly LibraryFunction l_add;
        public readonly LibraryFunction l_leaf;

        public readonly Nonterminal nt_start;

        public readonly SemgusType t_term;
        public readonly SemgusType t_int;

        public readonly SemanticRelationDeclaration r_start_sem;

        public readonly VariableEvaluation v_t;
        public readonly VariableEvaluation v_x;
        public readonly VariableEvaluation v_y;
        public readonly VariableEvaluation v_result;
        
        public readonly NonterminalTermDeclaration ntd_t1;
        public readonly NonterminalTermDeclaration ntd_t2;
        public readonly VariableEvaluation v_t1;
        public readonly VariableEvaluation v_t2;
        
        public readonly VariableEvaluation v_v1;
        public readonly VariableEvaluation v_v2;

        public readonly SemanticRelationInstance ri_start_sem;
        
        public readonly IProductionRewriteExpression rew_add;
        public readonly IProductionRewriteExpression rew_leaf_x;
        public readonly IProductionRewriteExpression rew_leaf_y;
        
        public readonly IFormula predicate_add;
        public readonly IFormula predicate_leaf_x;
        public readonly IFormula predicate_leaf_y;

        public ExampleSemgus() {
            this.l_and = new LibraryFunction("and");
            this.l_eq = new LibraryFunction("=");
            this.l_plus = new LibraryFunction("Plus");
            this.l_add = new LibraryFunction("+");
            this.l_leaf = new LibraryFunction("Leaf");
            this.nt_start = new Nonterminal("Start");
            this.t_term = new SemgusType(NonterminalTermDeclaration.TYPE_NAME);
            this.t_int = new SemgusType("Int");


            this.r_start_sem = new SemanticRelationDeclaration("Start.Sem", new[] { t_term, t_int, t_int, t_int });


            this.v_t = new VariableEvaluation(new NonterminalTermDeclaration("t", t_term, nt_start, Dec.NT_Term));
            this.v_x = new VariableEvaluation(new VariableDeclaration("x", t_int, Dec.SF_Input));
            this.v_y = new VariableEvaluation(new VariableDeclaration("y", t_int, Dec.SF_Input));
            this.v_result = new VariableEvaluation(new VariableDeclaration("result", t_int, Dec.SF_Output));

            this.ntd_t1 = new NonterminalTermDeclaration("t1", t_term, nt_start, Dec.PR_Subterm);
            this.ntd_t2 = new NonterminalTermDeclaration("t2", t_term, nt_start, Dec.PR_Subterm);

            this.v_t1 = new VariableEvaluation(ntd_t1);
            this.v_t2 = new VariableEvaluation(ntd_t2);

            this.v_v1 = new VariableEvaluation(new VariableDeclaration("v1", t_int, Dec.PR_Auxiliary));
            this.v_v2 = new VariableEvaluation(new VariableDeclaration("v2", t_int, Dec.PR_Auxiliary));

            this.ri_start_sem = new SemanticRelationInstance(r_start_sem, new[] { v_t.Variable, v_x.Variable, v_y.Variable, v_result.Variable });

            this.rew_add = new OpRewriteExpression(new Operator("+"), new[] { ntd_t1, ntd_t2 });
            this.rew_leaf_x = new AtomicRewriteExpression(new LeafTerm("x"));
            this.rew_leaf_y = new AtomicRewriteExpression(new LeafTerm("y"));

            this.predicate_add = new LibraryFunctionCall(l_and, new IFormula[]{
                new LibraryFunctionCall(l_eq,new IFormula[]{
                    v_t,
                    new LibraryFunctionCall(l_plus, new IFormula[]{v_t1, v_t2})
                }),
                new SemanticRelationQuery(r_start_sem, new IFormula[]{v_t1, v_x, v_y, v_v1}),
                new SemanticRelationQuery(r_start_sem, new IFormula[]{v_t2, v_x, v_y, v_v2}),
                new LibraryFunctionCall(l_eq, new IFormula[]{
                    v_result,
                    new LibraryFunctionCall(l_add,new IFormula[]{
                        v_v1,
                        v_v2
                    })
                })
            });
            

            this.predicate_leaf_x = new LibraryFunctionCall(l_and, new IFormula[]{
                new LibraryFunctionCall(l_eq, new IFormula[]{
                    v_t,
                    new LibraryFunctionCall(l_leaf,new IFormula[]{ new Literal<string>("x") })
                }),
                new LibraryFunctionCall(l_eq, new IFormula[]{
                    v_result,
                    v_x
                })
            });

            this.predicate_leaf_y = new LibraryFunctionCall(l_and, new IFormula[]{
                new LibraryFunctionCall(l_eq, new IFormula[]{
                    v_t,
                    new LibraryFunctionCall(l_leaf,new IFormula[]{ new Literal<string>("y") })
                }),
                new LibraryFunctionCall(l_eq, new IFormula[]{
                    v_result,
                    v_y
                })
            });

        }
    }
}
