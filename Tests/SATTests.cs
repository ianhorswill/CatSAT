#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SATTests.cs" company="Ian Horswill">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;
using static CatSAT.Language;

namespace Tests
{
    [TestClass]
    public class SATTests
    {
        [TestMethod]
        public void QuantificationNoConstantFoldingTest()
        {
            var p = new Problem("QuantificationConstantFoldingTest");
            p.Unique("a", "b");
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Assert.IsTrue(s["a"]^s["b"]);
            }
        }

        [TestMethod]
        public void QuantificationConstantFoldingSatisfiableTest()
        {
            var p = new Problem("QuantificationConstantFoldingTest");
            p.Unique(true, false, "a", "b");
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Assert.IsFalse(s["a"] || s["b"]);
            }
        }

        [TestMethod]
        public void QuantificationConstantFoldingSatisfiableTest2()
        {
            var p = new Problem("QuantificationConstantFoldingTest");
            p.AtLeast(2, true, false, "a", "b");
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Assert.IsTrue(s["a"] || s["b"]);
            }
        }

        [TestMethod]
        public void QuantificationConstantFoldingSatisfiableTest3()
        {
            var p = new Problem("QuantificationConstantFoldingTest");
            p.AtLeast(3, true, false, "a", "b");
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Assert.IsTrue(s["a"] && s["b"]);
            }
        }

        [TestMethod, ExpectedException(typeof(ContradictionException))]
        public void QuantificationConstantFoldingTooManyTest()
        {
            var p = new Problem("QuantificationConstantFoldingTest");
            p.Unique(true, true, "a", "b");
        }

        [TestMethod, ExpectedException(typeof(ContradictionException))]
        public void QuantificationConstantFoldingTooFewTest()
        {
            var p = new Problem("QuantificationConstantFoldingTest");
            p.AtLeast(3, false, false, "a", "b");
        }

        [TestMethod]
        public void PositiveSolveTest()
        {
            var p = new Problem("Positive solve test");
            p.AddClause("x", "y");
            p.AddClause("z");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.IsTrue(m.IsTrue("z"));
                Assert.IsTrue(m.IsTrue("x") || m.IsTrue("y"));
            }
        }

        [TestMethod]
        public void UniqueTest()
        {
            var p = new Problem("Unique test");
            p.AddClause(1, 1, "w", "x", "y", "z");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        public void FixedCardinalityTest()
        {
            var p = new Problem("Fixed cardinality test");
            p.AddClause(2, 2, "w", "x", "y", "z");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.AreEqual(2, count);
            }
        }

        [TestMethod]
        public void BoundedCardinalityTest()
        {
            var p = new Problem("Bounded cardinality test");
            p.AddClause(1, 3, "w", "x", "y", "z");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.IsTrue(count >= 1 && count <= 3);
            }
        }

        [TestMethod]
        public void MultipleCardinalityTest()
        {
            var p = new Problem("Multiple cardinality test");
            p.AddClause(2, 2, "w", "x", "y", "z");
            p.AddClause(1, 1, "x", "y");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.AreEqual(2, count);
                Assert.IsTrue(m.IsTrue("x") ^ m.IsTrue("y"));
            }
        }

        [TestMethod]
        public void HeavilyConstrainedMultipleCardinalityTest()
        {
            var p = new Problem("Heavily constrained multiple cardinality test");
            p.AddClause(2, 2, "w", "x", "y", "z");
            p.AddClause(1, 1, "x", "y");
            p.AddClause("w");
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                var count = 0;
                if (m.IsTrue("w")) count++;
                if (m.IsTrue("x")) count++;
                if (m.IsTrue("y")) count++;
                if (m.IsTrue("z")) count++;

                Assert.AreEqual(2, count);
                Assert.IsTrue(m.IsTrue("x") ^ m.IsTrue("y"));
                Assert.IsTrue(m.IsTrue("w"));
            }
        }

        [TestMethod, ExpectedException(typeof(TimeoutException))]
        public void UnsatisfiableTest()
        {
            var p = new Problem("Unsatisfiable test");
            p.AddClause(2, 2, "w", "x", "y", "z");
            p.AddClause(1, 1, "x", "y");
            p.AddClause("w");
            p.AddClause("z");
            p.Solve();
            Assert.Fail();
        }

        [TestMethod]
        public void NegativeSolveTest()
        {
            var p = new Problem("Negative solve test");
            var positiveSolutionCount = 0;
            var negativeSolutionCount = 0;
            p.AddClause("x", Not("y"));  // x -> y
            p.AddClause(Not("x"), "y");  // y -> x
            // This should only have two models: both true or both false.
            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                Assert.IsTrue(m.IsTrue("x") == m.IsTrue("y"));
                if (m.IsTrue("x"))
                    positiveSolutionCount++;
                else
                    negativeSolutionCount++;
            }
            Assert.IsTrue(positiveSolutionCount > 0, "Didn't generate any positive solutions!");
            Assert.IsTrue(negativeSolutionCount > 0, "Didn't generate any negative solutions!");
        }

        [TestMethod]
        public void ManualFluentTest()
        {
            var p = new Problem("Manual fluent test");
            var after = (Proposition) "after";
            var before = (Proposition) "before";
            var activate= (Proposition) "activate";
            var deactivate = (Proposition)"deactivate";

            // activate => after
            p.AddClause(after, Not(activate));
            // deactivate => not after
            p.AddClause(Not(after), Not(deactivate));
            // before => after | deactivate
            p.AddClause(Not(before), after, deactivate);
            // not before => not after | activate
            p.AddClause(before, Not(after), activate);
            // Can't simultaneously activate and deactivate
            p.AddClause(0, 1, activate, deactivate);

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                if (s[after])
                {
                    Assert.IsTrue(s[before] || s[activate]);
                    Assert.IsFalse(s[deactivate]);
                }
                else
                {
                    Assert.IsTrue(!s[before] || s[deactivate]);
                    Assert.IsFalse(s[activate]);
                }
            }
        }
    }
}

