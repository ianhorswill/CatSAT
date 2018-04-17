using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicoSAT;
using static PicoSAT.Language;
using static PicoSAT.Fluents;

namespace Tests
{
    [TestClass]
    public class FluentTests
    {
        [TestMethod]
        public void NullaryFluentTest()
        {
            var p = new Problem("Fluent test") { TimeHorizon = 10 };
            var f = Fluent("f", requireActivationSupport: false, requireDeactivationSupport: false);

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                for (int t = 1; t < p.TimeHorizon; t++)
                {
                    var before = f(t - 1);
                    var after = f(t);
                    var activate = Activate(before);
                    var deactivate = Deactivate(before);
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

        [TestMethod]
        public void UnaryFluentTest()
        {
            var domain = new[] { "a", "b", "c" };
            var p = new Problem("Fluent test") { TimeHorizon = 10 };
            var f = Fluent("f", domain, requireActivationSupport: false, requireDeactivationSupport: false);

            foreach (var d in domain)
                for (int i = 0; i < 100; i++)
                {
                    var s = p.Solve();
                    for (int t = 1; t < p.TimeHorizon; t++)
                    {
                        var before = f(d, t - 1);
                        var after = f(d, t);
                        var activate = Activate(before);
                        var deactivate = Deactivate(before);
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

        [TestMethod]
        public void BinaryFluentTest()
        {
            var domain = new[] { "a", "b", "c" };
            var p = new Problem("Fluent test") { TimeHorizon = 10 };
            var f = Fluent("f", domain, domain, requireActivationSupport: false, requireDeactivationSupport: false);

            foreach (var d1 in domain)
                foreach (var d2 in domain)
                    for (int i = 0; i < 100; i++)
                    {
                        var s = p.Solve();
                        for (int t = 1; t < p.TimeHorizon; t++)
                        {
                            var before = f(d1, d2, t - 1);
                            var after = f(d1, d2, t);
                            var activate = Activate(before);
                            var deactivate = Deactivate(before);
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

        [TestMethod]
        public void MurderTest()
        {
            var p = new Problem("murder test") { TimeHorizon = 10 };
            var cast = new[] {"fred", "lefty"};

            // FLUENT
            // alive(a, t) iff a alive at time t
            var alive = Fluent("alive", cast);

            // ACTION
            // kill(a,b,t) means a kills b at time t
            var kill = Predicate<string, string, int>("kill");

            // AXIOMS
            foreach (var t in ActionTimePoints)
            {
                foreach (var a in cast)
                {
                    // Disallow suicide
                    p.Assert(Not(kill(a, a, t)));

                    foreach (var b in cast)
                    {
                        if (a != b)
                            p.Assert(
                                // Preconditions: can't kill unless both parties are alive
                                (Expression)kill(a, b, t) >= alive(b, t),
                                (Expression)kill(a, b, t) >= alive(a, t),
                                // Postcondition
                                Deactivate(alive(b, t)) <= kill(a, b, t)
                            );
                    }
                }
            }

            // INITIAL CONDITIONS
            // Everyone is initially alive
            foreach (var c in cast)
                p.Assert(alive(c, 0));

            // GOAL
            // At least one character has to die.
            p.AtMost(1, cast.Select(who => alive(who, TimeHorizon-1)));

            // SOLVE
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                var murders = new List<Proposition>();

                // Find all the murders
                foreach (var t in ActionTimePoints)
                foreach (var a in cast)
                foreach (var b in cast)
                    if (s[kill(a, b, t)])
                        murders.Add(kill(a, b, t));

                // There should be one, and at most two
                Assert.IsTrue(murders.Count > 0 && murders.Count <= 2);

                // If there are two, then they'd better have been simultaneous.
                if (murders.Count == 2)
                    Assert.IsTrue(Equals(murders[0].Arg(2), murders[1].Arg(2)));
            }
        }
    }
}
