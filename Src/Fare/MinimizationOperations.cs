/*
 * dk.brics.automaton
 *
 * Copyright (c) 2001-2011 Anders Moeller
 * All rights reserved.
 * http://github.com/moodmosaic/Fare/
 * Original Java code:
 * http://www.brics.dk/automaton/
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

using System.Collections.Generic;
using System.Linq;

namespace Fare
{
    public static class MinimizationOperations
    {
        /// <summary>
        /// Minimizes (and determinizes if not already deterministic) the given automaton.
        /// </summary>
        /// <param name="a">
        /// The automaton.
        /// </param>
        public static void Minimize(Automaton a)
        {
            if (!a.IsSingleton)
            {
                switch (Automaton.Minimization)
                {
                    case Automaton._MinimizeHuffman:
                        MinimizeHuffman(a);
                        break;

                    case Automaton._MinimizeBrzozowski:
                        MinimizeBrzozowski(a);
                        break;

                    default:
                        MinimizeHopcroft(a);
                        break;
                }
            }

            a.RecomputeHashCode();
        }

        /// <summary>
        /// Minimizes the given automaton using Brzozowski's algorithm.
        /// </summary>
        /// <param name="a">
        /// The automaton.
        /// </param>
        public static void MinimizeBrzozowski(Automaton a)
        {
            if (a.IsSingleton)
            {
                return;
            }

            BasicOperations.Determinize(a, SpecialOperations.Reverse(a).ToList());
            BasicOperations.Determinize(a, SpecialOperations.Reverse(a).ToList());
        }

        public static void MinimizeHopcroft(Automaton a)
        {
            a.Determinize();
            var tr = a.Initial.Transitions;
            if (tr.Count == 1)
            {
                var t = tr[0];
                if (t.To == a.Initial && t.Min == char.MinValue && t.Max == char.MaxValue)
                {
                    return;
                }
            }

            a.Totalize();

            // Make arrays for numbered states and effective alphabet.
            var ss = a.GetStates();
            var states = new State[ss.Count];
            var number = 0;
            foreach (var q in ss)
            {
                states[number] = q;
                q.Number = number++;
            }

            var sigma = a.GetStartPoints();

            // Initialize data structures.
            var reverse = new List<List<LinkedList<State>>>();
            foreach (var s in states)
            {
                var v = new List<LinkedList<State>>();
                Initialize(ref v, sigma.Length);
                reverse.Add(v);
            }

            var reverseNonempty = new bool[states.Length, sigma.Length];

            var partition = new List<LinkedList<State>>();
            Initialize(ref partition, states.Length);

            var block = new int[states.Length];
            var active = new StateList[states.Length, sigma.Length];
            var active2 = new StateListNode[states.Length, sigma.Length];
            var pending = new LinkedList<IntPair>();
            var pending2 = new bool[sigma.Length, states.Length];
            var split = new List<State>();
            var split2 = new bool[states.Length];
            var refine = new List<int>();
            var refine2 = new bool[states.Length];

            var splitblock = new List<List<State>>();
            Initialize(ref splitblock, states.Length);

            for (var q = 0; q < states.Length; q++)
            {
                splitblock[q] = new List<State>();
                partition[q] = new LinkedList<State>();
                for (var x = 0; x < sigma.Length; x++)
                {
                    reverse[q][x] = new LinkedList<State>();
                    active[q, x] = new StateList();
                }
            }

            // Find initial partition and reverse edges.
            foreach (var qq in states)
            {
                var j = qq.Accept ? 0 : 1;

                partition[j].AddLast(qq);
                block[qq.Number] = j;
                for (var x = 0; x < sigma.Length; x++)
                {
                    var y = sigma[x];
                    var p = qq.Step(y);
                    reverse[p.Number][x].AddLast(qq);
                    reverseNonempty[p.Number, x] = true;
                }
            }

            // Initialize active sets.
            for (var j = 0; j <= 1; j++)
            {
                for (var x = 0; x < sigma.Length; x++)
                {
                    foreach (var qq in partition[j])
                    {
                        if (reverseNonempty[qq.Number, x])
                        {
                            active2[qq.Number, x] = active[j, x].Add(qq);
                        }
                    }
                }
            }

            // Initialize pending.
            for (var x = 0; x < sigma.Length; x++)
            {
                var a0 = active[0, x].Size;
                var a1 = active[1, x].Size;
                var j = a0 <= a1 ? 0 : 1;
                pending.AddLast(new IntPair(j, x));
                pending2[x, j] = true;
            }

            // Process pending until fixed point.
            var k = 2;
            while (pending.Count > 0)
            {
                var ip = pending.RemoveAndReturnFirst();
                var p = ip.N1;
                var x = ip.N2;
                pending2[x, p] = false;

                // Find states that need to be split off their blocks.
                for (var m = active[p, x].First; m != null; m = m.Next)
                {
                    foreach (var s in reverse[m.State.Number][x])
                    {
                        if (!split2[s.Number])
                        {
                            split2[s.Number] = true;
                            split.Add(s);
                            var j = block[s.Number];
                            splitblock[j].Add(s);
                            if (!refine2[j])
                            {
                                refine2[j] = true;
                                refine.Add(j);
                            }
                        }
                    }
                }

                // Refine blocks.
                foreach (var j in refine)
                {
                    if (splitblock[j].Count < partition[j].Count)
                    {
                        var b1 = partition[j];
                        var b2 = partition[k];
                        foreach (var s in splitblock[j])
                        {
                            b1.Remove(s);
                            b2.AddLast(s);
                            block[s.Number] = k;
                            for (var c = 0; c < sigma.Length; c++)
                            {
                                var sn = active2[s.Number, c];
                                if (sn != null && sn.StateList == active[j, c])
                                {
                                    sn.Remove();
                                    active2[s.Number, c] = active[k, c].Add(s);
                                }
                            }
                        }

                        // Update pending.
                        for (var c = 0; c < sigma.Length; c++)
                        {
                            var aj = active[j, c].Size;
                            var ak = active[k, c].Size;
                            if (!pending2[c, j] && 0 < aj && aj <= ak)
                            {
                                pending2[c, j] = true;
                                pending.AddLast(new IntPair(j, c));
                            }
                            else
                            {
                                pending2[c, k] = true;
                                pending.AddLast(new IntPair(k, c));
                            }
                        }

                        k++;
                    }

                    foreach (var s in splitblock[j])
                    {
                        split2[s.Number] = false;
                    }

                    refine2[j] = false;
                    splitblock[j].Clear();
                }

                split.Clear();
                refine.Clear();
            }

            // Make a new state for each equivalence class, set initial state.
            var newstates = new State[k];
            for (var n = 0; n < newstates.Length; n++)
            {
                var s = new State();
                newstates[n] = s;
                foreach (var q in partition[n])
                {
                    if (q == a.Initial)
                    {
                        a.Initial = s;
                    }

                    s.Accept = q.Accept;
                    s.Number = q.Number; // Select representative.
                    q.Number = n;
                }
            }

            // Build transitions and set acceptance.
            foreach (var s in newstates)
            {
                s.Accept = states[s.Number].Accept;
                foreach (var t in states[s.Number].Transitions)
                {
                    s.Transitions.Add(new Transition(t.Min, t.Max, newstates[t.To.Number]));
                }
            }

            a.RemoveDeadTransitions();
        }

        /// <summary>
        /// Minimizes the given automaton using Huffman's algorithm.
        /// </summary>
        /// <param name="a">
        /// The automaton.
        /// </param>
        public static void MinimizeHuffman(Automaton a)
        {
            a.Determinize();
            a.Totalize();
            var ss = a.GetStates();
            var transitions = new Transition[ss.Count][];
            var states = ss.ToArray();

            var mark = new List<List<bool>>();
            var triggers = new List<List<HashSet<IntPair>>>();
            foreach (var t in states)
            {
                var v = new List<HashSet<IntPair>>();
                Initialize(ref v, states.Length);
                triggers.Add(v);
            }

            // Initialize marks based on acceptance status and find transition arrays.
            for (var n1 = 0; n1 < states.Length; n1++)
            {
                states[n1].Number = n1;
                transitions[n1] = states[n1].GetSortedTransitions(false).ToArray();
                for (var n2 = n1 + 1; n2 < states.Length; n2++)
                {
                    if (states[n1].Accept != states[n2].Accept)
                    {
                        mark[n1][n2] = true;
                    }
                }
            }

            // For all pairs, see if states agree.
            for (var n1 = 0; n1 < states.Length; n1++)
            {
                for (var n2 = n1 + 1; n2 < states.Length; n2++)
                {
                    if (!mark[n1][n2])
                    {
                        if (StatesAgree(transitions, mark, n1, n2))
                        {
                            AddTriggers(transitions, triggers, n1, n2);
                        }
                        else
                        {
                            MarkPair(mark, triggers, n1, n2);
                        }
                    }
                }
            }

            // Assign equivalence class numbers to states.
            var numclasses = 0;
            foreach (var t in states)
            {
                t.Number = -1;
            }

            for (var n1 = 0; n1 < states.Length; n1++)
            {
                if (states[n1].Number == -1)
                {
                    states[n1].Number = numclasses;
                    for (var n2 = n1 + 1; n2 < states.Length; n2++)
                    {
                        if (!mark[n1][n2])
                        {
                            states[n2].Number = numclasses;
                        }
                    }

                    numclasses++;
                }
            }

            // Make a new state for each equivalence class.
            var newstates = new State[numclasses];
            for (var n = 0; n < numclasses; n++)
            {
                newstates[n] = new State();
            }

            // Select a class representative for each class and find the new initial state.
            for (var n = 0; n < states.Length; n++)
            {
                newstates[states[n].Number].Number = n;
                if (states[n] == a.Initial)
                {
                    a.Initial = newstates[states[n].Number];
                }
            }

            // Build transitions and set acceptance.
            for (var n = 0; n < numclasses; n++)
            {
                var s = newstates[n];
                s.Accept = states[s.Number].Accept;
                foreach (var t in states[s.Number].Transitions)
                {
                    s.Transitions.Add(new Transition(t.Min, t.Max, newstates[t.To.Number]));
                }
            }

            a.RemoveDeadTransitions();
        }

        private static void Initialize<T>(ref List<T> list, int size)
        {
            for (var i = 0; i < size; i++)
            {
                list.Add(default(T));
            }
        }

        private static void AddTriggers(Transition[][] transitions, IList<List<HashSet<IntPair>>> triggers, int n1, int n2)
        {
            var t1 = transitions[n1];
            var t2 = transitions[n2];
            for (int k1 = 0, k2 = 0; k1 < t1.Length && k2 < t2.Length;)
            {
                if (t1[k1].Max < t2[k2].Min)
                {
                    k1++;
                }
                else if (t2[k2].Max < t1[k1].Min)
                {
                    k2++;
                }
                else
                {
                    if (t1[k1].To != t2[k2].To)
                    {
                        var m1 = t1[k1].To.Number;
                        var m2 = t2[k2].To.Number;
                        if (m1 > m2)
                        {
                            var t = m1;
                            m1 = m2;
                            m2 = t;
                        }

                        if (triggers[m1][m2] == null)
                        {
                            triggers[m1].Insert(m2, new HashSet<IntPair>());
                        }

                        triggers[m1][m2].Add(new IntPair(n1, n2));
                    }

                    if (t1[k1].Max < t2[k2].Max)
                    {
                        k1++;
                    }
                    else
                    {
                        k2++;
                    }
                }
            }
        }

        private static void MarkPair(List<List<bool>> mark, IList<List<HashSet<IntPair>>> triggers, int n1, int n2)
        {
            mark[n1][n2] = true;
            if (triggers[n1][n2] != null)
            {
                foreach (var p in triggers[n1][n2])
                {
                    var m1 = p.N1;
                    var m2 = p.N2;
                    if (m1 > m2)
                    {
                        var t = m1;
                        m1 = m2;
                        m2 = t;
                    }

                    if (!mark[m1][m2])
                    {
                        MarkPair(mark, triggers, m1, m2);
                    }
                }
            }
        }

        private static bool StatesAgree(Transition[][] transitions, List<List<bool>> mark, int n1, int n2)
        {
            var t1 = transitions[n1];
            var t2 = transitions[n2];
            for (int k1 = 0, k2 = 0; k1 < t1.Length && k2 < t2.Length;)
            {
                if (t1[k1].Max < t2[k2].Min)
                {
                    k1++;
                }
                else if (t2[k2].Max < t1[k1].Min)
                {
                    k2++;
                }
                else
                {
                    var m1 = t1[k1].To.Number;
                    var m2 = t2[k2].To.Number;
                    if (m1 > m2)
                    {
                        var t = m1;
                        m1 = m2;
                        m2 = t;
                    }

                    if (mark[m1][m2])
                    {
                        return false;
                    }

                    if (t1[k1].Max < t2[k2].Max)
                    {
                        k1++;
                    }
                    else
                    {
                        k2++;
                    }
                }
            }

            return true;
        }

        private sealed class IntPair
        {
            private readonly int _N1;
            private readonly int _N2;

            public IntPair(int n1, int n2)
            {
                _N1 = n1;
                _N2 = n2;
            }

            public int N1 => _N1;

            public int N2 => _N2;
        }

        private sealed class StateList
        {
            public int Size { get; set; }

            public StateListNode First { get; set; }

            public StateListNode Last { get; set; }

            public StateListNode Add(State q) => new StateListNode(q, this);
        }

        private sealed class StateListNode
        {
            public StateListNode(State q, StateList sl)
            {
                State = q;
                StateList = sl;
                if (sl.Size++ == 0)
                {
                    sl.First = sl.Last = this;
                }
                else
                {
                    sl.Last.Next = this;
                    Prev = sl.Last;
                    sl.Last = this;
                }
            }

            public StateListNode Next { get; private set; }

            private StateListNode Prev { get; set; }

            public StateList StateList { get; private set; }

            public State State { get; private set; }

            public void Remove()
            {
                StateList.Size--;
                if (StateList.First == this)
                {
                    StateList.First = Next;
                }
                else
                {
                    Prev.Next = Next;
                }

                if (StateList.Last == this)
                {
                    StateList.Last = Prev;
                }
                else
                {
                    Next.Prev = Prev;
                }
            }
        }
    }
}