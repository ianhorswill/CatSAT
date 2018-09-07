#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SExpression.cs" company="Ian Horswill">
// Copyright (C) 2018 Ian Horswill
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//  
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PCGToy
{
    internal static class SExpression
    {
        //public static string ToSExpression(object value)
        //{
        //    var b = new StringBuilder();
        //    Write(value, b);
        //    return b.ToString();
        //}

        //static void Write(object value, StringBuilder b)
        //{
        //    switch (value)
        //    {
        //        case int i:
        //            b.Append(i);
        //            break;

        //        case float f:
        //            b.Append(f);
        //            break;

        //        case string s:
        //            if (StringSafe(s))
        //                b.Append(s);
        //            else
        //            {
        //                b.Append('"');
        //                b.Append(s);
        //                b.Append('"');
        //            }

        //            break;

        //        case bool tf:
        //            b.Append(tf?"true":"false");
        //            break;

        //        case IEnumerable e:
        //            b.Append('(');
        //            var first = true;
        //            foreach (var elt in e)
        //            {
        //                if (first)
        //                    first = false;
        //                else b.Append(' ');
        //                Write(elt, b);
        //            }

        //            b.Append(')');
        //            break;

        //        default:
        //            throw new ArgumentException($"Don't know how to render {value} as an s-expression.");
        //    }
        //}

        //private static bool StringSafe(string s)
        //{
        //    return s.IndexOfAny(BadChars) < 0;
        //}

        public static object Read(TextReader r)
        {
            var result = ReadInternal(r);
            Skip(r);
            return result;
        }

        private static object ReadInternal(TextReader r)
        {
            int c;
            Skip(r);
            switch (c = r.Read())
            {
                case -1:
                    throw new FileFormatException("Premature end of file");

                case '(':
                    return ReadList(r);

                case '"':
                    return ReadString(r);

                case '+':
                case '-':
                case '.':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ReadNumber(r, (char)c);

                default:
                    if (char.IsLetter((char) c))
                        return ReadSymbol(c, r);

                    throw new FileFormatException($"Unexpected character '{(char)c}'");
            }
        }

        private static object ReadNumber(TextReader r, char initial)
        {
            StringBuffer.Clear();
            var sign = initial == '-' ? -1 : 1;
            var decimalPointCount = initial == '.' ? 1 : 0;
            if (char.IsDigit(initial))
                StringBuffer.Append(initial);

            while (IsDigitOrDecimal(r.Peek()))
            {
                char c = (char) r.Read();
                StringBuffer.Append(c);
                if (c == '.')
                    decimalPointCount++;
            }

            switch (decimalPointCount)
            {
                case 0:
                    return sign * Int32.Parse(StringBuffer.ToString());

                case 1:
                    return sign * float.Parse(StringBuffer.ToString());

                default:
                    throw new FileFormatException($"Unknown number formatl {StringBuffer}");
            }
        }

        private static bool IsDigitOrDecimal(int c)
        {
            return c == '.' || char.IsDigit((char) c);
        }

        private static object ReadList(TextReader r)
        {
            var listBuffer = new List<object>();
            Skip(r);
            while (r.Peek() != ')')
            {
                listBuffer.Add(ReadInternal(r));
                Skip(r);
            }

            r.Read();

            return listBuffer;
        }

        private static object ReadSymbol(int initialChar, TextReader r)
        {
            StringBuffer.Clear();
            StringBuffer.Append((char) initialChar);
            while (!SymbolTerminator((char) r.Peek()))
                StringBuffer.Append((char) r.Read());

            return StringBuffer.ToString();
        }

        private static bool SymbolTerminator(char c)
        {
            return char.IsWhiteSpace(c) || c == '(' || c == ')';
        }

        private static readonly StringBuilder StringBuffer = new StringBuilder();
        //private static readonly char[] BadChars = new []{ ' ', '\r', '\n', '"', '(', ')' };

        private static string ReadString(TextReader r)
        {
            StringBuffer.Clear();
            int c=0;
            while (c != '"')
            {
                switch (c = r.Read())
                {
                    case -1:
                        throw new FileFormatException($"Premature end of file inside string \"{StringBuffer}\"");

                    case '"':
                        // Do nothing; the while loop will terminate
                        break;

                    case '\\':
                        StringBuffer.Append((char) r.Read());
                        break;

                    default:
                        StringBuffer.Append((char) c);
                        break;
                }
            }

            return StringBuffer.ToString();
        }

        private static void Skip(TextReader r)
        {
            while (SkipComment(r) || SkipWhitespace(r))
            { }
        }

        private static bool SkipComment(TextReader r)
        {
            if (r.Peek() != ';')
                return false;
            while (!CommentTerminator(r.Read()))
            { }

            return true;
        }

        private static bool CommentTerminator(int c)
        {
            switch (c)
            {
                case -1:
                case '\r':
                case '\n':
                    return true;

                default:
                    return false;
            }
        }

        private static bool SkipWhitespace(TextReader r)
        {
            if (!Whitespace(r.Peek()))
                return false;

            while (Whitespace(r.Peek()))
                r.Read();

            return true;
        }

        private static bool Whitespace(int c)
        {
            return c > 0 && char.IsWhiteSpace((char) c);
        }
    }

    /// <summary>
    /// Signals that a file loaded with the Generator class was not a valid PCGToy file.
    /// </summary>
    public class FileFormatException : Exception
    {
        internal FileFormatException(string message) : base(message)
        {
            
        }
    }
}
