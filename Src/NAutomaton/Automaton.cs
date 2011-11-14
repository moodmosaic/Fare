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

namespace NAutomaton
{
    /// <summary>
    /// Finite-state automaton with regular expression operations.
    ///   <p>
    ///     Class invariants:
    ///     <ul>
    ///       <li>
    ///         An automaton is either represented explicitly (with State and Transition} objects)
    ///         or with a singleton string (see Singleton property ExpandSingleton() method) in case the
    ///         automaton is known to accept exactly one string. (Implicitly, all states and transitions of
    ///         an automaton are reachable from its initial state.)</li>
    ///       <li>
    ///         Automata are always reduced (see method Rreduce()) and have no transitions to dead states 
    ///         (see RemoveDeadTransitions() method).
    ///       </li>
    ///       <li>
    ///         If an automaton is non deterministic, then IsDeterministic property returns false (but the 
    ///         converse is not required).
    ///       </li>
    ///       <li>
    ///         Automata provided as input to operations are generally assumed to be disjoint.</li>
    ///     </ul>
    ///   </p>
    ///   If the states or transitions are manipulated manually, the RestoreInvariant() method and 
    ///   SetDeterministic(bool) methods should be used afterwards to restore representation invariants
    ///   that are assumed by the built-in automata operations.
    /// </summary>
    public class Automaton
    {
        /// <summary>
        /// Minimize using Huffman's O(n<sup>2</sup>) algorithm.
        ///   This is the standard text-book algorithm.
        /// </summary>
        public const int MinimizeHuffman = 0;

        /// <summary>
        /// Minimize using Brzozowski's O(2<sup>n</sup>) algorithm. 
        ///   This algorithm uses the reverse-determinize-reverse-determinize trick, which has a bad
        ///   worst-case behavior but often works very well in practice even better than Hopcroft's!).
        /// </summary>
        public const int MinimizeBrzozowski = 1;

        /// <summary>
        /// Minimize using Hopcroft's O(n log n) algorithm.
        ///   This is regarded as one of the most generally efficient algorithms that exist.
        /// </summary>
        public const int MinimizeHopcroft = 2;

        private static bool minimizeAlways = false;

        /// <summary>
        /// The hash code.
        /// </summary>
        private int hashCode;

        /// <summary>
        /// The initial.
        /// </summary>
        private State initial;

        /// <summary>
        /// Initializes a new instance of the <see cref="Automaton"/> class that accepts the empty 
        ///   language. Using this constructor, automata can be constructed manually from 
        ///   <see cref="State"/> and <see cref="Transition"/> objects.
        /// </summary>
        public Automaton()
        {
            this.Initial = new State();
            this.IsDeterministic = true;
            this.Singleton = null;
        }

