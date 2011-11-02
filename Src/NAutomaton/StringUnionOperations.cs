using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NAutomaton
{
    /**
    * Operations for building minimal deterministic automata from sets of strings. 
    * The algorithm requires sorted input data, but is very fast (nearly linear with the input size).
    * 
    * @author Dawid Weiss
    */
    public sealed class StringUnionOperations
    {
    //    /**
    // * "register" for state interning.
    // */
    //    private HashMap<State, State> register = new HashMap<State, State>();

    //    /**
    //     * Root automaton state.
    //     */
    //    private State root = new State();

    //    /**
    //     * Previous sequence added to the automaton in {@link #add(CharSequence)}.
    //     */
    //    private StringBuilder previous;

    //    /**
    //     * Add another character sequence to this automaton. The sequence must be
    //     * lexicographically larger or equal compared to any previous sequences
    //     * added to this automaton (the input must be sorted).
    //     */
    //    public void add(CharSequence current) {
    //    assert register != null : "Automaton already built.";
    //    assert current.length() > 0 : "Input sequences must not be empty.";
    //    assert previous == null || LEXICOGRAPHIC_ORDER.compare(previous, current) <= 0 : 
    //        "Input must be sorted: " + previous + " >= " + current;
    //    assert setPrevious(current);

    //    // Descend in the automaton (find matching prefix). 
    //    int pos = 0, max = current.length();
    //    State next, state = root;
    //    while (pos < max && (next = state.lastChild(current.charAt(pos))) != null) {
    //        state = next;
    //        pos++;
    //    }

    //    if (state.hasChildren())
    //        replaceOrRegister(state);

    //    addSuffix(state, current, pos);
    //}

    //    /**
    //     * Finalize the automaton and return the root state. No more strings can be
    //     * added to the builder after this call.
    //     * 
    //     * @return Root automaton state.
    //     */
    //    public State complete()
    //    {
    //        if (this.register == null)
    //            throw new IllegalStateException();

    //        if (root.hasChildren())
    //            replaceOrRegister(root);

    //        register = null;
    //        return root;
    //    }

    //    /**
    //     * Internal recursive traversal for conversion.
    //     */
    //    private static dk.brics.automaton.State convert(State s,
    //            IdentityHashMap<State, dk.brics.automaton.State> visited) {
    //    dk.brics.automaton.State converted = visited.get(s);
    //    if (converted != null)
    //        return converted;

    //    converted = new dk.brics.automaton.State();
    //    converted.setAccept(s.is_final);

    //    visited.put(s, converted);
    //    int i = 0;
    //    char [] labels = s.labels;
    //    for (StringUnionOperations.State target : s.states) {
    //        converted.addTransition(new Transition(labels[i++], convert(target, visited)));
    //    }

    //    return converted;
    //}

    //    /**
    //     * Build a minimal, deterministic automaton from a sorted list of strings.
    //     */
    //    public static dk.brics.automaton.State build(CharSequence[] input) {
    //    final StringUnionOperations builder = new StringUnionOperations(); 

    //    for (CharSequence chs : input)
    //        builder.add(chs);

    //    return convert(builder.complete(), new IdentityHashMap<State, dk.brics.automaton.State>());
    //}

    //    /**
    //     * Copy <code>current</code> into an internal buffer.
    //     */
    //    private boolean setPrevious(CharSequence current)
    //    {
    //        if (previous == null)
    //            previous = new StringBuilder();

    //        previous.setLength(0);
    //        previous.append(current);

    //        return true;
    //    }

    //    /**
    //     * Replace last child of <code>state</code> with an already registered
    //     * state or register the last child state.
    //     */
    //    private void replaceOrRegister(State state) {
    //    final State child = state.lastChild();

    //    if (child.hasChildren())
    //        replaceOrRegister(child);

    //    final State registered = register.get(child);
    //    if (registered != null) {
    //        state.replaceLastChild(registered);
    //    } else {
    //        register.put(child, child);
    //    }
    //}

    //    /**
    //     * Add a suffix of <code>current</code> starting at <code>fromIndex</code>
    //     * (inclusive) to state <code>state</code>.
    //     */
    //    private void addSuffix(State state, CharSequence current, int fromIndex) {
    //    final int len = current.length();
    //    for (int i = fromIndex; i < len; i++) {
    //        state = state.newState(current.charAt(i));
    //    }
    //    state.is_final = true;
    //}

        /**
        * Lexicographic order of input sequences.
        */
        private sealed class LexicographicOrder : Comparer<char[]>
        {
            public override int Compare(char[] s1, char[] s2)
            {
                int lens1 = s1.Length;
                int lens2 = s2.Length;
                int max = Math.Min(lens1, lens2);

                for (int i = 0; i < max; i++)
                {
                    char c1 = s1[i];
                    char c2 = s2[i];
                    if (c1 != c2)
                        return c1 - c2;
                }

                return lens1 - lens2;
            }
        }

        /**
        * State with <code>char</code> labels on transitions.
        */
        private sealed class State
        {
            /** An empty set of labels. */
            private static readonly char[] noLabels = new char[0];

            /** An empty set of states. */
            private static readonly State[] noStates = new State[0];

            /**
             * Labels of outgoing transitions. Indexed identically to {@link #states}.
             * Labels must be sorted lexicographically.
             */
            private char[] labels = noLabels;

            /**
             * States reachable from outgoing transitions. Indexed identically to
             * {@link #labels}.
             */
            private State[] states = noStates;

            /**
             * <code>true</code> if this state corresponds to the end of at least one
             * input sequence.
             */
            private readonly bool isFinal;

            /**
             * Returns the target state of a transition leaving this state and labeled
             * with <code>label</code>. If no such transition exists, returns
             * <code>null</code>.
             */
            public State GetState(char label)
            {
                int index = Array.BinarySearch(labels, label);
                return index >= 0 ? states[index] : null;
            }

            /**
             * Returns an array of outgoing transition labels. The array is sorted in 
             * lexicographic order and indexes correspond to states returned from 
             * {@link #getStates()}.
             */
            public char[] GetTransitionLabels()
            {
                return this.labels;
            }

            /**
             * Returns an array of outgoing transitions from this state. The returned
             * array must not be changed.
             */
            public State[] GetStates()
            {
                return this.states;
            }

            /**
             * Two states are equal if:
             * <ul>
             * <li>they have an identical number of outgoing transitions, labeled with
             * the same labels</li>
             * <li>corresponding outgoing transitions lead to the same states (to states
             * with an identical right-language).
             * </ul>
             */
            public override bool Equals(Object obj)
            {
                var other = (State)obj;
                return isFinal == other.isFinal
                    && Object.Equals(this.labels, other.labels)
                    && State.ReferenceEquals(this.states, other.states);
            }

            /**
             * Return <code>true</code> if this state has any children (outgoing
             * transitions).
             */
            public bool HasChildren()
            {
                return labels.Length > 0;
            }

            /**
             * Is this state a final state in the automaton?
             */
            public bool IsFinal()
            {
                return isFinal;
            }

            /**
             * Compute the hash code of the <i>current</i> status of this state.
             */
            public override int GetHashCode()
            {
                int hash = isFinal ? 1 : 0;

                hash ^= hash * 31 + this.labels.Length;

                hash = this.labels.Aggregate(hash, (current, c) => current ^ current * 31 + c);

                /*
                 * Compare the right-language of this state using reference-identity of
                 * outgoing states. This is possible because states are interned (stored
                 * in registry) and traversed in post-order, so any outgoing transitions
                 * are already interned.
                 */
                return this.states.Aggregate(hash, (current, s) => current ^ RuntimeHelpers.GetHashCode(s));
            }

            /**
             * Create a new outgoing transition labeled <code>label</code> and return
             * the newly created target state for this transition.
             */
            private State NewState(char label)
            {
                if (Array.BinarySearch(labels, label) < 0)
                {
                    throw new InvalidOperationException("State already has transition labeled: " + label);
                }

                labels = CopyOf(labels, labels.Length + 1);
                states = CopyOf(states, states.Length + 1);

                labels[labels.Length - 1] = label;
                return states[states.Length - 1] = new State();
            }

            /**
             * Return the most recent transitions's target state.
             */
            private State LastChild()
            {
                if (!HasChildren())
                {
                    throw new InvalidOperationException("No outgoing transitions.");
                }

                return states[states.Length - 1];
            }

            /**
             * Return the associated state if the most recent transition
             * is labeled with <code>label</code>.
             */
            private State LastChild(char label)
            {
                int index = labels.Length - 1;
                State s = null;
                if (index >= 0 && labels[index] == label)
                {
                    s = states[index];
                }

                if (s != GetState(label))
                {
                    throw new InvalidOperationException("s");
                }

                return s;
            }

            /**
             * Replace the last added outgoing transition's target state with the given
             * state.
             */
            private void ReplaceLastChild(State state)
            {
                if (HasChildren() == false)
                {
                    throw new InvalidOperationException("No outgoing transitions.");
                }

                states[states.Length - 1] = state;
            }

            /**
             * JDK1.5-replacement of {@link Arrays#copyOf(char[], int)}
             */
            private static char[] CopyOf(char[] original, int newLength)
            {
                var copy = new char[newLength];
                Array.Copy(original, 0, copy, 0, Math.Min(original.Length, newLength));
                return copy;
            }

            /**
             * JDK1.5-replacement of {@link Arrays#copyOf(char[], int)}
             */
            private static State[] CopyOf(State[] original, int newLength)
            {
                var copy = new State[newLength];
                Array.Copy(original, 0, copy, 0, Math.Min(original.Length, newLength));
                return copy;
            }

            /**
             * Compare two lists of objects for reference-equality.
             */
            private static bool ReferenceEquals(Object[] a1, Object[] a2)
            {
                if (a1.Length != a2.Length)
                    return false;

                return !a1.Where((t, i) => t != a2[i]).Any();
            }
        }
    }
}