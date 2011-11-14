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
    public static class MinimizationOperations
    {
        /// <summary>
        /// Minimizes (and determinizes if not already deterministic) the given automaton.
        /// </summary>
        /// <param name="a">The automaton.</param>
        public static void Minimize(Automaton a)
        {
            if (!a.IsSingleton)
            {
                switch (Automaton.Minimization)
                {
                    case Automaton.MinimizeHuffman:
                        MinimizationOperations.MinimizeHuffman(a);
                        break;
                    case Automaton.MinimizeBrzozowski:
                        MinimizationOperations.MinimizeBrzozowski(a);
                        break;
                    default:
                        MinimizationOperations.MinimizeHopcroft(a);
                        break;
                }
            }

            a.RecomputeHashCode();
        }

        public static void MinimizeHopcroft(Automaton a)
        {
            throw new System.NotImplementedException();
        }

        public static void MinimizeBrzozowski(Automaton a)
        {
            throw new System.NotImplementedException();
        }

        public static void MinimizeHuffman(Automaton a)
        {
            throw new System.NotImplementedException();
        }
    }
}