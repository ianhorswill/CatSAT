#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Call.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// Represents a call to a predicate or function with specific arguments.
    /// This gets used as the name of the Proposition that the predicate returns when you call it.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class Call
    {
        /// <summary>
        /// Name of the predicate or other functor being called
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Arguments
        /// </summary>
        public readonly object[] Args;

        // Static buffers used for FromArgs to keep from having to allocate the vararg
        static readonly object[] ArgBuffer1 = new object[1];
        static readonly object[] ArgBuffer2 = new object[2];
        static readonly object[] ArgBuffer3 = new object[3];
        static readonly object[] ArgBuffer4 = new object[4];
        static readonly object[] ArgBuffer5 = new object[5];

        /// <summary>
        /// Find the (unique) Call object with the specified name and args in the specified problem.
        /// </summary>
        /// <param name="p">Problem to get the call for</param>
        /// <param name="name">Name of the predicate being called</param>
        /// <param name="arg1">Argument of the predicate</param>
        public static Call FromArgs(Problem p, string name, object arg1)
        {
            ArgBuffer1[0] = arg1;
            return FromArgArray(p, name, ArgBuffer1);
        }

        /// <summary>
        /// Find the (unique) Call object with the specified name and args in the specified problem.
        /// </summary>
        /// <param name="p">Problem to get the call for</param>
        /// <param name="name">Name of the predicate being called</param>
        /// <param name="arg1">Argument of the predicate</param>
        /// <param name="arg2">Argument of the predicate</param>
        public static Call FromArgs(Problem p, string name, object arg1, object arg2)
        {
            ArgBuffer2[0] = arg1;
            ArgBuffer2[1] = arg2;
            return FromArgArray(p, name, ArgBuffer2);
        }

        /// <summary>
        /// Find the (unique) Call object with the specified name and args in the specified problem.
        /// </summary>
        /// <param name="p">Problem to get the call for</param>
        /// <param name="name">Name of the predicate being called</param>
        /// <param name="arg1">Argument of the predicate</param>
        /// <param name="arg2">Argument of the predicate</param>
        /// <param name="arg3">Argument of the predicate</param>
        public static Call FromArgs(Problem p, string name, object arg1, object arg2, object arg3)
        {
            ArgBuffer3[0] = arg1;
            ArgBuffer3[1] = arg2;
            ArgBuffer3[2] = arg3;
            return FromArgArray(p, name, ArgBuffer3);
        }

        /// <summary>
        /// Find the (unique) Call object with the specified name and args in the specified problem.
        /// </summary>
        /// <param name="p">Problem to get the call for</param>
        /// <param name="name">Name of the predicate being called</param>
        /// <param name="arg1">Argument of the predicate</param>
        /// <param name="arg2">Argument of the predicate</param>
        /// <param name="arg3">Argument of the predicate</param>
        /// <param name="arg4">Argument of the predicate</param>
        public static Call FromArgs(Problem p, string name, object arg1, object arg2, object arg3, object arg4)
        {
            ArgBuffer4[0] = arg1;
            ArgBuffer4[1] = arg2;
            ArgBuffer4[2] = arg3;
            ArgBuffer4[3] = arg4;
            return FromArgArray(p, name, ArgBuffer4);
        }

        /// <summary>
        /// Find the (unique) Call object with the specified name and args in the specified problem.
        /// </summary>
        /// <param name="p">Problem to get the call for</param>
        /// <param name="name">Name of the predicate being called</param>
        /// <param name="arg1">Argument of the predicate</param>
        /// <param name="arg2">Argument of the predicate</param>
        /// <param name="arg3">Argument of the predicate</param>
        /// <param name="arg4">Argument of the predicate</param>
        /// <param name="arg5">Argument of the predicate</param>
        public static Call FromArgs(Problem p, string name, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            ArgBuffer5[0] = arg1;
            ArgBuffer5[1] = arg2;
            ArgBuffer5[2] = arg3;
            ArgBuffer5[3] = arg4;
            ArgBuffer5[4] = arg5;
            return FromArgArray(p, name, ArgBuffer5);
        }

        /// <summary>
        /// Find the (unique) Call object with the specified name and args in the specified problem.
        /// </summary>
        /// <param name="p">Problem to get the call for</param>
        /// <param name="name">Name of the predicate being called</param>
        /// <param name="args">Arguments of the predicate</param>
        public static Call FromArgArray(Problem p, string name, params object[] args)
        {
            return GetTrieRoot(p, name).CallWithArgs(name, args);
        }

        /// <summary>
        /// Return the root node of the trie for all calls to the specified name in the specified problem.
        /// </summary>
        private static TrieNode GetTrieRoot(Problem problem, string name)
        {
            if (problem.CallTries.TryGetValue(name, out var root))
                return root;
            return problem.CallTries[name] = new TrieNode();
        }
        
        /// <summary>
        /// Makes a new "call" object.  This is just an object used to fill in the name field for a proposition that conceptually
        /// represents the truth of some predicate with some specific arguments
        /// </summary>
        /// <param name="name">Name of the predicate or other functor</param>
        /// <param name="args">Arguments</param>
        private Call(string name, object[] args)
        {
            Name = name;
            Args = args;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = Name.GetHashCode();
            foreach (var a in Args)
                hash ^= a.GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is Call c)
            {
                if (Name != c.Name)
                    return false;
                if (Args.Length != c.Args.Length)
                    return false;
                for (int i = 0; i < Args.Length; i++)
                    if (!Args[i].Equals(c.Args[i]))
                        return false;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Name);
            b.Append('(');
            var firstOne = true;
            foreach (var a in Args)
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

        internal class TrieNode
        {
            public Call Call;
            public Dictionary<object, TrieNode> Children;

            internal Call CallWithArgs(string name, object[] args)
            {
                var n = FindNode(args);
                if (n.Call != null)
                    return n.Call;
                // Make a new call object; we clone the array on the assumption that it's one of the static buffers.
                return n.Call = new Call(name, (object[])args.Clone());
            }

            internal TrieNode FindNode(object[] args)
            {
                var node = this;
                foreach (var arg in args)
                    node = node.Lookup(arg);

                return node;
            }

            /// <summary>
            /// Look up one level of the Trie.
            /// Returns the child with specified key, creating one if needed.
            /// </summary>
            /// <param name="key">Next arg to look up</param>
            /// <returns>Child with the specified key</returns>
            private TrieNode Lookup(object key)
            {
                if (Children == null)
                    Children = new Dictionary<object, TrieNode>();
                if (Children.TryGetValue(key, out TrieNode result))
                {
                    return result;
                }
                return Children[key] = new TrieNode();
            }
        }
    }
}
