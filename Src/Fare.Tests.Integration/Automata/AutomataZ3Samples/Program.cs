/*
 *  The samples 1..9 illustrate typical use of the library Microsoft.Automata.Z3.dll
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.Automata;
using Microsoft.Automata.Z3;
using Microsoft.Z3;

using SFAz3 = Microsoft.Automata.SFA<Microsoft.Z3.FuncDecl, Microsoft.Z3.Term, Microsoft.Z3.Sort>;
using STz3 = Microsoft.Automata.ST<Microsoft.Z3.FuncDecl, Microsoft.Z3.Term, Microsoft.Z3.Sort>;
using Rulez3 = Microsoft.Automata.Rule<Microsoft.Z3.Term>;
using STBuilderZ3 = Microsoft.Automata.STBuilder<Microsoft.Z3.FuncDecl, Microsoft.Z3.Term, Microsoft.Z3.Sort>;

namespace Microsoft.Automata.Z3.ReleaseSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            int sample;
            if (args.Length != 1 || !int.TryParse(args[0], out sample) || sample < 1 || sample > 9)
                Console.WriteLine(@"Use: run <sample nr>
where sample nr is between 1 and 9.");
            else
            {
                switch (sample)
                {
                    case 1: { Sample1(); break; }
                    case 2: { Sample2(); break; }
                    case 3: { Sample3(); break; }
                    case 4: { Sample4(); break; }
                    case 5: { Sample5(); break; }
                    case 6: { Sample6(); break; }
                    case 7: { Sample7(); break; }
                    case 8: { Sample8(); break; }
                    case 9: { Sample9(); break; }
                }
            }
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// The main points illustrated are: 
        /// 1) Creation of automata over Z3 Terms from .Net regexes
        /// 2) Theory assertion of the axiomatic theory of automata.
        /// 3) Creation of logical formula involving acceptor axioms.
        /// 4) Model generation for given assertions.
        /// </summary>
        static void Sample1()
        {
            Z3Provider Z = new Z3Provider();
            Console.WriteLine("Sample 1: find a witness accepted by one automaton but not the other.");
            string a = @"^[A-Za-z0-9]+@(([A-Za-z0-9\-])+\.)+([A-Za-z\-])+$";                // .Net regex
            string b = @"^\d.*$";                                                           // .Net regex
            Console.WriteLine("Construct SFA A for regex: {0}", a);
            var A = new SFAz3(Z, Z.CharacterSort, Z.RegexConverter.Convert(a));             //  SFA for a
            Console.WriteLine("Construct SFA B for regex: {0}", b);
            var B = new SFAz3(Z, Z.CharacterSort, Z.RegexConverter.Convert(b));             //  SFA for b
            Console.WriteLine("Assert the theories of A and B"); 
            A.AssertTheory(); B.AssertTheory();                         // assert both SFA theories to Z3
            Term inputConst = Z.MkFreshConst("input", A.InputListSort); // declare List<char> constant
            var assertion = Z.MkAnd(A.MkAccept(inputConst),             // get solution for inputConst 
                                      Z.MkNot(B.MkAccept(inputConst))); // accepted by A but not by B
            Console.WriteLine("Generate a model for the assertion: A(input) && !B(input).");
            var model = Z.GetModel(assertion, inputConst);              // retrieve satisfying model
            string input = model[inputConst].StringValue;               // the witness in L(A)-L(B)
            Z.Dispose();
            Console.WriteLine("Generated value for input: {0}", StringUtility.Escape(input));
        }

        /// <summary>
        /// Illustrates dot and dgml generation from SFAs
        /// </summary>
        static void Sample2()
        {
            Console.WriteLine("Sample 2: dot and dgml generation from SFAs.");
            Z3Provider Z = new Z3Provider();
            string a = @"^[A-Za-z0-9]+@(([A-Za-z0-9\-])+\.)+([A-Za-z\-])+$";            
            Console.WriteLine("Construct SFA A for regex: {0}", a);
            var A = new SFAz3(Z, Z.CharacterSort, Z.RegexConverter.Convert(a));
            Console.WriteLine("Save A in Sample2.dot(.dgml)");
            A.Name = "Sample2";
            A.SaveAsDot();
            A.SaveAsDgml();
            Z.Dispose();
        }

        /// <summary>
        /// Product construction and member generation from SFAs using axioms applied to regexes.txt
        /// </summary>
        static void Sample3()
        {
            Console.WriteLine("Sample 3: Product construction and member generation from SFAs using axioms applied to regexes.txt");
            var z3p = new Z3Provider(CharacterEncoding.Unicode);

            List<string> regexes = new List<string>(File.ReadAllLines("regexes.txt"));

            for (int i = 0; i < regexes.Count; i++)
                for (int j = 0; j < regexes.Count; j++)
                {

                    string regexA = regexes[i];
                    string regexB = regexes[j];

                    Console.WriteLine("Push a new logical context.");
                    z3p.Push();

                    var A = z3p.RegexConverter.Convert(regexA, RegexOptions.None);
                    var B = z3p.RegexConverter.Convert(regexB, RegexOptions.None);

                    Console.WriteLine("--- Make the product of SFAs for regexes {0} and {1} ---", i, j);
                    var C = Automaton<Term>.MkProduct(A, B, z3p);

                    if (i == j)
                        if (C.IsEmpty) Console.WriteLine("ERROR");
                    if (!C.IsEmpty)
                    {
                        string s = GetMember(z3p, C);
                        if (!Regex.IsMatch(s, regexA, RegexOptions.None))
                            Console.WriteLine("ERROR");
                        if (!Regex.IsMatch(s, regexB, RegexOptions.None))
                            Console.WriteLine("ERROR");
                        Console.WriteLine("Generated member: {0}", StringUtility.Escape(s));
                    }
                    else
                    {
                        Console.WriteLine("Product is empty");
                    }

                    Console.WriteLine("Pop the logical context.");
                    z3p.Pop();
                }
            z3p.Dispose();
        }

        static string GetMember(Z3Provider z3p, Automaton<Term> C)
        {
            z3p.Chooser.RandomSeed = 123;
            var sTerm = new List<Term>(C.ChoosePathToSomeFinalState(z3p.Chooser)).ToArray();
            string s = new String(Array.ConvertAll(sTerm, m => z3p.GetCharValue(z3p.FindOneMember(m).Value)));
            return s;
        }

        /// <summary>
        /// Difference construction and member generation from SFAs using axioms applied to regexes.txt.
        /// (Over all Unicode characters.)
        /// </summary>
        static void Sample4()
        {
            Console.WriteLine("Sample 4 : Difference checking and member generation from SFAs using axioms applied to regexes.txt (over Unicode)");
            Sample4and5(CharacterEncoding.Unicode);
        }

        /// <summary>
        /// Difference construction and member generation from SFAs using axioms applied to regexes.txt.
        /// (Over the ASCII range.)
        /// </summary>
        static void Sample5()
        {
            Console.WriteLine("Sample 5 : Difference checking and member generation from SFAs using axioms applied to regexes.txt (over ASCII)");
            Sample4and5(CharacterEncoding.ASCII);
        }

        static void Sample4and5(CharacterEncoding encoding)
        {
            var z3p = new Z3Provider(encoding);
            var regexes = new List<string>(File.ReadAllLines("regexes.txt"));
            for (int i = 0; i < regexes.Count; i++)
                for (int j = 0; j < regexes.Count; j++)
                {
                    string regexA = regexes[i];
                    string regexB = regexes[j];

                    z3p.Push();

                    var A = z3p.RegexConverter.Convert(regexA, System.Text.RegularExpressions.RegexOptions.None);
                    var B = z3p.RegexConverter.Convert(regexB, System.Text.RegularExpressions.RegexOptions.None);

                    List<Term> witness;
                    Console.WriteLine("--- Check difference of SFA for regexes {0} / {1} ---", i, j);
                    bool isNonempty = Automaton<Term>.CheckDifference(A, B, -1, z3p, out witness);
                    if (isNonempty)
                    {
                        string s = new String(Array.ConvertAll(witness.ToArray(), c => z3p.GetCharValue(z3p.FindOneMember(c).Value)));
                        if (!Regex.IsMatch(s, regexA)) Console.WriteLine("ERROR");
                        if (Regex.IsMatch(s, regexB)) Console.WriteLine("ERROR");
                        Console.WriteLine("Witness: {0}", StringUtility.Escape(s));
                    }
                    else
                    {
                        Console.WriteLine("Difference is empty.");
                    }

                    z3p.Pop();
                }
            z3p.Dispose();
        }

        /// <summary>
        /// Illustrates the construction and exploration of a Symbolic Transducer (ST).
        /// </summary>
        static void Sample6()
        {
            var Z = new Z3Provider();
            STBuilderZ3 stb = new STBuilderZ3(Z);
            Console.WriteLine("Sample 6: Constructs an STs that decodes pairs of digits between '5' and '9'");
            Console.WriteLine("The generated ST is saved in Sample6.dot(.dgml)");
            Console.WriteLine("The ST uses a register r with the initial value 0 that records a previously seen digit");
            Console.WriteLine("Construct the rules of the ST corresponding to the following cases:");
            Console.WriteLine("- rule1: if the input c is not a digit in the range [5-9], then output c.");
            Console.WriteLine("- rule2: if r == 0 and c is in [5-9] then store c in the register r");
            Console.WriteLine("- rule3: if r != 0 and c is in [5-9] then set r to 0 and output (10*(r-48))+(c-48)");
            Console.WriteLine("Construct also the final rule:");
            Console.WriteLine("- when the end of input has been reached and r!=0 then output r");
            var st = MkDecodePairsST(Z, stb);
            st.Name = "DecodeDigitPairs";
            Console.WriteLine("Store the st in {0}.dot(.dgml)", st.Name);
            st.SaveAsDot();
            st.SaveAsDgml();
            var sft = st.Explore();
            sft.Name = "DecodeDigitPairs_F";
            Console.WriteLine("Explore the st, and store the resulting sft in {0}.dot(.dgml)", sft.Name);
            sft.SaveAsDot();
            sft.SaveAsDgml();
            Z.Dispose();
        }

        /// <summary>
        /// Illustrates analysis of SFTs.
        /// </summary>
        static void Sample7()
        {
            Console.WriteLine("Sample 7: Illustrates idempotence checking of an ST");
            Z3Provider Z = new Z3Provider(); // analysis uses the Z3 provider
            STBuilderZ3 stb = new STBuilderZ3(Z);
            Console.WriteLine("Use the same DecodeDigitPairs ST, say f, as in Sample 6.");
            var f = MkDecodePairsST(Z,stb).Explore();
            Console.WriteLine("Compose f with itself.");
            var fof = f + f; // self-compostion of f
            Console.WriteLine("Check idempotence of f, i.e., if f(f(x))=f(x) for all x.");
            if (!f.Eq1(fof))
            { // check idempotence of f
                Console.WriteLine("Found a witness on non-idempotence of f:");
                var w = f.Diff(fof); // find a witness where f and fof differ
                string input = w.Input.StringValue; // e.g. "5555"
                string output1 = w.Output1.StringValue; // e.g. f("5555") == "77"
                string output2 = w.Output2.StringValue; // e.f. f("77") == "M"
                Console.WriteLine("f({0})={1}, f(f({0}))={2}", input, output1, output2);
            }
            else
            {
                Console.WriteLine("f is idempotent");
            }
            Z.Dispose();
        }

        static STz3 MkDecodePairsST(Z3Provider Z, STBuilderZ3 stb)
        {
            var S = Z.CharSort;
            var _0 = Z.MkNumeral(0, S);
            var r = stb.MkRegister(S);
            var c = stb.MkInputVariable(S);
            var _5 = Z.MkCharTerm('5');
            var _9 = Z.MkCharTerm('9');
            var _48 = Z.MkCharTerm('0');
            var c_is_digit = Z.MkAnd(Z.MkCharLe(_5, c), Z.MkCharLe(c, _9));
            var not_c_is_digit = Z.MkNot(c_is_digit);
            var rule1 = Rulez3.Mk(not_c_is_digit, r, c);
            var r_is_zero = Z.MkEq(r, _0);
            var rule2 = Rulez3.Mk(Z.MkAnd(r_is_zero, c_is_digit), c);
            var combined = Z.MkCharAdd(Z.MkCharMul(Z.MkNumeral(10, S), Z.MkCharSub(r, _48)), Z.MkCharSub(c, _48));
            var rule3 = Rulez3.Mk(Z.MkAnd(Z.MkNot(r_is_zero), c_is_digit), _0, combined);
            var final = Rulez3.MkFinal(Z.MkNot(r_is_zero), r);
            var moves = new List<Move<Rulez3>>();
            //the finite state component is fixed 0
            moves.Add(Move<Rulez3>.M(0, 0, rule1));
            moves.Add(Move<Rulez3>.M(0, 0, rule2));
            moves.Add(Move<Rulez3>.M(0, 0, rule3));
            moves.Add(Move<Rulez3>.M(0, 0, final));
            var st = stb.MkST("DecodeDigitPairs", _0, S, S, S, 0, moves);
            return st;
        }

        /// <summary>
        /// Illustrates encoding of (a fragment of) HtmlDecode as an ST with two character registers and four Boolean registers.
        /// Shows application of the symbolic Boolean exploration algorithm, that eliminates the Boolean registers only.
        /// </summary>
        static void Sample8()
        {
            Console.WriteLine("Sample 8:  Illustrates encoding and Boolean exploration of (a fragment of) HtmlDecode as an ST with two character registers and four Boolean registers");
            Z3Provider Z = new Z3Provider(); // analysis uses the Z3 provider
            STBuilderZ3 stb = new STBuilderZ3(Z);
            Console.WriteLine("Construct the ST for HtmlDecode.");
            var st = MkHtmlDecodeWithFinalOutputs(stb, Z.CharSort);
            Console.WriteLine("Explore all Booleans in the ST.");
            var st1 = st.ExploreBools();
            st1.Name = st.Name + "_B";
            Console.WriteLine("The resulting ST is saved in {0}.dot(.dgml)", st1.Name);
            st1.SaveAsDot();
            st1.SaveAsDgml();
            Z.Dispose();
        }

        static STz3 MkHtmlDecodeWithFinalOutputs(STBuilderZ3 stb, Sort charSort)
        {

            var z3p = stb.Solver;
            Term tt = z3p.True;
            Term[] eps = new Term[] { };
            List<Move<Rulez3>> rules = new List<Move<Rulez3>>();
            Term x = stb.MkInputVariable(charSort);
            Sort regSort = z3p.MkTupleSort(charSort, charSort);
            //the compound register
            Term yz = stb.MkRegister(regSort);
            //the individual registers
            Term y = z3p.MkProj(0, yz);
            Term z = z3p.MkProj(1, yz);
            //constant characer values
            Term amp = z3p.MkNumeral((int)'&', charSort);
            Term sharp = z3p.MkNumeral((int)'#', charSort);
            Term semi = z3p.MkNumeral((int)';', charSort);
            Term zero = z3p.MkNumeral((int)'0', charSort);
            Term nine = z3p.MkNumeral((int)'9', charSort);
            Term _1 = z3p.MkNumeral(1, charSort);
            Term _0 = z3p.MkNumeral(0, charSort);
            Term _10 = z3p.MkNumeral(10, charSort);
            Term _48 = z3p.MkNumeral(48, charSort);
            //initial register value
            Term _11 = z3p.MkTuple(_1, _1);
            //various terms
            Term xNEQamp = z3p.MkNeq(x, amp);
            Term xEQamp = z3p.MkEq(x, amp);
            Term xNEQsharp = z3p.MkNeq(x, sharp);
            Term xEQsharp = z3p.MkEq(x, sharp);
            Term xNEQsemi = z3p.MkNeq(x, semi);
            Term xEQsemi = z3p.MkEq(x, semi);
            Term xIsDigit = z3p.MkAnd(z3p.MkCharLe(zero, x), z3p.MkCharLe(x, nine));
            Term yIsDigit = z3p.MkAnd(z3p.MkCharLe(zero, y), z3p.MkCharLe(y, nine));
            Term zIsDigit = z3p.MkAnd(z3p.MkCharLe(zero, z), z3p.MkCharLe(z, nine));
            Term yzAreDigits = z3p.MkAnd(yIsDigit, zIsDigit);
            Term xIsNotDigit = z3p.MkNot(xIsDigit);
            Term decode = z3p.MkCharAdd(z3p.MkCharMul(_10, z3p.MkCharSub(y, _48)), z3p.MkCharSub(z, _48));
            //final outputs 
            rules.Add(stb.MkFinalOutput(0, tt));
            rules.Add(stb.MkFinalOutput(1, tt, amp));
            rules.Add(stb.MkFinalOutput(2, tt, amp, sharp));
            rules.Add(stb.MkFinalOutput(3, tt, amp, sharp, y));
            rules.Add(stb.MkFinalOutput(4, tt, amp, sharp, y, z));
            //main rules 
            //rules from state q0
            rules.Add(stb.MkRule(0, 0, xNEQamp, yz, x));
            rules.Add(stb.MkRule(0, 1, xEQamp, yz));
            //rules from state q1
            rules.Add(stb.MkRule(1, 0, z3p.MkAnd(xNEQamp, xNEQsharp), yz, amp, x));
            rules.Add(stb.MkRule(1, 1, xEQamp, yz, amp));
            rules.Add(stb.MkRule(1, 2, xEQsharp, yz));
            //rules from state q2
            rules.Add(stb.MkRule(2, 0, z3p.MkAnd(xNEQamp, xIsNotDigit), yz, amp, sharp, x));
            rules.Add(stb.MkRule(2, 1, xEQamp, yz, amp, sharp));
            rules.Add(stb.MkRule(2, 3, xIsDigit, z3p.MkTuple(x, z)));
            //rules from state q3
            rules.Add(stb.MkRule(3, 0, z3p.MkAnd(xNEQamp, xIsNotDigit), _11, amp, sharp, y, x));
            rules.Add(stb.MkRule(3, 1, xEQamp, _11, amp, sharp, y));
            rules.Add(stb.MkRule(3, 4, xIsDigit, z3p.MkTuple(y, x)));
            //rules from state q4
            rules.Add(stb.MkRule(4, 0, xEQsemi, _11, decode));
            rules.Add(stb.MkRule(4, 0, z3p.MkAnd(xNEQsemi, xNEQamp), _11, amp, sharp, y, z, x));
            rules.Add(stb.MkRule(4, 1, xEQamp, _11, amp, sharp, y, z));

            STz3 st = stb.MkST("HtmlDecode", _11, charSort, charSort, regSort, 0, rules);
            return st;
        }

        /// <summary>
        /// Illustrates an analysis scenario involving the HtmlDecode ST.
        /// </summary>
        static void Sample9()
        {
            Console.WriteLine("Sample 7:  Illustrates analysis of the HtmlDecode ST");
            Z3Provider Z = new Z3Provider(); 
            STBuilderZ3 stb = new STBuilderZ3(Z);
            var st = MkHtmlDecodeWithFinalOutputs(stb, Z.CharSort);
            Console.Write("Full exploration of HtmlDecode ...");
            var sft = st.Explore();
            Console.WriteLine(" produces {0} states and {1} moves", sft.StateCount, sft.MoveCount);
            sft.Name = st.Name + "_F";
            Console.WriteLine("Fully explored version of {0} is saved in {1}.dot(.dgml)", st.Name, sft.Name);
            sft.SaveAsDot();
            sft.SaveAsDgml();
            var st_o_st = st + st;
            Console.WriteLine("Construct the self-composition {1}.dot(.dgml) of {0}", st.Name, st_o_st.Name);
            st_o_st.SaveAsDgml(20); //shorten the visible labels to be max 20 characters 
            st_o_st.SaveAsDot();
            Console.WriteLine("Find a shortest input that shows that HtmlDecode is not idempotent.");
            Console.WriteLine("Assert the theories of {0} and {1} to the solver", st.Name, st_o_st.Name);
            st.AssertTheory();
            st_o_st.AssertTheory();
            Console.WriteLine("Use the axioms to find a shortest input witness that shows nonidempotence of {0}", st.Name);
            var w = st.Diff(st_o_st);
            Console.WriteLine("A witness = {0}", StringUtility.Escape(w.Input.StringValue));
            Console.WriteLine("  {0}(witness) = {1}", st.Name, StringUtility.Escape(w.Output1.StringValue));
            Console.WriteLine("  {0}(witness) = {1}", st_o_st.Name, StringUtility.Escape(w.Output2.StringValue));
            Console.WriteLine("Finally, check that the .Net builtin HtmlDecode produces the same results.");
            string out1_expected = System.Net.WebUtility.HtmlDecode(w.Input.StringValue);
            string out2_expected = System.Net.WebUtility.HtmlDecode(out1_expected);
            Console.WriteLine("Actual output with H = System.Net.WebUtility.HtmlDecode:");
            Console.WriteLine("  H(witness) = {0}", StringUtility.Escape(out1_expected));
            Console.WriteLine("  H(H(witness)) = {0}", StringUtility.Escape(out2_expected));
            if (out1_expected != w.Output1.StringValue || out2_expected != w.Output2.StringValue)
                Console.WriteLine("!!!! the HtmlDecode ST is incorrect compared to H !!!!");
            Console.WriteLine("---------------------");
            string regex = "^[\u0021-\u002F]{3,}$"; 
            Console.WriteLine("Apply range restriction to {0} wrt the regex \"{1}\"", st.Name, regex);
            var rangeRestr = STz3.SFAtoST(new SFAz3(Z, Z.CharSort, Z.RegexConverter.Convert(regex)));
            var sfa = (st + rangeRestr).ToSFA().Determinize().Minimize().NormalizeLabels();
            sfa.Name = st.Name + "_R";
            Console.WriteLine("Save the resulting SFA as {0}.dot(.dgml)", sfa.Name);
            sfa.SaveAsDgml();
            sfa.SaveAsDot();
            sfa.AssertTheory();
            Console.WriteLine("Look for some input accepted by {0}", sfa.Name);
            Term input = Z.MkFreshConst("input", sfa.InputListSort);
            Term acceptor = sfa.MkAccept(input);
            var model = Z.GetModel(acceptor, input);
            Console.WriteLine("e.g. {0}  is in  L({1})", StringUtility.Escape(model[input].StringValue), sfa.Name);
            Z.Dispose();
        }
    }
}
