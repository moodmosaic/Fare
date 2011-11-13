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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace NAutomaton
{
    /// <summary>
    /// <tt>Automaton</tt> state.
    /// </summary>
    public class State : IComparable<State>
    {
        /// <summary>
        /// The id.
        /// </summary>
        private readonly int id;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class. Initially, the new state is a 
        ///   reject state.
        /// </summary>
        public State()
        {
            this.ResetTransitions();
            Interlocked.Increment(ref this.id);
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this State is Accept.
        /// </summary>
        public bool Accept { get; set; }

        /// <summary>
        /// Gets or sets this State Number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets this State Transitions.
        /// </summary>
        public IList<Transition> Transitions { get; set; }

        /// <summary>
        /// Compares the current object with another object of the same type.
        ///   States are ordered by the time of construction.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared.
        /// </returns>
        public int CompareTo(State other)
        {
            return other.Id - this.Id;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> describing this state. Normally invoked via <see cref="Automaton.ToString()"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> describing this state.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("state ").Append(this.Number);
            sb.Append(this.Accept ? " [accept]" : " [reject]");
            sb.Append(":\n");
            foreach (Transition t in this.Transitions)
            {
                sb.Append("  ").Append(t.ToString()).Append("\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// The reset transitions.
        /// </summary>
        private void ResetTransitions()
        {
            this.Transitions = new List<Transition>();
        }

        /// <summary>
        /// Adds an outgoing transition.
        /// </summary>
        /// <param name="t">
        /// The transition.
        /// </param>
        public void AddTransition(Transition t)
        {
            this.Transitions.Add(t);
        }

        /// <summary>
        /// Performs lookup in transitions, assuming determinism.
        /// </summary>
        /// <param name="c">
        /// The character to look up.
        /// </param>
        /// <returns>
        /// The destination state, null if no matching outgoing transition.
        /// </returns>
        public State Step(char c)
        {
            return (from t in this.Transitions where t.Min <= c && c <= t.Max select t.To).FirstOrDefault();
        }

        /// <summary>
        /// Performs lookup in transitions, allowing nondeterminism.
        /// </summary>
        /// <param name="c">
        /// The character to look up.
        /// </param>
        /// <param name="dest">
        /// The collection where destination states are stored.
        /// </param>
        public void Step(char c, Collection<State> dest)
        {
            foreach (Transition t in this.Transitions)
            {
                if (t.Min <= c && c <= t.Max)
                {
                    dest.Add(t.To);
                }
            }
        }

        /// <summary>
        /// Gets the transitions sorted by (min, reverse max, to) or (to, min, reverse max).
        /// </summary>
        /// <param name="toFirst">
        /// if set to <c>true</c> [to first].
        /// </param>
        /// <returns>
        /// The transitions sorted by (min, reverse max, to) or (to, min, reverse max).
        /// </returns>
        public IList<Transition> GetSortedTransitions(bool toFirst)
        {
            Transition[] e = this.Transitions.ToArray();
            Array.Sort(e, new TransitionComparer(toFirst));
            return e.ToList();
        }

        internal void AddEpsilon(State to)
        {
            if (to.Accept)
            {
                this.Accept = true;
            }

            foreach (Transition t in to.Transitions)
            {
                this.Transitions.Add(t);
            }
        }
    }
}