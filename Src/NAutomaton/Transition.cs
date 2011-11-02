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
using System.Diagnostics;
using System.Text;

namespace NAutomaton
{
    /** 
     * <tt>Automaton</tt> transition. 
     * <p>
     * A transition, which belongs to a source state, consists of a Unicode character interval
     * and a destination state.
     * @author Anders M&oslash;ller &lt;<a href="mailto:amoeller@cs.au.dk">amoeller@cs.au.dk</a>&gt;
     */
    [Serializable]
    public class Transition
    {
        /* 
         * * CLASS INVARIANT: min<=max
         */

        private readonly char min;
        private readonly char max;
        private readonly State to;

        /** 
         * Constructs a new singleton interval transition. 
         * @param c transition character
         * @param to destination state
         */
        public Transition(char c, State to)
        {
            this.min = c;
            this.max = c;
            this.to = to;
        }

        public State To
        {
            get { return this.to; }
        }

        public char Min
        {
            get { return min; }
        }

        public char Max
        {
            get { return max; }
        }

        /** 
         * Constructs a new transition. 
         * Both end points are included in the interval.
         * @param min transition interval minimum
         * @param max transition interval maximum
         * @param to destination state
         */
        public Transition(char min, char max, State to)
        {
            if (max < min)
            {
                char t = max;
                max = min;
                min = t;
            }

            this.min = min;
            this.max = max;
            this.to = to;
        }

        /** Returns minimum of this transition interval. */
        public char GetMin()
        {
            return Min;
        }

        /** Returns maximum of this transition interval. */
        public char GetMax()
        {
            return Max;
        }

        /** Returns destination of this transition. */
        public State GetDest()
        {
            return to;
        }

        /** 
         * Checks for equality.
         * @param obj object to compare with
         * @return true if <tt>obj</tt> is a transition with same 
         *         character interval and destination state as this transition.
         */
        public override bool Equals(Object obj)
        {
            if (obj.GetType() == typeof(Transition))
            {
                var t = (Transition)obj;
                return t.Min == Min && t.Max == Max && t.to == to;
            }

            return false;
        }

        /** 
         * Returns hash code.
         * The hash code is based on the character interval (not the destination state).
         * @return hash code
         */
        public override int GetHashCode()
        {
            return Min * 2 + Max * 3;
        }

        private static void AppendCharString(char c, StringBuilder b)
        {
            if (c >= 0x21 && c <= 0x7e && c != '\\' && c != '"')
                b.Append(c);
            else
            {
                b.Append("\\u");

                Debugger.Break(); // Java: string s = Integer.toHexString(c);
                string s = string.Format("{0:X}", Convert.ToInt32(c));

                if (c < 0x10)
                    b.Append("000").Append(s);
                else if (c < 0x100)
                    b.Append("00").Append(s);
                else if (c < 0x1000)
                    b.Append("0").Append(s);
                else
                    b.Append(s);
            }
        }

        /** 
         * Returns a string describing this state. Normally invoked via 
         * {@link Automaton#toString()}. 
         */
        public override string ToString()
        {
            var b = new StringBuilder();
            AppendCharString(Min, b);
            if (Min != Max)
            {
                b.Append("-");
                AppendCharString(Max, b);
            }
            b.Append(" -> ").Append(to.Number);
            return b.ToString();
        }

        private void AppendDot(StringBuilder b)
        {
            b.Append(" -> ").Append(to.Number).Append(" [label=\"");
            AppendCharString(Min, b);
            if (Min != Max)
            {
                b.Append("-");
                AppendCharString(Max, b);
            }
            b.Append("\"]\n");
        }
    }
}
