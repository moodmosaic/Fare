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
            
            sut = new Fare.Xeger(pattern, random, "aAbBcC 1");

            // Act
            result = Enumerable.Repeat(0, repeatCount)
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
        
        [Theory, MemberData(nameof(RegexPatternCharset))]
        public void GeneratedTextIsCorrectCharset(string pattern, string output)
        {
            // Arrange
            const int repeatCount = 3;
            
            var randomSeed = Environment.TickCount;
            this._testOutput.WriteLine($"Random seed: {randomSeed}");
            
            var random = new Random(randomSeed);
            
            var sut = new Fare.Xeger(pattern, random);

            // Act
            Tuple<string,string>[] result = Enumerable.Repeat(0, repeatCount)
                .Select(_ =>
                {
                    var generatedValue0 = sut.Generate();
                    var generatedValue1 = sut.RegexCharset;
                    this._testOutput.WriteLine($"charset value: {generatedValue1}");
                    return new Tuple<string,string>(generatedValue0, generatedValue1);
                })
                .ToArray();
            
            // Assert
            Assert.All(result, regex => Assert.True(output == null || regex.Item2 == new string(output.Cast<char>().Distinct().OrderBy(x=>x).ToArray())));            
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
            //@"\(.*l", - on "classic" went sometimes overflow (even before my changes)
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
            //@"\w+1\w{4}",
            @"\W+1\w?2\W{4}",
            @"^[^$]$"
        };
        
        public static TheoryData<string, string> RegexPatternCharset => new TheoryData<string, string>
        {
            {"[ab]{4,6}","ab"},
            {"[ab]{4,6}c","abc"},
            {"(a|b)*ab","ab"},
            {"[A-Za-z0-9]","ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"},
            {"[A-Za-z0-9_]","ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_"},
            {"[A-Za-z]","ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"},
            {"[ \t]"," \t"},
            {@"[(?<=\W)(?=\w)|(?<=\w)(?=\W)]",null},
            {"[\x00-\x1F\x7F]",null},
            {"[0-9]","0123456789"},
            {"[^0-9]",null},
            {"[\x21-\x7E]",null},
            {"[a-z]","abcdefghijklmnopqrstuvwxyz"},
            {"[\x20-\x7E]",null},
            {"[ \t\r\n\v\f]"," \t\r\n\v\f"},
            {"[^ \t\r\n\v\f]",null},
            {"[A-Z]","ABCDEFGHIJKLMNOPQRSTUVWXYZ"},
            {"[A-Fa-f0-9]","ABCDEFabcdef0123456789"},
            {"in[du]","indu"},
            {"x[0-9A-Z]","x0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"},
            {"[^A-M]in",null},
            {".gr",null},
            //@"\(.*l", - on "classic" went sometimes overflow (even before my changes)
            {"W*in","Win"},
            {"[xX][0-9a-z]","X0123456789abcdefghijklmnopqrstuvwxyz"},
            {@"\(\(\(ab\)*c\)*d\)\(ef\)*\(gh\)\{2\}\(ij\)*\(kl\)*\(mn\)*\(op\)*\(qr\)*",null},
            {@"((mailto\:|(news|(ht|f)tp(s?))\://){1}\S+)",null},
            {@"^http\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(/\S*)?$",null},
            {@"^([1-zA-Z0-1@.\s]{1,255})$",null},
            {"[A-Z][0-9A-Z]{10}","ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"},
            {"[A-Z][A-Za-z0-9]{10}","ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"},
            {"[A-Za-z0-9]{11}","ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"},
            {"[A-Za-z]{11}","ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"},
            {@"^[a-zA-Z''-'\s]{1,40}$",null},
            {@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$",null},
            {@"a[a-z]",@"abcdefghijklmnopqrstuvwxyz"},
            {@"[1-9][0-9]",@"0123456789"},
            {@"\d{8}",@"0123456789"},
            {@"\d{5}(-\d{4})?",@"0123456789-"},
            {@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}",@"0123456789."},
            {@"\D{8}",null},
            {@"\D{5}(-\D{4})?",null},
            {@"\D{1,3}\.\D{1,3}\.\D{1,3}\.\D{1,3}",null},
            {@"^(?:[a-z0-9])+$",null},
            {@"^(?i:[a-z0-9])+$",null},
            {@"^(?s:[a-z0-9])+$",null},
            {@"^(?m:[a-z0-9])+$",null},
            {@"^(?n:[a-z0-9])+$",null},
            {@"^(?x:[a-z0-9])+$",null},
            {"\\S+.*",null},
            {@"^(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:\(\s*([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9])\s*\)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)?([2-9]1[02-9]|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})(?:\s*(?:#|x\.?|ext\.?|extension)\s*(\d+))?$",null},
            {@"^\s1\s+2\s3\s?4\s*$",null},
            {@"(\s123)+",null},
            {@"\Sabc\S{3}111",null},
            {@"^\S\S  (\S)+$",null},
            {@"\\abc\\d",null},
            //{@"\w+1\w{4}",null},
            {@"\W+1\w?2\W{4}",null},
            {@"^[^$]$",null},
        };
    }
}