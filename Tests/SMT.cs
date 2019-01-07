#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SMT.cs" company="Ian Horswill">
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
using CatSAT.NonBoolean.SMT.Float;
using static CatSAT.Language;
using Random = CatSAT.Random;

namespace Tests
{
    [TestClass]
    public class SMT
    {
        [TestMethod]
        public void FloatTest()
        {
            var p = new Problem(nameof(FloatTest));
            var dom = new FloatDomain("unit", 0, 1);
            var x = (FloatVariable) dom.Instantiate("x");
            var y = (FloatVariable) dom.Instantiate("y");
            
            var a = (Proposition) "a";
            var b = (Proposition) "b";
            var c = (Proposition) "c";
            var d = (Proposition) "d";
            var e = (Proposition) "e";
            p.Assert((x > .2f) <= a);
            p.Assert((x > .3f) <= b);
            p.Assert((x < .5f) <= c);
            p.Assert((x < .8f) <= d);
            p.Assert((x == y) <= e);
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Console.WriteLine(s.Model);
                var xVal = x.Value(s);
                var yVal = y.Value(s);
                if (s[a])
                    Assert.IsTrue(xVal >= .2f);
                if (s[b])
                    Assert.IsTrue(xVal >= .3f);
                if (s[c])
                    Assert.IsTrue(xVal <= .5f);
                if (s[d])
                    Assert.IsTrue(xVal <= .8f);
                if (s[e])
                    Assert.AreEqual(xVal, yVal);
            }
        }

