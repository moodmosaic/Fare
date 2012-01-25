This directory contains a set of samples 
that illustrate the use of the core library 
Microsoft.Automata.Z3.dll. The samples are 
given in the C# source file: Program.cs.

They make use of a collection of sample regexes 
in the file: regexes.txt.

First, you need to build the samples:

> buildSamples.bat

This compiles the file Program.cs into an executable run.exe.

There are 9 samples, in order to run for example sample 5:

> run 5

Some of the samples create .dot and .dgml files for directed graphs. 
Use your favorite .dot viewer to view the .dot files. 
The .dgml files can be viewed with Visual Studio 2010.


This is a high-level summary of the examples:

Sample 1: Illustrates: Creation of automata 
          over Z3 Terms from .Net regexes, theory assertion, 
          and model generation.

Sample 2: dot and dgml generation from SFAs

Sample 3: Product construction and member generation 
          from SFAs using axioms applied to regexes.txt

Samples 4,5: Difference construction and member generation 
          from SFAs using axioms applied to regexes.txt

Sample 6: Construction and exploration of a symbolic transducer (ST).

Sample 7: Analysis of symbolic finite transducers SFTs.

Sample 8: Representation and Boolean exploration of 
          (a fragment of) HtmlDecode as an ST with 
          two character registers and four Boolean registers.

Sample 9: Illustrates an analysis scenario involving the 
          HtmlDecode ST.