using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

namespace NAutomaton
{
    public class State : IComparable<State>
    {
        private static int nextId;

        private readonly int id;

        private bool isAccept;
        private HashSet<Transition> transitions;

        public State()
        {
            ResetTransitions();
            id = nextId++;
        }

        public int Number { get; set; }

        public bool IsAccept
        {
            get { return isAccept; }
            set { isAccept = value; }
        }

        public HashSet<Transition> Transitions
        {
            get { return transitions; }
        }

        public int CompareTo(State s)
        {
            return s.id - id;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append("state ").Append(Number);
            b.Append(isAccept ? " [accept]" : " [reject]");
            b.Append(":\n");
            foreach (Transition t in transitions)
            {
                b.Append("  ").Append(t.ToString()).Append("\n");
            }
            return b.ToString();
        }

        public void AddTransition(Transition t)
        {
            transitions.Add(t);
        }

        public State Step(char c)
        {
            return (from t in transitions
                    where t.Min <= c && c <= t.Max
                    select t.To).FirstOrDefault();
        }

        public void Step(char c, Collection<State> dest)
        {
            foreach (Transition t in transitions)
            {
                if (t.Min <= c && c <= t.Max)
                {
                    dest.Add(t.To);
                }
            }
        }

        public IEnumerable<Transition> GetSortedTransitions(bool toFirst)
        {
            return GetSortedTransitionArray(toFirst);
        }

        private void ResetTransitions()
        {
            transitions = new HashSet<Transition>();
        }

        private IEnumerable<Transition> GetSortedTransitionArray(bool toFirst)
        {
            Transition[] e = transitions.ToArray();
            Array.Sort(e, new TransitionComparer(toFirst));
            return e;
        }
    }
}