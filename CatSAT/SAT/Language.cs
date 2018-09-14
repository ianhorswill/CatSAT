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

namespace CatSAT
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
            return arg1 => Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg1));
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
            return (arg1, arg2) => Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg1, arg2));
        }

        /// <summary>
        /// Make a unary predicate
        /// </summary>
        /// <typeparam name="T1">Argument type</typeparam>
        /// <typeparam name="TProp">Type of proposition to return</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, TProp> PredicateOfType<T1,TProp>(string name) where TProp : SpecialProposition, new()
        {
            var problem = Problem.Current;
            return arg1 => problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg1));
        }

        /// <summary>
        /// Make a binary predicate
        /// </summary>
        /// <typeparam name="T1">Type for argument 1</typeparam>
        /// <typeparam name="T2">Type for argument 2</typeparam>
        /// <typeparam name="TProp">Type of proposition to return</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, T2, TProp> PredicateOfType<T1, T2, TProp>(string name)
            where TProp: SpecialProposition, new()
        {
            var problem = Problem.Current;
            return (arg1, arg2) => problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg1, arg2));
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
                    return Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg2, arg1));
                return Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg1, arg2));
            };
        }

        /// <summary>
        /// Make a symmetric predicate
        /// This will enforce that predicate(a, b) == predicate(b, a)
        /// </summary>
        /// <typeparam name="T">Argument type</typeparam>
        /// <typeparam name="TProp">Type of Proposition to create</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        // ReSharper disable once UnusedMember.Global
        public static Func<T, T, TProp> SymmetricPredicateOfType<T, TProp>(string name) 
            where T : IComparable
            where TProp : SpecialProposition, new()
        {
            var problem = Problem.Current;
            return (arg1, arg2) =>
            {
                if (arg1.CompareTo(arg2) > 0)
                    return problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg2, arg1));
                return problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg1, arg2));
            };
        }

        /// <summary>
        /// Make a symmetric predicate with an extra argument at the end (used for symmetric fluents)
        /// This will enforce that predicate(a, b, c) == predicate(b, a, c)
        /// </summary>
        /// <typeparam name="T1">Argument type</typeparam>
        /// <typeparam name="TProp">Type of Proposition to create</typeparam>
        /// <typeparam name="T2">Type of the last argument</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, T1, T2, TProp> SymmetricPredicateOfType<T1, T2, TProp>(string name)
            where T1 : IComparable
            where TProp : SpecialProposition, new()
        {
            var problem = Problem.Current;
            return (arg1, arg2, arg3) =>
            {
                if (arg1.CompareTo(arg2) > 0)
                    return problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg2, arg1, arg3));
                return problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg1, arg2, arg3));
            };
        }

        /// <summary>
        /// Make a symmetric, reflexive predicate
        /// This will enforce that predicate(a, b) == predicate(b, a)
        /// </summary>
        /// <typeparam name="T">Argument type</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        // ReSharper disable once UnusedMember.Global
        public static Func<T, T, Proposition> SymmetricTransitiveRelation<T>(string name) where T : IComparable
        {
            return (arg1, arg2) =>
            {
                var diff = arg1.CompareTo(arg2);
                if (diff > 0)
                    return Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg2, arg1));
                if (diff == 0)
                    return Proposition.True;
                // diff < 0
                return Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg1, arg2));
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
            return (arg1, arg2, arg3) => Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg1, arg2, arg3));
        }

        /// <summary>
        /// Make a binary predicate
        /// </summary>
        /// <typeparam name="T1">Type for argument 1</typeparam>
        /// <typeparam name="T2">Type for argument 2</typeparam>
        /// <typeparam name="T3">Type for argument 3</typeparam>
        /// <typeparam name="TProp">Type of proposition to return</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        public static Func<T1, T2, T3, TProp> PredicateOfType<T1, T2, T3, TProp>(string name)
            where TProp : SpecialProposition, new()
        {
            var problem = Problem.Current;
            return (arg1, arg2, arg3) => problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg1, arg2, arg3));
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
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, T2, T3, T4, Proposition> Predicate<T1, T2, T3, T4>(string name)
        {
            return (arg1, arg2, arg3, arg4) => Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg1, arg2, arg3, arg4));
        }

        /// <summary>
        /// Make a binary predicate
        /// </summary>
        /// <typeparam name="T1">Type for argument 1</typeparam>
        /// <typeparam name="T2">Type for argument 2</typeparam>
        /// <typeparam name="T3">Type for argument 3</typeparam>
        /// <typeparam name="T4">Type for argument 4</typeparam>
        /// <typeparam name="TProp">Type of proposition to return</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, T2, T3, T4, TProp> PredicateOfType<T1, T2, T3, T4, TProp>(string name)
            where TProp : SpecialProposition, new()
        {
            var problem = Problem.Current;
            return (arg1, arg2, arg3, arg4) => problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg1, arg2, arg3, arg4));
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
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, T2, T3, T4, T5, Proposition> Predicate<T1, T2, T3, T4, T5>(string name)
        {
            return (arg1, arg2, arg3, arg4, arg5) => Proposition.MakeProposition(Call.FromArgs(Problem.Current, name, arg1, arg2, arg3, arg4, arg5));
        }

        /// <summary>
        /// Make a binary predicate
        /// </summary>
        /// <typeparam name="T1">Type for argument 1</typeparam>
        /// <typeparam name="T2">Type for argument 2</typeparam>
        /// <typeparam name="T3">Type for argument 3</typeparam>
        /// <typeparam name="T4">Type for argument 4</typeparam>
        /// <typeparam name="T5">Type for argument 5</typeparam>
        /// <typeparam name="TProp">Type of proposition to return</typeparam>
        /// <param name="name">Name of the predicate</param>
        /// <returns>The predicate object, i.e. a function from arguments to Propositions</returns>
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, T2, T3, T4, T5, TProp> PredicateOfType<T1, T2, T3, T4, T5, TProp>(string name)
            where TProp : SpecialProposition, new()
        {
            var problem = Problem.Current;
            return (arg1, arg2, arg3, arg4, arg5) => problem.GetSpecialProposition<TProp>(Call.FromArgs(Problem.Current, name, arg1, arg2, arg3, arg4, arg5));
        }

        /// <summary>
        /// Make a unary function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, Call> Function<T1>(string name)
        {
            return (arg1) => Call.FromArgs(Problem.Current, name, arg1);
        }

        /// <summary>
        /// Make a binary function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, T2, Call> Function<T1, T2>(string name)
        {
            return (arg1, arg2) => Call.FromArgs(Problem.Current, name, arg1, arg2);
        }

        /// <summary>
        /// Make a 3-argument function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, T2, T3, Call> Function<T1, T2, T3>(string name)
        {
            return (arg1, arg2, arg3) => Call.FromArgs(Problem.Current, name, arg1, arg2, arg3);
        }

        /// <summary>
        /// Make a 4-argument function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, T2, T3, T4, Call> Function<T1, T2, T3, T4>(string name)
        {
            return (arg1, arg2, arg3, arg4) => Call.FromArgs(Problem.Current, name, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Make a 5-argument function in the sense of a term generator
        /// </summary>
        /// <param name="name">Name of the function</param>
        // ReSharper disable once UnusedMember.Global
        public static Func<T1, T2, T3, T4, T5, Call> Function<T1, T2, T3, T4, T5>(string name)
        {
            return (arg1, arg2, arg3, arg4, arg5) => Call.FromArgs(Problem.Current, name, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// Returns a negation for the specified proposition
        /// </summary>
        /// <param name="p">Proposition to negate</param>
        public static Literal Not(Literal p)
        {
            if (p is Negation n)
                return n.Proposition;
            return Negation.Not((Proposition)p);
        }
    }
}
