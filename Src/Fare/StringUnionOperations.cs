﻿/*
* http://github.com/moodmosaic/Fare/
* Original Java code:
* http://www.brics.dk/automaton/
*
* Operations for building minimal deterministic automata from sets of strings.
* The algorithm requires sorted input data, but is very fast (nearly linear with the input size).
*
* @author Dawid Weiss
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Fare
{
    public sealed class StringUnionOperations
    {
        private static readonly IComparer<char[]> _LexicographicOrder = new LexicographicOrder();

        private readonly State _Root = new State();

        private StringBuilder _Previous;
        private IDictionary<State, State> _Register = new Dictionary<State, State>();

        public static IComparer<char[]> LexicographicOrderComparer
        {
            get { return _LexicographicOrder; }
        }

        public static Fare.State Build(IEnumerable<char[]> input)
        {
            var builder = new StringUnionOperations();

            foreach (var chs in input)
            {
                builder.Add(chs);
            }

            return Convert(builder.Complete(), new Dictionary<State, Fare.State>());
        }

        public void Add(char[] current)
        {
            Debug.Assert(this._Register != null, "Automaton already built.");
            Debug.Assert(current.Length > 0, "Input sequences must not be empty.");
            Debug.Assert(
                this._Previous == null ||
                LexicographicOrderComparer.Compare(this._Previous.ToString().ToCharArray(), current) <= 0,
                "Input must be sorted: " + this._Previous + " >= " + current);
            Debug.Assert(this.SetPrevious(current));

            // Descend in the automaton (find matching prefix).
            var pos = 0;
            var max = current.Length;
            State next;
            var state = _Root;
            while (pos < max && (next = state.GetLastChild(current[pos])) != null)
            {
                state = next;
                pos++;
            }

            if (state.HasChildren)
            {
                this.ReplaceOrRegister(state);
            }

            AddSuffix(state, current, pos);
        }

        private static void AddSuffix(State state, char[] current, int fromIndex)
        {
            for (var i = fromIndex; i < current.Length; i++)
            {
                state = state.NewState(current[i]);
            }

            state.IsFinal = true;
        }

        private static Fare.State Convert(State s, IDictionary<State, Fare.State> visited)
        {
            var converted = visited[s];
            if (converted != null)
            {
                return converted;
            }

            converted = new Fare.State();
            converted.Accept = s.IsFinal;

            visited.Add(s, converted);
            var i = 0;
            var labels = s.TransitionLabels;
            foreach (var target in s.States)
            {
                converted.AddTransition(new Transition(labels[i++], Convert(target, visited)));
            }

            return converted;
        }

        private State Complete()
        {
            if (this._Register == null)
            {
                throw new InvalidOperationException("register is null");
            }

            if (this._Root.HasChildren)
            {
                this.ReplaceOrRegister(this._Root);
            }

            this._Register = null;
            return this._Root;
        }

        private void ReplaceOrRegister(State state)
        {
            var child = state.LastChild;

            if (child.HasChildren)
            {
                this.ReplaceOrRegister(child);
            }

            var registered = this._Register[child];
            if (registered != null)
            {
                state.ReplaceLastChild(registered);
            }
            else
            {
                this._Register.Add(child, child);
            }
        }

        private bool SetPrevious(char[] current)
        {
            if (this._Previous == null)
            {
                this._Previous = new StringBuilder();
            }

            this._Previous.Length = 0;
            this._Previous.Append(current);

            return true;
        }

        private sealed class LexicographicOrder : IComparer<char[]>
        {
            public int Compare(char[] s1, char[] s2)
            {
                var lens1 = s1.Length;
                var lens2 = s2.Length;
                var max = Math.Min(lens1, lens2);

                for (var i = 0; i < max; i++)
                {
                    var c1 = s1[i];
                    var c2 = s2[i];
                    if (c1 != c2)
                    {
                        return c1 - c2;
                    }
                }

                return lens1 - lens2;
            }
        }

        private sealed class State
        {
            private static readonly char[] _NoLabels = Array.Empty<char>();
            private static readonly State[] _NoStates = Array.Empty<State>();
            private bool _IsFinal;

            private char[] _Labels = _NoLabels;
            private State[] _States = _NoStates;

            public char[] TransitionLabels
            {
                get { return this._Labels; }
            }

            public IEnumerable<State> States
            {
                get { return this._States; }
            }

            public bool HasChildren
            {
                get { return this._Labels.Length > 0; }
            }

            public bool IsFinal
            {
                get { return this._IsFinal; }
                set { this._IsFinal = value; }
            }

            public State LastChild
            {
                get
                {
                    Debug.Assert(this.HasChildren, "No outgoing transitions.");
                    return this._States[this._States.Length - 1];
                }
            }

            public override bool Equals(object obj)
            {
                var other = obj as State;
                if (other == null)
                {
                    return false;
                }

                return this._IsFinal == other._IsFinal
                    && ReferenceEquals(_States, other._States)
                    && Equals(_Labels, other._Labels);
            }

            public override int GetHashCode()
            {
                var hash = this._IsFinal ? 1 : 0;
                hash ^= (hash * 31) + this._Labels.Length;
                hash = this._Labels.Aggregate(hash, (current, c) => current ^ (current * 31) + c);

                // Compare the right-language of this state using reference-identity of outgoing
                // states. This is possible because states are interned (stored in registry) and
                // traversed in post-order, so any outgoing transitions are already interned.
                return this._States.Aggregate(hash, (current, s) => current ^ RuntimeHelpers.GetHashCode(s));
            }

            public State NewState(char label)
            {
                Debug.Assert(
                    Array.BinarySearch(this._Labels, label) < 0,
                    "State already has transition labeled: " + label);

                this._Labels = CopyOf(this._Labels, this._Labels.Length + 1);
                this._States = CopyOf(this._States, this._States.Length + 1);

                this._Labels[this._Labels.Length - 1] = label;
                return _States[this._States.Length - 1] = new State();
            }

            public State GetLastChild(char label)
            {
                var index = this._Labels.Length - 1;
                State s = null;
                if (index >= 0 && this._Labels[index] == label)
                {
                    s = this._States[index];
                }

                Debug.Assert(s == this.GetState(label));
                return s;
            }

            public void ReplaceLastChild(State state)
            {
                Debug.Assert(this.HasChildren, "No outgoing transitions.");
                this._States[this._States.Length - 1] = state;
            }

            private static char[] CopyOf(char[] original, int newLength)
            {
                var copy = new char[newLength];
                Array.Copy(original, 0, copy, 0, Math.Min(original.Length, newLength));
                return copy;
            }

            private static State[] CopyOf(State[] original, int newLength)
            {
                var copy = new State[newLength];
                Array.Copy(original, 0, copy, 0, Math.Min(original.Length, newLength));
                return copy;
            }

            private static bool ReferenceEquals(object[] a1, object[] a2)
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }

                return !a1.Where((t, i) => t != a2[i]).Any();
            }

            private State GetState(char label)
            {
                var index = Array.BinarySearch(this._Labels, label);
                return index >= 0 ? _States[index] : null;
            }
        }
    }
}