using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicoSAT;
using static PicoSAT.Language;

namespace Tests
{
    [TestClass]
    public class SATTests
    {
        [TestMethod]
        public void PositiveSolveTest()
        {
            var p = new Problem();
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
            var p = new Problem();
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
            var p = new Problem();
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
            var p = new Problem();
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
            var p = new Problem();
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
            var p = new Problem();
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

        [TestMethod]
        public void UnssatisfiableTest()
        {
            var p = new Problem();
            p.AddClause(2, 2, "w", "x", "y", "z");
            p.AddClause(1, 1, "x", "y");
            p.AddClause("w");
            p.AddClause("z");
            Assert.AreEqual(null, p.Solve(false));
        }

        [TestMethod]
        public void NegativeSolveTest()
        {
            var p = new Problem();
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
            var p = new Problem("Fluent test");
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

