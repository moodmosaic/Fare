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

namespace Fare
{
    public static class BasicAutomata
    {
        /// <summary>
        /// Returns a new (deterministic) automaton that accepts any single character.
        /// </summary>
        /// <returns>A new (deterministic) automaton that accepts any single character.</returns>
        public static Automaton MakeAnyChar()
        {
            return BasicAutomata.MakeCharRange(char.MinValue, char.MaxValue);
        }

        /// <summary>
        /// Returns a new (deterministic) automaton that accepts all strings.
        /// </summary>
        /// <returns>
        /// A new (deterministic) automaton that accepts all strings.
        /// </returns>
        public static Automaton MakeAnyString()
        {
            var state = new State();
            state.Accept = true;
            state.Transitions.Add(new Transition(char.MinValue, char.MaxValue, state));

            var a = new Automaton();
            a.Initial = state;
            a.IsDeterministic = true;
            return a;
        }

        /// <summary>
        /// Returns a new (deterministic) automaton that accepts a single character of the given value.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>A new (deterministic) automaton that accepts a single character of the given value.</returns>
        public static Automaton MakeChar(char c)
        {
            var a = new Automaton();
            a.Singleton = c.ToString();
            a.IsDeterministic = true;
            return a;
        }

        /// <summary>
        /// Returns a new (deterministic) automaton that accepts a single char whose value is in the
        /// given interval (including both end points).
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>
        /// A new (deterministic) automaton that accepts a single char whose value is in the
        /// given interval (including both end points).
        /// </returns>
        public static Automaton MakeCharRange(char min, char max)
        {
            if (min == max)
            {
                return BasicAutomata.MakeChar(min);
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

            a.IsDeterministic = true;
            return a;
        }

        /// <summary>
        /// Returns a new (deterministic) automaton with the empty language.
        /// </summary>
        /// <returns>
        /// A new (deterministic) automaton with the empty language.
        /// </returns>
        public static Automaton MakeEmpty()
        {
            var a = new Automaton();
            var s = new State();
            a.Initial = s;
            a.IsDeterministic = true;
            return a;
        }

        /// <summary>
        /// Returns a new (deterministic) automaton that accepts only the empty string.
        /// </summary>
        /// <returns>
        /// A new (deterministic) automaton that accepts only the empty string.
        /// </returns>
        public static Automaton MakeEmptyString()
        {
            var a = new Automaton();
            a.Singleton = string.Empty;
            a.IsDeterministic = true;
            return a;
        }

        /// <summary>
        /// Returns a new automaton that accepts strings representing decimal non-negative integers in
        /// the given interval.
        /// </summary>
        /// <param name="min">The minimum value of interval.</param>
        /// <param name="max">The maximum value of inverval (both end points are included in the 
        /// interval).</param>
        /// <param name="digits">If f >0, use fixed number of digits (strings must be prefixed by 0's 
        /// to obtain the right length) otherwise, the number of digits is not fixed.</param>
        /// <returns>A new automaton that accepts strings representing decimal non-negative integers 
        /// in the given interval.</returns>
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
            a.Initial = BasicAutomata.Between(x, y, 0, initials, digits <= 0);
            if (digits <= 0)
            {
                List<StatePair> pairs = (from p in initials
                                         where a.Initial != p
                                         select new StatePair(a.Initial, p)).ToList();
                a.AddEpsilons(pairs);
                a.Initial.AddTransition(new Transition('0', a.Initial));
                a.IsDeterministic = false;
            }
            else
            {
                a.IsDeterministic = true;
            }

            a.CheckMinimizeAlways();
            return a;
        }

        /// <summary>
        /// Returns a new (deterministic) automaton that accepts the single given string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>A new (deterministic) automaton that accepts the single given string.</returns>
        public static Automaton MakeString(string s)
        {
            var a = new Automaton();
            a.Singleton = s;
            a.IsDeterministic = true;
            return a;
        }

