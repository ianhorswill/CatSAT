#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoryTests.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;
using static CatSAT.Language;
using static CatSAT.Fluents;
using static CatSAT.Actions;

namespace Tests
{
    [TestClass]
    public class StoryTests
    {
        [TestInitialize]
        public void StartLogging()
        {
            Problem.LogPerformanceDataToConsole = true;
        }

        [TestMethod]
        public void StoryTest()
        {
            var p = new Problem("murder test") { TimeHorizon = 4, Timeout = 50000 };
            var cast = new[] { "Fred", "Betty", "Frieda" };

            // FLUENTS

            // alive(a, t) iff a alive at time t
            var alive = Fluent("alive", cast);
            var married = Fluent("married", cast);

            // Everyone is initially alive an unmarried
            foreach (var c in cast)
                p.Assert(alive(c, 0),
                    Not(married(c, 0)));

            var hates = Fluent("hates", cast, cast);
            var loves = Fluent("loves", cast, cast);
            var marriedTo = SymmetricFluent("marriedTo", cast);

            foreach (var c1 in cast)
                foreach (var c2 in cast)
                    p.Assert(Not(marriedTo(c1, c2, 0)),
                        Not(loves(c1, c2, 0)));

            // Love and hate disable one another
            foreach (var agent in cast)
                foreach (var patient in cast)
                    foreach (var t in ActionTimePoints)
                        p.Assert(Deactivate(hates(agent, patient, t)) <= Activate(loves(agent, patient, t)),
                            Deactivate(loves(agent, patient, t)) <= Activate(hates(agent, patient, t)));
            
            // ACTIONS
            // kill(a,b,t) means a kills b at time t
            var kill = Action("kill", cast, cast);
            
            Precondition(kill, (a, b, t) => alive(b, t));
            Precondition(kill, (a, b, t) => alive(a, t));
            Precondition(kill, (a, b, t) => hates(a, b, t));
            Deletes(kill, (a, b, t) => alive(b, t));

            // fallFor(faller, loveInterest, time)
            var fallFor = Action("fallFor", cast, cast);
            Precondition(fallFor, (f, l, t) => Not(loves(f,l, t)));
            Precondition(fallFor, (f, l, t) => alive(f, t));
            Precondition(fallFor, (f, l, t) => alive(l, t));
            Precondition(fallFor, (f, l, t) => f != l);
            Adds(fallFor, (f, l, t) => loves(f, l, t));

            // marry(a, b, t)
            var marry = SymmetricAction("marry", cast);
            Precondition(marry, (a, b, t) => loves(a, b, t));
            Precondition(marry, (a, b, t) => loves(b, a, t));
            Precondition(marry, (a, b, t) => a != b);
            Precondition(marry, (a, b, t) => alive(a, t));
            Precondition(marry, (a, b, t) => alive(b, t));
            Precondition(marry, (a, b, t) => Not(married(a, t)));
            Precondition(marry, (a, b, t) => Not(married(b, t)));
            Adds(marry, (a, b, t) => marriedTo(a, b, t));
            Adds(marry, (a, b, t) => married(a, t));
            Adds(marry, (a, b, t) => married(b, t));

            // You can't marry or fall in love with yourself
            foreach (var t in ActionTimePoints)
            foreach (var c in cast)
            {
                p.Assert(Not(marry(c, c, t)), Not(fallFor(c, c, t)));
            }

            IEnumerable<ActionInstantiation> PossibleActions(int t)
            {
                return Instances(kill, t).Concat(Instances(fallFor, t)).Concat(Instances(marry, t));
            }

            foreach (var t in ActionTimePoints)
                // Exactly one action per time point
                p.AtMost(1, PossibleActions(t));

            // Tragedy strikes
            //foreach (var c in cast)
            //    p.Assert(Not(alive(c, TimeHorizon-1)));

            //p.Assert(married("Fred", 3));

            p.Optimize();

            Console.WriteLine(p.Stats);

            var s =p.Solve();

            foreach (var t in ActionTimePoints)
            {
                Console.Write($"Time {t}: ");
                foreach (var a in PossibleActions(t))
                    if (s[a])
                        Console.Write($"{a}, ");
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void StoryTellerDemoTest()
        {
            var p = new Problem("Storyteller demo rebuild");
            var cast = new[] {"red", "green", "blue"}; // characters don't have names and gender doesn't matter
            var rich = Predicate<string>("rich");
            var caged = Predicate<string>("caged");
            var hasSword = Predicate<string>("hasSword");
            var evil = Predicate<string>("evil");
            var kill = Predicate<string, string>("kill");
            var loves = Predicate<string, string>("loves");
            var dead = Predicate<string>("dead");
            var tombstone = Predicate<string>("tombstone");
            var someoneFree = (Proposition) "someoneFree";

            // Panel 1 -> panel 2
            foreach (var x in cast)
            {
                p.Assert(
                    evil(x) == Not(rich(x)),
                    caged(x) > rich(x),
                    hasSword(x) == (rich(x) & Not(caged(x))),
                    someoneFree <= Not(caged(x)),
                    Not(kill(x,x))
                );
                // You can't kill multiple people
                p.AtMost(1, cast, y => kill(x, y));
                foreach (var y in cast)
                    p.Assert(
                        kill(x, y) > hasSword(x),
                        kill(x,y) > evil(y)
                    );
            }

            // Panel 2 -> panel 3
            foreach (var x in cast)
            {
                foreach (var y in cast)
                {
                    p.Assert(
                        dead(y) <= kill(x, y),
                        tombstone(x) <= (caged(x) & evil(y) & Not(dead(y))),
                        tombstone(x) <= (Expression)Not(someoneFree),
                        tombstone(x) <= dead(x)
                    );
                    foreach (var z in cast)
                        p.Assert(loves(x, y) <= (caged(x) & kill(y, z)));
                }
            }

            Console.WriteLine(p.Stats);
            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Console.WriteLine(s.Model);
            }
        }
    }
}
