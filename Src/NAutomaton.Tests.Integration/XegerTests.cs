using System.Text.RegularExpressions;
using Xunit;
using Xunit.Extensions;

namespace NAutomaton.Tests.Integration
{
    public sealed class XegerTests
    {
        [Theory]
        [InlineData("[ab]{4,6}c")]
        [InlineData("(a|b)*ab")]
        public void ShouldGenerateTextCorrectly(string regex)
        {
            var generator = new Xeger(regex);
            //for (int i = 0; i < 100; i++)
            //{
                string text = generator.Generate();
                Assert.True(Regex.IsMatch(text, regex));
            //}
        }
    }
}
