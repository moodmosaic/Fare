using System;
using System.Collections.Generic;
using System.Linq;

namespace Fare
{
    /// <summary>
    /// Special automata operations.
    /// </summary>
    public static class SpecialOperations
    {
        /// <summary>
        /// Reverses the language of the given (non-singleton) automaton while returning the set of 
        /// new initial states.
        /// </summary>
        /// <param name="a">The automaton.</param>
        /// <returns></returns>
        public static HashSet<State> Reverse(Automaton a)
        {
            // Reverse all edges.
            var m = new Dictionary<State, HashSet<Transition>>();
            HashSet<State> states = a.GetStates();
            HashSet<State> accept = a.GetAcceptStates();
            foreach (State r in states)
            {
                m.Add(r, new HashSet<Transition>());
                r.Accept = false;
            }

            foreach (State r in states)
            {
                foreach (Transition t in r.Transitions)
                {
                    m[t.To].Add(new Transition(t.Min, t.Max, r));
                }
            }

            foreach (State r in states)
            {
                r.Transitions = m[r].ToList();
            }

            // Make new initial+final states.
            a.Initial.Accept = true;
            a.Initial = new State();
            foreach (State r in accept)
            {
                a.Initial.AddEpsilon(r); // Ensures that all initial states are reachable.
            }

            a.IsDeterministic = false;
            return accept;
        }

        /// <summary>
        /// Returns an automaton that accepts the overlap of strings that in more than one way can be 
        /// split into a left part being accepted by <code>a1</code> and a right part being accepted 
        /// by <code>a2</code>.
        /// </summary>
        /// <param name="a1">The a1.</param>
        /// <param name="a2">The a2.</param>
        /// <returns></returns>
        public static Automaton Overlap(Automaton a1, Automaton a2)
        {
            throw new NotImplementedException();
        }

        private static void AcceptToAccept(Automaton a)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an automaton that accepts the single chars that occur in strings that are accepted
        /// by the given automaton. Never modifies the input automaton.
        /// </summary>
        /// <param name="a">The a.</param>
        /// <returns></returns>
        public static Automaton SingleChars(Automaton a)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an automaton that accepts the trimmed language of the given automaton. The 
        /// resulting automaton is constructed as follows: 1) Whenever a <code>c</code> character is
        /// allowed in the original automaton, one or more <code>set</code> characters are allowed in
        /// the new automaton. 2) The automaton is prefixed and postfixed with any number of <code>
        /// set</code> characters.
        /// </summary>
        /// <param name="a">The a.</param>
        /// <param name="set">The set of characters to be trimmed.</param>
        /// <param name="c">The canonical trim character (assumed to be in <code>set</code>).</param>
        /// <returns></returns>
        public static Automaton Trim(Automaton a, String set, char c)
        {
            throw new NotImplementedException();
        }

        private static void AddSetTransitions(State s, String set, State p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an automaton that accepts the compressed language of the given automaton. 
        /// Whenever a <code>c</code> character is allowed in the original automaton, one or more 
        /// <code>set</code> characters are allowed in the new automaton.
        /// </summary>
        /// <param name="a">The a.</param>
        /// <param name="set">The set of characters to be compressed.</param>
        /// <param name="c">The canonical compress character (assumed to be in <code>set</code>).
        /// </param>
        /// <returns></returns>
        public static Automaton Compress(Automaton a, String set, char c)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Returns an automaton where all transition labels have been substituted. 
        /// <p> Each transition labeled <code>c</code> is changed to a set of transitions, one for 
        /// each character in <code>map(c)</code>. If <code>map(c)</code> is null, then the 
        /// transition is unchanged.
        /// </summary>
        /// <param name="a">The a.</param>
        /// <param name="map">The map from characters to sets of characters (where characters 
        /// are <code>Character</code> objects).</param>
        /// <returns></returns>
        public static Automaton Subst(Automaton a, Map<Character, Set<Character>> map) 
        {
            throw new NotImplementedException();
        }
    }
}
