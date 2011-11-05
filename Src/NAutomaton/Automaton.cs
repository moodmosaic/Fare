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
using System.IO;
using System.Linq;
using System.Text;

namespace NAutomaton
{
    public class Automaton
    {
        public const int MinimizeHuffman = 0;
        public const int MinimizeBrzozowski = 1;
        public const int MinimizeHopcroft = 2;

        private static int minimization = MinimizeHopcroft;
        private static bool minimizeAlways;
        private static bool allowMutation;
        private static bool? isDebug;

        private bool deterministic;
        private int hashCode;
        private State initial;
        private string singleton;

        public Automaton()
        {
            this.initial       = new State();
            this.deterministic = true;
            this.singleton     = null;
        }

        private bool IsDebug
        {
            get
            {
                if (isDebug == null)
                {
                    isDebug = Convert.ToBoolean(Environment.GetEnvironmentVariable("dk.brics.automaton.debug") != null);
                }

                return isDebug.Value;
            }
        }

        public static int Minimization
        {
            set { minimization = value; }
        }

        public static bool MinimizeAlways
        {
            set { minimizeAlways = value; }
        }

        private static bool AllowMutate
        {
            get { return allowMutation; }
        }

        private bool IsSingleton
        {
            get { return this.singleton != null; }
        }

        public virtual string Singleton
        {
            get { return this.singleton; }
        }

        public virtual State InitialState
        {
            set
            {
                this.initial = value;
                this.singleton = null;
            }
            get
            {
                this.ExpandSingleton();
                return this.initial;
            }
        }

        public virtual bool IsDeterministic
        {
            get { return this.deterministic; }
            set { this.deterministic = value; }
        }

        public virtual object Info { set; get; }

        public virtual HashSet<State> States
        {
            get
            {
                this.ExpandSingleton();
                HashSet<State> visited;
                if (this.IsDebug)
                {
                    visited = new HashSet<State>(); // LinkedHashSet.
                }
                else
                {
                    visited = new HashSet<State>();
                }
                var worklist = new LinkedList<State>();
                worklist.AddLast(this.initial);
                visited.Add(this.initial);
                while (worklist.Count > 0)
                {
                    State s = worklist.RemoveAndReturnFirst();
                    ICollection<Transition> tr;
                    if (IsDebug)
                    {
                        tr = s.GetSortedTransitions(false);
                    }
                    else
                    {
                        tr = s.Transitions;
                    }
                    foreach (Transition t in tr)
                    {
                        if (!visited.Contains(t.To))
                        {
                            visited.Add(t.To);
                            worklist.AddLast(t.To);
                        }
                    }
                }
                return visited;
            }
        }

        public virtual HashSet<State> AcceptStates
        {
            get
            {
                this.ExpandSingleton();
                var accepts = new HashSet<State>();
                var visited = new HashSet<State>();
                var worklist = new LinkedList<State>();
                worklist.AddLast(initial);
                visited.Add(initial);
                while (worklist.Count > 0)
                {
                    State s = worklist.RemoveAndReturnFirst();
                    if (s.IsAccept)
                    {
                        accepts.Add(s);
                    }
                    foreach (Transition t in s.Transitions)
                    {
                        if (!visited.Contains(t.To))
                        {
                            visited.Add(t.To);
                            worklist.AddLast(t.To);
                        }
                    }
                }
                return accepts;
            }
        }

        private static HashSet<State> StateNumbers
        {
            set
            {
                int number = 0;
                foreach (State s in value)
                {
                    s.Number = number++;
                }
            }
        }

        private char[] StartPoints
        {
            get
            {
                var pointset = new HashSet<char?>();
                foreach (State s in States)
                {
                    pointset.Add(Char.MinValue);
                    foreach (Transition t in s.Transitions)
                    {
                        pointset.Add(t.Min);
                        if (t.Max < Char.MaxValue)
                        {
                            pointset.Add((char)(t.Max + 1));
                        }
                    }
                }
                var points = new char[pointset.Count];
                int n = 0;
                foreach (var m in pointset)
                {
                    points[n++] = m.Value;
                }
                Array.Sort(points);
                return points;
            }
        }

        public virtual HashSet<State> LiveStates
        {
            get
            {
                this.ExpandSingleton();
                return this.GetLiveStates(States);
            }
        }

        public virtual int NumberOfStates
        {
            get
            {
                if (this.IsSingleton)
                {
                    return singleton.Length + 1;
                }
                return this.States.Count;
            }
        }

        public virtual int NumberOfTransitions
        {
            get
            {
                if (this.IsSingleton)
                {
                    return singleton.Length;
                }
                return this.States.Sum(s => s.Transitions.Count());
            }
        }