        /// <summary>
        /// Constructs sub-automaton corresponding to decimal numbers of length x.Substring(n).Length.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="n">The n.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Constructs sub-automaton corresponding to decimal numbers of value at least x.Substring(n)
        /// and length x.Substring(n).Length.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="n">The n.</param>
        /// <param name="initials">The initials.</param>
        /// <param name="zeros">if set to <c>true</c> [zeros].</param>
        /// <returns></returns>
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
                    s.AddTransition(new Transition((char)(c + 1), '9', AnyOfRightLength(x, n + 1)));
                }
            }

            return s;
        }

        /// <summary>
        /// Constructs sub-automaton corresponding to decimal numbers of value at most x.Substring(n)
        /// and length x.Substring(n).Length.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="n">The n.</param>
        /// <returns></returns>
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
                s.AddTransition(new Transition(c, AtMost(x, (char)n + 1)));
                if (c > '0')
                {
                    s.AddTransition(new Transition('0', (char)(c - 1), AnyOfRightLength(x, n + 1)));
                }
            }

            return s;
        }

        /// <summary>
        /// Constructs sub-automaton corresponding to decimal numbers of value between x.Substring(n)
        /// and y.Substring(n) and of length x.Substring(n).Length (which must be equal to 
        /// y.Substring(n).Length).
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="n">The n.</param>
        /// <param name="initials">The initials.</param>
        /// <param name="zeros">if set to <c>true</c> [zeros].</param>
        /// <returns></returns>
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
                else
                {
                    // cx < cy
                    s.AddTransition(new Transition(cx, BasicAutomata.AtLeast(x, n + 1, initials, zeros && cx == '0')));
                    s.AddTransition(new Transition(cy, BasicAutomata.AtMost(y, n + 1)));
                    if (cx + 1 < cy)
                    {
                        s.AddTransition(new Transition((char)(cx + 1), (char)(cy - 1), BasicAutomata.AnyOfRightLength(x, n + 1)));
                    }
                }
            }

            return s;
        }

        /// <summary>
        /// Returns a new (deterministic) automaton that accepts a single character in the given set.
        /// </summary>
        /// <param name="set">The set.</param>
        /// <returns></returns>
        public static Automaton MakeCharSet(String set)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a new (deterministic and minimal) automaton that accepts the union of the given
        /// set of strings. The input character sequences are internally sorted in-place, so the 
        /// input array is modified. @see StringUnionOperations.
        /// </summary>
        /// <param name="strings">The strings.</param>
        /// <returns></returns>
        public static Automaton MakeStringUnion(params char[] strings) // TODO: CharSequence... strings
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs automaton that accept strings representing nonnegative integer that are not 
        /// larger than the given value.
        /// </summary>
        /// <param name="n">The n string representation of maximum value.</param>
        /// <returns></returns>
        public static Automaton MakeMaxInteger(String n)
        {
            throw new NotImplementedException();
        }

        private static void MaxInteger(String n, int i, StringBuilder b) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs automaton that accept strings representing nonnegative integers that are not
        /// less that the given value.
        /// </summary>
        /// <param name="n">The n string representation of minimum value.</param>
        /// <returns></returns>
        public static Automaton MakeMinInteger(String n) 
        {
            throw new NotImplementedException();
        }

        private static void MinInteger(String n, int i, StringBuilder b) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs automaton that accept strings representing decimal numbers that can be 
        /// written with at most the given number of digits. Surrounding whitespace is permitted.
        /// </summary>
        /// <param name="i">The i max number of necessary digits.</param>
        /// <returns></returns>
        public static Automaton MakeTotalDigits(int i) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs automaton that accept strings representing decimal numbers that can be 
        /// written with at most the given number of digits in the fraction part. Surrounding
        /// whitespace is permitted.
        /// </summary>
        /// <param name="i">The i max number of necessary fraction digits.</param>
        /// <returns></returns>
        public static Automaton MakeFractionDigits(int i) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs automaton that accept strings representing the given integer. Surrounding 
        /// whitespace is permitted.
        /// </summary>
        /// <param name="value">The value string representation of integer.</param>
        /// <returns></returns>
        public static Automaton MakeIntegerValue(String value) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs automaton that accept strings representing the given decimal number.
        /// Surrounding whitespace is permitted.
        /// </summary>
        /// <param name="value">The value string representation of decimal number.</param>
        /// <returns></returns>
        public static Automaton MakeDecimalValue(String value) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs deterministic automaton that matches strings that contain the given substring.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static Automaton MakeStringMatcher(String s) 
        {
            throw new NotImplementedException();
        }

    }
}