        /// <summary>
        /// Gets the minimization algorithm (default: 
        /// <code>
        /// MINIMIZE_HOPCROFT
        /// </code>
        /// ).
        /// </summary>
        public static int Minimization
        {
            get { return Automaton.MinimizeHopcroft; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether operations may modify the input automata (default:
        ///   <code>
        /// false
        /// </code>
        /// ).
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow mutation]; otherwise, <c>false</c>.
        /// </value>
        public static bool AllowMutation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this automaton is definitely deterministic (i.e.,
        ///   there are no choices for any run, but a run may crash).
        /// </summary>
        /// <value>
        /// <c>true</c> then this automaton is definitely deterministic (i.e., there are no 
        ///   choices for any run, but a run may crash)., <c>false</c>.
        /// </value>
        public bool IsDeterministic { get; set; }

        /// <summary>
        /// Gets or sets the initial state of this automaton.
        /// </summary>
        /// <value>
        /// The initial state of this automaton.
        /// </value>
        public State Initial
        {
            get
            {
                this.ExpandSingleton();
                return this.initial;
            }

            set
            {
                this.Singleton = null;
                this.initial = value;
            }
        }

        /// <summary>
        /// Gets or sets the singleton string. Null if not applicable.
        /// </summary>
        /// <value>
        /// The singleton string. Null if not applicable.
        /// </value>
        public string Singleton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is singleton.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is singleton; otherwise, <c>false</c>.
        /// </value>
        public bool IsSingleton
        {
            get { return this.Singleton != null; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is debug.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is debug; otherwise, <c>false</c>.
        /// </value>
        public bool IsDebug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsEmpty.
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Gets the number of states in this automaton.
        /// </summary>
        /// Returns the number of states in this automaton.
        public int NumberOfStates
        {
            get
            {
                if (this.IsSingleton)
                {
                    return this.Singleton.Length + 1;
                }

                return this.GetStates().Count;
            }
        }

        /// <summary>
        /// Gets the number of transitions in this automaton. This number is counted
        ///   as the total number of edges, where one edge may be a character interval.
        /// </summary>
        public int NumberOfTransitions
        {
            get
            {
                if (this.IsSingleton)
                {
                    return this.Singleton.Length;
                }

                return this.GetStates().Sum(s => s.Transitions.Count);
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures
        /// like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (this.hashCode == 0)
            {
                this.Minimize();
            }

            return this.hashCode;
        }

        /// <summary>
        /// The minimize.
        /// </summary>
        public void Minimize()
        {
            MinimizationOperations.Minimize(this);
        }

        public Automaton Intersection(Automaton automaton)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a shallow copy of the current Automaton.
        /// </summary>
        /// <returns>
        /// A shallow copy of the current Automaton.
        /// </returns>
        public Automaton Clone()
        {
            var a = (Automaton)this.MemberwiseClone();
            if (!this.IsSingleton)
            {
                HashSet<State> states = this.GetStates();
                var d = states.ToDictionary(s => s, s => new State());

                foreach (State s in states)
                {
                    State p;
                    if (!d.TryGetValue(s, out p))
                    {
                        continue;
                    }

                    p.Accept = s.Accept;
                    if (s == this.Initial)
                    {
                        a.Initial = p;
                    }

                    foreach (Transition t in s.Transitions)
                    {
                        State to;
                        d.TryGetValue(t.To, out to);
                        p.Transitions.Add(new Transition(t.Min, t.Max, to));
                    }
                }
            }

            return a;
        }

        /// <summary>
        /// Gets the set of states that are reachable from the initial state.
        /// </summary>
        /// <returns>
        /// The set of states that are reachable from the initial state.
        /// </returns>
        public HashSet<State> GetStates()
        {
            this.ExpandSingleton();
            HashSet<State> visited;
            if (this.IsDebug)
            {
                visited = new HashSet<State>(); // LinkedHashSet
            }
            else
            {
                visited = new HashSet<State>();
            }

            var worklist = new LinkedList<State>();
            worklist.AddLast(this.Initial);
            visited.Add(this.Initial);
            while (worklist.Count > 0)
            {
                State s = worklist.RemoveAndReturnFirst();
                HashSet<Transition> tr = this.IsDebug
                                             ? new HashSet<Transition>(s.GetSortedTransitions(false))
                                             : new HashSet<Transition>(s.Transitions);
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

        public Automaton Complement()
        {
            throw new NotImplementedException();
        }

        public Automaton Repeat(int min, int max)
        {
            throw new NotImplementedException();
        }

        public Automaton Repeat(int min)
        {
            throw new NotImplementedException();
        }

        public Automaton Repeat()
        {
            throw new NotImplementedException();
        }

        public Automaton Optional()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A clone of this automaton, expands if singleton.
        /// </summary>
        /// <returns>
        /// Returns a clone of this automaton, expands if singleton.
        /// </returns>
        public Automaton CloneExpanded()
        {
            Automaton a = this.Clone();
            a.ExpandSingleton();
            return a;
        }

        /// <summary>
        /// A clone of this automaton unless 
        /// <code>
        /// allowMutation
        /// </code>
        /// is set, expands if singleton.
        /// </summary>
        /// <returns>
        /// Returns a clone of this automaton unless 
        /// <code>
        /// allowMutation
        /// </code>
        /// is set, expands if singleton.
        /// </returns>
        public Automaton CloneExpandedIfRequired()
        {
            if (Automaton.AllowMutation)
            {
                this.ExpandSingleton();
                return this;
            }

            return this.CloneExpanded();
        }

        /// <summary>
        /// The expand singleton.
        /// </summary>
        private void ExpandSingleton()
        {
            if (this.IsSingleton)
            {
                var p = new State();
                initial = p;
                foreach (char t in this.Singleton)
                {
                    var q = new State();
                    p.Transitions.Add(new Transition(t, q));
                    p = q;
                }

                p.Accept = true;
                this.IsDeterministic = true;
                this.Singleton = null;
            }
        }

        /// <summary>
        /// The check minimize always.
        /// </summary>
        public void CheckMinimizeAlways()
        {
            if (minimizeAlways)
            {
                this.Minimize();
            }
        }

        /// <summary>
        /// The clear hash code.
        /// </summary>
        public void ClearHashCode()
        {
            this.hashCode = 0;
        }

        /// <summary>
        /// Recomputes the hash code.
        ///   The automaton must be minimal when this operation is performed.
        /// </summary>
        public void RecomputeHashCode()
        {
            this.hashCode = (this.NumberOfStates * 3) + (this.NumberOfTransitions * 2);
            if (hashCode == 0)
            {
                hashCode = 1;
            }
        }
    }
}