        [TestMethod]
        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public void PSMTNPCStatTest()
        {
            var p = new Problem("NPC stats");
            var str = new FloatVariable("str", 0, 10);
            var con = new FloatVariable("con", 0, 10);
            var dex = new FloatVariable("dex", 0, 10);
            var intel = new FloatVariable("int", 0, 10);
            var wis = new FloatVariable("wis", 0, 10);
            var charisma = new FloatVariable("char", 0, 10);
            var classDomain = new FDomain<string>("character class", 
                "fighter", "magic user", "cleric", "thief");
            var cClass = classDomain.Instantiate("class");
            p.Assert(
                (str > intel) <= (cClass == "fighter"),
                (str > 5) <= (cClass == "fighter"),
                (con > 5) <= (cClass == "fighter"),
                (intel < 8) <= (cClass == "fighter"),
                (str < intel) <= (cClass == "magic user"),
                (intel > 5) <= (cClass == "magic user"),
                (str < 8) <= (cClass == "magic user"),
                (wis > 5) <= (cClass == "cleric"),
                (con < wis) <= (cClass == "cleric"),
                (dex > 5) <= (cClass == "thief"),
                (charisma > 5) <= (cClass == "thief"),
                (wis < 5) <= (cClass == "thief"),
                (dex > str) <= (cClass == "thief"),
                (charisma > intel) <= (cClass == "thief")
            );
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                if (s[(cClass == "fighter")])
                    Assert.IsTrue(str.Value(s) >= intel.Value(s));
                if (s[(cClass == "fighter")])
                    Assert.IsTrue(str.Value(s) >=5);
                if (s[(cClass == "fighter")])
                    Assert.IsTrue(con.Value(s) >= 5);
                if (s[(cClass == "fighter")])
                    Assert.IsTrue(intel.Value(s) <= 8);

                if (s[(cClass == "magic user")])
                    Assert.IsTrue(str.Value(s) <= intel.Value(s));
                if (s[(cClass == "magic user")])
                    Assert.IsTrue(intel.Value(s) >= 5);
                if (s[(cClass == "magic user")])
                    Assert.IsTrue(str.Value(s) <= 8);

                if (s[(cClass == "cleric")])
                    Assert.IsTrue(wis.Value(s) >= 5);
                if (s[(cClass == "cleric")])
                    Assert.IsTrue(con.Value(s) <= wis.Value(s));

                if (s[(cClass == "thief")])
                    Assert.IsTrue(dex.Value(s) >= 5);
                if (s[(cClass == "thief")])
                    Assert.IsTrue(charisma.Value(s) >= 5);
                if (s[(cClass == "thief")])
                    Assert.IsTrue(wis.Value(s) <= 5);
                if (s[(cClass == "thief")])
                    Assert.IsTrue(dex.Value(s) >= str.Value(s));
                if (s[(cClass == "thief")])
                    Assert.IsTrue(charisma.Value(s) >= intel.Value(s));

                Console.WriteLine(s.Model);
            }
        }

        [TestMethod]
        public void SumConstraintTest()
        {
            var p = new Problem(nameof(SumConstraintTest));
            var dom = new FloatDomain("unit", 0, 1);
            var x = (FloatVariable) dom.Instantiate("x");
            var y = (FloatVariable) dom.Instantiate("y");
            //p.Assert("foo");
            var sum = x + y;

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Console.WriteLine(s.Model);
                Assert.IsTrue(Math.Abs(sum.Value(s) - (x.Value(s)+y.Value(s))) < 0.00001f);
            }
        }

        [TestMethod]
        public void ConstantProductConstraintTest()
        {
            for (int j = 0; j < 100; j++)
            {
                var p = new Problem(nameof(ConstantProductConstraintTest));
                var dom = new FloatDomain("signed unit", -1, 1);
                var x = (FloatVariable) dom.Instantiate("x");
                var c = Random.Float(-100, 100);

                var product = c * x;

                for (int i = 0; i < 100; i++)
                {
                    var s = p.Solve();
                    Console.WriteLine(s.Model);
                    Assert.IsTrue(Math.Abs(product.Value(s) / (x.Value(s)) - c) < 0.0001f);
                }
            }
        }

        [TestMethod]
        public void UnsignedProductTest()
        {
            var p = new Problem(nameof(UnsignedProductTest));
            var dom = new FloatDomain("unit", 0, 1);
            var x = (FloatVariable) dom.Instantiate("x");
            var y = (FloatVariable) dom.Instantiate("y");
            //p.Assert("foo");
            var product = x * y;

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Console.WriteLine(s.Model);
                Assert.IsTrue(Math.Abs(product.Value(s) - x.Value(s)*y.Value(s)) < 0.00001f);
            }
        }

        [TestMethod]
        public void SignedProductTest()
        {
            var p = new Problem(nameof(SignedProductTest));
            var dom = new FloatDomain("signed unit", -1, 1);
            var x = (FloatVariable) dom.Instantiate("x");
            var y = (FloatVariable) dom.Instantiate("y");
            //p.Assert("foo");
            var product = x * y;

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Console.WriteLine(s.Model);
                Assert.IsTrue(Math.Abs(product.Value(s) - x.Value(s)*y.Value(s)) < 0.00001f);
            }
        }

        [TestMethod]
        public void NaiveSquareTest()
        {
            var p = new Problem(nameof(NaiveSquareTest));
            var dom = new FloatDomain("signed unit", -1, 1);
            var x = (FloatVariable) dom.Instantiate("x");
            //p.Assert("foo");
            var square = x * x;

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Console.WriteLine(s.Model);
                Assert.IsTrue(Math.Abs(square.Value(s) - x.Value(s)*x.Value(s)) < 0.00001f);
            }
        }

        /// <summary>
        /// Test that FloatVariables that aren't supposed to be defined actually aren't defined.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void UndefinedFloatVarTest()
        {
            var p = new Problem("test");
            var dom = new FloatDomain("signed unit", -1, 1);
            var prop = (Proposition) "prop";
            var x = new FloatVariable("x", dom, prop, p);
            p.Assert(Not(prop));
            Solution s = null;
            for (int i = 0; i < 100; i++)
            {
                s = p.Solve();
                Console.WriteLine(s.Model);
                Assert.IsFalse(s[prop]);
                Assert.IsFalse(x.IsDefinedIn(s));
            }
            Console.WriteLine(x.Value(s));
        }

        /// <summary>
        /// Check that if a defined var is equated to an undefined var, the equation has no effect
        /// </summary>
        [TestMethod]
        public void UndefinedFloatVarEquationHasNoEffectTest()
        {
            var p = new Problem("test");
            var dom = new FloatDomain("signed unit", -1, 1);
            var prop = (Proposition) "prop";
            var x = new FloatVariable("x", dom, prop, p);
            var y = (FloatVariable) dom.Instantiate("y");
            p.Assert(Not(prop));
            p.Assert(x == y);

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Assert.IsFalse(x.IsDefinedIn(s));
                Assert.IsTrue(y.IsDefinedIn(s));
                Assert.IsFalse(ReferenceEquals(x.Representative, y.Representative));
            }
        }

        /// <summary>
        /// Check that if a defined var is bounded by an undefined var, the bound has no effect
        /// </summary>
        [TestMethod]
        public void UndefinedFloatVarBoundHasNoEffectTest()
        {
            var p = new Problem("test");
            var dom = new FloatDomain("signed unit", -1, 1);
            var prop = (Proposition) "prop";
            var x = new FloatVariable("x", dom, prop, p);
            var y = (FloatVariable) dom.Instantiate("y");
            p.Assert(Not(prop));
            p.Assert(x > y);

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Assert.IsFalse(x.IsDefinedIn(s));
                Assert.IsTrue(y.IsDefinedIn(s));
                Assert.IsTrue(ReferenceEquals(y, y.Representative));
                Assert.IsTrue(y.UpperVariableBounds == null);
            }
        }

        /// <summary>
        /// Check that if a defined var is functionally constrained by an undefined var, the constraint has no effect
        /// </summary>
        [TestMethod]
        public void UndefinedFloatVarFunctionalConstraintHasNoEffectTest()
        {
            var p = new Problem("test");
            var dom = new FloatDomain("signed unit", -1, 1);
            var prop = (Proposition) "prop";
            var x = new FloatVariable("x", dom, prop, p);
            var y = (FloatVariable) dom.Instantiate("y");
            var sum = x + y;

            Assert.IsTrue(ReferenceEquals(prop, sum.Condition));

            p.Assert(Not(prop));
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Assert.IsFalse(x.IsDefinedIn(s));
                Assert.IsTrue(y.IsDefinedIn(s));
                Assert.IsTrue(ReferenceEquals(y, y.Representative));
                Assert.IsFalse(sum.IsDefinedIn(s));
                Assert.IsTrue(y.ActiveFunctionalConstraints == null);
            }
        }

        /// <summary>
        /// Check that a sum is defined iff its inputs are defined, and that when it is defined it
        /// is in fact the sum.
        /// </summary>
        [TestMethod]
        public void SumConditionTest()
        {
            var prog = new Problem("test");
            var dom = new FloatDomain("signed unit", -1, 1);
            var p = (Proposition) "p";
            var x = (FloatVariable) dom.Instantiate("x", prog, p);
            var q = (Proposition) "q";
            var y = (FloatVariable) dom.Instantiate("y", prog, q);
            var sum = x + y;
            Assert.IsTrue(ReferenceEquals(sum.Condition, prog.Conjunction(p, q)));

            for (int i = 0; i < 100; i++)
            {
                var s = prog.Solve();
                Console.WriteLine(s.Model);
                Assert.AreEqual(sum.IsDefinedIn(s), x.IsDefinedIn(s) & y.IsDefinedIn(s));
                if (sum.IsDefinedIn(s))
                    Assert.IsTrue(Math.Abs(sum.Value(s) - (x.Value(s) + y.Value(s))) < 0.00001f);
            }
        }
    }
}
