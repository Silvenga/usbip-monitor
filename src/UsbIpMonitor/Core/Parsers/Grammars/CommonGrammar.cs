using Pidgin;
using static Pidgin.Parser;

namespace UsbIpMonitor.Core.Parsers.Grammars
{
    public static class CommonGrammar
    {
        public static readonly Parser<char, char> Space = Parser<char>.Token(' ');
        public static readonly Parser<char, char> Ascii = Parser<char>.Token(x => x < 128);
        public static Parser<char, char> AsciiExcept(char c) => Parser<char>.Token(x => x < 128 && x != c);
        public static Parser<char, string> AsciiStringExcept(char c) => AsciiExcept(c).AtLeastOnceString();
        public static Parser<char, char> Anything => Parser<char>.Token(_ => true);
        public static Parser<char, char> AnyCharExceptEndOfLine => AnyCharExcept('\r', '\n');
        public static Parser<char, Unit> SkipRestOfLine => AnyCharExceptEndOfLine.SkipUntil(EndOfLine);
    }
}
