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

        public static Automaton Concatenate(IList<Automaton> list)
        {
            throw new NotImplementedException();
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

        public static Automaton Complement(Automaton automaton)
        {
            throw new NotImplementedException();
        }

        public static Automaton Intersection(Automaton automaton, Automaton a)
        {
            throw new NotImplementedException();
        }

        public static Automaton Optional(Automaton automaton)
        {
            throw new NotImplementedException();
        }

        public static Automaton Repeat(Automaton automaton)
        {
            throw new NotImplementedException();
        }

        public static Automaton Repeat(Automaton automaton, int min)
        {
            throw new NotImplementedException();
        }

        public static Automaton Repeat(Automaton automaton, int min, int max)
        {
            throw new NotImplementedException();
        }
    }
}
