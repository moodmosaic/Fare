/*
 * dk.brics.automaton
 * 
 * Copyright (c) 2001-2011 Anders Moeller
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace NAutomaton
{
    public static class BasicOperations
    {
        /// <summary>
        /// Returns an automaton that accepts the union of the languages of the given automata.
        /// </summary>
        /// <param name="automatons">The l.</param>
        /// <returns>
        /// An automaton that accepts the union of the languages of the given automata.
        /// </returns>
        /// <remarks>
        /// Complexity: linear in number of states.
        /// </remarks>
        public static Automaton Union(IList<Automaton> automatons)
        {
            var ids = new HashSet<int>();
            foreach (Automaton a in automatons)
            {
                ids.Add(RuntimeHelpers.GetHashCode(a));
            }

            bool hasAliases = ids.Count != automatons.Count;
            var s = new State();
            foreach (Automaton b in automatons)
            {
                if (b.IsEmpty)
                {
                    continue;
                }

                Automaton bb = b;
                bb = hasAliases ? bb.CloneExpanded() : bb.CloneExpandedIfRequired();

                s.AddEpsilon(bb.Initial);
            }

            var automaton = new Automaton();
            automaton.Initial = s;
            automaton.IsDeterministic = false;
            automaton.ClearHashCode();
            automaton.CheckMinimizeAlways();
            return automaton;
        }

        public static Automaton Complement(Automaton automaton)
        {
            throw new NotImplementedException();
        }

        public static Automaton Concatenate(Automaton a1, Automaton a2)
        {
            if (a1.IsSingleton && a2.IsSingleton)
            {
                return BasicAutomata.MakeString(a1.Singleton + a2.Singleton);
            }

            if (BasicOperations.IsEmpty(a1) || BasicOperations.IsEmpty(a2))
            {
                return BasicAutomata.MakeEmpty();
            }

            bool deterministic = a1.IsSingleton && a2.IsDeterministic;
            if (a1 == a2)
            {
                a1 = a1.CloneExpanded();
                a2 = a2.CloneExpanded();
            }
            else
            {
                a1 = a1.CloneExpandedIfRequired();
                a2 = a2.CloneExpandedIfRequired();
            }

            foreach (State s in a1.GetAcceptStates())
            {
                s.Accept = false;
                s.AddEpsilon(a2.Initial);
            }

            a1.IsDeterministic = deterministic;
            a1.ClearHashCode();
            a1.CheckMinimizeAlways();
            return a1;
        }

        public static Automaton Concatenate(IList<Automaton> l)
        {
            if (l.Count == 0)
            {
                return BasicAutomata.MakeEmptyString();
            }

            bool allSingleton = true;
            foreach (Automaton a in l)
            {
                if (!a.IsSingleton)
                {
                    allSingleton = false;
                    break;
                }
            }

            if (allSingleton)
            {
                var b = new StringBuilder();
                foreach (Automaton a in l)
                {
                    b.Append(a.Singleton);
                }

                return BasicAutomata.MakeString(b.ToString());
            }
            else
            {
                foreach (Automaton a in l)
                {
                    if (a.IsEmpty)
                    {
                        return BasicAutomata.MakeEmpty();
                    }
                }

                var ids = new HashSet<int>();
                foreach (Automaton a in l)
                {
                    ids.Add(RuntimeHelpers.GetHashCode(a));
                }

                bool hasAliases = ids.Count != l.Count;
                Automaton b = l[0];
                if (hasAliases)
                {
                    b = b.CloneExpanded();
                }
                else
                {
                    b = b.CloneExpandedIfRequired();
                }

                var ac = b.GetAcceptStates();
                bool first = true;
                foreach (Automaton a in l)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        if (a.IsEmptyString())
                        {
                            continue;
                        }

                        Automaton aa = a;
                        if (hasAliases)
                        {
                            aa = aa.CloneExpanded();
                        }
                        else
                        {
                            aa = aa.CloneExpandedIfRequired();
                        }

                        HashSet<State> ns = aa.GetAcceptStates();
                        foreach (State s in ac)
                        {
                            s.Accept = false;
                            s.AddEpsilon(aa.Initial);
                            if (s.Accept)
                            {
                                ns.Add(s);
                            }
                        }

                        ac = ns;
                    }
                }

                b.IsDeterministic = false;
                b.ClearHashCode();
                b.CheckMinimizeAlways();
                return b;
            }
        }

        /// <summary>
        /// Determinizes the specified automaton.
        /// </summary>
        /// <remarks>
        /// Complexity: exponential in number of states.
        /// </remarks>
        /// <param name="a">The automaton.</param>
        public static void Determinize(Automaton a)
        {
            if (a.IsDeterministic || a.IsSingleton)
            {
                return;
            }

            var initialset = new HashSet<State>();
            initialset.Add(a.Initial);
            BasicOperations.Determinize(a, initialset.ToList());
        }

        /// <summary>
        /// Determinizes the given automaton using the given set of initial states.
        /// </summary>
        /// <param name="a">The automaton.</param>
        /// <param name="initialset">The initial states.</param>
        private static void Determinize(Automaton a, List<State> initialset)
        {
            char[] points = a.GetStartPoints();

            var comparer = new ListOfStateComparer();

            // Subset construction.
            var sets = new Dictionary<List<State>, List<State>>(comparer);
            var worklist = new LinkedList<List<State>>();
            var newstate = new Dictionary<List<State>, State>();

            sets.Add(initialset, initialset);
            worklist.AddLast(initialset);
            a.Initial = new State();
            newstate.Add(initialset, a.Initial);

            while (worklist.Count > 0)
            {
                List<State> s = worklist.RemoveAndReturnFirst();
                State r;
                newstate.TryGetValue(s, out r);
                foreach (State q in s)
                {
                    if (q.Accept)
                    {
                        r.Accept = true;
                        break;
                    }
                }

                for (int n = 0; n < points.Length; n++)
                {
                    var p = (from qq in s from t in qq.Transitions where t.Min <= points[n] && points[n] <= t.Max select t.To).ToList();

                    if (!sets.ContainsKey(p))
                    {
                        sets.Add(p, p);
                        worklist.AddLast(p);
                        newstate.Add(p, new State());
                    }

                    State q;
                    newstate.TryGetValue(p, out q);
                    char min = points[n];
                    char max;
                    if (n + 1 < points.Length)
                    {
                        max = (char)(points[n + 1] - 1);
                    }
                    else
                    {
                        max = char.MaxValue;
                    }

                    r.Transitions.Add(new Transition(min, max, q));
                }
            }

            a.IsDeterministic = true;
            a.RemoveDeadTransitions();
        }

        private sealed class ListOfStateComparer : IEqualityComparer<List<State>>
        {
            public bool Equals(List<State> x, List<State> y)
            {
                if (x.Count != y.Count)
                {
                    return false;
                }

                return x.SequenceEqual(y);
            }

            public int GetHashCode(List<State> obj)
            {
                return obj.Sum(o => o.GetHashCode());
            }
        }

        /// <summary>
        /// Determines whether the given automaton accepts no strings.
        /// </summary>
        /// <param name="a">The automaton.</param>
        /// <returns>
        ///   <c>true</c> if the given automaton accepts no strings; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(Automaton a)
        {
            if (a.IsSingleton)
            {
                return false;
            }

            return !a.Initial.Accept && a.Initial.Transitions.Count == 0;
        }

        /// <summary>
        /// Determines whether the given automaton accepts the empty string and nothing else.
        /// </summary>
        /// <param name="a">The automaton.</param>
        /// <returns>
        ///   <c>true</c> if the given automaton accepts the empty string and nothing else; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmptyString(Automaton a)
        {
            if (a.IsSingleton)
            {
                return a.Singleton.Length == 0;
            }

            return a.Initial.Accept && a.Initial.Transitions.Count == 0;
        }

        public static Automaton Intersection(Automaton automaton, Automaton a)
        {
            throw new NotImplementedException();
        }

        public static Automaton Optional(Automaton automaton)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Accepts the Kleene star (zero or more concatenated repetitions) of the language of the
        /// given automaton. Never modifies the input automaton language.
        /// </summary>
        /// <param name="a">The automaton.</param>
        /// <returns>
        /// An automaton that accepts the Kleene star (zero or more concatenated repetitions)
        /// of the language of the given automaton. Never modifies the input automaton language.
        /// </returns>
        /// <remarks>
        /// Complexity: linear in number of states.
        /// </remarks>
        public static Automaton Repeat(Automaton a)
        {
            a = a.CloneExpanded();
            var s = new State();
            s.Accept = true;
            s.AddEpsilon(a.Initial);
            foreach (State p in a.GetAcceptStates())
            {
                p.AddEpsilon(s);
            }

            a.Initial = s;
            a.IsDeterministic = false;
            a.ClearHashCode();
            a.CheckMinimizeAlways();
            return a;
        }

        /// <summary>
        /// Accepts <code>min</code> or more concatenated repetitions of the language of the given 
        /// automaton.
        /// </summary>
        /// <param name="a">The automaton.</param>
        /// <param name="min">The minimum concatenated repetitions of the language of the given 
        /// automaton.</param>
        /// <returns>Returns an automaton that accepts <code>min</code> or more concatenated 
        /// repetitions of the language of the given automaton.
        /// </returns>
        /// <remarks>
        /// Complexity: linear in number of states and in <code>min</code>.
        /// </remarks>
        public static Automaton Repeat(Automaton a, int min)
        {
            if (min == 0)
            {
                return BasicOperations.Repeat(a);
            }

            var @as = new List<Automaton>();
            while (min-- > 0)
            {
                @as.Add(a);
            }
            
            @as.Add(BasicOperations.Repeat(a));
            return BasicOperations.Concatenate(@as);
        }

        /// <summary>
        /// Accepts between <code>min</code> and <code>max</code> (including both) concatenated
        /// repetitions of the language of the given automaton.
        /// </summary>
        /// <param name="a">The automaton.</param>
        /// <param name="min">The minimum concatenated repetitions of the language of the given
        /// automaton.</param>
        /// <param name="max">The maximum concatenated repetitions of the language of the given
        /// automaton.</param>
        /// <returns>
        /// Returns an automaton that accepts between <code>min</code> and <code>max</code>
        /// (including both) concatenated repetitions of the language of the given automaton.
        /// </returns>
        /// <remarks>
        /// Complexity: linear in number of states and in <code>min</code> and <code>max</code>.
        /// </remarks>
        public static Automaton Repeat(Automaton a, int min, int max)
        {
            if (min > max)
            {
                return BasicAutomata.MakeEmpty();
            }

            max -= min;
            a.ExpandSingleton();
            Automaton b;
            if (min == 0)
            {
                b = BasicAutomata.MakeEmptyString();
            }
            else if (min == 1)
            {
                b = a.Clone();
            }
            else
            {
                var @as = new List<Automaton>();
                while (min-- > 0)
                {
                    @as.Add(a);
                }

                b = BasicOperations.Concatenate(@as);
            }

            if (max > 0)
            {
                Automaton d = a.Clone();
                while (--max > 0)
                {
                    Automaton c = a.Clone();
                    foreach (State p in c.GetAcceptStates())
                    {
                        p.AddEpsilon(d.Initial);
                    }

                    d = c;
                }

                foreach (State p in b.GetAcceptStates())
                {
                    p.AddEpsilon(d.Initial);
                }

                b.IsDeterministic = false;
                b.ClearHashCode();
                b.CheckMinimizeAlways();
            }

            return b;
        }
    }
}
