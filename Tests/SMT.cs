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
    }
}
