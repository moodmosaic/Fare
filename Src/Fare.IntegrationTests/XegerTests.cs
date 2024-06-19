using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Fare.IntegrationTests
{
    public sealed class XegerTests
    {
        private readonly ITestOutputHelper _testOutput;

        public XegerTests(ITestOutputHelper testOutput)
        {
            this._testOutput = testOutput;
        }

        [Theory, MemberData(nameof(RegexPatternTestCases))]
        public void GeneratedTextIsCorrect(string pattern)
        {
            // Arrange
            const int repeatCount = 3;
            
            var randomSeed = Environment.TickCount;
            this._testOutput.WriteLine($"Random seed: {randomSeed}");
            
            var random = new Random(randomSeed);
            
            var sut = new Fare.Xeger(pattern, random);

            // Act
            var result = Enumerable.Repeat(0, repeatCount)
                .Select(_ =>
                {
                    var generatedValue = sut.Generate();
                    this._testOutput.WriteLine($"Generated value: {generatedValue}");
                    return generatedValue;
                })
                .ToArray();

            // Assert
            Assert.All(result, regex => Assert.Matches(pattern, regex));
        }

        [Theory, MemberData(nameof(BadRegexPatternTestCases))]
        public void RegexCanNotBeParsed(string pattern)
        {
            Assert.ThrowsAny<Exception>(() => new Fare.Xeger(pattern, new Random()).Generate());
        }

#if REX_AVAILABLE
        [Theory, MemberData(nameof(RegexPatternTestCases))]
        public void GeneratedTextIsCorrectWithRexEngine(string pattern)
        {
            const int repeatCount = 3;
            var settings = new Rex.RexSettings(pattern) { k = 1 };
            // Fix generated value doesn't always meet the pattern
            settings.encoding = Rex.CharacterEncoding.ASCII;

            var result = Enumerable.Repeat(0, repeatCount)
                .Select(_ =>
                {
                    var generatedValue  = Rex.RexEngine.GenerateMembers(settings).Single();
                    this._testOutput.WriteLine($"Generated value: {generatedValue}");
                    return generatedValue;
                })
                .ToArray();

            Assert.All(result, regex => Assert.Matches(pattern, regex));
        }
#endif
        public static TheoryData<string> BadRegexPatternTestCases => new TheoryData<string>
        {
            @"\w{7,1}",
            @"[\w-.]+",  // unescaped "-" is interpreted as as invalid character range, see https://learn.microsoft.com/dotnet/standard/base-types/character-classes-in-regular-expressions#positive-character-group--
            @"[z-a]",
            @"[[",
        };

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
            @"\D{8}",
            @"\D{5}(-\D{4})?",
            @"\D{1,3}\.\D{1,3}\.\D{1,3}\.\D{1,3}",
            @"^(?:[a-z0-9])+$",
            @"^(?i:[a-z0-9])+$",
            @"^(?s:[a-z0-9])+$",
            @"^(?m:[a-z0-9])+$",
            @"^(?n:[a-z0-9])+$",
            @"^(?x:[a-z0-9])+$",
            "\\S+.*",
            @"^(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:\(\s*([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9])\s*\)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)?([2-9]1[02-9]|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})(?:\s*(?:#|x\.?|ext\.?|extension)\s*(\d+))?$",
            @"^\s1\s+2\s3\s?4\s*$",
            @"(\s123)+",
            @"\Sabc\S{3}111",
            @"^\S\S  (\S)+$",
            @"\\abc\\d",
            @"\w+1\w{4}",
            @"\W+1\w?2\W{4}",
            @"^[^$]$",
            @"[\w\-.]",
            @"[\w.-]",
            @"[-\w]",
        };
    }
}