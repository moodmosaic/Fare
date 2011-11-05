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
using System.Text;

namespace NAutomaton
{
    public class Automaton
    {
        public const int MINIMIZE_HUFFMAN = 0;

        public const int MINIMIZE_BRZOZOWSKI = 1;

        public const int MINIMIZE_HOPCROFT = 2;

        internal static int minimization = MINIMIZE_HOPCROFT;

        internal State initial;

        internal bool deterministic;

        internal object info;

        internal int hash_code;

        internal string singleton;

        internal static bool minimize_always = false;

        internal static bool allow_mutation = false;

        internal static bool? is_debug = null;
         
        public Automaton()
        {
            initial = new State();
            deterministic = true;
            singleton = null;
        }

        internal virtual bool isDebug
        {
            get
            {
                if (is_debug == null)
                {
                    is_debug = Convert.ToBoolean(Environment.GetEnvironmentVariable("dk.brics.automaton.debug") != null);
                }

                return (bool)is_debug;
            }
        }

        public static int Minimization
        {
            set
            {
                minimization = value;
            }
        }

        public static bool MinimizeAlways
        {
            set
            {
                minimize_always = value;
            }
        }

        public static bool setAllowMutate(bool flag)
        {
            bool b = allow_mutation;
            allow_mutation = flag;
            return b;
        }

        internal static bool AllowMutate
        {
            get
            {
                return allow_mutation;
            }
        }

        internal virtual void checkMinimizeAlways()
        {
            if (minimize_always)
            {
                minimize();
            }
        }

        internal virtual bool isSingleton
        {
            get
            {
                return singleton!=null;
            }
        }

        public virtual string Singleton
        {
            get
            {
                return singleton;
            }
        }

        public virtual State InitialState
        {
            set
            {
                initial = value;
                singleton = null;
            }
            get
            {
                expandSingleton();
                return initial;
            }
        }

        public virtual bool isDeterministic
        {
            get
            {
                return deterministic;
            }
            set
            {
                this.deterministic = value;
            }
        }

        public virtual object Info
        {
            set
            {
                this.info = value;
            }
            get
            {
                return info;
            }
        }

