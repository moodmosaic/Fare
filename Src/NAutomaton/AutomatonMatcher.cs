/*
 * dk.brics.automaton - AutomatonMatcher
 *
 * Copyright (c) 2008-2011 John Gibson
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
    public class AutomatonMatcher : IMatchResult
    {
        private readonly RunAutomaton automaton;
        private readonly char[] chars;

        private int matchEnd = -1;
        private int matchStart = -1;

        public AutomatonMatcher(char[] chars, RunAutomaton automaton)
        {
            this.chars = chars;
            this.automaton = automaton;
        }

        private int MatchStart
        {
            get { return matchStart; }
        }

        private int MatchEnd
        {
            get { return matchEnd; }
        }

        private char[] Chars
        {
            get { return chars; }
        }

        public virtual int End()
        {
            MatchGood();
            return matchEnd;
        }

        public virtual int End(int group)
        {
            OnlyZero(group);
            return End();
        }

        public virtual string Group()
        {
            MatchGood();

            // Original code (Java): return chars.subSequence(matchStart, matchEnd).ToString();
            return chars.ToString().Substring(matchStart, matchEnd);
        }

        public virtual string Group(int group)
        {
            OnlyZero(group);
            return Group();
        }

        public virtual int GroupCount()
        {
            return 0;
        }

        public virtual int Start()
        {
            MatchGood();
            return matchStart;
        }

        public virtual int Start(int group)
        {
            OnlyZero(group);
            return Start();
        }

        public virtual bool Find()
        {
            int begin;
            switch (MatchStart)
            {
                case -2:
                    return false;
                case -1:
                    begin = 0;
                    break;
                default:
                    begin = MatchEnd;
                    // This occurs when a previous find() call matched the empty string. 
                    // This can happen when the pattern is a* for example.
                    if (begin == MatchStart)
                    {
                        begin += 1;
                        if (begin > Chars.Length)
                        {
                            SetMatch(-2, -2);
                            return false;
                        }
                    }
                    break;
            }

            int matchStart;
            int matchEnd;
            if (automaton.IsAccept(automaton.InitialState))
            {
                matchStart = begin;
                matchEnd = begin;
            }
            else
            {
                matchStart = -1;
                matchEnd = -1;
            }
            int l = Chars.Length;
            while (begin < l)
            {
                int p = automaton.InitialState;
                for (int i = begin; i < l; i++)
                {
                    int newState = automaton.Step(p, Chars[i]);
                    if (newState == -1)
                    {
                        break;
                    }
                    if (automaton.IsAccept(newState))
                    {
                        // Found a match from begin to (i + 1).
                        matchStart = begin;
                        matchEnd = (i + 1);
                    }
                    p = newState;
                }
                if (matchStart != -1)
                {
                    SetMatch(matchStart, matchEnd);
                    return true;
                }
                begin += 1;
            }
            if (matchStart != -1)
            {
                SetMatch(matchStart, matchEnd);
                return true;
            }

            SetMatch(-2, -2);
            return false;
        }

        public virtual IMatchResult ToMatchResult()
        {
            var match = new AutomatonMatcher(chars, automaton);
            match.matchStart = matchStart;
            match.matchEnd = matchEnd;
            return match;
        }

        private void SetMatch(int matchStart, int matchEnd)
        {
            if (matchStart > matchEnd)
            {
                throw new ArgumentException("Start must be less than or equal to end: " + matchStart + ", " + matchEnd);
            }

            this.matchStart = matchStart;
            this.matchEnd = matchEnd;
        }

        private static void OnlyZero(int group)
        {
            if (group != 0)
            {
                throw new IndexOutOfRangeException("The only group supported is 0.");
            }
        }

        private void MatchGood()
        {
            if ((matchStart < 0) || (matchEnd < 0))
            {
                throw new InvalidOperationException("There was no available match.");
            }
        }
    }
}