        public virtual bool IsEmptyString
        {
            get { return BasicOperations.IsEmptyString(this); }
        }

        public virtual bool IsEmpty
        {
            get { return BasicOperations.IsEmpty(this); }
        }

        public virtual bool IsTotal
        {
            get { return BasicOperations.IsTotal(this); }
        }

        public virtual bool IsFinite
        {
            get { return SpecialOperations.IsFinite(this); }
        }

        public virtual HashSet<string> FiniteStrings
        {
            get { return SpecialOperations.GetFiniteStrings(this); }
        }

        public virtual string CommonPrefix
        {
            get { return SpecialOperations.GetCommonPrefix(this); }
        }

        public static bool SetAllowMutate(bool flag)
        {
            bool b = allowMutation;
            allowMutation = flag;
            return b;
        }

        private void CheckMinimizeAlways()
        {
            if (minimizeAlways)
            {
                this.Minimize();
            }
        }

        private void Totalize()
        {
            var s = new State();
            s.Transitions.Add(new Transition(Char.MinValue, Char.MaxValue, s));
            foreach (State p in States)
            {
                int maxi = Char.MinValue;
                foreach (Transition t in p.GetSortedTransitions(false))
                {
                    if (t.Min > maxi)
                    {
                        p.Transitions.Add(new Transition((char)maxi, (char)(t.Min - 1), s));
                    }
                    if (t.Max + 1 > maxi)
                    {
                        maxi = t.Max + 1;
                    }
                }
                if (maxi <= Char.MaxValue)
                {
                    p.Transitions.Add(new Transition((char)maxi, Char.MaxValue, s));
                }
            }
        }

        public virtual void RestoreInvariant()
        {
            this.RemoveDeadTransitions();
        }

        public virtual void Reduce()
        {
            if (this.IsSingleton)
            {
                return;
            }
            HashSet<State> states = States;
            StateNumbers = states;
            foreach (State s in states)
            {
                IList<Transition> st = s.GetSortedTransitions(true);
                s.ResetTransitions();
                State p = null;
                int min = -1, max = -1;
                foreach (Transition t in st)
                {
                    if (p == t.To)
                    {
                        if (t.Min <= max + 1)
                        {
                            if (t.Max > max)
                            {
                                max = t.Max;
                            }
                        }
                        else
                        {
                            if (p != null)
                            {
                                s.Transitions.Add(new Transition((char)min, (char)max, p));
                            }
                            min = t.Min;
                            max = t.Max;
                        }
                    }
                    else
                    {
                        if (p != null)
                        {
                            s.Transitions.Add(new Transition((char)min, (char)max, p));
                        }
                        p = t.To;
                        min = t.Min;
                        max = t.Max;
                    }
                }
                if (p != null)
                {
                    s.Transitions.Add(new Transition((char)min, (char)max, p));
                }
            }
            this.ClearHashCode();
        }

        private HashSet<State> GetLiveStates(HashSet<State> states)
        {
            Dictionary<State, HashSet<State>> map = states.ToDictionary(s => s, s => new HashSet<State>());
            foreach (State s in states)
            {
                foreach (Transition t in s.Transitions)
                {
                    map[t.To].Add(s);
                }
            }
            var live = new HashSet<State>(this.AcceptStates);
            var worklist = new LinkedList<State>(live);
            while (worklist.Count > 0)
            {
                State s = worklist.RemoveAndReturnFirst();
                foreach (State p in map[s])
                {
                    if (!live.Contains(p))
                    {
                        live.Add(p);
                        worklist.AddLast(p);
                    }
                }
            }
            return live;
        }

        public virtual void RemoveDeadTransitions()
        {
            this.ClearHashCode();
            if (this.IsSingleton)
            {
                return;
            }
            HashSet<State> states = this.States;
            HashSet<State> live = this.GetLiveStates(states);
            foreach (State s in states)
            {
                HashSet<Transition> st = s.Transitions;
                s.ResetTransitions();
                foreach (Transition t in st)
                {
                    if (live.Contains(t.To))
                    {
                        s.Transitions.Add(t);
                    }
                }
            }
            this.Reduce();
        }

        private static Transition[][] GetSortedTransitions(HashSet<State> states)
        {
            StateNumbers = states;
            var transitions = new Transition[states.Count][];
            foreach (State s in states)
            {
                transitions[s.Number] = s.GetSortedTransitions(false).ToArray();
            }
            return transitions;
        }

