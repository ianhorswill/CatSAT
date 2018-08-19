using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicoSAT;
using PicoSAT.NonBoolean.SMT.Float;

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
            var v = (FloatVariable) dom.Instantiate("x");
            
            var a = (Proposition) "a";
            var b = (Proposition) "b";
            var c = (Proposition) "c";
            var d = (Proposition) "d";
            p.Assert((Expression) a >= (v > .2f));
            p.Assert((Expression) b >= (v > .3f));
            p.Assert((Expression) c >= (v < .5f));
            p.Assert((Expression) d >= (v < .8f));
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Console.WriteLine(s.Model);
                var xVal = v.Value(s);
                if (s[a])
                    Assert.IsTrue(xVal>=.2f);
                if (s[b])
                    Assert.IsTrue(xVal > .3f);
                if (s[c])
                    Assert.IsTrue(xVal < .5f);
                if (s[d])
                    Assert.IsTrue(xVal < .8f);
            }
        }
    }
}
