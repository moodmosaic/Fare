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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAutomaton
{
    public static class Datatypes
    {
        private static readonly IDictionary<string, Automaton> automata;
        private static readonly Automaton ws;
        private static readonly HashSet<string> unicodeblockNames;
        private static readonly HashSet<string> unicodecategoryNames;
        private static readonly HashSet<string> xmlNames;

        private static readonly string[] unicodeblockNamesArray = {
                                                                      "BasicLatin",
                                                                      "Latin-1Supplement",
                                                                      "LatinExtended-A",
                                                                      "LatinExtended-B",
                                                                      "IPAExtensions",
                                                                      "SpacingModifierLetters",
                                                                      "CombiningDiacriticalMarks",
                                                                      "Greek",
                                                                      "Cyrillic",
                                                                      "Armenian",
                                                                      "Hebrew",
                                                                      "Arabic",
                                                                      "Syriac",
                                                                      "Thaana",
                                                                      "Devanagari",
                                                                      "Bengali",
                                                                      "Gurmukhi",
                                                                      "Gujarati",
                                                                      "Oriya",
                                                                      "Tamil",
                                                                      "Telugu",
                                                                      "Kannada",
                                                                      "Malayalam",
                                                                      "Sinhala",
                                                                      "Thai",
                                                                      "Lao",
                                                                      "Tibetan",
                                                                      "Myanmar",
                                                                      "Georgian",
                                                                      "HangulJamo",
                                                                      "Ethiopic",
                                                                      "Cherokee",
                                                                      "UnifiedCanadianAboriginalSyllabics",
                                                                      "Ogham",
                                                                      "Runic",
                                                                      "Khmer",
                                                                      "Mongolian",
                                                                      "LatinExtendedAdditional",
                                                                      "GreekExtended",
                                                                      "GeneralPunctuation",
                                                                      "SuperscriptsandSubscripts",
                                                                      "CurrencySymbols",
                                                                      "CombiningMarksforSymbols",
                                                                      "LetterlikeSymbols",
                                                                      "NumberForms",
                                                                      "Arrows",
                                                                      "MathematicalOperators",
                                                                      "MiscellaneousTechnical",
                                                                      "ControlPictures",
                                                                      "OpticalCharacterRecognition",
                                                                      "EnclosedAlphanumerics",
                                                                      "BoxDrawing",
                                                                      "BlockElements",
                                                                      "GeometricShapes",
                                                                      "MiscellaneousSymbols",
                                                                      "Dingbats",
                                                                      "BraillePatterns",
                                                                      "CJKRadicalsSupplement",
                                                                      "KangxiRadicals",
                                                                      "IdeographicDescriptionCharacters",
                                                                      "CJKSymbolsandPunctuation",
                                                                      "Hiragana",
                                                                      "Katakana",
                                                                      "Bopomofo",
                                                                      "HangulCompatibilityJamo",
                                                                      "Kanbun",
                                                                      "BopomofoExtended",
                                                                      "EnclosedCJKLettersandMonths",
                                                                      "CJKCompatibility",
                                                                      "CJKUnifiedIdeographsExtensionA",
                                                                      "CJKUnifiedIdeographs",
                                                                      "YiSyllables",
                                                                      "YiRadicals",
                                                                      "HangulSyllables",
                                                                      "CJKCompatibilityIdeographs",
                                                                      "AlphabeticPresentationForms",
                                                                      "ArabicPresentationForms-A",
                                                                      "CombiningHalfMarks",
                                                                      "CJKCompatibilityForms",
                                                                      "SmallFormVariants",
                                                                      "ArabicPresentationForms-B",
                                                                      "Specials",
                                                                      "HalfwidthandFullwidthForms",
                                                                      "Specials",
                                                                      "OldItalic",
                                                                      "Gothic",
                                                                      "Deseret",
                                                                      "ByzantineMusicalSymbols",
                                                                      "MusicalSymbols",
                                                                      "MathematicalAlphanumericSymbols",
                                                                      "CJKUnifiedIdeographsExtensionB",
                                                                      "CJKCompatibilityIdeographsSupplement",
                                                                      "Tags"
                                                                  };

        private static readonly string[] unicodecategoryNamesArray = {
                                                                         "Lu",
                                                                         "Ll",
                                                                         "Lt",
                                                                         "Lm",
                                                                         "Lo",
                                                                         "L",
                                                                         "Mn",
                                                                         "Mc",
                                                                         "Me",
                                                                         "M",
                                                                         "Nd",
                                                                         "Nl",
                                                                         "No",
                                                                         "N",
                                                                         "Pc",
                                                                         "Pd",
                                                                         "Ps",
                                                                         "Pe",
                                                                         "Pi",
                                                                         "Pf",
                                                                         "Po",
                                                                         "P",
                                                                         "Zs",
                                                                         "Zl",
                                                                         "Zp",
                                                                         "Z",
                                                                         "Sm",
                                                                         "Sc",
                                                                         "Sk",
                                                                         "So",
                                                                         "S",
                                                                         "Cc",
                                                                         "Cf",
                                                                         "Co",
                                                                         "Cn",
                                                                         "C"
                                                                     };

        private static readonly string[] xmlNamesArray = {
                                                             "NCName",
                                                             "QName",
                                                             "Char",
                                                             "NameChar",
                                                             "URI",
                                                             "anyname",
                                                             "noap",
                                                             "whitespace",
                                                             "whitespacechar",
                                                             "string",
                                                             "boolean",
                                                             "decimal",
                                                             "float",
                                                             "integer",
                                                             "duration",
                                                             "dateTime",
                                                             "time",
                                                             "date",
                                                             "gYearMonth",
                                                             "gYear",
                                                             "gMonthDay",
                                                             "gDay",
                                                             "hexBinary",
                                                             "base64Binary",
                                                             "NCName2",
                                                             "NCNames",
                                                             "QName2",
                                                             "Nmtoken2",
                                                             "Nmtokens",
                                                             "Name2",
                                                             "Names",
                                                             "language"
                                                         };

        static Datatypes()
        {
            automata = new Dictionary<string, Automaton>();
            ws = Automaton.Minimize(Automaton.MakeCharSet(" \t\n\r").Repeat());
            unicodeblockNames = new HashSet<string>(unicodeblockNamesArray);
            unicodecategoryNames = new HashSet<string>(unicodecategoryNamesArray);
            xmlNames = new HashSet<string>(xmlNamesArray);
        }

        public static Automaton WhitespaceAutomaton
        {
            get { return ws; }
        }

        private static void Main() // Private for now, till porting is done.
        {
            long t = DateTime.Now.Millisecond;
            bool b = Automaton.SetAllowMutate(true);
            BuildAll();
            Automaton.SetAllowMutate(b);
            Console.WriteLine("Storing automata...");
            foreach (KeyValuePair<string, Automaton> entry in automata)
            {
                Store(entry.Key, entry.Value);
            }

            Console.WriteLine("Time for building automata: " + (DateTime.Now.Millisecond - t) + "ms");
        }

        public static Automaton Get(string name)
        {
            Automaton a = automata[name];
            if (a == null)
            {
                a = Load(name);
                automata.Add(name, a);
            }
            return a;
        }

        public static bool IsUnicodeBlockName(string name)
        {
            return unicodeblockNames.Contains(name);
        }

        public static bool IsUnicodeCategoryName(string name)
        {
            return unicodecategoryNames.Contains(name);
        }

        public static bool IsXmlName(string name)
        {
            return xmlNames.Contains(name);
        }

        public static bool Exists(string name)
        {
            throw new NotImplementedException();
        }

        private static Automaton Load(string name)
        {
            throw new NotImplementedException();
        }

        private static void Store(string name, Automaton a)
        {
            throw new NotImplementedException();
        }

        private static void BuildAll()
        {
            string[] xmlexps = {
                                   "Extender",
                                   "[\u3031-\u3035\u309D-\u309E\u30FC-\u30FE\u00B7\u02D0\u02D1\u0387\u0640\u0E46\u0EC6\u3005]"
                                   ,
                                   "CombiningChar",
                                   "[\u0300-\u0345\u0360-\u0361\u0483-\u0486\u0591-\u05A1\u05A3-\u05B9\u05BB-\u05BD\u05C1-\u05C2\u064B-\u0652" +
                                   "\u06D6-\u06DC\u06DD-\u06DF\u06E0-\u06E4\u06E7-\u06E8\u06EA-\u06ED\u0901-\u0903\u093E-\u094C\u0951-\u0954" +
                                   "\u0962-\u0963\u0981-\u0983\u09C0-\u09C4\u09C7-\u09C8\u09CB-\u09CD\u09E2-\u09E3\u0A40-\u0A42\u0A47-\u0A48" +
                                   "\u0A4B-\u0A4D\u0A70-\u0A71\u0A81-\u0A83\u0ABE-\u0AC5\u0AC7-\u0AC9\u0ACB-\u0ACD\u0B01-\u0B03\u0B3E-\u0B43" +
                                   "\u0B47-\u0B48\u0B4B-\u0B4D\u0B56-\u0B57\u0B82-\u0B83\u0BBE-\u0BC2\u0BC6-\u0BC8\u0BCA-\u0BCD\u0C01-\u0C03" +
                                   "\u0C3E-\u0C44\u0C46-\u0C48\u0C4A-\u0C4D\u0C55-\u0C56\u0C82-\u0C83\u0CBE-\u0CC4\u0CC6-\u0CC8\u0CCA-\u0CCD" +
                                   "\u0CD5-\u0CD6\u0D02-\u0D03\u0D3E-\u0D43\u0D46-\u0D48\u0D4A-\u0D4D\u0E34-\u0E3A\u0E47-\u0E4E\u0EB4-\u0EB9" +
                                   "\u0EBB-\u0EBC\u0EC8-\u0ECD\u0F18-\u0F19\u0F71-\u0F84\u0F86-\u0F8B\u0F90-\u0F95\u0F99-\u0FAD\u0FB1-\u0FB7" +
                                   "\u20D0-\u20DC\u302A-\u302F\u05BF\u05C4\u0670\u093C\u094D\u09BC\u09BE\u09BF\u09D7\u0A02\u0A3C\u0A3E\u0A3F" +
                                   "\u0ABC\u0B3C\u0BD7\u0D57\u0E31\u0EB1\u0F35\u0F37\u0F39\u0F3E\u0F3F\u0F97\u0FB9\u20E1\u3099\u309A]"
                                   ,
                                   "Digit",
                                   "[\u0030-\u0039\u0660-\u0669\u06F0-\u06F9\u0966-\u096F\u09E6-\u09EF\u0A66-\u0A6F\u0AE6-\u0AEF\u0B66-\u0B6F" +
                                   "\u0BE7-\u0BEF\u0C66-\u0C6F\u0CE6-\u0CEF\u0D66-\u0D6F\u0E50-\u0E59\u0ED0-\u0ED9\u0F20-\u0F29]"
                                   ,
                                   "Ideographic",
                                   "[\u4E00-\u9FA5\u3021-\u3029\u3007]",
                                   "BaseChar",
                                   "[\u0041-\u005A\u0061-\u007A\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF\u0100-\u0131\u0134-\u013E\u0141-\u0148" +
                                   "\u014A-\u017E\u0180-\u01C3\u01CD-\u01F0\u01F4-\u01F5\u01FA-\u0217\u0250-\u02A8\u02BB-\u02C1\u0388-\u038A" +
                                   "\u038E-\u03A1\u03A3-\u03CE\u03D0-\u03D6\u03E2-\u03F3\u0401-\u040C\u040E-\u044F\u0451-\u045C\u045E-\u0481" +
                                   "\u0490-\u04C4\u04C7-\u04C8\u04CB-\u04CC\u04D0-\u04EB\u04EE-\u04F5\u04F8-\u04F9\u0531-\u0556\u0561-\u0586" +
                                   "\u05D0-\u05EA\u05F0-\u05F2\u0621-\u063A\u0641-\u064A\u0671-\u06B7\u06BA-\u06BE\u06C0-\u06CE\u06D0-\u06D3" +
                                   "\u06E5-\u06E6\u0905-\u0939\u0958-\u0961\u0985-\u098C\u098F-\u0990\u0993-\u09A8\u09AA-\u09B0\u09B6-\u09B9" +
                                   "\u09DC-\u09DD\u09DF-\u09E1\u09F0-\u09F1\u0A05-\u0A0A\u0A0F-\u0A10\u0A13-\u0A28\u0A2A-\u0A30\u0A32-\u0A33" +
                                   "\u0A35-\u0A36\u0A38-\u0A39\u0A59-\u0A5C\u0A72-\u0A74\u0A85-\u0A8B\u0A8F-\u0A91\u0A93-\u0AA8\u0AAA-\u0AB0" +
                                   "\u0AB2-\u0AB3\u0AB5-\u0AB9\u0B05-\u0B0C\u0B0F-\u0B10\u0B13-\u0B28\u0B2A-\u0B30\u0B32-\u0B33\u0B36-\u0B39" +
                                   "\u0B5C-\u0B5D\u0B5F-\u0B61\u0B85-\u0B8A\u0B8E-\u0B90\u0B92-\u0B95\u0B99-\u0B9A\u0B9E-\u0B9F\u0BA3-\u0BA4" +
                                   "\u0BA8-\u0BAA\u0BAE-\u0BB5\u0BB7-\u0BB9\u0C05-\u0C0C\u0C0E-\u0C10\u0C12-\u0C28\u0C2A-\u0C33\u0C35-\u0C39" +
                                   "\u0C60-\u0C61\u0C85-\u0C8C\u0C8E-\u0C90\u0C92-\u0CA8\u0CAA-\u0CB3\u0CB5-\u0CB9\u0CE0-\u0CE1\u0D05-\u0D0C" +
                                   "\u0D0E-\u0D10\u0D12-\u0D28\u0D2A-\u0D39\u0D60-\u0D61\u0E01-\u0E2E\u0E32-\u0E33\u0E40-\u0E45\u0E81-\u0E82" +
                                   "\u0E87-\u0E88\u0E94-\u0E97\u0E99-\u0E9F\u0EA1-\u0EA3\u0EAA-\u0EAB\u0EAD-\u0EAE\u0EB2-\u0EB3\u0EC0-\u0EC4" +
                                   "\u0F40-\u0F47\u0F49-\u0F69\u10A0-\u10C5\u10D0-\u10F6\u1102-\u1103\u1105-\u1107\u110B-\u110C\u110E-\u1112" +
                                   "\u1154-\u1155\u115F-\u1161\u116D-\u116E\u1172-\u1173\u11AE-\u11AF\u11B7-\u11B8\u11BC-\u11C2\u1E00-\u1E9B" +
                                   "\u1EA0-\u1EF9\u1F00-\u1F15\u1F18-\u1F1D\u1F20-\u1F45\u1F48-\u1F4D\u1F50-\u1F57\u1F5F-\u1F7D\u1F80-\u1FB4" +
                                   "\u1FB6-\u1FBC\u1FC2-\u1FC4\u1FC6-\u1FCC\u1FD0-\u1FD3\u1FD6-\u1FDB\u1FE0-\u1FEC\u1FF2-\u1FF4\u1FF6-\u1FFC" +
                                   "\u212A-\u212B\u2180-\u2182\u3041-\u3094\u30A1-\u30FA\u3105-\u312C\uAC00-\uD7A3" +
                                   "\u0386\u038C\u03DA\u03DC\u03DE\u03E0\u0559\u06D5\u093D\u09B2\u0A5E\u0A8D\u0ABD\u0AE0\u0B3D\u0B9C\u0CDE\u0E30\u0E84\u0E8A" +
                                   "\u0E8D\u0EA5\u0EA7\u0EB0\u0EBD\u1100\u1109\u113C\u113E\u1140\u114C\u114E\u1150\u1159\u1163\u1165\u1167\u1169\u1175\u119E" +
                                   "\u11A8\u11AB\u11BA\u11EB\u11F0\u11F9\u1F59\u1F5B\u1F5D\u1FBE\u2126\u212E]",
                                   "Letter", "<BaseChar>|<Ideographic>",
                                   "NCNameChar", "<Letter>|<Digit>|[-._]|<CombiningChar>|<Extender>",
                                   "NameChar", "<NCNameChar>|:",
                                   "Nmtoken", "<NameChar>+",
                                   "NCName", "(<Letter>|_)<NCNameChar>*",
                                   "Name", "(<Letter>|[_:])<NameChar>*",
                                   "QName", "(<NCName>:)?<NCName>",
                                   "Char", @"[\t\n\r\u0020-\uD7FF\ue000-\ufffd]|[\uD800-\uDBFF][\uDC00-\uDFFF]",
                                   "whitespacechar", "[ \t\n\r]"
                               };

            Console.WriteLine("Building XML automata...");
            IDictionary<string, Automaton> t = BuildMap(xmlexps);
            PutFrom("NCName", t);
            PutFrom("QName", t);
            PutFrom("Char", t);
            PutFrom("NameChar", t);
            PutFrom("Letter", t);
            PutFrom("whitespacechar", t);

            Put(automata, "whitespace", ws);

            string[] uriexps = {
                                   "digit", "[0-9]",
                                   "upalpha", "[A-Z]",
                                   "lowalpha", "[a-z]",
                                   "alpha", "<lowalpha>|<upalpha>",
                                   "alphanum", "<alpha>|<digit>",
                                   "hex", "<digit>|[a-f]|[A-F]",
                                   "escaped", "%<hex><hex>",
                                   "mark", "[-_.!~*'()]",
                                   "unreserved", "<alphanum>|<mark>",
                                   // "reserved", "[;/?:@&=+$,]",
                                   "reserved", "[;/?:@&=+$,\\[\\]]", // RFC 2732
                                   "uric", "<reserved>|<unreserved>|<escaped>",
                                   "fragment", "<uric>*",
                                   "query", "<uric>*",
                                   "pchar", "<unreserved>|<escaped>|[:@&=+$,]",
                                   "param", "<pchar>*",
                                   "segment", "<pchar>*(;<param>)*",
                                   "path_segments", "<segment>(/<segment>)*",
                                   "abs_path", "/<path_segments>",
                                   "uric_no_slash", "<unreserved>|<escaped>|[;?:@&=+$,]",
                                   "opaque_part", "<uric_no_slash><uric>*",
                                   //"path", "(<abs_path>|<opaque_part>)?",  // not used
                                   "port", "<digit>*",
                                   // "IPv4address", "(<digit>{1,}\\.){3}<digit>{1,}",
                                   "IPv4address", "(<digit>{1,3}\\.){3}<digit>{1,3}", // RFC 2732 / 2373
                                   "hexseq", "<hex>{1,4}(:<hex>{1,4})*", // RFC 2373
                                   "hexpart", "<hexseq>|<hexseq>::<hexseq>?|::<hexseq>", // RFC 2373
                                   "IPv6address", "<hexpart>(:<IPv4address>)?", // RFC 2373
                                   "toplabel", "<alpha>|(<alpha>(<alphanum>|-)*<alphanum>)",
                                   "domainlabel", "<alphanum>|(<alphanum>(<alphanum>|-)*<alphanum>)",
                                   "hostname", "(<domainlabel>\\.)*<toplabel>\\.?",
                                   // "host", "<hostname>|<IPv4address>", 
                                   "host", "<hostname>|<IPv4address>|\\[<IPv6address>\\]", // RFC 2732
                                   "hostport", "<host>(:<port>)?",
                                   "userinfo", "(<unreserved>|<escaped>|[;:&=+$,])*",
                                   "server", "((<userinfo>\\@)?<hostport>)?",
                                   "reg_name", "(<unreserved>|<escaped>|[$,;:@&=+])+",
                                   "authority", "<server>|<reg_name>",
                                   "scheme", "<alpha>(<alpha>|<digit>|[-+.])*",
                                   "rel_segment", "(<unreserved>|<escaped>|[;@&=+$,])+",
                                   "rel_path", "<rel_segment><abs_path>?",
                                   "net_path", "//<authority><abs_path>?",
                                   "hier_part", "(<net_path>|<abs_path>)(\\?<query>)?",
                                   "relativeURI", "(<net_path>|<abs_path>|<rel_path>)(\\?<query>)?",
                                   "absoluteURI", "<scheme>:(<hier_part>|<opaque_part>)",
                                   "URI", "(<absoluteURI>|<relativeURI>)?(\\#<fragment>)?"
                               };

            Console.WriteLine("Building URI automaton...");
            PutFrom("URI", BuildMap(uriexps));
            Put(automata, "anyname", Automaton.Minimize(Automaton.MakeChar('{')
                                                            .Concatenate(automata["URI"].Clone()).Concatenate(
                                                                Automaton.MakeChar('}')).Optional()
                                                            .Concatenate(automata["NCName"].Clone())));

            Put(automata, "noap", new RegExp("~(@[@%]@)").ToAutomaton());

            string[] xsdmisc = {
                                   "_", "[ \t\n\r]*",
                                   "d", "[0-9]",
                                   "Z", "[-+](<00-13>:<00-59>|14:00)|Z",
                                   "Y", "(<d>{4,})&~(0000)",
                                   "M", "<01-12>",
                                   "D", "<01-31>",
                                   "T", "<00-23>:<00-59>:<00-59>|24:00:00",
                                   "B64", "[A-Za-z0-9+/]",
                                   "B16", "[AEIMQUYcgkosw048]",
                                   "B04", "[AQgw]",
                                   "B04S", "<B04> ?",
                                   "B16S", "<B16> ?",
                                   "B64S", "<B64> ?"
                               };
            string[] xsdexps = {
                                   "boolean", "<_>(true|false|1|0)<_>",
                                   "decimal", "<_>([-+]?<d>+(\\.<d>+)?)<_>",
                                   "float", "<_>([-+]?<d>+(\\.<d>+)?([Ee][-+]?<d>+)?|INF|-INF|NaN)<_>",
                                   "integer", "<_>[-+]?[0-9]+<_>",
                                   "duration",
                                   "<_>(-?P(((<d>+Y)?(<d>+M)?(<d>+D)?(T(((<d>+H)?(<d>+M)?(<d>+(\\.<d>+)?S)?)&~()))?)&~()))<_>"
                                   ,
                                   "dateTime", "<_>(-?<Y>-<M>-<D>T<T>(\\.<d>+)?<Z>?)<_>",
                                   "time", "<_>(<T>(\\.<d>+)?<Z>?)<_>",
                                   "date", "<_>(-?<Y>-<M>-<D><Z>?)<_>",
                                   "gYearMonth", "<_>(-?<Y>-<M><Z>?)<_>",
                                   "gYear", "<_>(-?<Y><Z>?)<_>",
                                   "gMonthDay", "<_>(--<M>-<D><Z>?)<_>",
                                   "gDay", "<_>(--<D><Z>?)<_>",
                                   "gMonth", "<_>(--<M><Z>?)<_>",
                                   "hexBinary", "<_>([0-9a-fA-F]{2}*)<_>",
                                   "base64Binary",
                                   "<_>(((<B64S><B64S><B64S><B64S>)*((<B64S><B64S><B64S><B64>)|(<B64S><B64S><B16S>=)|(<B64S><B04S>= ?=)))?)<_>"
                                   ,
                                   "language", "<_>[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*<_>",
                                   "nonPositiveInteger", "<_>(0+|-<d>+)<_>",
                                   "negativeInteger", "<_>(-[1-9]<d>*)<_>",
                                   "nonNegativeInteger", "<_>(<d>+)<_>",
                                   "positiveInteger", "<_>([1-9]<d>*)<_>"
                               };

            Console.WriteLine("Building XML Schema automata...");
            IDictionary<string, Automaton> m = BuildMap(xsdmisc);
            PutWith(xsdexps, m);

            Put(m, "UNSIGNEDLONG", Automaton.MakeMaxInteger("18446744073709551615"));
            Put(m, "UNSIGNEDINT", Automaton.MakeMaxInteger("4294967295"));
            Put(m, "UNSIGNEDSHORT", Automaton.MakeMaxInteger("65535"));
            Put(m, "UNSIGNEDBYTE", Automaton.MakeMaxInteger("255"));
            Put(m, "LONG", Automaton.MakeMaxInteger("9223372036854775807"));
            Put(m, "LONG_NEG", Automaton.MakeMaxInteger("9223372036854775808"));
            Put(m, "INT", Automaton.MakeMaxInteger("2147483647"));
            Put(m, "INT_NEG", Automaton.MakeMaxInteger("2147483648"));
            Put(m, "SHORT", Automaton.MakeMaxInteger("32767"));
            Put(m, "SHORT_NEG", Automaton.MakeMaxInteger("32768"));
            Put(m, "BYTE", Automaton.MakeMaxInteger("127"));
            Put(m, "BYTE_NEG", Automaton.MakeMaxInteger("128"));

            var dictionaries = new List<IDictionary<string, Automaton>>();
            dictionaries.Add(t);
            dictionaries.Add(m);
            IDictionary<string, Automaton> u = dictionaries.SelectMany(dict => dict)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(group => group.Key, group => group.First());

            string[] xsdexps2 = {
                                    "Nmtoken2", "<_><Nmtoken><_>",
                                    "Name2", "<_><Name><_>",
                                    "NCName2", "<_><NCName><_>",
                                    "QName2", "<_><QName><_>",
                                    "Nmtokens", "<_>(<Nmtoken><_>)+",
                                    "NCNames", "<_>(<NCName><_>)+",
                                    "Names", "<_>(<Name><_>)+",
                                    "unsignedLong", "<_><UNSIGNEDLONG><_>",
                                    "unsignedInt", "<_><UNSIGNEDINT><_>",
                                    "unsignedShort", "<_><UNSIGNEDSHORT><_>",
                                    "unsignedByte", "<_><UNSIGNEDBYTE><_>",
                                    "long", "<_>(<LONG>|-<LONG_NEG>)<_>",
                                    "int", "<_>(<INT>|-<INT_NEG>)<_>",
                                    "short", "<_>(<SHORT>|-<SHORT_NEG>)<_>",
                                    "byte", "<_>(<BYTE>|-<BYTE_NEG>)<_>",
                                    "string", "<Char>*"
                                };
            PutWith(xsdexps2, u);

            Console.WriteLine("Building Unicode block automata...");
            Put(automata, "BasicLatin", Automaton.MakeCharRange('\u0000', '\u007F'));
            Put(automata, "Latin-1Supplement", Automaton.MakeCharRange('\u0080', '\u00FF'));
            Put(automata, "LatinExtended-A", Automaton.MakeCharRange('\u0100', '\u017F'));
            Put(automata, "LatinExtended-B", Automaton.MakeCharRange('\u0180', '\u024F'));
            Put(automata, "IPAExtensions", Automaton.MakeCharRange('\u0250', '\u02AF'));
            Put(automata, "SpacingModifierLetters", Automaton.MakeCharRange('\u02B0', '\u02FF'));
            Put(automata, "CombiningDiacriticalMarks", Automaton.MakeCharRange('\u0300', '\u036F'));
            Put(automata, "Greek", Automaton.MakeCharRange('\u0370', '\u03FF'));
            Put(automata, "Cyrillic", Automaton.MakeCharRange('\u0400', '\u04FF'));
            Put(automata, "Armenian", Automaton.MakeCharRange('\u0530', '\u058F'));
            Put(automata, "Hebrew", Automaton.MakeCharRange('\u0590', '\u05FF'));
            Put(automata, "Arabic", Automaton.MakeCharRange('\u0600', '\u06FF'));
            Put(automata, "Syriac", Automaton.MakeCharRange('\u0700', '\u074F'));
            Put(automata, "Thaana", Automaton.MakeCharRange('\u0780', '\u07BF'));
            Put(automata, "Devanagari", Automaton.MakeCharRange('\u0900', '\u097F'));
            Put(automata, "Bengali", Automaton.MakeCharRange('\u0980', '\u09FF'));
            Put(automata, "Gurmukhi", Automaton.MakeCharRange('\u0A00', '\u0A7F'));
            Put(automata, "Gujarati", Automaton.MakeCharRange('\u0A80', '\u0AFF'));
            Put(automata, "Oriya", Automaton.MakeCharRange('\u0B00', '\u0B7F'));
            Put(automata, "Tamil", Automaton.MakeCharRange('\u0B80', '\u0BFF'));
            Put(automata, "Telugu", Automaton.MakeCharRange('\u0C00', '\u0C7F'));
            Put(automata, "Kannada", Automaton.MakeCharRange('\u0C80', '\u0CFF'));
            Put(automata, "Malayalam", Automaton.MakeCharRange('\u0D00', '\u0D7F'));
            Put(automata, "Sinhala", Automaton.MakeCharRange('\u0D80', '\u0DFF'));
            Put(automata, "Thai", Automaton.MakeCharRange('\u0E00', '\u0E7F'));
            Put(automata, "Lao", Automaton.MakeCharRange('\u0E80', '\u0EFF'));
            Put(automata, "Tibetan", Automaton.MakeCharRange('\u0F00', '\u0FFF'));
            Put(automata, "Myanmar", Automaton.MakeCharRange('\u1000', '\u109F'));
            Put(automata, "Georgian", Automaton.MakeCharRange('\u10A0', '\u10FF'));
            Put(automata, "HangulJamo", Automaton.MakeCharRange('\u1100', '\u11FF'));
            Put(automata, "Ethiopic", Automaton.MakeCharRange('\u1200', '\u137F'));
            Put(automata, "Cherokee", Automaton.MakeCharRange('\u13A0', '\u13FF'));
            Put(automata, "UnifiedCanadianAboriginalSyllabics", Automaton.MakeCharRange('\u1400', '\u167F'));
            Put(automata, "Ogham", Automaton.MakeCharRange('\u1680', '\u169F'));
            Put(automata, "Runic", Automaton.MakeCharRange('\u16A0', '\u16FF'));
            Put(automata, "Khmer", Automaton.MakeCharRange('\u1780', '\u17FF'));
            Put(automata, "Mongolian", Automaton.MakeCharRange('\u1800', '\u18AF'));
            Put(automata, "LatinExtendedAdditional", Automaton.MakeCharRange('\u1E00', '\u1EFF'));
            Put(automata, "GreekExtended", Automaton.MakeCharRange('\u1F00', '\u1FFF'));
            Put(automata, "GeneralPunctuation", Automaton.MakeCharRange('\u2000', '\u206F'));
            Put(automata, "SuperscriptsandSubscripts", Automaton.MakeCharRange('\u2070', '\u209F'));
            Put(automata, "CurrencySymbols", Automaton.MakeCharRange('\u20A0', '\u20CF'));
            Put(automata, "CombiningMarksforSymbols", Automaton.MakeCharRange('\u20D0', '\u20FF'));
            Put(automata, "LetterlikeSymbols", Automaton.MakeCharRange('\u2100', '\u214F'));
            Put(automata, "NumberForms", Automaton.MakeCharRange('\u2150', '\u218F'));
            Put(automata, "Arrows", Automaton.MakeCharRange('\u2190', '\u21FF'));
            Put(automata, "MathematicalOperators", Automaton.MakeCharRange('\u2200', '\u22FF'));
            Put(automata, "MiscellaneousTechnical", Automaton.MakeCharRange('\u2300', '\u23FF'));
            Put(automata, "ControlPictures", Automaton.MakeCharRange('\u2400', '\u243F'));
            Put(automata, "OpticalCharacterRecognition", Automaton.MakeCharRange('\u2440', '\u245F'));
            Put(automata, "EnclosedAlphanumerics", Automaton.MakeCharRange('\u2460', '\u24FF'));
            Put(automata, "BoxDrawing", Automaton.MakeCharRange('\u2500', '\u257F'));
            Put(automata, "BlockElements", Automaton.MakeCharRange('\u2580', '\u259F'));
            Put(automata, "GeometricShapes", Automaton.MakeCharRange('\u25A0', '\u25FF'));
            Put(automata, "MiscellaneousSymbols", Automaton.MakeCharRange('\u2600', '\u26FF'));
            Put(automata, "Dingbats", Automaton.MakeCharRange('\u2700', '\u27BF'));
            Put(automata, "BraillePatterns", Automaton.MakeCharRange('\u2800', '\u28FF'));
            Put(automata, "CJKRadicalsSupplement", Automaton.MakeCharRange('\u2E80', '\u2EFF'));
            Put(automata, "KangxiRadicals", Automaton.MakeCharRange('\u2F00', '\u2FDF'));
            Put(automata, "IdeographicDescriptionCharacters", Automaton.MakeCharRange('\u2FF0', '\u2FFF'));
            Put(automata, "CJKSymbolsandPunctuation", Automaton.MakeCharRange('\u3000', '\u303F'));
            Put(automata, "Hiragana", Automaton.MakeCharRange('\u3040', '\u309F'));
            Put(automata, "Katakana", Automaton.MakeCharRange('\u30A0', '\u30FF'));
            Put(automata, "Bopomofo", Automaton.MakeCharRange('\u3100', '\u312F'));
            Put(automata, "HangulCompatibilityJamo", Automaton.MakeCharRange('\u3130', '\u318F'));
            Put(automata, "Kanbun", Automaton.MakeCharRange('\u3190', '\u319F'));
            Put(automata, "BopomofoExtended", Automaton.MakeCharRange('\u31A0', '\u31BF'));
            Put(automata, "EnclosedCJKLettersandMonths", Automaton.MakeCharRange('\u3200', '\u32FF'));
            Put(automata, "CJKCompatibility", Automaton.MakeCharRange('\u3300', '\u33FF'));
            Put(automata, "CJKUnifiedIdeographsExtensionA", Automaton.MakeCharRange('\u3400', '\u4DB5'));
            Put(automata, "CJKUnifiedIdeographs", Automaton.MakeCharRange('\u4E00', '\u9FFF'));
            Put(automata, "YiSyllables", Automaton.MakeCharRange('\uA000', '\uA48F'));
            Put(automata, "YiRadicals", Automaton.MakeCharRange('\uA490', '\uA4CF'));
            Put(automata, "HangulSyllables", Automaton.MakeCharRange('\uAC00', '\uD7A3'));
            Put(automata, "CJKCompatibilityIdeographs", Automaton.MakeCharRange('\uF900', '\uFAFF'));
            Put(automata, "AlphabeticPresentationForms", Automaton.MakeCharRange('\uFB00', '\uFB4F'));
            Put(automata, "ArabicPresentationForms-A", Automaton.MakeCharRange('\uFB50', '\uFDFF'));
            Put(automata, "CombiningHalfMarks", Automaton.MakeCharRange('\uFE20', '\uFE2F'));
            Put(automata, "CJKCompatibilityForms", Automaton.MakeCharRange('\uFE30', '\uFE4F'));
            Put(automata, "SmallFormVariants", Automaton.MakeCharRange('\uFE50', '\uFE6F'));
            Put(automata, "ArabicPresentationForms-B", Automaton.MakeCharRange('\uFE70', '\uFEFE'));
            Put(automata, "Specials", Automaton.MakeCharRange('\uFEFF', '\uFEFF'));
            Put(automata, "HalfwidthandFullwidthForms", Automaton.MakeCharRange('\uFF00', '\uFFEF'));
            Put(automata, "Specials", Automaton.MakeCharRange('\uFFF0', '\uFFFD'));

            /*
            Put(automata, "OldItalic", Automaton.MakeChar('\ud800').Concatenate(Automaton.MakeCharRange('\udf00', '\udf2f')));
            Put(automata, "Gothic", Automaton.MakeChar('\ud800').Concatenate(Automaton.MakeCharRange('\udf30', '\udf4f')));
            Put(automata, "Deseret", Automaton.MakeChar('\ud801').Concatenate(Automaton.MakeCharRange('\udc00', '\udc4f')));
            Put(automata, "ByzantineMusicalSymbols", Automaton.MakeChar('\ud834').Concatenate(Automaton.MakeCharRange('\udc00', '\udcff')));
            Put(automata, "MusicalSymbols", Automaton.MakeChar('\ud834').Concatenate(Automaton.MakeCharRange('\udd00', '\uddff')));
            Put(automata, "MathematicalAlphanumericSymbols", Automaton.MakeChar('\ud835').Concatenate(Automaton.MakeCharRange('\udc00', '\udfff')));

            Put(automata, "CJKUnifiedIdeographsExtensionB", Automaton.MakeCharRange('\ud840', '\ud868').Concatenate(Automaton.MakeCharRange('\udc00', '\udfff'))
                                                           .Union(Automaton.MakeChar('\ud869').Concatenate(Automaton.MakeCharRange('\udc00', '\uded6'))));

            Put(automata, "CJKCompatibilityIdeographsSupplement", Automaton.MakeChar('\ud87e').Concatenate(Automaton.MakeCharRange('\udc00', '\ude1f')));
            Put(automata, "Tags", Automaton.MakeChar('\udb40').Concatenate(Automaton.MakeCharRange('\udc00', '\udc7f')));

            Put(automata, "PrivateUse", Automaton.MakeCharRange('\uE000', '\uF8FF')
                                       .Union(Automaton.MakeCharRange('\udb80', '\udbbe').Concatenate(Automaton.MakeCharRange('\udc00', '\udfff'))
                                              .Union(Automaton.MakeChar('\udbbf').Concatenate(Automaton.MakeCharRange('\udc00', '\udffd'))))
                                       .Union(Automaton.MakeCharRange('\udbc0', '\udbfe').Concatenate(Automaton.MakeCharRange('\udc00', '\udfff'))
                                              .Union(Automaton.MakeChar('\udbff').Concatenate(Automaton.MakeCharRange('\udc00', '\udffd')))));
             */

            Console.WriteLine("Building Unicode category automata...");
            IDictionary<string, HashSet<int>> categories = new Dictionary<string, HashSet<int>>();
            try
            {
                /*
                StreamTokenizer st = new StreamTokenizer(new BufferedReader(new FileReader("src/Unicode.txt")));
                st.resetSyntax();
                st.whitespaceChars(';', ';');
                st.whitespaceChars('\n', ' ');
                st.wordChars('0', '9');
                st.wordChars('a', 'z');
                st.wordChars('A', 'Z');
                while (st.nextToken() != StreamTokenizer.TT_EOF)
                {
                    int cp = Integer.parseInt(st.sval, 16);
                    st.nextToken();
                    string cat = st.sval;
                    HashSet<int> c = categories[cat];
                    if (c == null)
                    {
                        c = new TreeSet<Integer>();
                        categories.Put(cat, c);
                    }
                    c.add(cp);
                    string ccat = cat.substring(0, 1);
                    c = categories.get(ccat);
                    if (c == null)
                    {
                        c = new TreeSet<Integer>();
                        categories.Put(ccat, c);
                    }
                    c.Add(cp);
                }
                 */
            }
            catch (IOException)
            {
                Environment.Exit(-1);
            }
            IList<Automaton> assigned = new List<Automaton>();

            foreach (KeyValuePair<string, HashSet<int>> entry in categories)
            {
                IList<Automaton> la1 = new List<Automaton>();
                IList<Automaton> la2 = new List<Automaton>();
                foreach (int cp in entry.Value)
                {
                    la1.Add(MakeCodePoint(cp));
                    if (la1.Count == 50)
                    {
                        la2.Add(Automaton.Minimize(Automaton.Union(la1)));
                        la1.Clear();
                    }
                }
                la2.Add(Automaton.Union(la1));
                Automaton a = Automaton.Minimize(Automaton.Union(la2));
                Put(automata, entry.Key, a);
                assigned.Add(a);
            }

            Automaton cn =
                Automaton.Minimize(automata["Char"].Clone().Intersection(Automaton.Union(assigned).Complement()));
            Put(automata, "Cn", cn);
            Put(automata, "C", automata["C"].Clone().Union(cn));
        }

        private static Automaton MakeCodePoint(int cp)
        {
            if (cp >= 0x10000)
            {
                cp -= 0x10000;
                char[] cu = {(char) (0xd800 + (cp >> 10)), (char) (0xdc00 + (cp & 0x3ff))};
                return Automaton.MakeString(new string(cu));
            }
            return Automaton.MakeChar((char) cp);
        }

        private static IDictionary<string, Automaton> BuildMap(string[] exps)
        {
            IDictionary<string, Automaton> map = new Dictionary<string, Automaton>();
            int i = 0;
            while (i + 1 < exps.Length)
            {
                Put(map, exps[i++], new RegExp(exps[i++]).ToAutomaton(map));
            }
            return map;
        }

        private static void PutWith(string[] exps, IDictionary<string, Automaton> use)
        {
            int i = 0;
            while (i + 1 < exps.Length)
            {
                Put(automata, exps[i++], new RegExp(exps[i++]).ToAutomaton(use));
            }
        }

        private static void PutFrom(string name, IDictionary<string, Automaton> from)
        {
            automata.Add(name, from[name]);
        }

        private static void Put(IDictionary<string, Automaton> map, string name, Automaton a)
        {
            map.Add(name, a);
            Console.WriteLine("  " + name + ": " + a.NumberOfStates + " states, " + a.NumberOfTransitions +
                              " transitions");
        }
    }
}