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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace NAutomaton
{
    public static class BasicOperations
    {
        public static Automaton Concatenate(Automaton a1, Automaton a2)
        {
            if (a1.IsSingleton && a2.IsSingleton)
            {
                return BasicAutomata.MakeString(a1.Singleton + a2.Singleton);
            }
            if (IsEmpty(a1) || IsEmpty(a2))
            {
                return BasicAutomata.MakeEmpty();
            }
            bool deterMinistic = a1.IsSingleton && a2.IsDeterministic;
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
            foreach (State s in a1.AcceptStates)
            {
                s.Accept = false;
                s.AddEpsilon(a2.Initial);
            }
            a1.IsDeterministic = deterMinistic;
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
            bool allSingleton = l.All(a => a.IsSingleton);
            if (allSingleton)
            {
                var b = new StringBuilder();
                foreach (Automaton a in l)
                {
                    b.Append(a.IsSingleton);
                }
                return BasicAutomata.MakeString(b.ToString());
            }
            else
            {
                if (l.Any(a => a.IsEmpty))
                {
                    return BasicAutomata.MakeEmpty();
                }
                var ids = new HashSet<int?>();
                foreach (Automaton a in l)
                {
                    ids.Add(RuntimeHelpers.GetHashCode(a));
                }
                bool hasAliases = ids.Count != l.Count;
                Automaton b = l[0];
                b = hasAliases ? b.CloneExpanded() : b.CloneExpandedIfRequired();
                HashSet<State> ac = b.AcceptStates;
                bool first = true;
                foreach (Automaton a in l)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        if (a.IsEmptyString)
                        {
                            continue;
                        }
                        Automaton aa = a;
                        aa = hasAliases ? aa.CloneExpanded() : aa.CloneExpandedIfRequired();
                        HashSet<State> ns = aa.AcceptStates;
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

        public static Automaton Optional(Automaton a)
        {
            a = a.CloneExpandedIfRequired();
            var s = new State();
            s.AddEpsilon(a.Initial);
            s.Accept = true;
            a.Initial = s;
            a.IsDeterministic = false;
            a.ClearHashCode();
            a.CheckMinimizeAlways();
            return a;
        }

        public static Automaton Repeat(Automaton a)
        {
            a = a.CloneExpanded();
            var s = new State();
            s.Accept = true;
            s.AddEpsilon(a.Initial);
            foreach (State p in a.AcceptStates)
            {
                p.AddEpsilon(s);
            }
            a.Initial = s;
            a.IsDeterministic = false;
            a.ClearHashCode();
            a.CheckMinimizeAlways();
            return a;
        }

        public static Automaton Repeat(Automaton a, int min)
        {
            if (min == 0)
            {
                return Repeat(a);
            }
            IList<Automaton> @as = new List<Automaton>();
            while (min-- > 0)
            {
                @as.Add(a);
            }
            @as.Add(Repeat(a));
            return Concatenate(@as);
        }

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
                IList<Automaton> @as = new List<Automaton>();
                while (min-- > 0)
                {
                    @as.Add(a);
                }
                b = Concatenate(@as);
            }
            if (max > 0)
            {
                Automaton d = a.Clone();
                while (--max > 0)
                {
                    Automaton c = a.Clone();
                    foreach (State p in c.AcceptStates)
                    {
                        p.AddEpsilon(d.Initial);
                    }
                    d = c;
                }
                foreach (State p in b.AcceptStates)
                {
                    p.AddEpsilon(d.Initial);
                }
                b.IsDeterministic = false;
                b.ClearHashCode();
                b.CheckMinimizeAlways();
            }
            return b;
        }

        public static Automaton Complement(Automaton a)
        {
            a = a.CloneExpandedIfRequired();
            a.Determinize();
            a.Totalize();
            foreach (State p in a.States)
            {
                p.Accept = !p.Accept;
            }
            a.RemoveDeadTransitions();
            return a;
        }

        public static Automaton Minus(Automaton a1, Automaton a2)
        {
            if (a1.IsEmpty || a1 == a2)
            {
                return BasicAutomata.MakeEmpty();
            }
            if (a2.IsEmpty)
            {
                return a1.CloneIfRequired();
            }
            if (a1.IsSingleton)
            {
                if (a2.Run(a1.Singleton))
                {
                    return BasicAutomata.MakeEmpty();
                }

                return a1.CloneIfRequired();
            }
            return Intersection(a1, a2.Complement());
        }

        public static Automaton Intersection(Automaton a1, Automaton a2)
        {
            if (a1.IsSingleton)
            {
                if (a2.Run(a1.Singleton))
                {
                    return a1.CloneIfRequired();
                }

                return BasicAutomata.MakeEmpty();
            }
            if (a2.IsSingleton)
            {
                if (a1.Run(a2.Singleton))
                {
                    return a2.CloneIfRequired();
                }

                return BasicAutomata.MakeEmpty();
            }
            if (a1 == a2)
            {
                return a1.CloneIfRequired();
            }
            Transition[][] transitions1 = Automaton.GetSortedTransitions(a1.States);
            Transition[][] transitions2 = Automaton.GetSortedTransitions(a2.States);
            var c = new Automaton();
            var worklist = new LinkedList<StatePair>();
            var newstates = new Dictionary<StatePair, StatePair>();
            var p = new StatePair(c.Initial, a1.Initial, a2.Initial);
            worklist.AddLast(p);
            newstates.Add(p, p);
            while (worklist.Count > 0)
            {
                p = worklist.RemoveAndReturnFirst();
                p.S.Accept = p.S1.Accept && p.S2.Accept;
                Transition[] t1 = transitions1[p.S1.Number];
                Transition[] t2 = transitions2[p.S2.Number];
                for (int n1 = 0, b2 = 0; n1 < t1.Length; n1++)
                {
                    while (b2 < t2.Length && t2[b2].Max < t1[n1].Min)
                    {
                        b2++;
                    }
                    for (int n2 = b2; n2 < t2.Length && t1[n1].Max >= t2[n2].Min; n2++)
                    {
                        if (t2[n2].Max >= t1[n1].Min)
                        {
                            var q = new StatePair(t1[n1].To, t2[n2].To);
                            StatePair r = newstates[q];
                            if (r == null)
                            {
                                q.S = new State();
                                worklist.AddLast(q);
                                newstates.Add(q, q);
                                r = q;
                            }
                            char min = t1[n1].Min > t2[n2].Min ? t1[n1].Min : t2[n2].Min;
                            char max = t1[n1].Max < t2[n2].Max ? t1[n1].Max : t2[n2].Max;
                            p.S.Transitions.Add(new Transition(min, max, r.S));
                        }
                    }
                }
            }
            c.IsDeterministic = a1.IsDeterministic && a2.IsDeterministic;
            c.RemoveDeadTransitions();
            c.CheckMinimizeAlways();
            return c;
        }

        public static bool SubsetOf(Automaton a1, Automaton a2)
        {
            if (a1 == a2)
            {
                return true;
            }
            if (a1.IsSingleton)
            {
                if (a2.IsSingleton)
                {
                    return a1.Singleton.Equals(a2.Singleton);
                }
                return a2.Run(a1.Singleton);
            }
            a2.Determinize();
            Transition[][] transitions1 = Automaton.GetSortedTransitions(a1.States);
            Transition[][] transitions2 = Automaton.GetSortedTransitions(a2.States);
            var worklist = new LinkedList<StatePair>();
            var visited = new HashSet<StatePair>();
            var p = new StatePair(a1.Initial, a2.Initial);
            worklist.AddLast(p);
            visited.Add(p);
            while (worklist.Count > 0)
            {
                p = worklist.RemoveAndReturnFirst();
                if (p.S1.Accept && !p.S2.Accept)
                {
                    return false;
                }
                Transition[] t1 = transitions1[p.S1.Number];
                Transition[] t2 = transitions2[p.S2.Number];
                for (int n1 = 0, b2 = 0; n1 < t1.Length; n1++)
                {
                    while (b2 < t2.Length && t2[b2].Max < t1[n1].Min)
                    {
                        b2++;
                    }
                    int min1 = t1[n1].Min, max1 = t1[n1].Max;
                    for (int n2 = b2; n2 < t2.Length && t1[n1].Max >= t2[n2].Min; n2++)
                    {
                        if (t2[n2].Min > min1)
                        {
                            return false;
                        }
                        if (t2[n2].Max < Char.MaxValue)
                        {
                            min1 = t2[n2].Max + 1;
                        }
                        else
                        {
                            min1 = Char.MaxValue;
                            max1 = Char.MinValue;
                        }
                        var q = new StatePair(t1[n1].To, t2[n2].To);
                        if (!visited.Contains(q))
                        {
                            worklist.AddLast(q);
                            visited.Add(q);
                        }
                    }
                    if (min1 <= max1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static Automaton Union(Automaton a1, Automaton a2)
        {
            if ((a1.IsSingleton && a2.IsSingleton && a1.Singleton.Equals(a2.Singleton)) || a1 == a2)
            {
                return a1.CloneIfRequired();
            }
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
            var s = new State();
            s.AddEpsilon(a1.Initial);
            s.AddEpsilon(a2.Initial);
            a1.Initial = s;
            a1.IsDeterministic = false;
            a1.ClearHashCode();
            a1.CheckMinimizeAlways();
            return a1;
        }

        public static Automaton Union(ICollection<Automaton> l)
        {
            var ids = new HashSet<int>();
            foreach (Automaton automaton in l)
            {
                ids.Add(RuntimeHelpers.GetHashCode(automaton));
            }
            bool hasAliases = ids.Count != l.Count;
            var s = new State();
            foreach (Automaton b in l)
            {
                if (b.IsEmpty)
                {
                    continue;
                }
                Automaton bb = b;
                bb = hasAliases ? bb.CloneExpanded() : bb.CloneExpandedIfRequired();
                s.AddEpsilon(bb.Initial);
            }
            var a = new Automaton();
            a.Initial = s;
            a.IsDeterministic = false;
            a.ClearHashCode();
            a.CheckMinimizeAlways();
            return a;
        }

        public static void Determinize(Automaton a)
        {
            if (a.IsDeterministic || a.IsSingleton)
            {
                return;
            }
            var initialset = new HashSet<State>();
            initialset.Add(a.Initial);
            Determinize(a, initialset);
        }

        public static void Determinize(Automaton a, HashSet<State> initialset)
        {
            char[] points = a.StartPoints;

            IDictionary<HashSet<State>, HashSet<State>> sets = new Dictionary<HashSet<State>, HashSet<State>>(HashSet<State>.CreateSetComparer());

            var worklist = new Stack<HashSet<State>>();
            
            
            IDictionary<HashSet<State>, State> newstate = new Dictionary<HashSet<State>, State>();

            sets.Add(initialset, initialset);
            worklist.Push(initialset);
            a.Initial = new State();
            newstate.Add(initialset, a.Initial);
            while (worklist.Count > 0) {
			HashSet<State> s = worklist.Pop();
			State r = newstate[s];
			foreach (State q in s)
				if (q.Accept) {
					r.Accept = true;
					break;
				}
			for (int n = 0; n < points.Length; n++) {
				var p = new HashSet<State>();
                foreach (State q in s)
                {
                    foreach (Transition t in q.Transitions)
                    {
                        if (t.Min <= points[n] && points[n] <= t.Max)
                        {
                            p.Add(t.To);
                        }
                    }
                }
				if (!sets.ContainsKey(p)) {
					sets.Add(p, p);
					worklist.Push(p);
					newstate.Add(p, new State());
				}

                State q1 = newstate[p];
				char min = points[n];
				char max;
				if (n + 1 < points.Length)
					max = (char)(points[n + 1] - 1);
				else
					max = Char.MaxValue;
                r.Transitions.Add(new Transition(min, max, q1));
			}
		}
		a.IsDeterministic = true;
		a.RemoveDeadTransitions();
	}

        private sealed class HashSetOfStateEqualityComparer : IEqualityComparer<HashSet<State>>
        {
            public bool Equals(HashSet<State> x, HashSet<State> y)
            {
                if (x.Count != y.Count)
                {
                    return false;
                }

                return  x.Intersect(y).Any();
            }

            public int GetHashCode(HashSet<State> obj)
            {
                return obj.Sum(state => state.GetHashCode());
            }
        }

        public static void AddEpsilons(Automaton a, ICollection<StatePair> pairs)
        {
            a.ExpandSingleton();
            var forward = new Dictionary<State, HashSet<State>>();
            var back = new Dictionary<State, HashSet<State>>();
            foreach (StatePair p in pairs)
            {
                HashSet<State> to = forward[p.S1];
                if (to == null)
                {
                    to = new HashSet<State>();
                    forward.Add(p.S1, to);
                }
                to.Add(p.S2);
                HashSet<State> from = back[p.S2];
                if (from == null)
                {
                    from = new HashSet<State>();
                    back.Add(p.S2, from);
                }
                from.Add(p.S1);
            }

            var worklist = new LinkedList<StatePair>(pairs);
            var workset = new HashSet<StatePair>(pairs);
            while (worklist.Count != 0)
            {
                StatePair p = worklist.RemoveAndReturnFirst();
                workset.Remove(p);
                HashSet<State> to = forward[p.S2];
                HashSet<State> from = back[p.S1];
                if (to != null)
                {
                    foreach (State s in to)
                    {
                        var pp = new StatePair(p.S1, s);
                        if (!pairs.Contains(pp))
                        {
                            pairs.Add(pp);
                            forward[p.S1].Add(s);
                            back[s].Add(p.S1);
                            worklist.AddLast(pp);
                            workset.Add(pp);
                            if (from != null)
                            {
                                foreach (State q in from)
                                {
                                    var qq = new StatePair(q, p.S1);
                                    if (!workset.Contains(qq))
                                    {
                                        worklist.AddLast(qq);
                                        workset.Add(qq);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // Add transitions.
            foreach (StatePair p in pairs)
            {
                p.S1.AddEpsilon(p.S2);
            }
            a.IsDeterministic = false;
            a.ClearHashCode();
            a.CheckMinimizeAlways();
        }

        public static bool IsEmptyString(Automaton a)
        {
            if (a.IsSingleton)
            {
                return a.Singleton.Length == 0;
            }

            return a.Initial.Accept && a.Initial.Transitions.Count == 0;
        }

        public static bool IsEmpty(Automaton a)
        {
            if (a.IsSingleton)
            {
                return false;
            }
            return !a.Initial.Accept && a.Initial.Transitions.Count == 0;
        }

        public static bool IsTotal(Automaton a)
        {
            if (a.IsSingleton)
            {
                return false;
            }
            if (a.Initial.Accept && a.Initial.Transitions.Count == 1)
            {
                // Original code (Java): Transition t = a.initial.transitions.iterator().next();
                HashSet<Transition>.Enumerator enumerator = a.Initial.Transitions.GetEnumerator();
                enumerator.MoveNext();
                Transition t = enumerator.Current;

                return t.To == a.Initial && t.Min == Char.MinValue && t.Max == Char.MaxValue;
            }
            return false;
        }

        public static string GetShortestExample(Automaton a, bool accepted)
        {
            if (a.IsSingleton)
            {
                if (accepted)
                {
                    return a.Singleton;
                }

                if (a.Singleton.Length > 0)
                {
                    return "";
                }

                return "\u0000";
            }
            return GetShortestExample(a.Initial, accepted);
        }

        public static string GetShortestExample(State s, bool accepted)
        {
            IDictionary<State, string> path = new Dictionary<State, string>();
            var queue = new LinkedList<State>();
            path.Add(s, "");
            queue.AddLast(s);
            string best = null;
            while (queue.Count != 0)
            {
                State q = queue.RemoveAndReturnFirst();
                string p = path[q];
                if (q.Accept == accepted)
                {
                    if (best == null || p.Length < best.Length || (p.Length == best.Length && p.CompareTo(best) < 0))
                    {
                        best = p;
                    }
                }
                else
                {
                    foreach (Transition t in q.Transitions)
                    {
                        string tp = path[t.To];
                        string np = p + t.Min;
                        if (tp == null || (tp.Length == np.Length && np.CompareTo(tp) < 0))
                        {
                            if (tp == null)
                            {
                                queue.AddLast(t.To);
                            }
                            path.Add(t.To, np);
                        }
                    }
                }
            }
            return best;
        }

        public static bool Run(Automaton a, string s)
        {
            if (a.IsSingleton)
            {
                return s.Equals(a.IsSingleton);
            }
            if (a.IsDeterministic)
            {
                State p = a.Initial;
                foreach (char t in s)
                {
                    State q = p.Step(t);
                    if (q == null)
                    {
                        return false;
                    }
                    p = q;
                }
                return p.Accept;
            }

            HashSet<State> states = a.States;
            Automaton.StateNumbers = states;
            var pp = new LinkedList<State>();
            var ppOther = new LinkedList<State>();
            var bb = new BitArray(states.Count);
            var bbOther = new BitArray(states.Count);
            pp.AddLast(a.Initial);
            var dest = new List<State>();
            bool accept = a.Initial.Accept;

            foreach (char c in s)
            {
                accept = false;
                ppOther.Clear();
                bbOther.SetAll(false);
                foreach (State p in pp)
                {
                    dest.Clear();
                    p.Step(c, dest);
                    foreach (State q in dest)
                    {
                        if (q.Accept)
                        {
                            accept = true;
                        }
                        if (!bbOther.Get(q.Number))
                        {
                            bbOther.Set(q.Number, true);
                            ppOther.AddLast(q);
                        }
                    }
                }
                LinkedList<State> tp = pp;
                pp = ppOther;
                ppOther = tp;
                BitArray tb = bb;
                bb = bbOther;
                bbOther = tb;
            }
            return accept;
        }
    }
}