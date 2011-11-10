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
using System.Text;

namespace NAutomaton
{
    public class RunAutomaton
    {
        private readonly bool[] accept;
        private readonly int initial;
        private readonly char[] points;
        private readonly int size;
        private readonly int[] transitions;
        private int[] classmap;

        public RunAutomaton(Automaton a)
            : this(a, true)
        {
        }

        public RunAutomaton(Automaton a, bool tableize)
        {
            a.Determinize();
            points = a.StartPoints;
            HashSet<State> states = a.States;
            Automaton.StateNumbers = states;
            initial = a.Initial.Number;
            size = states.Count;
            accept = new bool[size];
            transitions = new int[size*points.Length];
            for (int n = 0; n < size*points.Length; n++)
            {
                transitions[n] = -1;
            }
            foreach (State s in states)
            {
                int n = s.Number;
                accept[n] = s.Accept;
                for (int c = 0; c < points.Length; c++)
                {
                    State q = s.Step(points[c]);
                    if (q != null)
                    {
                        transitions[n*points.Length + c] = q.Number;
                    }
                }
            }
            if (tableize)
            {
                SetAlphabet();
            }
        }

        public virtual int Size
        {
            get { return size; }
        }

        public virtual int Initial
        {
            get { return initial; }
        }

        public virtual char[] CharIntervals
        {
            get { return (char[]) points.Clone(); }
        }

        private void SetAlphabet()
        {
            classmap = new int[Char.MaxValue - Char.MinValue + 1];
            int i = 0;
            for (int j = 0; j <= Char.MaxValue - Char.MinValue; j++)
            {
                if (i + 1 < points.Length && j == points[i + 1])
                {
                    i++;
                }
                classmap[j] = i;
            }
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append("initial state: ").Append(initial).Append("\n");
            for (int i = 0; i < size; i++)
            {
                b.Append("state " + i);
                b.Append(accept[i] ? " [accept]:\n" : " [reject]:\n");
                for (int j = 0; j < points.Length; j++)
                {
                    int k = transitions[i*points.Length + j];
                    if (k != -1)
                    {
                        char min = points[j];
                        char max;
                        if (j + 1 < points.Length)
                        {
                            max = (char) (points[j + 1] - 1);
                        }
                        else
                        {
                            max = Char.MaxValue;
                        }
                        b.Append(" ");
                        Transition.AppendCharString(min, b);
                        if (min != max)
                        {
                            b.Append("-");
                            Transition.AppendCharString(max, b);
                        }
                        b.Append(" -> ").Append(k).Append("\n");
                    }
                }
            }
            return b.ToString();
        }

        public virtual bool IsAccept(int state)
        {
            return accept[state];
        }

        private int GetCharClass(char c)
        {
            return SpecialOperations.FindIndex(c, points);
        }

        public static RunAutomaton Load(Uri url)
        {
            throw new NotImplementedException();
        }

        public static RunAutomaton Load(Stream stream)
        {
            throw new NotImplementedException();
        }

        public virtual void Store(Stream stream)
        {
            throw new NotImplementedException();
        }

        public virtual int Step(int state, char c)
        {
            if (classmap == null)
            {
                return transitions[state*points.Length + GetCharClass(c)];
            }
            return transitions[state*points.Length + classmap[c - Char.MinValue]];
        }

        public virtual bool Run(string s)
        {
            int p = initial;
            int l = s.Length;
            for (int i = 0; i < l; i++)
            {
                p = Step(p, s[i]);
                if (p == -1)
                {
                    return false;
                }
            }
            return accept[p];
        }

        public virtual int Run(string s, int offset)
        {
            int p = initial;
            int l = s.Length;
            int max = -1;
            for (int r = 0; offset <= l; offset++, r++)
            {
                if (accept[p])
                {
                    max = r;
                }
                if (offset == l)
                {
                    break;
                }
                p = Step(p, s[offset]);
                if (p == -1)
                {
                    break;
                }
            }
            return max;
        }

        public virtual AutomatonMatcher NewMatcher(char[] s)
        {
            return new AutomatonMatcher(s, this);
        }

        public virtual AutomatonMatcher NewMatcher(char[] s, int startOffset, int endOffset)
        {
            // Original code (Java): return new AutomatonMatcher(s.subSequence(startOffset, endOffset), this);
            throw new NotImplementedException();
        }
    }
}