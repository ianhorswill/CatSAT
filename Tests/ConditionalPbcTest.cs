#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PseudoSMT.cs" company="Ian Horswill">
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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;
using CatSAT.NonBoolean.SMT.Float;

namespace Tests
{
    [TestClass]
    public class ConditionalPbcTest
    {
        [TestMethod]
        public void ConditionalTest1()
        {
            var p = new Problem("cardinality");
            var clause = p.AddClause(2, 2, "a", "b", "c");
            p.AddClause(1, 1, "a", "c");
            p.AddClause(1, 1, true,"a");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("a")) count++;
                if (m.IsTrue("b")) count++;
                if (m.IsTrue("c")) count++;

                Assert.IsTrue(m.IsTrue("a") ^ m.IsTrue("c"));
                Assert.IsTrue(m.IsTrue("b"));
                Assert.IsFalse(m.IsTrue("c"));
            }
        }

        [TestMethod]
        public void ConditionalTest2()
        {
            var p = new Problem("cardinality");
            var clause = p.AddClause(2, 2, "a", "b", "c");
            p.AddClause(1, 1, false, "a", "c");
            p.AddClause(1, 1, true, "c");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("a")) count++;
                if (m.IsTrue("b")) count++;
                if (m.IsTrue("c")) count++;

                Assert.IsTrue(m.IsTrue("a") ^ m.IsTrue("b"));
            }
        }
        [TestMethod]
        public void ConditionalTest3()
        {
            var p = new Problem("cardinality");
            var clause = p.AddClause(3, 3, "a", "b", "c", "d"); 
            p.AddClause(1, 1, "a", "c");
            p.AddClause(1, 1, "b", "c");
            p.AddClause(1, 1, false, "c");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("a")) count++;
                if (m.IsTrue("b")) count++;
                if (m.IsTrue("c")) count++;
                if (m.IsTrue("d")) count++;

                Assert.IsTrue(m.IsTrue("d"));
            }
        }


        [TestMethod]
        public void ConditionalTest4()
        {
            var p = new Problem("conflicts");
            p.AddClause(3, 3, false,"a", "b", "c", "d");
            p.AddClause(1, 1, "a", "c");
            p.AddClause(1, 1, "b", "c");
            p.AddClause(1, 1, true, "c");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("a")) count++;
                if (m.IsTrue("b")) count++;
                if (m.IsTrue("c")) count++;
                if (m.IsTrue("d")) count++;

                Assert.IsTrue(m.IsTrue("c"));
            }
        }



        [TestMethod]
        public void ConditionalTest5()
        {
            var prob = new Problem("big clauses");
            prob.AddClause(1, 1, true,"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ab", "cd", "ac", "ad");
            var clause = prob.AddClause(1, 1, "ef", "eg", "hi", "hl", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "kk", "ll", "mm", "nn", "oo", "pp", "qq", "rr", "ss", "tt", "uu", "vv", "ww", "xx", "yy", "zz");
            prob.AddClause(1, 1, "aab", "abb", "ccd", "ddc", "aaa", "bbb", "ccc", "ddd", "eee", "fff", "ggg", "hhh", "iii", "jjj", "kkk", "lll", "mmm", "nnn", "ooo", "ppp", "qqq", "rrr", "sss", "ttt", "uuu", "vvv", "www", "xxx", "yyy", "zzz");
            prob.AddClause(1, 1, true,"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ab", "cd", "ac", "ad");
            prob.AddClause(1, 1, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ab", "cd", "ac", "ad");
            for (int i = 0; i < 3; i++)
            {
                var m = prob.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.AreEqual("ef", prob.KeyOf(clause, 0).Name);
            }
        }
    }
}
