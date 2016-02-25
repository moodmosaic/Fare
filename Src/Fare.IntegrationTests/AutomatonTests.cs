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
            var automaton1 = new RegExp(pattern1).ToAutomaton();
            var automaton2 = new RegExp(pattern2).ToAutomaton();

            var intersection = automaton1.Intersection(automaton2);

            Assert.True(intersection.Run(matchingString));
        }


        [Theory]
        [InlineData(".*", "ab", "ac")]
        [InlineData("cab.*", "a.*", "abc")]
        public void StringDoesntMatchBothRegexAutomaton(string pattern1, string pattern2, string matchingString)
        {
            var automaton1 = new RegExp(pattern1).ToAutomaton();
            var automaton2 = new RegExp(pattern2).ToAutomaton();

            var intersection = automaton1.Intersection(automaton2);

            Assert.False(intersection.Run(matchingString));
        }
    }
}