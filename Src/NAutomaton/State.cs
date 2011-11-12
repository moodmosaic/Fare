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
using System.Text;

namespace NAutomaton
{
    public class State : IComparable<State>, IEquatable<State>
    {
        private static int nextId;

        private readonly int id;

        private bool accept;
        private HashSet<Transition> transitions;

        public State()
        {
            transitions = new HashSet<Transition>();

            this.ResetTransitions();
            this.id = nextId++;
        }

        public int Number { get; set; }

        public bool Accept
        {
            get { return this.accept; }
            set { this.accept = value; }
        }

        public HashSet<Transition> Transitions
        {
            get { return this.transitions; }
        }

        public int Id
        {
            get { return id; }
        }

        internal void AddEpsilon(State to)
        {
            if (to.accept)
            {
                accept = true;
            }

            foreach (Transition t in to.transitions)
            {
                transitions.Add(t);
            }
        }

        public void AddTransition(Transition t)
        {
            this.transitions.Add(t);
        }

        public State Step(char c)
        {
            return (from t in this.transitions
                    where t.Min <= c && c <= t.Max
                    select t.To).FirstOrDefault();
        }

        public void Step(char c, ICollection<State> dest)
        {
            foreach (Transition t in this.transitions)
            {
                if (t.Min <= c && c <= t.Max)
                {
                    dest.Add(t.To);
                }
            }
        }

        public IList<Transition> GetSortedTransitions(bool toFirst)
        {
            Transition[] e = this.transitions.ToArray();
            Array.Sort(e, new TransitionComparer(toFirst));
            return e.ToList();
        }

        public void ResetTransitions()
        {
            this.transitions = new HashSet<Transition>();
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append("state ").Append(this.Number);
            b.Append(accept ? " [accept]" : " [reject]");
            b.Append(":\n");
            foreach (Transition t in this.transitions)
            {
                b.Append("  ").Append(t.ToString()).Append("\n");
            }
            return b.ToString();
        }

        public int CompareTo(State s)
        {
            return s.Id - this.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(State))
            {
                return false;
            }

            return Equals((State)obj);
        }

        public bool Equals(State other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Id == Id;

            //&& other.Accept.Equals(Accept) && Equals(other.Transitions, Transitions) && other.Number == Number;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = id * 397;
            
                //result = (result * 397) ^ accept.GetHashCode();
                //result = (result * 397) ^ (transitions != null ? transitions.GetHashCode() : 0);
                
                result = (result * 397) ^ Number;
                return result;
            }
        }
    }
}