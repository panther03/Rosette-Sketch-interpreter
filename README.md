# SemGuS solvers with Rosette and Sketch

This repository contains my work in creating a SemGuS solver based in Rosette and Sketch. Each are designated into their own C# project directories.
Note that they use the parser as a dependency and at the moment it requires that the SemGuS-Parser directory exist directly outside this repository.

Interpret/Execute can only be called on generated nodes?
distinction between semgus file rules and enumerated nodes
how do I run that directly from the file
is any of it even useful to me?

does something have to be run on inputs to make it work?

loop through each rule interpreter in the interpretation grammar

subterms are (+ Start:t1 Start:t2)
args are (Term Int Int Int)


- move over to new interpreter stuff
- move up printcode to non-terminal level (investigate interpretation grammar?)