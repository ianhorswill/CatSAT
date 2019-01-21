using System;
using CatSAT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UtilityTests
    {
        /// <summary>
        /// Make sure the utility stored in the solution is correct
        /// </summary>
        [TestMethod]
        public void SolutionUtilityTest()
        {
            var p = new Problem(nameof(SolutionUtilityTest));
            var a = (Proposition) "a";
            a.Utility = 1;
            var b = (Proposition) "b";
            b.Utility = -1;
            var c = (Proposition) "c";
            c.Utility = 1.5f;
            var d = (Proposition) "d";
            d.Utility = 3.3f;
            var zero = (Proposition) "zeroUtility";
            p.Quantify(1,4,a,b,c,d,zero);

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();

                float U(Proposition prop)
                {
                    if (s[prop])
                        return prop.Utility;
                    return 0;
                }
                Assert.IsTrue(Math.Abs(s.Utility - (U(a)+U(b)+U(c)+U(d)+U(zero))) < 0.0001f);
            }
        }

        /// <summary>
        /// Test that it finds good solutions
        /// </summary>
        [TestMethod]
        public void UtilityMaximizationTest()
        {
            var p = new Problem(nameof(UtilityMaximizationTest));
            var a = (Proposition) "a";
            a.Utility = 1;
            var b = (Proposition) "b";
            b.Utility = -1;
            var c = (Proposition) "c";
            c.Utility = 1.5f;
            var d = (Proposition) "d";
            d.Utility = 3.3f;
            var zero = (Proposition) "zeroUtility";
            p.Quantify(1,4,a,b,c,d,zero);

            for (int i = 0; i < 100; i++)
            {
                var s = p.HighUtilitySolution(1000);
                Console.WriteLine(s.Model);

                Assert.IsTrue(Math.Abs((1+1.5+3.3) - s.Utility) < 0.00001f);
            }
        }
    }
}
