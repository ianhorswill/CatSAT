#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluentTests.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;
using static CatSAT.Language;
using static CatSAT.Fluents;
using static CatSAT.Actions;

namespace Tests
{
    [TestClass]
    public class FluentTests
    {
        [TestInitialize]
        public void StartLogging()
        {
            Problem.LogPerformanceDataToConsole = true;
            Problem.LogFile = "../../../Test timings.csv";
        }

        [TestMethod]
        public void NullaryFluentTest()
        {
            var p = new Problem("Nullary fluent test") { TimeHorizon = 10 };
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
            var p = new Problem("Unary fluent test") { TimeHorizon = 10 };
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
            var p = new Problem("Binary fluent test") { TimeHorizon = 10 };
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
            var kill = Action("kill", cast, cast);

            Precondition(kill, (a, b, t) => alive(b, t));
            Precondition(kill, (a, b, t) => alive(a, t));
            Deletes(kill, (a, b, t) => alive(b, t));

            // AXIOMS
            // No suicide
            foreach (var t in ActionTimePoints)
            foreach (var a in cast)
                p.Assert(Not(kill(a, a, t)));

            // INITIAL CONDITIONS
            // Everyone is initially alive
            foreach (var c in cast)
                p.Assert(alive(c, 0));

            // GOAL
            // At least one character has to die.
            p.AtMost(1, cast, who => alive(who, TimeHorizon-1));

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
