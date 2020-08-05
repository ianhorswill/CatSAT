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
    //These tests are used to monitor the change of wp under different cases.
    public class NoiseLevel
    {
        [TestMethod]
        //Test wp in regular constrains
        public void CardinalityConstrainTest()
        {
            var p = new Problem("cardinality");
            var clause = p.AddClause(2, 4, "w", "x", "y", "z");
            p.AddClause(0, 2, "x", "y");
            p.AddClause("w");
            p.AddClause(0, 3, "w", "x", "y");
            p.AddClause(0, 2, "x", "y");
            p.AddClause("w");
            for (int i = 0; i < 10; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;


                Assert.AreEqual("w", p.KeyOf(clause, 0).Name);
            }
        }
        [TestMethod]
        //Test wp in float cases
        public void SumConstrainTest()
        {
            var pp = new Problem("SumConstraintTest");
            var dom = new FloatDomain("unit", 0, 1);
            var a = (FloatVariable)dom.Instantiate("a");
            var b = (FloatVariable)dom.Instantiate("b");
            var sum = a + b;

            for (int i = 0; i < 100; i++)
            {
                var s = pp.Solve();
                Console.WriteLine(s.Model);
                Assert.IsTrue(Math.Abs(sum.Value(s) - (a.Value(s) + b.Value(s))) < 0.00001f);
            }
        }

        // [TestMethod]
        // //Test if noise pushed up with Big unsolvable constrains
        // public void BigConstrainTest()
        // {
        //     var prob = new Problem("bigCardinality");
        //     var clause = prob.AddClause(30, 100, "a", "b", "c", "d", "w", "x", "y", "z");
        //     prob.AddClause(30, 100, "a", "b", "x", "y", "z");
        //     prob.AddClause(30, 100, "e", "f", "c", "d", "g", "h", "y", "z"); ;
        //     
        //     for (int i = 0; i < 10; i++)
        //     {
        //         var m = prob.Solve();
        //         var count = 0;
        //         if (m.IsTrue("w")) count++;
        //         if (m.IsTrue("x")) count++;
        //         if (m.IsTrue("y")) count++;
        //         if (m.IsTrue("z")) count++;
        //
        //         Assert.AreEqual("a", prob.KeyOf(clause, 0).Name);
        //     }
        // }
        /*[TestMethod]
        //Test if noise pushed up with Big unsolvable constrains
        public void BigConstrainTest2()
        {
            var prob = new Problem("bigCardinality2");
            var clause = prob.AddClause(30, 1000, "a", "b", "c","z");
            prob.AddClause(30, 100, "x", "y", "z");
            prob.AddClause(30, 100, "e", "f", "c", "d", "y", "z"); ;

            for (int i = 0; i < 10; i++)
            {
                var m = prob.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.AreEqual("a", prob.KeyOf(clause, 0).Name);
            }
        }*/

        [TestMethod]
        //Test if noise pushed up with Big constrains
        public void BigConstrainTest3()
        {
            var prob = new Problem("bigCardinality3");
            var clause = prob.AddClause(10, 29, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l","m", "n","o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ll", "mm", "nn", "oo");
            prob.AddClause(20, 29, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "p", "q", "r", "s", "t", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "pp", "qq", "rr", "ss", "tt");
            prob.AddClause(10, 29, "a", "b", "c", "d", "e", "f", "g", "h", "i", "t", "u", "v", "w", "x", "y", "z", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "kk", "ll", "mm", "nn", "oo"); 

            for (int i = 0; i < 5; i++)
            {
                var m = prob.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                //Assert.AreEqual(3, count);
                //Assert.IsTrue(m.IsTrue("w"));
                Assert.AreEqual("a", prob.KeyOf(clause, 0).Name);
            }
        }


        [TestMethod]
        //Test if noise pushed up with Big pseudo bool constrains
        public void BigConstrainTest4()
        {
            var prob = new Problem("bigCardinality3");
            var clause = prob.AddClause(1, 1, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ab", "cd", "ac", "ad");
            prob.AddClause(1, 1, "ef", "eg", "hi", "hl", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "kk", "ll", "mm", "nn", "oo", "pp", "qq", "rr", "ss", "tt", "uu", "vv", "ww", "xx", "yy", "zz");
            prob.AddClause(1, 1, "aab", "abb", "ccd", "ddc", "aaa", "bbb", "ccc", "ddd", "eee", "fff", "ggg", "hhh", "iii", "jjj", "kkk", "lll", "mmm", "nnn", "ooo", "ppp", "qqq", "rrr", "sss", "ttt", "uuu", "vvv", "www", "xxx", "yyy", "zzz");
           //prob.AddClause(1, 1, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ab", "cd", "ac", "ad");
            //prob.AddClause(1, 1, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ab", "cd", "ac", "ad");
            for (int i = 0; i < 3; i++)
            {
                var m = prob.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.AreEqual("a", prob.KeyOf(clause, 0).Name);
            }
        }


        [TestMethod]
        //Test wp under regular pseudo bool constrains
        public void BigConstrainTest5()
        {
            var p = new Problem("cardinality");
            var clause = p.AddClause(1, 1, "w", "x", "y", "z");
            p.AddClause("w");
            p.AddClause(1, 1, "a", "b", "c");
            p.AddClause(1, 1, "e", "f");
            p.AddClause(1, 1, "e");
            for (int i = 0; i < 3; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.AreEqual("w", p.KeyOf(clause, 0).Name);
            }
        }
    }


}
