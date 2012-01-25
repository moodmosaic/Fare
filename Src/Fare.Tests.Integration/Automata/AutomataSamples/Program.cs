/*
 *  Samples that illustrate typical use of the library Microsoft.Automata.dll
 */

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Automata;

namespace Microsoft.Automata.ReleaseSamples
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
        /// Illustrates creation of automata from .Net regexes and member generation.
        /// </summary>
        static void Sample1()
        {
            Console.WriteLine("Sample 1: creation of automata from .Net regexes and member generation");
            var solver = new CharSetSolver(CharacterEncoding.ASCII);  //new solver using ASCII encoding
            string r = @"^[A-Za-z0-9]+@(([A-Za-z0-9\-])+\.)+([A-Za-z\-])+$";   // regex for "almost" valid emails
            Console.WriteLine("Create automaton A for regex: {0}", r);
            var A = solver.Convert(r);                    //accepts strings that match the regex r
            Console.WriteLine("----- generate 10 members in A ------");
            int seed = solver.Chooser.RandomSeed;         //record the used random seed
            for (int i = 0; i < 10; i++)                  //generate 10 random members
                Console.WriteLine(solver.GenerateMember(A));
            Console.WriteLine("----- reset the random seed and repeat the generation of 10 members from A ------");
            solver.Chooser.RandomSeed = seed;             //reset the seed 
            for (int i = 0; i < 10; i++)                  //repeat the member generation
                Console.WriteLine(solver.GenerateMember(A));
        }

        /// <summary>
        /// Illustrates epsilon elimination and serialization
        /// </summary>
        static void Sample2()
        {
            Console.WriteLine("Sample 2: epsilon elimination and saving automata in .dot and .dgml formats.");
            var solver = new CharSetSolver(CharacterEncoding.ASCII);  //new solver using ASCII encoding
            string r = @"^[A-Za-z0-9]+@(([A-Za-z0-9\-])+\.)+([A-Za-z\-])+$";   // regex for "almost" valid emails
            Console.WriteLine("Create automaton A for regex: {0}", r);
            var A = solver.Convert(r);                  //accepts strings that match the regex r
            Console.WriteLine("----- save A as Sample2a.dot(.dgml) ------");
            solver.SaveAsDot(A, "Sample2a.dot");        //save the automaton in dot format
            solver.SaveAsDgml(A, "Sample2a.dgml");      //save the automaton in dgml format
            Console.WriteLine("Construct B as a result of removing epsilons from A.");
            var B = A.RemoveEpsilons(solver.MkOr);          //remove epsilons from A, uses disjunction of character sets to combine transitions
            Console.WriteLine("----- save B as Sample2b.dot(.dgml) ------");
            solver.SaveAsDot(B, "Sample2b.dot");
            solver.SaveAsDgml(B, "Sample2b.dgml");      //save the automaton in dgml format
        }

        /// <summary>
        /// Illustrates automata algorithms for difference, equivalence, determinization and minimization
        /// </summary>
        static void Sample3()
        {
            Console.WriteLine("Sample 3: difference, equivalence, determinization and minimization.");
            var solver = new CharSetSolver(CharacterEncoding.Unicode);//charset solver
            string a = @"^[A-Za-z0-9]+@(([A-Za-z0-9\-])+\.)+([A-Za-z\-])+$";    //.Net regex
            string b = @"^\d.*$";                                               //.Net regex
            Console.WriteLine("Create automaton A for regex: {0}", a);
            Automaton<CharSet> A = solver.Convert(a);    //create the equivalent automata
            Console.WriteLine("Create automaton B for regex: {0}", b);
            Automaton<CharSet> B = solver.Convert(b);
            Console.WriteLine("Create automaton C for A-B");
            Automaton<CharSet> C = A.Minus(B, solver);       //construct the difference 
            Console.WriteLine("----- save C as Sample3a.dot(.dgml) ------");
            solver.SaveAsDot(C, "Sample3a.dot");             //save in dot format
            solver.SaveAsDgml(C, "Sample3a.dgml");
            Console.WriteLine("Create automaton M for Minimize(Determinize(C))");
            var M = C.Determinize(solver).Minimize(solver);  //determinize and then minimize the automaton
            Console.WriteLine("----- save M in Sample3b.dot(.dgml) ------");
            solver.SaveAsDot(M, "Sample3b.dot");
            solver.SaveAsDgml(M, "Sample3b.dgml");
            Console.WriteLine("Check equivalence of C and M, result:{0}", C.IsEquivalentWith(M, solver)); //check equivalence of C and M
        }

        /// <summary>
        /// Illustrates two regexes that create large automata.
        /// </summary>
        static void Sample4()
        {
            Console.WriteLine("Sample 4: two regexes that create large automata..");
            var solver = new CharSetSolver(CharacterEncoding.Unicode);
            string a = @"^((\w|\d|\-|\.)+)@{1}(((\w|\d|\-){1,67})|((\w|\d|\-)+\.(\w|\d|\-){1,67}))\.((([a-z]|[A-Z]|\d){2,4})(\.([a-z]|A|Z|\d){2})?)\z";
            string b = @"^((([A-Z]|[a-z]|[0-9]|\-|\.)+)@(([A-Z]|[a-z]|[0-9]|\-|\.)+)\.(([A-Z]|[a-z]){2,5}){1,25})+(((([A-Z]|[a-z]|[0-9]|\-|\.)+)@(([A-Z]|[a-z]|[0-9]|\-|\.)+)\.(([A-Z]|[a-z]){2,5}){1,25})+)*\z";
            Console.WriteLine("Create automaton A for regex: {0}", a);
            Automaton<CharSet> A = solver.Convert(a);
            Console.WriteLine("Create automaton B for regex: {0}", b);
            Automaton<CharSet> B = solver.Convert(b);
            Console.WriteLine("----- save A as Sample4a.dot(.dgml) ------");
            Console.WriteLine("Sample4a has {0} states and {1} moves.", A.StateCount, A.MoveCount);
            solver.SaveAsDot(A, "Sample4a.dot");
            solver.SaveAsDgml(A, "Sample4a.dgml");
            Console.WriteLine("Generate 10 members in A:");
            for (int i = 0; i < 10; i++)
                Console.WriteLine(StringUtility.Escape(solver.GenerateMember(A)));
            Console.WriteLine("----- save B as Sample4b.dot(.dgml) ------");
            Console.WriteLine("Sample4b has {0} states and {1} moves.", B.StateCount, B.MoveCount);
            solver.SaveAsDot(B, "Sample4b.dot");
            solver.SaveAsDgml(B, "Sample4b.dgml");
            Console.WriteLine("Generate 10 members in B:");
            for (int i = 0; i < 10; i++)
                Console.WriteLine(StringUtility.Escape(solver.GenerateMember(B)));
        }

        /// <summary>
        /// Illustrates difference checking with witness generation applied to some typical regexes
        /// from the file regexes.txt.
        /// </summary>
        static void Sample5()
        {
            Console.WriteLine("Sample 5: difference checking of regexes in regexes.txt");
            var solver = new CharSetSolver(CharacterEncoding.Unicode);
            string[] regexes = System.IO.File.ReadAllLines("regexes.txt");

            for (int i = 0; i < regexes.Length; i++)
                for (int j = 0; j < regexes.Length; j++)
                    if (i != j)
                    {
                        var A = solver.Convert(regexes[i]);
                        var B = solver.Convert(regexes[j]);
                        List<CharSet> witness;
                        bool diff = Automaton<CharSet>.CheckDifference(A, B, 0, solver, out witness);
                        if (diff)
                        {
                            string w = solver.ChooseString(witness);
                            Console.WriteLine("member(regex[{0}]-regex[{1}]): {2}", i, j, StringUtility.Escape(w));
                        }
                    }
        }

        /// <summary>
        /// Converts all the regexes in regexes.txt into automata, eliminates epsilons and saves them in dot and dgml format.
        /// </summary>
        static void Sample6()
        {
            Console.WriteLine("Sample 6: onverts all the regexes in regexes.txt into automata withpout epsilons");
            var solver = new CharSetSolver(CharacterEncoding.Unicode);
            string[] regexes = System.IO.File.ReadAllLines("regexes.txt");
            for (int i = 0; i < regexes.Length; i++)
            {
                var A = solver.Convert(regexes[i]).RemoveEpsilons(solver.MkOr);
                string file = "Sample6_" + i;
                Console.WriteLine("----- save the automaton for regex {0} as Sample6_{0}.dot(.dgml)------", i);
                Console.WriteLine("the automaton for regex {0} has {1} states and {2} moves", i, A.StateCount, A.MoveCount);
                solver.SaveAsDot(A, file);   //extension .dot  is added automatically when missing
                solver.SaveAsDgml(A, file);  //extension .dgml is added automatically when missing
            }
        }

        /// <summary>
        /// Illustrates the product construct, determinization and minimization of for two large automata.
        /// </summary>
        static void Sample7()
        {
            Console.WriteLine("Sample 7: product construct for two large automata");
            var solver = new CharSetSolver(CharacterEncoding.Unicode);
            string a = @"^((\w|\d|\-|\.)+)@{1}(((\w|\d|\-){1,67})|((\w|\d|\-)+\.(\w|\d|\-){1,67}))\.((([a-z]|[A-Z]|\d){2,4})(\.([a-z]|A|Z|\d){2})?)\z";
            string b = @"^((([A-Z]|[a-z]|[0-9]|\-|\.)+)@(([A-Z]|[a-z]|[0-9]|\-|\.)+)\.(([A-Z]|[a-z]){2,5}){1,25})+(((([A-Z]|[a-z]|[0-9]|\-|\.)+)@(([A-Z]|[a-z]|[0-9]|\-|\.)+)\.(([A-Z]|[a-z]){2,5}){1,25})+)*\z";
            Console.WriteLine("Converting regex {0} to automaton A...", a);
            Automaton<CharSet> A = solver.Convert(a);
            Console.WriteLine("A has {0} states and {1} moves.", A.StateCount, A.MoveCount);
            Console.WriteLine("Converting regex {0} to automaton B...", b);
            Automaton<CharSet> B = solver.Convert(b);
            Console.WriteLine("B has {0} states and {1} moves.", B.StateCount, B.MoveCount);
            Console.WriteLine("Constructing the product AxB...");
            var AxB = A.Intersect(B, solver);
            Console.WriteLine("AxB has {0} states and {1} moves.", AxB.StateCount, AxB.MoveCount);
            Console.WriteLine("Generating 10 members in AxB:");
            for (int i = 0; i < 10; i++)
                Console.WriteLine(StringUtility.Escape(solver.GenerateMember(AxB)));
            Console.WriteLine("Determinizing and mimimizing AxB...");
            var AxBmin = AxB.Determinize(solver).Minimize(solver);
            Console.WriteLine("AxBmin has {0} states and {1} moves.", AxBmin.StateCount, AxBmin.MoveCount);
            Console.WriteLine("save AxBmin in Sample7.dot(.dgml)");
            solver.SaveAsDot(AxBmin, "Sample7.dot");
            solver.SaveAsDgml(AxBmin, "Sample7.dgml");
        }

        /// <summary>
        /// Illustrates the functionality of the minterm generation algorithm.
        /// </summary>
        static void Sample8()
        {
            Console.WriteLine("Sample 8: shows functionality of the minterm generation algorithm.");
            CharSetSolver solver = new CharSetSolver(CharacterEncoding.ASCII);
            Console.WriteLine("Suppose A, B, C, D are the following character sets:");
            Console.WriteLine("  A = {'1','2','3','4'}");
            //A = {'1','2','3','4'}
            CharSet A = solver.MkRangeConstraint(false, '1', '4');
            Console.WriteLine("  B = {'2','3','5','6','8'}");
            //B = {'2','3','5','6','8'}
            CharSet B = solver.MkRangesConstraint(false, new char[][] { new char[] { '2', '3' }, new char[] { '5', '6' }, new char[] { '8', '8' } });
            //C = {'3','4','6','7','9'}
            Console.WriteLine("  C = {'3','4','6','7','9'}");
            CharSet C = solver.MkRangesConstraint(false, new char[][] { new char[] { '3', '4' }, new char[] { '6', '7' }, new char[] { '9', '9' } });
            //D = {'0','8','9'}
            Console.WriteLine("  D = {'0','8','9'}");
            CharSet D = solver.MkRangesConstraint(false, new char[][] { new char[] { '0', '0' }, new char[] { '8', '9' } });

            //all minterms or nonepty areas of the Venn diagram, with sets A, B, C, D, 
            //e.g. one minterm is complement(A U B U C U D) that is all non-digits
            //another minterm is the intersection of A, B and C, that is {'3'},
            //in total there is one minterm of each digit, and a minterm for all non-digits, thus 11 minterms in total
            var combinations = new List<Pair<bool[], CharSet>>(solver.GenerateMinterms(new CharSet[] { A, B, C, D }));
            Console.WriteLine("There are {0} nonempty minterms wrt (A,B,C,D).", combinations.Count);
            Console.WriteLine("The minterms are:");
            foreach (var c in combinations)
            {
                string minterm = (c.First[0] ? "(A," : "(!A,") + (c.First[1] ? "B," : "!B,") + (c.First[2] ? "C," : "!C,") + (c.First[3] ? "D)" : "!D)");
                string content = new String(new List<char>(solver.GenerateAllMembers(c.Second, false)).ToArray());
                Console.WriteLine(minterm + ":" + StringUtility.Escape(content));
            }
        }

       /// <summary>
        /// Illustrates how to define and use a custom Boolean agebra solver.
        /// </summary>
        static void Sample9()
        {
            Console.WriteLine("Sample 9: how to define and use a custom Boolean agebra solver");
            Console.WriteLine("A set of symbols is represented by MySet(uint s) where the i'th bit of s = 1 means that s contains the i'th symbol");
            var solver = new MyBoolAlg(); //custom solver
            //create a simple Automaton using MySet's as labels
            var movesA = new List<Move<MySet>>();
            movesA.Add(Move<MySet>.M(0, 1, new MySet(0xF))); //contains symbols nr 0..3, bit i means the i'th symbol is in the set
            movesA.Add(Move<MySet>.M(1, 1, new MySet(0xF0))); //contains symbols nr 4..7
            var A = Automaton<MySet>.Create(0, new int[] { 1 }, movesA);
            Console.WriteLine("Construct Automaton<MySet> A with two states 0 and 1, 0 is the initial state, 1 is the final state.");
            Console.WriteLine("A has moves:");
            foreach (var move in A.GetMoves())
            {
                Console.WriteLine("  {0} --{1}--> {2}", move.SourceState, move.Condition, move.TargetState);
            }

            var movesB = new List<Move<MySet>>();
            movesB.Add(Move<MySet>.M(0, 0, new MySet(0x3c))); //contains symbols nr 2..5
            var B = Automaton<MySet>.Create(0, new int[] { 0 }, movesB);
            Console.WriteLine("Construct Automaton<MySet> B with a single state 0 that is both initial and final.");
            Console.WriteLine("B has moves:");
            foreach (var move in A.GetMoves())
            {
                Console.WriteLine("  {0} --{1}--> {2}", move.SourceState, move.Condition, move.TargetState);
            }


            //make an intersection of A and B
            var AxB = A.Intersect(B, solver);
            //AxB should have the moves (0 --MysSet(0xC)--> 1) and (1 --MysSet(0x30)--> 1)
            Console.WriteLine("Construct the product A x B, the moves are:");
            foreach (var move in AxB.GetMoves())
            {
                Console.WriteLine("{0} --{1}--> {2}", move.SourceState, move.Condition, move.TargetState);
            }

            //make the difference of A - B
            var AminB = A.Minus(B, solver);
            Console.WriteLine("Construct the difference A - B, the moves are:");
            foreach (var move in AminB.GetMoves())
            {
                Console.WriteLine("{0} --{1}--> {2}", move.SourceState, move.Condition, move.TargetState);
            }

            //make the difference of B - A
            var BminA = B.Minus(A, solver);
            Console.WriteLine("Construct the difference B - A, the moves are:");
            foreach (var move in BminA.GetMoves())
            {
                Console.WriteLine("{0} --{1}--> {2}", move.SourceState, move.Condition, move.TargetState);
            }
        }

        //--- Sample custom Boolean algebra solver ---

        /// <summary>
        /// Represents a vocabulary of 32 sybmols by using uint.
        /// The i'th symbol is in the set iff the corresponding bit is set.
        /// </summary>
        internal class MySet
        {
            uint elems;

            static internal MySet Empty = new MySet(0);
            static internal MySet Full = new MySet(uint.MaxValue);

            internal MySet(uint elems)
            {
                this.elems = elems;
            }

            public override bool Equals(object obj)
            {
                MySet s = obj as MySet;
                if (s == null)
                    return false;
                return elems == s.elems;
            }

            public override int GetHashCode()
            {
                return (int)elems;
            }

            public MySet Union(MySet s)
            {
                return new MySet(elems | s.elems);
            }

            public MySet Intersect(MySet s)
            {
                return new MySet(elems & s.elems);
            }

            public MySet Complement()
            {
                return new MySet(~elems);
            }

            public override string ToString()
            {
                return string.Format("MySet(0x{0})", elems.ToString("X"));
            }
        }

        /// <summary>
        /// Boolean algebra with minterm generation over MySet's
        /// </summary>
        internal class MyBoolAlg : IBoolAlgMinterm<MySet>
        {

            MintermGenerator<MySet> mtg; //use the default built-in minterm generator
            internal MyBoolAlg()
            {
                //create the minterm generator for this solver
                mtg = new MintermGenerator<MySet>(this);
            }

            public bool AreEquivalent(MySet predicate1, MySet predicate2)
            {
                return predicate1.Equals(predicate2);
            }

            public MySet False
            {
                get { return MySet.Empty; }
            }

            public MySet MkAnd(IEnumerable<MySet> predicates)
            {
                MySet res = MySet.Full;
                foreach (var s in predicates)
                    res = res.Intersect(s);
                return res;
            }

            public MySet MkNot(MySet predicate)
            {
                return predicate.Complement();
            }

            public MySet MkOr(IEnumerable<MySet> predicates)
            {
                MySet res = MySet.Empty;
                foreach (var s in predicates)
                    res = res.Union(s);
                return res;
            }

            public MySet True
            {
                get { return MySet.Full; }
            }

            public bool IsSatisfiable(MySet predicate)
            {
                return !predicate.Equals(MySet.Empty);
            }

            public MySet MkAnd(MySet predicate1, MySet predicate2)
            {
                return predicate1.Intersect(predicate2);
            }

            public MySet MkOr(MySet predicate1, MySet predicate2)
            {
                return predicate1.Union(predicate2);
            }

            public IEnumerable<Pair<bool[], MySet>> GenerateMinterms(params MySet[] constraints)
            {
                return mtg.GenerateMinterms(constraints);
            }
        }
    }
}
