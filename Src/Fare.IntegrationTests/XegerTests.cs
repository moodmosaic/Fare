using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Fare.IntegrationTests
{
    public sealed class XegerTests
    {
        [Theory, MemberData(nameof(RegexPatternTestCases))]
        public void GeneratedTextIsCorrect(string pattern)
        {
            const int repeatCount = 3;
            var sut = new Fare.Xeger(pattern);

            var result = Enumerable.Repeat(0, repeatCount).Select(_ => sut.Generate()).ToArray();

            Assert.All(result, regex => Assert.Matches(pattern, regex));
        }

        [Theory, MemberData(nameof(RegexPatternTestCases))]
        public void GeneratedTextIsCorrectWithRexEngine(string pattern)
        {
            const int repeatCount = 3;
            var settings = new Rex.RexSettings(pattern) { k = 1 };
            settings.seed = 102;

            var result = Enumerable.Repeat(0, repeatCount).Select(_ => Rex.RexEngine.GenerateMembers(settings).Single()).ToArray();

            Assert.All(result, regex => Assert.Matches(pattern, regex));
        }

        public static TheoryData<string> RegexPatternTestCases => new TheoryData<string>
        {
            "[ab]{4,6}",
            "[ab]{4,6}c",
            "(a|b)*ab",
            "[A-Za-z0-9]",
            "[A-Za-z0-9_]",
            "[A-Za-z]",
            "[ \t]",
            @"[(?<=\W)(?=\w)|(?<=\w)(?=\W)]",
            "[\x00-\x1F\x7F]",
            "[0-9]",
            "[^0-9]",
            "[\x21-\x7E]",
            "[a-z]",
            "[\x20-\x7E]",
            "[ \t\r\n\v\f]",
            "[^ \t\r\n\v\f]",
            "[A-Z]",
            "[A-Fa-f0-9]",
            "in[du]",
            "x[0-9A-Z]",
            "[^A-M]in",
            ".gr",
            @"\(.*l",
            "W*in",
            "[xX][0-9a-z]",
            @"\(\(\(ab\)*c\)*d\)\(ef\)*\(gh\)\{2\}\(ij\)*\(kl\)*\(mn\)*\(op\)*\(qr\)*",
            @"((mailto\:|(news|(ht|f)tp(s?))\://){1}\S+)",
            @"^http\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(/\S*)?$",
            @"^([1-zA-Z0-1@.\s]{1,255})$",
            "[A-Z][0-9A-Z]{10}",
            "[A-Z][A-Za-z0-9]{10}",
            "[A-Za-z0-9]{11}",
            "[A-Za-z]{11}",
            @"^[a-zA-Z''-'\s]{1,40}$",
            @"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$",
            @"a[a-z]",
            @"[1-9][0-9]",
            @"\d{8}",
            @"\d{5}(-\d{4})?",
            @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}",
            @"^(?:[a-z0-9])+$",
            @"^(?i:[a-z0-9])+$",
            @"^(?s:[a-z0-9])+$",
            @"^(?m:[a-z0-9])+$",
            @"^(?n:[a-z0-9])+$",
            @"^(?x:[a-z0-9])+$",
            "\\S+.*"
        };
    }
}