This directory contains a set of samples that illustrate 
the use of the core library Microsoft.Automata.dll

The samples are given in the C# source file: Program.cs.
They make use of a collection of regexes in the file: regexes.txt.

First, you need to build the samples:

> buildSamples.bat

This compiles the file Program.cs into an executable run.exe.

There are 9 samples, in order to run for example sample 5:

> run 5

Several of the samples create .dot and .dgml files 
for directed graphs. Use your favorite .dot viewer 
to inspect the .dot files. The .dgml files can be 
viewed with Visual Studio 2010.


This is a high-level summary of the examples:

Sample 1: Illustrates creation of automata 
          from .Net regexes and member generation.

Sample 2: Illustrates epsilon elimination and 
          serialization in dot and dgml formats.

Sample 3: Illustrates automata algorithms for 
          difference, equivalence, determinization 
          and minimization.

Sample 4: Illustrates two regexes that create large 
          automata.

Sample 5: Illustrates difference checking with witness 
          generation applied to some typical regexes 
          from the file regexes.txt.

Sample 6: Converts all the regexes in regexes.txt into 
          automata, eliminates epsilons and saves them 
          in dot and dgml format.

Sample 7: Illustrates the product construct for two 
          large automata.

Sample 8: Illustrates the functionality of the minterm 
          generation algorithm.

Sample 9: Illustrates how to define and use a custom 
          Boolean algebra solver.