        public virtual void ExpandSingleton()
        {
            if (this.IsSingleton)
            {
                var p = new State();
                initial = p;
                foreach (char t in singleton)
                {
                    var q = new State();
                    p.Transitions.Add(new Transition(t, q));
                    p = q;
                }
                p.IsAccept = true;
                this.deterministic = true;
                this.singleton = null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is Automaton))
            {
                return false;
            }
            var a = (Automaton)obj;
            if (this.IsSingleton && a.IsSingleton)
            {
                return singleton.Equals(a.singleton);
            }
            return this.GetHashCode() == a.GetHashCode() && this.SubsetOf(a) && a.SubsetOf(this);
        }

        public override int GetHashCode()
        {
            if (this.hashCode == 0)
            {
                this.Minimize();
            }
            return this.hashCode;
        }

        private void RecomputeHashCode()
        {
            this.hashCode = this.NumberOfStates * 3 + this.NumberOfTransitions * 2;
            if (this.hashCode == 0)
            {
                this.hashCode = 1;
            }
        }

        private void ClearHashCode()
        {
            this.hashCode = 0;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            if (this.IsSingleton)
            {
                b.Append("singleton: ");
                foreach (char c in singleton)
                {
                    Transition.AppendCharString(c, b);
                }
                b.AppendLine();
            }
            else
            {
                HashSet<State> states = States;
                Automaton.StateNumbers = states;
                b.Append("initial state: ").Append(initial.Number).Append("\n");
                foreach (State s in states)
                {
                    b.Append(s.ToString());
                }
            }
            return b.ToString();
        }

        public virtual string ToDot()
        {
            var b = new StringBuilder("digraph Automaton {\n");
            b.Append("  rankdir = LR;\n");
            HashSet<State> states = States;
            Automaton.StateNumbers = states;
            foreach (State s in states)
            {
                b.Append("  ").Append(s.Number);
                b.Append(s.IsAccept ? " [shape=doublecircle,label=\"\"];\n" : " [shape=circle,label=\"\"];\n");
                if (s == initial)
                {
                    b.Append("  initial [shape=plaintext,label=\"\"];\n");
                    b.Append("  initial -> ").Append(s.Number).Append("\n");
                }
                foreach (Transition t in s.Transitions)
                {
                    b.Append("  ").Append(s.Number);
                    t.AppendDot(b);
                }
            }
            return b.Append("}\n").ToString();
        }

        private Automaton CloneExpanded()
        {
            Automaton a = this.Clone();
            a.ExpandSingleton();
            return a;
        }

        private Automaton CloneExpandedIfRequired()
        {
            if (allowMutation)
            {
                this.ExpandSingleton();
                return this;
            }

            return this.CloneExpanded();
        }

        public Automaton Clone()
        {
            var a = (Automaton)this.MemberwiseClone();
            if (!this.IsSingleton)
            {
                Dictionary<State, State> m = this.States.ToDictionary(s => s, s => new State());

                foreach (State s in this.States)
                {
                    State p = m[s];
                    p.IsAccept = s.IsAccept;
                    if (s == initial)
                    {
                        a.initial = p;
                    }
                    foreach (Transition t in s.Transitions)
                    {
                        p.Transitions.Add(new Transition(t.Min, t.Max, m[t.To]));
                    }
                }
            }
            return a;
        }

        private Automaton CloneIfRequired()
        {
            if (allowMutation)
            {
                return this;
            }
            return this.Clone();
        }

        public static Automaton Load(Uri url)
        {
            throw new NotImplementedException();
        }

        public static Automaton Load(Stream stream)
        {
            throw new NotImplementedException();
        }

        public virtual void Store(Stream stream)
        {
            throw new NotImplementedException();
        }

        public static Automaton MakeEmpty()
        {
            return BasicAutomata.MakeEmpty();
        }

        public static Automaton MakeEmptyString()
        {
            return BasicAutomata.MakeEmptyString();
        }

        public static Automaton MakeAnyString()
        {
            return BasicAutomata.MakeAnyString();
        }

        public static Automaton MakeAnyChar()
        {
            return BasicAutomata.MakeAnyChar();
        }

        public static Automaton MakeChar(char c)
        {
            return BasicAutomata.MakeChar(c);
        }

        public static Automaton MakeCharRange(char min, char max)
        {
            return BasicAutomata.MakeCharRange(min, max);
        }

        public static Automaton MakeCharSet(string set)
        {
            return BasicAutomata.MakeCharSet(set);
        }

        public static Automaton MakeInterval(int min, int max, int digits)
        {
            return BasicAutomata.MakeInterval(min, max, digits);
        }

        public static Automaton MakeString(string s)
        {
            return BasicAutomata.MakeString(s);
        }

        public static Automaton MakeStringUnion(params char[][] strings)
        {
            return BasicAutomata.MakeStringUnion(strings);
        }

        public static Automaton MakeMaxInteger(string n)
        {
            return BasicAutomata.MakeMaxInteger(n);
        }