        public virtual HashSet<State> States
        {
            get
            {
                expandSingleton();
                HashSet<State> visited;
                if (isDebug)
                {
                    visited = new HashSet<State>();
                }
                else
                {
                    visited = new HashSet<State>();
                }
                LinkedList<State> worklist = new LinkedList<State>();
                worklist.AddLast(initial);
                visited.Add(initial);
                while (worklist.Count > 0)
                {
                    State s = worklist.RemoveAndReturnFirst();
                    ICollection<Transition> tr;
                    if (isDebug)
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
                expandSingleton();
                HashSet<State> accepts = new HashSet<State>();
                HashSet<State> visited = new HashSet<State>();
                LinkedList<State> worklist = new LinkedList<State>();
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

        ///	 
        ///	 <summary>  Assigns consecutive numbers to the given states.  </summary>
        ///	 
        internal static HashSet<State> StateNumbers
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

        ///	 
        ///	 <summary>  Adds transitions to explicit crash state to ensure that transition function is total.  </summary>
        ///	 
        internal virtual void totalize()
        {
            State s = new State();
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

        ///	
        ///	 <summary>  Restores representation invariant.
        ///	  This method must be invoked before any built-in automata operation is performed 
        ///	  if automaton states or transitions are manipulated manually. </summary>
        ///	  <seealso cref= #setDeterministic(boolean) </seealso>
        ///	 
        public virtual void restoreInvariant()
        {
            removeDeadTransitions();
        }

        ///	 
        ///	 <summary>  Reduces this automaton.
        ///	  An automaton is "reduced" by combining overlapping and adjacent edge intervals with same destination.  </summary>
        ///	 
        public virtual void reduce()
        {
            if (Singleton)
            {
                return;
            }
            Set<State> states = States;
            StateNumbers = states;
            foreach (State s in states)
            {
                IList<Transition> st = s.getSortedTransitions(true);
                s.resetTransitions();
                State p = null;
                int min = -1, max = -1;
                foreach (Transition t in st)
                {
                    if (p == t.to)
                    {
                        if (t.min <= max + 1)
                        {
                            if (t.max > max)
                            {
                                max = t.max;
                            }
                        }
                        else
                        {
                            if (p != null)
                            {
                                s.transitions.add(new Transition((char)min, (char)max, p));
                            }
                            min = t.min;
                            max = t.max;
                        }
                    }
                    else
                    {
                        if (p != null)
                        {
                            s.transitions.add(new Transition((char)min, (char)max, p));
                        }
                        p = t.to;
                        min = t.min;
                        max = t.max;
                    }
                }
                if (p != null)
                {
                    s.transitions.add(new Transition((char)min, (char)max, p));
                }
            }
            clearHashCode();
        }

        ///	 
        ///	 <summary>  Returns sorted array of all interval start points.  </summary>
        ///	 
        internal virtual char[] StartPoints
        {
            get
            {
                Set<char?> pointset = new HashSet<char?>();
                foreach (State s in States)
                {
                    pointset.add(Char.MinValue);
                    foreach (Transition t in s.transitions)
                    {
                        pointset.add(t.min);
                        if (t.max < Char.MaxValue)
                        {
                            pointset.add((char)(t.max + 1));
                        }
                    }
                }
                char[] points = new char[pointset.size()];
                int n = 0;
                foreach (char? m in pointset)
                {
                    points[n++] = m;
                }
                Arrays.sort(points);
                return points;
            }
        }

        ///	 
        ///	 <summary>  Returns the set of live states. A state is "live" if an accept state is reachable from it.  </summary>
        ///	  <returns> set of <seealso cref="State"/> objects </returns>
        ///	 
        public virtual Set<State> LiveStates
        {
            get
            {
                expandSingleton();
                return getLiveStates(States);
            }
        }

        private Set<State> getLiveStates(Set<State> states)
        {
            Dictionary<State, Set<State>> map = new Dictionary<State, Set<State>>();
            foreach (State s in states)
            {
                map.Add(s, new HashSet<State>());
            }
            foreach (State s in states)
            {
                foreach (Transition t in s.transitions)
                {
                    map[t.to].add(s);
                }
            }
            Set<State> live = new HashSet<State>(AcceptStates);
            LinkedList<State> worklist = new LinkedList<State>(live);
            while (worklist.Count > 0)
            {
                State s = worklist.RemoveFirst();
                foreach (State p in map[s])
                {
                    if (!live.contains(p))
                    {
                        live.add(p);
                        worklist.AddLast(p);
                    }
                }
            }
            return live;
        }

        ///	 
        ///	 <summary>  Removes transitions to dead states and calls <seealso cref="#reduce()"/> and <seealso cref="#clearHashCode()"/>.
        ///	  (A state is "dead" if no accept state is reachable from it.) </summary>
        ///	 
        public virtual void removeDeadTransitions()
        {
            clearHashCode();
            if (Singleton)
            {
                return;
            }
            Set<State> states = States;
            Set<State> live = getLiveStates(states);
            foreach (State s in states)
            {
                Set<Transition> st = s.transitions;
                s.resetTransitions();
                foreach (Transition t in st)
                {
                    if (live.contains(t.to))
                    {
                        s.transitions.add(t);
                    }
                }
            }
            reduce();
        }

        ///	 
        ///	 <summary>  Returns a sorted array of transitions for each state (and sets state numbers).  </summary>
        ///	 
        internal static Transition[][] getSortedTransitions(Set<State> states)
        {
            StateNumbers = states;
            Transition[][] transitions = new Transition[states.size()][];
            foreach (State s in states)
            {
                transitions[s.number] = s.getSortedTransitionArray(false);
            }
            return transitions;
        }

        ///	 
        ///	 <summary>  Expands singleton representation to normal representation.
        ///	  Does nothing if not in singleton representation.  </summary>
        ///	 
        public virtual void expandSingleton()
        {
            if (Singleton)
            {
                State p = new State();
                initial = p;
                for (int i = 0; i < singleton.Length; i++)
                {
                    State q = new State();
                    p.transitions.add(new Transition(singleton[i], q));
                    p = q;
                }
                p.accept = true;
                deterministic = true;
                singleton = null;
            }
        }

        ///	
        ///	 <summary>  Returns the number of states in this automaton. </summary>
        ///	 
        public virtual int NumberOfStates
        {
            get
            {
                if (Singleton)
                {
                    return singleton.Length + 1;
                }
                return States.size();
            }
        }

        ///	
        ///	 <summary>  Returns the number of transitions in this automaton. This number is counted
        ///	  as the total number of edges, where one edge may be a character interval. </summary>
        ///	 
        public virtual int NumberOfTransitions
        {
            get
            {
                if (Singleton)
                {
                    return singleton.Length;
                }
                int c = 0;
                foreach (State s in States)
                {
                    c += s.transitions.size();
                }
                return c;
            }
        }

        ///	
        ///	 <summary>  Returns true if the language of this automaton is equal to the language
        ///	  of the given automaton. Implemented using <code>hashCode</code> and
        ///	  <code>subsetOf</code>. </summary>
        ///	 
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
            Automaton a = (Automaton)obj;
            if (Singleton && a.Singleton)
            {
                return singleton.Equals(a.singleton);
            }
            return GetHashCode() == a.GetHashCode() && subsetOf(a) && a.subsetOf(this);
        }

        ///	
        ///	 <summary>  Returns hash code for this automaton. The hash code is based on the
        ///	  number of states and transitions in the minimized automaton.
        ///	  Invoking this method may involve minimizing the automaton. </summary>
        ///	 
        public override int GetHashCode()
        {
            if (hash_code == 0)
            {
                minimize();
            }
            return hash_code;
        }

        ///	
        ///	 <summary>  Recomputes the hash code.
        ///	  The automaton must be minimal when this operation is performed. </summary>
        ///	 
        internal virtual void recomputeHashCode()
        {
            hash_code = NumberOfStates  3 + NumberOfTransitions  2;
            if (hash_code == 0)
            {
                hash_code = 1;
            }
        }

        ///	
        ///	 <summary>  Must be invoked when the stored hash code may no longer be valid. </summary>
        ///	 
        internal virtual void clearHashCode()
        {
            hash_code = 0;
        }

        ///	
        ///	 <summary>  Returns a string representation of this automaton. </summary>
        ///	 
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            if (Singleton)
            {
                b.Append("singleton: ");
                foreach (char c in singleton.ToCharArray())
                {
                    Transition.appendCharString(c, b);
                }
                b.AppendLine();
            }
            else
            {
                Set<State> states = States;
                StateNumbers = states;
                b.Append("initial state: ").append(initial.number).append("\n");
                foreach (State s in states)
                {
                    b.Append(s.ToString());
                }
            }
            return b.ToString();
        }

        ///	
        ///	 <summary>  Returns <a href="http://www.research.att.com/sw/tools/graphviz/" target="_top">Graphviz Dot</a> 
        ///	  representation of this automaton. </summary>
        ///	 
        public virtual string toDot()
        {
            StringBuilder b = new StringBuilder("digraph Automaton {\n");
            b.Append("  rankdir = LR;\n");
            Set<State> states = States;
            StateNumbers = states;
            foreach (State s in states)
            {
                b.Append("  ").append(s.number);
                if (s.accept)
                {
                    b.Append(" [shape=doublecircle,label=\"\"];\n");
                }
                else
                {
                    b.Append(" [shape=circle,label=\"\"];\n");
                }
                if (s == initial)
                {
                    b.Append("  initial [shape=plaintext,label=\"\"];\n");
                    b.Append("  initial -> ").append(s.number).append("\n");
                }
                foreach (Transition t in s.transitions)
                {
                    b.Append("  ").append(s.number);
                    t.appendDot(b);
                }
            }
            return b.Append("}\n").ToString();
        }

        ///	
        ///	 <summary>  Returns a clone of this automaton, expands if singleton. </summary>
        ///	 
        internal virtual Automaton cloneExpanded()
        {
            Automaton a = Clone();
            a.expandSingleton();
            return a;
        }

        ///	
        ///	 <summary>  Returns a clone of this automaton unless <code>allow_mutation</code> is set, expands if singleton. </summary>
        ///	 
        internal virtual Automaton cloneExpandedIfRequired()
        {
            if (allow_mutation)
            {
                expandSingleton();
                return this;
            }
            else
            {
                return cloneExpanded();
            }
        }

        ///	
        ///	 <summary>  Returns a clone of this automaton. </summary>
        ///	 
        public override Automaton Clone()
        {
            try
            {
                Automaton a = (Automaton)base.Clone();
                if (!Singleton)
                {
                    Dictionary<State, State> m = new Dictionary<State, State>();
                    Set<State> states = States;
                    foreach (State s in states)
                    {
                        m.Add(s, new State());
                    }
                    foreach (State s in states)
                    {
                        State p = m[s];
                        p.accept = s.accept;
                        if (s == initial)
                        {
                            a.initial = p;
                        }
                        foreach (Transition t in s.transitions)
                        {
                            p.transitions.add(new Transition(t.min, t.max, m[t.to]));
                        }
                    }
                }
                return a;
            }
            catch (CloneNotSupportedException e)
            {
                throw new Exception(e);
            }
        }

        ///	
        ///	 <summary>  Returns a clone of this automaton, or this automaton itself if <code>allow_mutation</code> flag is set.  </summary>
        ///	 
        internal virtual Automaton cloneIfRequired()
        {
            if (allow_mutation)
            {
                return this;
            }
            else
            {
                return Clone();
            }
        }

        ///	 
        ///	 <summary>  Retrieves a serialized <code>Automaton</code> located by a URL. </summary>
        ///	  <param name="url"> URL of serialized automaton </param>
        ///	  <exception cref="IOException"> if input/output related exception occurs </exception>
        ///	  <exception cref="OptionalDataException"> if the data is not a serialized object </exception>
        ///	  <exception cref="InvalidClassException"> if the class serial number does not match </exception>
        ///	  <exception cref="ClassCastException"> if the data is not a serialized <code>Automaton</code> </exception>
        ///	  <exception cref="ClassNotFoundException"> if the class of the serialized object cannot be found </exception>
        ///	 
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static Automaton load(java.net.URL url) throws java.io.IOException, java.io.OptionalDataException, ClassCastException, ClassNotFoundException, java.io.InvalidClassException
        public static Automaton load(URL url)
        {
            return load(url.openStream());
        }

        ///	
        ///	 <summary>  Retrieves a serialized <code>Automaton</code> from a stream. </summary>
        ///	  <param name="stream"> input stream with serialized automaton </param>
        ///	  <exception cref="IOException"> if input/output related exception occurs </exception>
        ///	  <exception cref="OptionalDataException"> if the data is not a serialized object </exception>
        ///	  <exception cref="InvalidClassException"> if the class serial number does not match </exception>
        ///	  <exception cref="ClassCastException"> if the data is not a serialized <code>Automaton</code> </exception>
        ///	  <exception cref="ClassNotFoundException"> if the class of the serialized object cannot be found </exception>
        ///	 
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static Automaton load(java.io.InputStream stream) throws java.io.IOException, java.io.OptionalDataException, ClassCastException, ClassNotFoundException, java.io.InvalidClassException
        public static Automaton load(InputStream stream)
        {
            ObjectInputStream s = new ObjectInputStream(stream);
            return (Automaton)s.readObject();
        }

        ///	
        ///	 <summary>  Writes this <code>Automaton</code> to the given stream. </summary>
        ///	  <param name="stream"> output stream for serialized automaton </param>
        ///	  <exception cref="IOException"> if input/output related exception occurs </exception>
        ///	 
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void store(java.io.OutputStream stream) throws java.io.IOException
        public virtual void store(OutputStream stream)
        {
            ObjectOutputStream s = new ObjectOutputStream(stream);
            s.writeObject(this);
            s.flush();
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeEmpty()"/>. </summary>
        ///	 
        public static Automaton makeEmpty()
        {
            return BasicAutomata.makeEmpty();
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeEmptyString()"/>. </summary>
        ///	 
        public static Automaton makeEmptyString()
        {
            return BasicAutomata.makeEmptyString();
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeAnyString()"/>. </summary>
        ///	 
        public static Automaton makeAnyString()
        {
            return BasicAutomata.makeAnyString();
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeAnyChar()"/>. </summary>
        ///	 
        public static Automaton makeAnyChar()
        {
            return BasicAutomata.makeAnyChar();
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeChar(char)"/>. </summary>
        ///	 
        public static Automaton makeChar(char c)
        {
            return BasicAutomata.makeChar(c);
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeCharRange(char, char)"/>. </summary>
        ///	 
        public static Automaton makeCharRange(char min, char max)
        {
            return BasicAutomata.makeCharRange(min, max);
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeCharSet(String)"/>. </summary>
        ///	 
        public static Automaton makeCharSet(string set)
        {
            return BasicAutomata.makeCharSet(set);
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeInterval(int, int, int)"/>. </summary>
        ///	 
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static Automaton makeInterval(int min, int max, int digits) throws IllegalArgumentException
        public static Automaton makeInterval(int min, int max, int digits)
        {
            return BasicAutomata.makeInterval(min, max, digits);
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicAutomata#makeString(String)"/>. </summary>
        ///	 
        public static Automaton makeString(string s)
        {
            return BasicAutomata.makeString(s);
        }

        ///     
        ///     <summary>  See <seealso cref="BasicAutomata#makeStringUnion(CharSequence...)"/>. </summary>
        ///     
        public static Automaton makeStringUnion(params CharSequence[] strings)
        {
            return BasicAutomata.makeStringUnion(strings);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicAutomata#makeMaxInteger(String)"/>. </summary>
        ///	 
        public static Automaton makeMaxInteger(string n)
        {
            return BasicAutomata.makeMaxInteger(n);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicAutomata#makeMinInteger(String)"/>. </summary>
        ///	 
        public static Automaton makeMinInteger(string n)
        {
            return BasicAutomata.makeMinInteger(n);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicAutomata#makeTotalDigits(int)"/>. </summary>
        ///	 
        public static Automaton makeTotalDigits(int i)
        {
            return BasicAutomata.makeTotalDigits(i);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicAutomata#makeFractionDigits(int)"/>. </summary>
        ///	 
        public static Automaton makeFractionDigits(int i)
        {
            return BasicAutomata.makeFractionDigits(i);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicAutomata#makeIntegerValue(String)"/>. </summary>
        ///	 
        public static Automaton makeIntegerValue(string value)
        {
            return BasicAutomata.makeIntegerValue(value);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicAutomata#makeDecimalValue(String)"/>. </summary>
        ///	 
        public static Automaton makeDecimalValue(string value)
        {
            return BasicAutomata.makeDecimalValue(value);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicAutomata#makeStringMatcher(String)"/>. </summary>
        ///	 
        public static Automaton makeStringMatcher(string s)
        {
            return BasicAutomata.makeStringMatcher(s);
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicOperations#concatenate(Automaton, Automaton)"/>. </summary>
        ///	 
        public virtual Automaton concatenate(Automaton a)
        {
            return BasicOperations.concatenate(this, a);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#concatenate(List)"/>. </summary>
        ///	 
        public static Automaton concatenate(IList<Automaton> l)
        {
            return BasicOperations.concatenate(l);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#optional(Automaton)"/>. </summary>
        ///	 
        public virtual Automaton optional()
        {
            return BasicOperations.optional(this);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#repeat(Automaton)"/>. </summary>
        ///	 
        public virtual Automaton repeat()
        {
            return BasicOperations.repeat(this);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#repeat(Automaton, int)"/>. </summary>
        ///	 
        public virtual Automaton repeat(int min)
        {
            return BasicOperations.repeat(this, min);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#repeat(Automaton, int, int)"/>. </summary>
        ///	 
        public virtual Automaton repeat(int min, int max)
        {
            return BasicOperations.repeat(this, min, max);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#complement(Automaton)"/>. </summary>
        ///	 
        public virtual Automaton complement()
        {
            return BasicOperations.complement(this);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#minus(Automaton, Automaton)"/>. </summary>
        ///	 
        public virtual Automaton minus(Automaton a)
        {
            return BasicOperations.minus(this, a);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#intersection(Automaton, Automaton)"/>. </summary>
        ///	 
        public virtual Automaton intersection(Automaton a)
        {
            return BasicOperations.intersection(this, a);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#subsetOf(Automaton, Automaton)"/>. </summary>
        ///	 
        public virtual bool subsetOf(Automaton a)
        {
            return BasicOperations.subsetOf(this, a);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#union(Automaton, Automaton)"/>. </summary>
        ///	 
        public virtual Automaton union(Automaton a)
        {
            return BasicOperations.union(this, a);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#union(Collection)"/>. </summary>
        ///	 
        public static Automaton union(ICollection<Automaton> l)
        {
            return BasicOperations.union(l);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#determinize(Automaton)"/>. </summary>
        ///	 
        public virtual void determinize()
        {
            BasicOperations.determinize(this);
        }

        ///	 
        ///	 <summary>  See <seealso cref="BasicOperations#addEpsilons(Automaton, Collection)"/>. </summary>
        ///	 
        public virtual void addEpsilons(ICollection<StatePair> pairs)
        {
            BasicOperations.addEpsilons(this, pairs);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#isEmptyString(Automaton)"/>. </summary>
        ///	 
        public virtual bool isEmptyString()
        {
            get
            {
                return BasicOperations.isEmptyString(this);
            }
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#isEmpty(Automaton)"/>. </summary>
        ///	 
        public virtual bool isEmpty()
        {
            get
            {
                return BasicOperations.isEmpty(this);
            }
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#isTotal(Automaton)"/>. </summary>
        ///	 
        public virtual bool isTotal()
        {
            get
            {
                return BasicOperations.isTotal(this);
            }
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#getShortestExample(Automaton, boolean)"/>. </summary>
        ///	 
        public virtual string getShortestExample(bool accepted)
        {
            return BasicOperations.getShortestExample(this, accepted);
        }

        ///	
        ///	 <summary>  See <seealso cref="BasicOperations#run(Automaton, String)"/>. </summary>
        ///	 
        public virtual bool run(string s)
        {
            return BasicOperations.run(this, s);
        }

        ///	
        ///	 <summary>  See <seealso cref="MinimizationOperations#minimize(Automaton)"/>. </summary>
        ///	 
        public virtual void minimize()
        {
            MinimizationOperations.minimize(this);
        }

        ///	
        ///	 <summary>  See <seealso cref="MinimizationOperations#minimize(Automaton)"/>.
        ///	  Returns the automaton being given as argument. </summary>
        ///	 
        public static Automaton minimize(Automaton a)
        {
            a.minimize();
            return a;
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#overlap(Automaton, Automaton)"/>. </summary>
        ///	 
        public virtual Automaton overlap(Automaton a)
        {
            return SpecialOperations.overlap(this, a);
        }

        ///	 
        ///	 <summary>  See <seealso cref="SpecialOperations#singleChars(Automaton)"/>. </summary>
        ///	 
        public virtual Automaton singleChars()
        {
            return SpecialOperations.singleChars(this);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#trim(Automaton, String, char)"/>. </summary>
        ///	 
        public virtual Automaton trim(string set, char c)
        {
            return SpecialOperations.Trim(this, set, c);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#compress(Automaton, String, char)"/>. </summary>
        ///	 
        public virtual Automaton compress(string set, char c)
        {
            return SpecialOperations.compress(this, set, c);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#subst(Automaton, Map)"/>. </summary>
        ///	 
        public virtual Automaton subst(IDictionary<char?, Set<char?>> map)
        {
            return SpecialOperations.subst(this, map);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#subst(Automaton, char, String)"/>. </summary>
        ///	 
        public virtual Automaton subst(char c, string s)
        {
            return SpecialOperations.subst(this, c, s);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#homomorph(Automaton, char[], char[])"/>. </summary>
        ///	 
        public virtual Automaton homomorph(char[] source, char[] dest)
        {
            return SpecialOperations.homomorph(this, source, dest);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#projectChars(Automaton, Set)"/>. </summary>
        ///	 
        public virtual Automaton projectChars(Set<char?> chars)
        {
            return SpecialOperations.projectChars(this, chars);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#isFinite(Automaton)"/>. </summary>
        ///	 
        public virtual bool isFinite()
        {
            get
            {
                return SpecialOperations.isFinite(this);
            }
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#getStrings(Automaton, int)"/>. </summary>
        ///	 
        public virtual Set<string> getStrings(int length)
        {
            return SpecialOperations.getStrings(this, length);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#getFiniteStrings(Automaton)"/>. </summary>
        ///	 
        public virtual Set<string> FiniteStrings
        {
            get
            {
                return SpecialOperations.getFiniteStrings(this);
            }
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#getFiniteStrings(Automaton, int)"/>. </summary>
        ///	 
        public virtual Set<string> getFiniteStrings(int limit)
        {
            return SpecialOperations.getFiniteStrings(this, limit);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#getCommonPrefix(Automaton)"/>. </summary>
        ///	 
        public virtual string CommonPrefix
        {
            get
            {
                return SpecialOperations.getCommonPrefix(this);
            }
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#prefixClose(Automaton)"/>. </summary>
        ///	 
        public virtual void prefixClose()
        {
            SpecialOperations.prefixClose(this);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#hexCases(Automaton)"/>. </summary>
        ///	 
        public static Automaton hexCases(Automaton a)
        {
            return SpecialOperations.hexCases(a);
        }

        ///	
        ///	 <summary>  See <seealso cref="SpecialOperations#replaceWhitespace(Automaton)"/>. </summary>
        ///	 
        public static Automaton replaceWhitespace(Automaton a)
        {
            return SpecialOperations.replaceWhitespace(a);
        }

        ///	
        ///	 <summary>  See <seealso cref="ShuffleOperations#shuffleSubsetOf(Collection, Automaton, Character, Character)"/>. </summary>
        ///	  
        public static string shuffleSubsetOf(ICollection<Automaton> ca, Automaton a, char? suspend_shuffle, char? resume_shuffle)
        {
            return ShuffleOperations.shuffleSubsetOf(ca, a, suspend_shuffle, resume_shuffle);
        }

        ///	 
        ///	 <summary>  See <seealso cref="ShuffleOperations#shuffle(Automaton, Automaton)"/>. </summary>
        ///	 
        public virtual Automaton shuffle(Automaton a)
        {
            return ShuffleOperations.shuffle(this, a);
        }
    }

}