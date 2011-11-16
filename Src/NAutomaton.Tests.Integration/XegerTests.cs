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
        [InlineData("[A-Za-z0-9]")]
        [InlineData("[A-Za-z0-9_]")]
        [InlineData("[A-Za-z]")]
        [InlineData("[ \t]")]
        [InlineData(@"[(?<=\W)(?=\w)|(?<=\w)(?=\W)]")]
        [InlineData("[\x00-\x1F\x7F]")]
        [InlineData("[0-9]")]
        //[InlineData("[^0-9]")]
        [InlineData("[\x21-\x7E]")]
        public void ShouldGenerateTextCorrectly(string regex)
        {
            var generator = new Xeger(regex);
            for (int i = 0; i < 100; i++)
            {
                string text = generator.Generate();
                Assert.True(Regex.IsMatch(text, regex));
            }
        }
    }
}