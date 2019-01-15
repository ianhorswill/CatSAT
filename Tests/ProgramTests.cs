#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProgramTests.cs" company="Ian Horswill">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;

namespace Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void AntecedentTrackingTest()
        {
            var p = new Problem(nameof(AntecedentTrackingTest));
            var x = (Proposition) "x";
            var y = (Proposition) "y";
            var z = (Proposition) "z";
            var w = (Proposition) "w";
            foreach (var prop in new[] {x, y, z, w})
            {
                Assert.IsFalse(prop.IsImplicationConsequent);
                Assert.IsFalse(prop.IsRuleHead);
                Assert.IsFalse(prop.IsAntecedent);
            }
            p.Assert(w > x);
            Assert.IsTrue(x.IsImplicationConsequent);
            Assert.IsFalse(x.IsRuleHead);
            Assert.IsFalse(x.IsDependency);
            Assert.IsFalse(x.IsAntecedent);
            Assert.IsTrue(w.IsAntecedent);
            Assert.IsFalse(w.IsImplicationConsequent);
            Assert.IsFalse(w.IsRuleHead);
            Assert.IsTrue(w.IsDependency);
            p.Assert(x <= (y & z));
            Assert.IsTrue(x.IsRuleHead);
            Assert.IsTrue(x.IsDependency);
            Assert.IsTrue(y.IsAntecedent);
            Assert.IsFalse(y.IsRuleHead);
            Assert.IsFalse(y.IsImplicationConsequent);
            Assert.IsTrue(y.IsDependency);
            Assert.IsTrue(z.IsAntecedent);
            Assert.IsFalse(z.IsRuleHead);
            Assert.IsFalse(z.IsImplicationConsequent);
            Assert.IsTrue(z.IsDependency);
            var a = (Proposition) "a";
            p.Assert(Language.Not(x) > a);
            Assert.IsTrue(x.IsAntecedent);
            Assert.IsTrue(x.IsImplicationConsequent);
            Assert.IsTrue(x.IsRuleHead);
        }

        [TestMethod]
        public void AddClauseTest()
        {
            var p = new Problem("Add clause test");
            var clause = p.AddClause("x", "y");
            Assert.AreEqual("x", p.KeyOf(clause, 0).Name);
            Assert.AreEqual("y", p.KeyOf(clause, 1));
        }

        [TestMethod]
        public void EmptyProgramTest()
        {
            new Problem("Empty program test").Solve();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ConjunctionTest()
        {
            var prob = new Problem("ConjunctionTest");
            var p = (Proposition) "p";
            var q = (Proposition) "q";
            Assert.IsTrue(ReferenceEquals(p, prob.Conjunction(p, p)));
            var conj = prob.Conjunction(p, q);
            var reverseConj = prob.Conjunction(q, p);
            Assert.IsTrue(ReferenceEquals(conj, reverseConj));
            for (int i = 0; i < 100; i++)
            {
                var s = prob.Solve();
                Console.WriteLine(s.Model);
                Assert.IsTrue(s[conj] == (s[p] & s[q]));
            }
        }
    }
}
