/*
 * Copyright 2009 Wilfred Springer
 * http://github.com/moodmosaic/Fare/
 * Original Java code:
 * http://code.google.com/p/xeger/
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Fare
{
    /// <summary>
    /// An object that will generate text from a regular expression. In a way,
    /// it's the opposite of a regular expression matcher: an instance of this class
    /// will produce text that is guaranteed to match the regular expression passed in.
    /// </summary>
    public class Xeger
    {
        private const RegExpSyntaxOptions AllExceptAnyString = RegExpSyntaxOptions.All & ~RegExpSyntaxOptions.Anystring;

        // Base of the exponential bias applied when picking a transition.
        // A transition leading to a state at distance d from an accept state is weighted
        // by BiasBase^(-d), so transitions that make progress toward acceptance are
        // favoured over back-edges. BiasBase == 1 reproduces the original uniform walk;
        // BiasBase -> infinity collapses to shortest-path generation. 2.0 is a good
        // middle ground: walks stay random but cannot wander for thousands of steps.
        private const double BiasBase = 2.0;

        private readonly Automaton automation;
        private readonly Random random;
        private readonly Dictionary<State, int> distanceToAccept;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xeger"/> class.
        /// </summary>
        /// <param name="regex">The regex.</param>
        /// <param name="random">The random.</param>
        public Xeger(string regex, Random random)
        {
            if (string.IsNullOrEmpty(regex))
            {
                throw new ArgumentNullException("regex");
            }

            if (random == null)
            {
                throw new ArgumentNullException("random");
            }


            regex = RemoveStartEndMarkers(regex);
            this.automation = new RegExp(regex, AllExceptAnyString).ToAutomaton();
            this.random = random;
            this.distanceToAccept = ComputeDistanceToAccept(this.automation);
        }

        /// <summary>
        /// Computes the shortest distance (in number of transitions) from every reachable
        /// state to the nearest accept state, using a reverse BFS from all accept states.
        /// States that cannot reach any accept state are absent from the returned map.
        /// </summary>
        private static Dictionary<State, int> ComputeDistanceToAccept(Automaton automaton)
        {
            var states = automaton.GetStates();

            // Build reverse adjacency: for every transition s -> t, record s as a predecessor of t.
            var predecessors = new Dictionary<State, List<State>>(states.Count);
            foreach (var s in states)
            {
                predecessors[s] = new List<State>();
            }
            foreach (var s in states)
            {
                foreach (var t in s.Transitions)
                {
                    if (!predecessors.TryGetValue(t.To, out var preds))
                    {
                        preds = new List<State>();
                        predecessors[t.To] = preds;
                    }
                    preds.Add(s);
                }
            }

            var distance = new Dictionary<State, int>(states.Count);
            var queue = new Queue<State>();
            foreach (var s in states)
            {
                if (s.Accept)
                {
                    distance[s] = 0;
                    queue.Enqueue(s);
                }
            }

            while (queue.Count > 0)
            {
                var s = queue.Dequeue();
                var d = distance[s];
                foreach (var pred in predecessors[s])
                {
                    if (!distance.ContainsKey(pred))
                    {
                        distance[pred] = d + 1;
                        queue.Enqueue(pred);
                    }
                }
            }

            return distance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xeger"/> class.<br/>
        /// Note that if multiple instances are created within short time using this overload,<br/>
        /// the instances might generate identical random strings.<br/>
        /// To avoid this, use the constructor overload that accepts an argument of type Random.
        /// </summary>
        /// <param name="regex">The regex.</param>
        public Xeger(string regex)
            : this(regex, new Random())
        {
        }

        /// <summary>
        /// Generates a random String that is guaranteed to match the regular expression passed to the constructor.
        /// </summary>
        /// <returns></returns>
        public string Generate()
        {
            var builder = new StringBuilder();
            this.Generate(builder, this.automation.Initial);
            return builder.ToString();
        }

        /// <summary>
        /// Generates a random number within the given bounds.
        /// </summary>
        /// <param name="min">The minimum number (inclusive).</param>
        /// <param name="max">The maximum number (inclusive).</param>
        /// <param name="random">The object used as the randomizer.</param>
        /// <returns>A random number in the given range.</returns>
        private static int GetRandomInt(int min, int max, Random random)
        {
            int maxForRandom = max - min + 1;
            return random.Next(maxForRandom) + min;
        }

        private void Generate(StringBuilder builder, State initialState)
        {
            var state = initialState;

            while (true)
            {
                var transitions = state.GetSortedTransitions(true);
                if (transitions.Count == 0)
                {
                    if (!state.Accept)
                    {
                        throw new InvalidOperationException("The regex is not solvable");
                    }

                    return;
                }

                // Weight each outgoing transition by BiasBase^(-distanceToAccept(target)).
                // Targets closer to an accept state get exponentially more probability mass,
                // so the random walk is naturally pulled toward acceptance instead of
                // wandering through back-edges. If the current state is itself an accept
                // state, "stop" is included as an additional option with weight 1 (i.e.
                // the same weight a self-loop on a distance-0 state would receive).
                var weights = new double[transitions.Count];
                double totalWeight = 0.0;
                for (int i = 0; i < transitions.Count; i++)
                {
                    var w = this.distanceToAccept.TryGetValue(transitions[i].To, out int d)
                        ? Math.Pow(BiasBase, -d) :
                        0.0; // Target cannot reach any accept state (a "dead" state). Avoid it.

                    weights[i] = w;
                    totalWeight += w;
                }

                double stopWeight = state.Accept ? 1.0 : 0.0;
                totalWeight += stopWeight;

                if (totalWeight == 0.0)
                {
                    // We're at a non-accept state with no usable outgoing transitions:
                    // the automaton is malformed for our purposes.
                    throw new InvalidOperationException("The regex is not solvable");
                }

                double r = this.random.NextDouble() * totalWeight;
                if (state.Accept)
                {
                    if (r < stopWeight)
                    {
                        // 0 is considered stop.
                        return;
                    }

                    r -= stopWeight;
                }

                int chosen = transitions.Count - 1;
                for (int i = 0; i < transitions.Count; i++)
                {
                    r -= weights[i];
                    if (r <= 0.0)
                    {
                        chosen = i;
                        break;
                    }
                }

                // Moving on to next transition.
                Transition transition = transitions[chosen];
                this.AppendChoice(builder, transition);
                state = transition.To;
            }
        }

        private void AppendChoice(StringBuilder builder, Transition transition)
        {
            var c = (char)Xeger.GetRandomInt(transition.Min, transition.Max, random);
            builder.Append(c);
        }

        private string RemoveStartEndMarkers(string regExp)
        {
            if (regExp.StartsWith("^"))
            {
                regExp = regExp.Substring(1);
            }

            if (regExp.EndsWith("$"))
            {
                regExp = regExp.Substring(0, regExp.Length - 1);
            }

            return regExp;
        }
    }
}
