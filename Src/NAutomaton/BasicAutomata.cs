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

namespace NAutomaton
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

        public static Automaton MakeAnyString()
        {
            throw new NotImplementedException();
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

        public static Automaton MakeEmpty()
        {
            throw new NotImplementedException();
        }

        public static Automaton MakeEmptyString()
        {
            throw new NotImplementedException();
        }

        public static Automaton MakeInterval(int min, int max, int digits)
        {
            throw new NotImplementedException();
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
    }
}
