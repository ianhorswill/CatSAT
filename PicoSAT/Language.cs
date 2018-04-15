#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Language.cs" company="Ian Horswill">
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
using System.Diagnostics;
using System.Text;

namespace PicoSAT
{
    /// <summary>
    /// Definitions for embedded solver language.
    /// </summary>
    public static class Language
    {
        /// <summary>
        /// Make a unary predicate
        /// </summary>
        /// <typeparam name="T1">Argument type</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, Proposition> Predicate<T1>(string name)
        {
            return arg1 => Proposition.MakeProposition(new Call(name, arg1));
        }

        /// <summary>
        /// Make a binary predicate
        /// </summary>
        /// <typeparam name="T1">Type for argument 1</typeparam>
        /// <typeparam name="T2">Type for argument 2</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, T2, Proposition> Predicate<T1, T2>(string name)
        {
            return (arg1, arg2) => Proposition.MakeProposition(new Call(name, arg1, arg2));
        }

        /// <summary>
        /// Make a symmetric predicate
        /// This will enforce that predicate(a, b) == predicate(b, a)
        /// </summary>
        /// <typeparam name="T">Argument type</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T, T, Proposition> SymmetricPredicate<T>(string name) where T : IComparable
        {
            return (arg1, arg2) =>
            {
                if (arg1.CompareTo(arg2) > 0)
                    return Proposition.MakeProposition(new Call(name, arg2, arg1));
                return Proposition.MakeProposition(new Call(name, arg1, arg2));
            };
        }

        /// <summary>
        /// Make a symmetric, reflexive predicate
        /// This will enforce that predicate(a, b) == predicate(b, a)
        /// </summary>
        /// <typeparam name="T">Argument type</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T, T, Proposition> SymmetricTransitiveRelation<T>(string name) where T : IComparable
        {
            return (arg1, arg2) =>
            {
                var diff = arg1.CompareTo(arg2);
                if (diff > 0)
                    return Proposition.MakeProposition(new Call(name, arg2, arg1));
                if (diff == 0)
                    return Proposition.True;
                // diff < 0
                return Proposition.MakeProposition(new Call(name, arg1, arg2));
            };
        }

        /// <summary>
        /// Make a ternary predicate
        /// </summary>
        /// <typeparam name="T1">Type for argument 1</typeparam>
        /// <typeparam name="T2">Type for argument 2</typeparam>
        /// <typeparam name="T3">Type for argument 3</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, T2, T3, Proposition> Predicate<T1, T2, T3>(string name)
        {
            return (arg1, arg2, arg3) => Proposition.MakeProposition(new Call(name, arg1, arg2, arg3));
        }

        /// <summary>
        /// Make a quaternary predicate
        /// </summary>
        /// <typeparam name="T1">Type for argument 1</typeparam>
        /// <typeparam name="T2">Type for argument 2</typeparam>
        /// <typeparam name="T3">Type for argument 3</typeparam>
        /// <typeparam name="T4">Type for argument 4</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, T2, T3, T4, Proposition> Predicate<T1, T2, T3, T4>(string name)
        {
            return (arg1, arg2, arg3, arg4) => Proposition.MakeProposition(new Call(name, arg1, arg2, arg3, arg4));
        }

        /// <summary>
        /// Make a 5-argument predicate
        /// </summary>
        /// <typeparam name="T1">Type for argument 1</typeparam>
        /// <typeparam name="T2">Type for argument 2</typeparam>
        /// <typeparam name="T3">Type for argument 3</typeparam>
        /// <typeparam name="T4">Type for argument 4</typeparam>
        /// <typeparam name="T5">Type for argument 5</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, T2, T3, T4, T5, Proposition> Predicate<T1, T2, T3, T4, T5>(string name)
        {
            return (arg1, arg2, arg3, arg4, arg5) => Proposition.MakeProposition(new Call(name, arg1, arg2, arg3, arg4, arg5));
        }

        /// <summary>
        /// Make a unary function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        public static Func<T1, Call> Function<T1>(string name)
        {
            return (arg1) => new Call(name, arg1);
        }

        /// <summary>
        /// Make a binary function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        public static Func<T1, T2, Call> Function<T1, T2>(string name)
        {
            return (arg1, arg2) => new Call(name, arg1, arg2);
        }

        /// <summary>
        /// Make a 3-argument function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        public static Func<T1, T2, T3, Call> Function<T1, T2, T3>(string name)
        {
            return (arg1, arg2, arg3) => new Call(name, arg1, arg2, arg3);
        }

        /// <summary>
        /// Make a 4-argument function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        public static Func<T1, T2, T3, T4, Call> Function<T1, T2, T3, T4>(string name)
        {
            return (arg1, arg2, arg3, arg4) => new Call(name, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Make a 5-argument function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        public static Func<T1, T2, T3, T4, T5, Call> Function<T1, T2, T3, T4, T5>(string name)
        {
            return (arg1, arg2, arg3, arg4, arg5) => new Call(name, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// Returns a negation for the specified proposition
        /// </summary>
        /// <param name="p">Proposition to negate</param>
        public static Literal Not(Proposition p)
        {
            return Negation.Not(p);
        }

        /// <summary>
        /// Represents a call to a predicate or function with specific arguments.
        /// This gets used as the name of the Proposition that the predicate returns when you call it.
        /// </summary>
        [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
        public sealed class Call
        {
            private readonly string name;
            private readonly object[] args;

            public Call(string name, params object[] args)
            {
                this.name = name;
                this.args = args;
            }

            public override int GetHashCode()
            {
                var hash = name.GetHashCode();
                foreach (var a in args)
                    hash ^= a.GetHashCode();
                return hash;
            }

            public override bool Equals(object obj)
            {
                if (obj is Call c)
                {
                    if (name != c.name)
                        return false;
                    if (args.Length != c.args.Length)
                        return false;
                    for (int i = 0; i < args.Length; i++)
                        if (!args[i].Equals(c.args[i]))
                            return false;
                    return true;
                }
                return false;
            }

            public override string ToString()
            {
                var b = new StringBuilder();
                b.Append(name);
                b.Append('(');
                var firstOne = true;
                foreach (var a in args)
                {
                    if (firstOne)
                        firstOne = false;
                    else
                        b.Append(", ");
                    b.Append(a);
                }

                b.Append(')');
                return b.ToString();
            }

            private string DebuggerDisplay => ToString();
        }
    }
}