        public static Automaton MakeMinInteger(string n)
        {
            return BasicAutomata.MakeMinInteger(n);
        }

        public static Automaton MakeTotalDigits(int i)
        {
            return BasicAutomata.MakeTotalDigits(i);
        }

        public static Automaton MakeFractionDigits(int i)
        {
            return BasicAutomata.MakeFractionDigits(i);
        }

        public static Automaton MakeIntegerValue(string value)
        {
            return BasicAutomata.MakeIntegerValue(value);
        }

        public static Automaton MakeDecimalValue(string value)
        {
            return BasicAutomata.MakeDecimalValue(value);
        }

        public static Automaton MakeStringMatcher(string s)
        {
            return BasicAutomata.MakeStringMatcher(s);
        }

        public virtual Automaton Concatenate(Automaton a)
        {
            return BasicOperations.Concatenate(this, a);
        }

        public static Automaton Concatenate(IList<Automaton> l)
        {
            return BasicOperations.Concatenate(l);
        }

        public virtual Automaton Optional()
        {
            return BasicOperations.Optional(this);
        }

        public virtual Automaton Repeat()
        {
            return BasicOperations.Repeat(this);
        }

        public virtual Automaton Repeat(int min)
        {
            return BasicOperations.Repeat(this, min);
        }

        public virtual Automaton Repeat(int min, int max)
        {
            return BasicOperations.Repeat(this, min, max);
        }

        public virtual Automaton Complement()
        {
            return BasicOperations.Complement(this);
        }

        public virtual Automaton Minus(Automaton a)
        {
            return BasicOperations.Minus(this, a);
        }

        public virtual Automaton Intersection(Automaton a)
        {
            return BasicOperations.Intersection(this, a);
        }

        public virtual bool SubsetOf(Automaton a)
        {
            return BasicOperations.SubsetOf(this, a);
        }

        public virtual Automaton Union(Automaton a)
        {
            return BasicOperations.Union(this, a);
        }

        public static Automaton Union(ICollection<Automaton> l)
        {
            return BasicOperations.Union(l);
        }

        public virtual void Determinize()
        {
            BasicOperations.Determinize(this);
        }

        public virtual void AddEpsilons(ICollection<StatePair> pairs)
        {
            BasicOperations.AddEpsilons(this, pairs);
        }

        public virtual string GetShortestExample(bool accepted)
        {
            return BasicOperations.GetShortestExample(this, accepted);
        }

        public virtual bool Run(string s)
        {
            return BasicOperations.Run(this, s);
        }

        public virtual void Minimize()
        {
            MinimizationOperations.Minimize(this);
        }

        public static Automaton Minimize(Automaton a)
        {
            a.Minimize();
            return a;
        }

        public virtual Automaton Overlap(Automaton a)
        {
            return SpecialOperations.Overlap(this, a);
        }

        public virtual Automaton SingleChars()
        {
            return SpecialOperations.SingleChars(this);
        }

        public virtual Automaton Trim(string set, char c)
        {
            return SpecialOperations.Trim(this, set, c);
        }

        public virtual Automaton Compress(string set, char c)
        {
            return SpecialOperations.Compress(this, set, c);
        }

        public virtual Automaton Subst(IDictionary<char?, HashSet<char?>> map)
        {
            return SpecialOperations.Subst(this, map);
        }

        public virtual Automaton Subst(char c, string s)
        {
            return SpecialOperations.Subst(this, c, s);
        }

        public virtual Automaton Homomorph(char[] source, char[] dest)
        {
            return SpecialOperations.Homomorph(this, source, dest);
        }

        public virtual Automaton ProjectChars(HashSet<char?> chars)
        {
            return SpecialOperations.ProjectChars(this, chars);
        }

        public virtual HashSet<string> GetStrings(int length)
        {
            return SpecialOperations.GetStrings(this, length);
        }

        public virtual HashSet<string> GetFiniteStrings(int limit)
        {
            return SpecialOperations.GetFiniteStrings(this, limit);
        }

        public virtual void PrefixClose()
        {
            SpecialOperations.PrefixClose(this);
        }

        public static Automaton HexCases(Automaton a)
        {
            return SpecialOperations.HexCases(a);
        }

        public static Automaton ReplaceWhitespace(Automaton a)
        {
            return SpecialOperations.ReplaceWhitespace(a);
        }

        public static string ShuffleSubsetOf(ICollection<Automaton> ca, Automaton a, char? suspendShuffle, char? resumeShuffle)
        {
            return ShuffleOperations.ShuffleSubsetOf(ca, a, suspendShuffle, resumeShuffle);
        }

        public virtual Automaton Shuffle(Automaton a)
        {
            return ShuffleOperations.Shuffle(this, a);
        }
    }
}