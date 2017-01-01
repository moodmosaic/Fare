Fare - [F]inite [A]utomata and [R]egular [E]xpressions
===================

<p>Project Fare is an effort to bring a <a href="http://en.wikipedia.org/wiki/Deterministic_finite-state_machine" target="_blank" title="Deterministic finite-state machine">DFA</a>/<a href="http://en.wikipedia.org/wiki/Nondeterministic_finite-state_machine" target="_blank" title="Nondeterministic finite-state machine">NFA</a> (finite-state automata) implementation from Java to .NET.&#0160;There are quite a few implementations available in other languages today. This project aims to fill the gap in .NET.</p>
<p>Fare is a .NET port of the well established Java library <a href="http://www.brics.dk/automaton/" target="_blank" title="dk.brics.automaton">dk.brics.automaton</a> with API as close as possible to the corresponding dk.brics.automaton classes.</p>

Design changes
--------------

* Included a .NET port of [Xeger] (http://code.google.com/p/xeger/), for generating random text from regular expressions. Xeger does <i>not</i> support all valid Java regular expressions. The full set of what is defined here and is summarized at (http://code.google.com/p/xeger/wiki/XegerLimitations).
* Implemented object equality.
* Many getters and setters have been replaced by .NET properties.
* Many foreach loops have been converted to LINQ-expressions.
* Notes from porting Java code in .NET can be found [here] (http://www.nikosbaxevanis.com/bonus-bits/2011/11/notes-from-porting-java-code-to-net.html).

<i>Based on version 1.11-8 of dk.brics.automaton released on September 7, 2011. [ChangeLog] (http://www.brics.dk/automaton/ChangeLog)</i>

NuGet package
--------------

Fare is [available via NuGet](https://www.nuget.org/packages/Fare/).

Versioning
--------------

Fare reached version 1 without following a particular versioning scheme. From version 1 and above, Fare follows [Semantic Versioning 2.0.0](http://semver.org/spec/v2.0.0.html).

Which projects use Fare?
--------------

Fare is used in:
* [AutoFixture](https://github.com/AutoFixture/AutoFixture) for [supporting the RegularExpressionAttribute](http://nikosbaxevanis.com/blog/2011/12/11/regularexpressionattribute-support-in-autofixture/) class.
* [EntroTester](https://github.com/ymotton/EntroTester) for generating Regular Expressions that match a given input string.
