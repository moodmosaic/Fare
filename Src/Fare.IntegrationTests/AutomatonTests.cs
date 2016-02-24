using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace Fare.IntegrationTests
{
    public sealed class AutomatonTests
    {

        [Theory]
        [InlineData("ab", "ab", "ab")]
        [InlineData("ab.*", "a.*", "abc")]
        public void StringMatchesBothRegexAutomaton(string pattern1, string pattern2, string matchingString)
        {
            //
            // Given
            //
            Automaton automaton1 = new RegExp(pattern1).ToAutomaton();
            Automaton automaton2 = new RegExp(pattern2).ToAutomaton();

            //
            // When
            //
            Automaton intersection = automaton1.Intersection(automaton2);

            //
            // Then
            //
            Assert.True(intersection.Run(matchingString));
        }


        [Theory]
        [InlineData(".*", "ab", "ac")]
        [InlineData("cab.*", "a.*", "abc")]
        public void StringDoesntMatchBothRegexAutomaton(string pattern1, string pattern2, string matchingString)
        {
            //
            // Given
            //
            Automaton automaton1 = new RegExp(pattern1).ToAutomaton();
            Automaton automaton2 = new RegExp(pattern2).ToAutomaton();

            //
            // When
            //
            Automaton intersection = automaton1.Intersection(automaton2);

            //
            // Then
            //
            Assert.False(intersection.Run(matchingString));
        }
    }
}
