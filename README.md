# SemGuS solvers with Rosette and Sketch

This repository contains my work in creating a SemGuS solver based in Rosette and Sketch. Each are designated into their own C# project directories.
Note that they use the parser as a dependency and at the moment it requires that the SemGuS-Parser directory exist directly outside this repository.

# Running

```
cd src/RosetteSolver
dotnet run <path-to-example-sem-file.sem> <output.rkt>
```

# Current limitations

Firstly, the SketchSolver is not in the repository anymore, I had a prototype at one point but then I updated to use a newer format so it is no longer applicable. It was never functional anyway.

Secondly, I will outline some of the limitations with the RosetteSolver. Currently, the only two examples which it can synthesize to a compiling Racket/Rosette module are `mul-impv.sem` and `max-exp.sem`. Of the two, only `max-exp.sem` generates a correct solution. `mul-impv.sem` runs out of time/memory.

The other impv tests don't compile just because in order to handle assigment to multiple variables, the solver invokes a helper function which was specifically made for the S nonterminal in `mul-impv.sem`. It just needs to be generated based on the number of input and output variables for a given nonterminal, which should be relatively easy to do.

None of the regex tests work at the moment because the solver lacks the ability to handle more than two semantic rules in one production. The SemGuS format allows multiple semantic rules mainly for things like conditionals in a while loop or if statement, which looks something like this:

```
 (($while (B b.t1) (S s.t1))
            (and
                (B.Sem b.t1 x0 y0 c0 b.v1)
                (= b.v1 true)                  ; Condition evaluates to true under G0
                (S.Sem s.t1 x0 y0 c0 x1 y1 c1) ; G0 -> G1: assign G1 by evaluating s on G0
                (S.Sem s.t  x1 y1 c1 x2 y2 c2) ; G1 -> G2: assign G2 by evaluating t on G1
            )
            (and
                (B.Sem b.t1 x0 y0 c0 b.v1)
                (= b.v1 false)                 ; Condition evaluates to false under G0
                (= x2 x0) (= y2 y0) (= c2 c0)  ; G2 := G0; do not execute s
            )
        )
```

The solver supports this kind of structure, but it breaks down for more than two, which in the regex examples is used to model a switch-case sort of flow. It is theoretically easy to add this, it just has to break into an if-else ladder with `if` Racket statements.