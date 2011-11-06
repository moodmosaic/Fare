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
    public static class BasicAutomata
    {
        public static Automaton MakeEmpty()
        {
            return new Automaton
                       {
                           Initial = new State(),
                           Deterministic = true
                       };
        }

        public static Automaton MakeEmptyString()
        {
            return new Automaton
                       {
                           Singleton = "",
                           Deterministic = true
                       };
        }

        public static Automaton MakeAnyString()
        {
            var state = new State();
            state.Accept = true;
            state.Transitions.Add(new Transition(Char.MinValue, Char.MaxValue, state));

            return new Automaton
                       {
                           Initial = state,
                           Deterministic = true
                       };
        }

        public static Automaton MakeAnyChar()
        {
            return MakeCharRange(Char.MinValue, Char.MaxValue);
        }

        public static Automaton MakeChar(char c)
        {
            return new Automaton
                       {
                           Singleton = char.ToString(c),
                           Deterministic = true
                       };
        }

        public static Automaton MakeCharRange(char min, char max)
        {
            if (min == max)
            {
                return MakeChar(min);
            }

            var a = new Automaton();
            var s1 = new State();
            var s2 = new State();

            a.Initial = s1;
            s2.Accept = true;

            if (min <= max)
            {
                s1.Transitions.Add(new Transition(min, max, s2));
            }

            a.Deterministic = true;

            return a;
        }

        public static Automaton MakeCharSet(string set)
        {
            if (set.Length == 1)
            {
                return MakeChar(set[0]);
            }

            var a = new Automaton();
            var s1 = new State();
            var s2 = new State();

            a.Initial = s1;
            s2.Accept = true;

            foreach (char t in set)
            {
                s1.Transitions.Add(new Transition(t, s2));
            }

            a.Deterministic = true;
            a.Reduce();

            return a;
        }

        private static State AnyOfRightLength(string x, int n)
        {
            var s = new State();

            if (x.Length == n)
            {
                s.Accept = true;
            }
            else
            {
                s.AddTransition(new Transition('0', '9', AnyOfRightLength(x, n + 1)));
            }

            return s;
        }

        private static State AtLeast(string x, int n, ICollection<State> initials, bool zeros)
        {
            var s = new State();
            if (x.Length == n)
            {
                s.Accept = true;
            }
            else
            {
                if (zeros)
                {
                    initials.Add(s);
                }
                char c = x[n];
                s.AddTransition(new Transition(c, AtLeast(x, n + 1, initials, zeros && c == '0')));
                if (c < '9')
                {
                    s.AddTransition(new Transition((char) (c + 1), '9', AnyOfRightLength(x, n + 1)));
                }
            }
            return s;
        }

        private static State AtMost(string x, int n)
        {
            var s = new State();

            if (x.Length == n)
            {
                s.Accept = true;
            }
            else
            {
                char c = x[n];
                s.AddTransition(new Transition(c, AtMost(x, (char) n + 1)));
                if (c > '0')
                {
                    s.AddTransition(new Transition('0', (char) (c - 1), AnyOfRightLength(x, n + 1)));
                }
            }

            return s;
        }

        private static State Between(string x, string y, int n, ICollection<State> initials, bool zeros)
        {
            var s = new State();

            if (x.Length == n)
            {
                s.Accept = true;
            }
            else
            {
                if (zeros)
                {
                    initials.Add(s);
                }
                char cx = x[n];
                char cy = y[n];
                if (cx == cy)
                {
                    s.AddTransition(new Transition(cx, Between(x, y, n + 1, initials, zeros && cx == '0')));
                }
                else // cx < cy
                {
                    s.AddTransition(new Transition(cx, AtLeast(x, n + 1, initials, zeros && cx == '0')));
                    s.AddTransition(new Transition(cy, AtMost(y, n + 1)));
                    if (cx + 1 < cy)
                    {
                        s.AddTransition(new Transition((char) (cx + 1), (char) (cy - 1), AnyOfRightLength(x, n + 1)));
                    }
                }
            }
            return s;
        }

        public static Automaton MakeInterval(int min, int max, int digits)
        {
            var a = new Automaton();
            string x = Convert.ToString(min);
            string y = Convert.ToString(max);
            if (min > max || (digits > 0 && y.Length > digits))
            {
                throw new ArgumentException();
            }
            int d = digits > 0 ? digits : y.Length;
            var bx = new StringBuilder();
            for (int i = x.Length; i < d; i++)
            {
                bx.Append('0');
            }
            bx.Append(x);
            x = bx.ToString();
            var by = new StringBuilder();
            for (int i = y.Length; i < d; i++)
            {
                by.Append('0');
            }
            by.Append(y);
            y = by.ToString();
            ICollection<State> initials = new List<State>();
            a.Initial = Between(x, y, 0, initials, digits <= 0);
            if (digits <= 0)
            {
                List<StatePair> pairs =
                    (from p in initials where a.Initial != p select new StatePair(a.Initial, p)).ToList();
                a.AddEpsilons(pairs);
                a.Initial.AddTransition(new Transition('0', a.Initial));
                a.Deterministic = false;
            }
            else
            {
                a.Deterministic = true;
            }
            a.CheckMinimizeAlways();
            return a;
        }

        public static Automaton MakeString(string s)
        {
            return new Automaton
                       {
                           Singleton = s,
                           Deterministic = true
                       };
        }

        public static Automaton MakeStringUnion(params char[][] strings)
        {
            if (strings.Length == 0)
            {
                return MakeEmpty();
            }
            Array.Sort(strings, StringUnionOperations.LexicographicOrderComparer);
            var a = new Automaton();
            a.Initial = StringUnionOperations.Build(strings);
            a.Deterministic = true;
            a.Reduce();
            a.RecomputeHashCode();
            return a;
        }

        public static Automaton MakeMaxInteger(string n)
        {
            int i = 0;
            while (i < n.Length && n[i] == '0')
            {
                i++;
            }
            var b = new StringBuilder();
            b.Append("0*(0|");
            if (i < n.Length)
            {
                b.Append("[0-9]{1," + (n.Length - i - 1) + "}|");
            }
            MaxInteger(n.Substring(i), 0, b);
            b.Append(")");
            return Automaton.Minimize((new RegExp(b.ToString())).ToAutomaton());
        }

        private static void MaxInteger(string n, int i, StringBuilder b)
        {
            b.Append('(');
            if (i < n.Length)
            {
                char c = n[i];
                if (c != '0')
                {
                    b.Append("[0-" + (char) (c - 1) + "][0-9]{" + (n.Length - i - 1) + "}|");
                }
                b.Append(c);
                MaxInteger(n, i + 1, b);
            }
            b.Append(')');
        }

        public static Automaton MakeMinInteger(string n)
        {
            int i = 0;
            while (i + 1 < n.Length && n[i] == '0')
            {
                i++;
            }
            var b = new StringBuilder();
            b.Append("0*");
            MinInteger(n.Substring(i), 0, b);
            b.Append("[0-9]*");
            return Automaton.Minimize((new RegExp(b.ToString())).ToAutomaton());
        }

        private static void MinInteger(string n, int i, StringBuilder b)
        {
            b.Append('(');
            if (i < n.Length)
            {
                char c = n[i];
                if (c != '9')
                {
                    b.Append("[" + (char) (c + 1) + "-9][0-9]{" + (n.Length - i - 1) + "}|");
                }
                b.Append(c);
                MinInteger(n, i + 1, b);
            }
            b.Append(')');
        }

        public static Automaton MakeTotalDigits(int i)
        {
            return
                Automaton.Minimize(
                    (new RegExp("[ \t\n\r]*[-+]?0*([0-9]{0," + i + "}|((([0-9]\\.*){0," + i + "})&@\\.@)0*)[ \t\n\r]*"))
                        .ToAutomaton());
        }

        public static Automaton MakeFractionDigits(int i)
        {
            return
                Automaton.Minimize(
                    (new RegExp("[ \t\n\r]*[-+]?[0-9]+(\\.[0-9]{0," + i + "}0*)?[ \t\n\r]*")).ToAutomaton());
        }

        public static Automaton MakeIntegerValue(string value)
        {
            bool minus = false;
            int i = 0;
            while (i < value.Length)
            {
                char c = value[i];
                if (c == '-')
                {
                    minus = true;
                }
                if (c >= '1' && c <= '9')
                {
                    break;
                }
                i++;
            }
            var b = new StringBuilder();
            b.Append(value.Substring(i));
            if (b.Length == 0)
            {
                b.Append("0");
            }
            Automaton s = minus ? Automaton.MakeChar('-') : Automaton.MakeChar('+').Optional();
            Automaton ws = Datatypes.WhitespaceAutomaton;
            return Automaton.Minimize(
                ws.Concatenate(
                    s.Concatenate(Automaton.MakeChar('0').Repeat())
                        .Concatenate(Automaton.MakeString(b.ToString())))
                    .Concatenate(ws));
        }

        public static Automaton MakeDecimalValue(string value)
        {
            bool minus = false;
            int i = 0;
            while (i < value.Length)
            {
                char c = value[i];
                if (c == '-')
                {
                    minus = true;
                }
                if ((c >= '1' && c <= '9') || c == '.')
                {
                    break;
                }
                i++;
            }
            var b1 = new StringBuilder();
            var b2 = new StringBuilder();
            int p = value.IndexOf('.', i);
            if (p == -1)
            {
                b1.Append(value.Substring(i));
            }
            else
            {
                b1.Append(value.Substring(i, p - i));
                i = value.Length - 1;
                while (i > p)
                {
                    char c = value[i];
                    if (c >= '1' && c <= '9')
                    {
                        break;
                    }
                    i--;
                }
                b2.Append(value.Substring(p + 1, i + 1 - (p + 1)));
            }
            if (b1.Length == 0)
            {
                b1.Append("0");
            }
            Automaton s = minus ? Automaton.MakeChar('-') : Automaton.MakeChar('+').Optional();
            Automaton d;
            if (b2.Length == 0)
            {
                d = Automaton.MakeChar('.').Concatenate(Automaton.MakeChar('0').Repeat(1)).Optional();
            }
            else
            {
                d = Automaton.MakeChar('.')
                    .Concatenate(Automaton.MakeString(b2.ToString()))
                    .Concatenate(Automaton.MakeChar('0')
                                     .Repeat());
            }
            Automaton ws = Datatypes.WhitespaceAutomaton;
            return Automaton.Minimize(
                ws.Concatenate(
                    s.Concatenate(Automaton.MakeChar('0').Repeat())
                        .Concatenate(Automaton.MakeString(b1.ToString()))
                        .Concatenate(d))
                    .Concatenate(ws));
        }

        public static Automaton MakeStringMatcher(string s)
        {
            var a = new Automaton();
            var states = new State[s.Length + 1];
            states[0] = a.Initial;
            for (int i = 0; i < s.Length; i++)
            {
                states[i + 1] = new State();
            }
            State f = states[s.Length];
            f.Accept = true;
            f.Transitions.Add(new Transition(Char.MinValue, Char.MaxValue, f));
            for (int i = 0; i < s.Length; i++)
            {
                var done = new HashSet<char?>();
                char c = s[i];
                states[i].Transitions.Add(new Transition(c, states[i + 1]));
                done.Add(c);
                for (int j = i; j >= 1; j--)
                {
                    char d = s[j - 1];
                    if (!done.Contains(d) && s.Substring(0, j - 1).Equals(s.Substring(i - j + 1, i - (i - j + 1))))
                    {
                        states[i].Transitions.Add(new Transition(d, states[j]));
                        done.Add(d);
                    }
                }
                var da = new char[done.Count];
                int h = 0;
                foreach (char w in done)
                {
                    da[h++] = w;
                }
                Array.Sort(da);
                int from = Char.MinValue;
                int k = 0;
                while (from <= Char.MaxValue)
                {
                    while (k < da.Length && da[k] == from)
                    {
                        k++;
                        from++;
                    }
                    if (from <= Char.MaxValue)
                    {
                        int to = Char.MaxValue;
                        if (k < da.Length)
                        {
                            to = da[k] - 1;
                            k++;
                        }
                        states[i].Transitions.Add(new Transition((char) from, (char) to, states[0]));
                        from = to + 2;
                    }
                }
            }
            a.Deterministic = true;
            return a;
        }
    }
}