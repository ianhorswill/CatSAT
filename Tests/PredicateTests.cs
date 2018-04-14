using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicoSAT;
using static PicoSAT.Language;
using static System.Linq.Enumerable;

namespace Tests
{
    [TestClass]
    public class PredicateTests
    {
        [TestMethod]
        public void NRooksTest()
        {
            var n = 8;

            var p = new Problem("N rooks");

            // rook(i, j) means there's a root at row i, column j
            var rook = Predicate<int, int>("rook");

            //
            // Add constraints to the program
            //

            // There should be a total of n rooks
            p.Exactly(n, Range(0, n).SelectMany(i => Range(0, n).Select(j => rook(i, j))));

            // There should be one rook in each row
            foreach (var i in Range(0, n))
                p.Unique(Range(0, n).Select(j => rook(i, j)));

            // There should be one rook in each column
            foreach (var j in Range(0, n))
                p.Unique(Range(0, n).Select(i => rook(i, j)));

            var m = p.Solve();

            //
            // Test the constraints in the solution
            //

            m.Exactly(n, Range(0, n).SelectMany(i => Range(0, n).Select(j => rook(i, j))));
            foreach (var i in Range(0, n))
                m.Unique(Range(0, n).Select(j => rook(i, j)));
            foreach (var j in Range(0, n))
                m.Unique(Range(0, n).Select(i => rook(i, j)));
        }

        /// <summary>
        /// Same as N Rooks, but we don't tell it that there has to be a rook in every row and column.
        /// So it has less information to use to prune the search.
        /// </summary>
        [TestMethod]
        public void NRooksHarderTest()
        {
            var n = 8;

            var p = new Problem("N rooks hard") { MaxTries = 2000 };
            

            // rook(i, j) means there's a root at row i, column j
            var rook = Predicate<int, int>("rook");

            //
            // Add constraints to the program
            //

            // There should be a total of n rooks
            p.Exactly(n, Range(0, n).SelectMany(i => Range(0, n).Select(j => rook(i, j))));

            // There should be at most one rook in each row
            foreach (var i in Range(0, n))
                p.AtMost(1, Range(0, n).Select(j => rook(i, j)));

            // There should be at most one rook in each column
            foreach (var j in Range(0, n))
                p.AtMost(1, Range(0, n).Select(i => rook(i, j)));

            var m = p.Solve();

            //
            // Test the constraints in the solution
            //

            m.Exactly(n, Range(0, n).SelectMany(i => Range(0, n).Select(j => rook(i, j))));
            foreach (var i in Range(0, n))
                m.AtMost(1, Range(0, n).Select(j => rook(i, j)));
            foreach (var j in Range(0, n))
                m.AtMost(1, Range(0, n).Select(i => rook(i, j)));
        }

        [TestMethod]
        public void InductionTest()
        {
            var n = 8;

            var p = new Problem("induction test");
            var pred = Predicate<int>("pred");
            p.Assert(pred(0));
            for (var i = 0; i < n; i++)
                p.Assert(pred(i) == pred(i-1));
            for (int t = 0; t < 100; t++)
            {
                var m = p.Solve();
                Assert.IsTrue(m.All(Range(0, n).Select(i => pred(i))));
            }
        }

        [TestMethod, ExpectedException(typeof(NonTightProblemException))]
        public void NonstrictTest()
        {
            var n = 8;

            var p = new Problem("induction test");
            var pred = Predicate<int>("pred");

            for (var i = 1; i < n - 1; i++)
                // cell set iff neighbor is set.
            {
                p.Assert(
                    pred(i) <= pred(i - 1),
                    pred(i) <= pred(i + 1)
                );
            }

            // Solve it.
            for (int t = 0; t < 100; t++)
            {
                var m = p.Solve();
                // This has 3 solutions: all false, all true, all but one end true, all but either end true.
                var c = m.Count(Range(0, n).Select(pred));
                Assert.IsTrue(c == 0 || c >= n - 2);
            }
        }
        
        [TestMethod]
        public void ConnectedChainTest()
        {
            var n = 8;

            var p = new Problem("connected chain test");
            Func<int, int, Proposition> adjacent = (i, j) => Math.Abs(i - j) < 2;
            var connected = Predicate<int, int>("connected");

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                        p.Assert(connected(i, i));
                    else
                    {
                        // i is connected to j if the node on either side is connected to j and i is adjacent to the neighbor.
                        p.Assert(
                            connected(i, j) <= adjacent(i, i - 1), connected(i - 1, j),
                            connected(i, j) <= adjacent(i, i + 1), connected(i + 1, j)
                        );
                    }
                }
            }

            var m = p.Solve();
            for (int trial = 0; trial < 100; trial++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        Assert.IsTrue(m.IsTrue(connected(i, j)));
                    }
                }
            }
        }
    }
}
