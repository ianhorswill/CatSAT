#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LanguageTests.cs" company="Ian Horswill">
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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;
using static CatSAT.Language;

namespace Tests
{
    [TestClass]
    public class LanguageTests
    {
        [TestMethod]
        public void ImplicationTest()
        {
            var p = new Problem("Implication test");
            var s = (Proposition)"s";
            var t = (Proposition)"t";

            p.Assert(
                (t & (Proposition) true) > s,
                t
            );

            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.IsTrue(m.IsTrue(s));
                Assert.IsTrue(m.IsTrue(t));
            }
        }

        [TestMethod]
        public void BiconditionalTest()
        {
            var p = new Problem("Biconditional test");
            var s = (Proposition)"s";
            var t = (Proposition)"t";
            p.Assert(s == t);
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.AreEqual(m.IsTrue(s), m.IsTrue(t));
            }
        }

        [TestMethod]
        public void BiconditionalTest2()
        {
            var p = new Problem("Biconditional test 2");
            var s = (Proposition)"s";
            var t = (Proposition)"t";
            var u = (Proposition) "u";
            p.Assert(s == (t&u));
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.AreEqual(m[s], m[t] && m[u]);
            }
        }

        [TestMethod]
        public void CompletionTest()
        {
            var p = new Problem("Completion test");

            var s = (Proposition)"s";
            var t = (Proposition)"t";
            var u = (Proposition)"u";

            var a = (Proposition)"a";
            var b = (Proposition)"b";

            var c = (Proposition)"c";

            p.Assert(
                s <= (t & u),
                s <= (a & b),
                s <= c
            );

            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.AreEqual(m.IsTrue(s),
                    (m.IsTrue(t) && m.IsTrue("u")) || (m.IsTrue(a) && m.IsTrue(b) || m.IsTrue(c)));
            }
        }

        [TestMethod]
        public void CallEqualTest()
        {
            var p = new Problem();
            Assert.AreEqual(Call.FromArgs(Problem.Current, "foo", 1, 2), Call.FromArgs(Problem.Current, "foo", 1, 2));
        }

        [TestMethod]
        public void CallHashTest()
        {
            var p = new Problem();
            Assert.AreEqual(Call.FromArgs(Problem.Current, "foo", 1, 2).GetHashCode(), Call.FromArgs(Problem.Current, "foo", 1, 2).GetHashCode());
        }

        [TestMethod]
        public void CallNotEqualTest()
        {
            var p = new Problem();
            Assert.AreNotEqual(Call.FromArgs(Problem.Current, "foo", 1, 2), Call.FromArgs(Problem.Current, "bar", 1, 2));
            Assert.AreNotEqual(Call.FromArgs(Problem.Current, "foo", 1, 2), Call.FromArgs(Problem.Current, "foo", 0, 2));
            Assert.AreNotEqual(Call.FromArgs(Problem.Current, "foo", 1, 2), Call.FromArgs(Problem.Current, "foo", 1, 0));
        }

        [TestMethod]
        public void ConstantFoldingTest()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Problem();

            Assert.AreEqual(Proposition.True, (Proposition)true & (Proposition)true);
            Assert.AreEqual(Proposition.False, (Proposition)false & (Proposition)true);
            Assert.AreEqual(Proposition.False, (Proposition)true & (Proposition)false);
            Assert.AreEqual(Proposition.False, (Proposition)false & (Proposition)false);

            var p = (Proposition) "p";

            Assert.AreEqual(p, (Proposition)true & p);
            Assert.AreEqual(Proposition.False, (Proposition)false & p);

            Assert.AreEqual(p, p & (Proposition)true);
            Assert.AreEqual(Proposition.False, p & (Proposition)false);

            Assert.AreEqual(Proposition.False, Not(true));
            Assert.AreEqual(Proposition.True, Not(false));
        }

        [TestMethod]
        public void FalseRuleTest()
        {
            var prog = new Problem("False rule test");
            var p = (Proposition) "p";
            prog.Assert( p <= false );
            prog.Solve();  // Force it to expand rule to completion

            // This should not generate any clauses
            Assert.AreEqual(0, prog.Clauses.Count);
        }

        [TestMethod]
        public void TrueRuleTest()
        {
            var prog = new Problem("True rule test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            prog.Assert(
                p <= true,
                p <= false,
                p <= q
            );
            var s = prog.Solve();  // Force it to expand rule to completion

            // This should have compiled to zero clauses but p should still always be true
            Assert.AreEqual(0, prog.Clauses.Count);
            Assert.IsTrue(s[p]);
        }

        [TestMethod]
        public void IgnoreFalseRuleTest()
        {
            var prog = new Problem("Ignore false rule test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            prog.Assert(
                p <= false,
                p <= q,
                p <= false
            );
            prog.Solve();  // Force it to expand rule to completion

            // This should have compiled to two clauses
            Assert.AreEqual(2, prog.Clauses.Count);
            // First clause should be q => p
            Assert.AreEqual(2, prog.Clauses[0].Disjuncts.Length);
            Assert.AreEqual(1, prog.Clauses[0].Disjuncts[0]);
            Assert.AreEqual(-2, prog.Clauses[0].Disjuncts[1]);
            // Section clause should be p => q
            Assert.AreEqual(2, prog.Clauses[1].Disjuncts.Length);
            Assert.AreEqual(-1, prog.Clauses[1].Disjuncts[0]);
            Assert.AreEqual(2, prog.Clauses[1].Disjuncts[1]);
        }

        [TestMethod]
        public void ContrapositiveTest()
        {
            var prog = new Problem("Contrapositive test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            var r = (Proposition)"r";
            prog.Assert(
                p <= q,
                p <= r,
                Not(p)
            );
            for (int i = 0 ; i < 100; i++)
            {
                var m = prog.Solve();

                // Under completion semantics, only model should be the empty model.
                Assert.IsFalse(m.IsTrue(p));
                Assert.IsFalse(m.IsTrue(q));
                Assert.IsFalse(m.IsTrue(r));
            }
        }

        [TestMethod]
        public void OptimizationTest()
        {
            var prog = new Problem("Optimzer test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            var r = (Proposition)"r";
            var s = (Proposition) "s";
            prog.Assert(
                p <= q,
                p <= r,
                Not(p)
            );
            prog.Optimize();
            Assert.IsTrue(prog.IsAlwaysFalse(p));
            Assert.IsTrue(prog.IsAlwaysFalse(q));
            Assert.IsTrue(prog.IsAlwaysFalse(r));
            Assert.IsFalse(prog.IsConstant(s));
        }

        [TestMethod, ExpectedException(typeof(ContradictionException))]
        public void ContradictionTest()
        {
            var prog = new Problem("Contradiction test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            var r = (Proposition)"r";
            prog.Assert(
                p <= q,
                p <= r,
                Not(p),
                q
            );
            prog.Optimize();
            Assert.Fail();
        }

        [TestMethod]
        public void LowTechCharacterGeneratorTest()
        {
            var prog = new Problem("Character generator");
            prog.Assert("character");

            // Races 
            Partition("character", "human", "electroid", "insectoid");

            // Classes 
            Partition("character", "fighter", "magic user", "cleric", "thief");
            prog.Inconsistent("electroid", "cleric");

            // Nationalities of humans 
            Partition("human", "landia", "placeville", "cityburgh");

            // Religions of clerics 
            Partition("cleric", "monotheist", "pantheist", "lovecraftian", "dawkinsian");

            // Lovecraftianism is outlawed in Landia 
            prog.Inconsistent("landia", "lovecraftian");

            // Insectoids believe in strict hierarchies 
            prog.Inconsistent("insectoid", "pantheist");

            // Lovecraftianism is the state religion of cityburgh 
            prog.Inconsistent("cityburgh", "cleric", Not("lovecraftian"));

            for (int i = 0; i < 100; i++)
                Console.WriteLine(prog.Solve().Model);
        }

        class CharacterObject
        {
#pragma warning disable 649
            // ReSharper disable InconsistentNaming
            public string cclass;
            public string race;
            public string nationality;
            public string religion;
            // ReSharper restore InconsistentNaming
#pragma warning restore 649
        }

        [TestMethod]
        public void CharacterGeneratorTest()
        {
            var prog = new Problem("Character generator");
            var race = new FDVariable<string>("race",
                                              "human", "electroid", "insectoid");
            var cclass = new FDVariable<string>("cclass",
                                                "fighter", "magic user", "cleric", "thief");
            // Electroids are atheists
            prog.Inconsistent(race == "electroid", cclass == "cleric");

            // Nationalities of humans
            var nationality = new FDVariable<string>("nationality", race == "human",
                                                     "landia", "placeville", "cityburgh");
            // Religions of clerics
            var religion = new FDVariable<string>("religion", cclass == "cleric",
                                                  "monotheist", "pantheist", "lovecraftian", "dawkinsian");
            // Lovecraftianism is outlawed in Landia
            prog.Inconsistent(nationality == "landia", religion == "lovecraftian");
            // Insectoids believe in strict hierarchies
            prog.Inconsistent(race == "insectoid", religion == "pantheist");
            // Lovecraftianism is the state religion of cityburgh
            prog.Inconsistent(nationality == "cityburgh", cclass == "cleric", Not(religion == "lovecraftian"));

            for (int i = 0; i < 100; i++)
            {
                var solution = prog.Solve();
                var characterObject = new CharacterObject();

                string Value(FDVariable<string> v)
                {
                    return v.IsDefinedIn(solution) ? v.Value(solution) : null;
                }
                solution.Populate(characterObject);
                Assert.AreEqual(Value(race), characterObject.race);
                Assert.AreEqual(Value(cclass), characterObject.cclass);
                Assert.AreEqual(Value(nationality), characterObject.nationality);
                Assert.AreEqual(Value(religion), characterObject.religion);
                Console.WriteLine(solution.Model);
            }
        }

        [TestMethod]
        public void StructCharacterGeneratorTest()
        {
            var prog = new Problem("Struct character generator");
            var characterType = new Struct("Character",
                new []{
                    new Member("race", null, "human", "electroid", "insectoid"),
                    new Member("class", null, "fighter", "magic user", "cleric", "thief"),
                    new Member("nationality", "race=human", "landia", "placeville", "cityburgh"),
                    new Member("religion", "class=cleric", "monotheist", "pantheist", "lovecraftian", "dawkinsian")
                },
                (p, v) =>
                {
                    // Electroids are atheists
                    p.Inconsistent(v["race"] == "electroid", v["class"] == "cleric");
                    // Lovecraftianism is outlawed in Landia
                    p.Inconsistent(v["nationality"] == "landia", v["religion"] == "lovecraftian");
                    // Insectoids believe in strict hierarchies
                    p.Inconsistent(v["race"] == "insectoid", v["religion"] == "pantheist");
                    // Lovecraftianism is the state religion of cityburgh
                    p.Inconsistent(v["nationality"] == "cityburgh", v["class"] == "cleric", v["religion"] != "lovecraftian");
                });

            prog.Instantiate("character", characterType);
            for (int i = 0; i < 100; i++)
                Console.WriteLine(prog.Solve().Model);
        }

        class Character : CompiledStruct
        {
            public Character(object name, Problem p, Literal condition = null) : base(name, p, condition)
            {
                // Electroids are atheists
                p.Inconsistent(Race == "electroid", Class == "cleric");
                // Lovecraftianism is outlawed in Landia
                p.Inconsistent(Nationality == "landia", Religion == "lovecraftian");
                // Insectoids believe in strict hierarchies
                p.Inconsistent(Race == "insectoid", Religion == "pantheist");
                // Lovecraftianism is the state religion of cityburgh
                p.Inconsistent(Nationality == "cityburgh", Class == "cleric", Religion != "lovecraftian");
            }

            [Domain("race", "human", "electroid", "insectoid")]
            // ReSharper disable MemberCanBePrivate.Local
#pragma warning disable 649
            public readonly FDVariable<string> Race;

            [Domain("class", "fighter", "magic user", "cleric", "thief")]
            public readonly FDVariable<string> Class;

            [Domain("nationality", "landia", "placeville", "cityburgh"), Condition("Race", "human")]
            public readonly FDVariable<string> Nationality;

            [Domain("religion", "monotheist", "pantheist", "lovecraftian", "dawkinsian"), Condition("Class", "cleric")]
            public readonly FDVariable<string> Religion;
#pragma warning restore 649
            // ReSharper restore MemberCanBePrivate.Local
        }

        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedMember.Local
        enum Races
        {
            Human,
            Electroid,
            Insectoid
        }

        enum Classes
        {
            Fighter,
            MagicUser,
            Cleric,
            Thief
        }

        enum Nationalities
        {
            Landia,
            Placeville,
            Cityburgh
        }

        enum Religions
        {
            Monotheist,
            Pantheist,
            Lovecraftian,
            Dawkinsian
        }
        // ReSharper restore UnusedMember.Local
        // ReSharper restore UnusedMember.Global


        class CharacterWithEnums : CompiledStruct
        {
            public CharacterWithEnums(object name, Problem p, Literal condition = null) : base(name, p, condition)
            {
                // Electroids are atheists
                p.Inconsistent(Race == Races.Electroid, Class == Classes.Cleric);
                // Lovecraftianism is outlawed in Landia
                p.Inconsistent(Nationality == Nationalities.Landia, Religion == Religions.Lovecraftian);
                // Insectoids believe in strict hierarchies
                p.Inconsistent(Race == Races.Insectoid, Religion == Religions.Pantheist);
                // Lovecraftianism is the state religion of cityburgh
                p.Inconsistent(Nationality == Nationalities.Cityburgh, Class == Classes.Cleric, Religion != Religions.Lovecraftian);
            }
            
            // ReSharper disable MemberCanBePrivate.Local
#pragma warning disable 649
            public readonly EnumVariable<Races> Race;
            public readonly EnumVariable<Classes> Class;

            [Condition("Race", Races.Human)]
            public readonly EnumVariable<Nationalities> Nationality;

            [Condition("Class", Classes.Cleric)]
            public readonly EnumVariable<Religions> Religion;
#pragma warning restore 649
            // ReSharper restore MemberCanBePrivate.Local
        }

        [TestMethod]
        public void CompileStructWithEnumsCharacterGeneratorTest()
        {
            var d = new CompiledStructType(typeof(CharacterWithEnums));
            var prog = new Problem("compiled struct with enums character generator");
            prog.Instantiate("character", d);
            for (int i = 0; i < 100; i++)
                Console.WriteLine(prog.Solve().Model);
        }

        [TestMethod]
        public void CompileStructCharacterGeneratorTest()
        {
            var d = new CompiledStructType(typeof(Character));
            var prog = new Problem("compiled struct character generator");
            prog.Instantiate("character", d);
            for (int i = 0; i < 100; i++)
                Console.WriteLine(prog.Solve().Model);
        }

        [TestMethod]
        public void CompileStructPartyGeneratorTest()
        {
            var d = new CompiledStructType(typeof(Character));
            var prog = new Problem("compiled struct party generator");
            var party = new[] { "fred", "jenny", "sally" };

            // Make one for each party member
            var partyVars = party.Select(c => (Character)prog.Instantiate(c, d)).ToArray();
            // All the classes have to be different
            prog.AllDifferent(partyVars.Select(c => c.Class));

            for (int i = 0; i < 100; i++)
                Console.WriteLine(prog.Solve().Model);
        }

        [TestMethod]
        public void PartyGeneratorTest()
        {
            var prog = new Problem("Party generator");
            var cast = new[] {"fred", "jenny", "sally"};
            var character = Predicate<string>("character");
            var human = Predicate<string>("human");
            var electroid = Predicate<string>("electroid");
            var insectoid = Predicate<string>("insectoid");
            var fighter = Predicate<string>("fighter");
            var magicUser = Predicate<string>("magic_user");
            var cleric = Predicate<string>("cleric");
            var thief = Predicate<string>("thief");
            var landia = Predicate<string>("landia");
            var placeville = Predicate<string>("placeville");
            var cityburgh = Predicate<string>("cityburgh");
            var monotheist = Predicate<string>("monotheist");
            var pantheist = Predicate<string>("pantheist");
            var lovecraftian = Predicate<string>("lovecraftian");
            var dawkinsian = Predicate<string>("dawkinsian");


            foreach (var who in cast)
            {
                prog.Assert(character(who));
                // Races
                Partition(character(who), human(who), electroid(who), insectoid(who));

                // Classes
                Partition(character(who), fighter(who), magicUser(who), cleric(who), thief(who));
                prog.Inconsistent(electroid(who), cleric(who));

                // Nationalities of humans
                Partition(human(who), landia(who), placeville(who), cityburgh(who));

                // Religions of clerics
                Partition(cleric(who), monotheist(who), pantheist(who), lovecraftian(who), dawkinsian(who));
                // Lovecraftianism is outlawed in Landia
                prog.Inconsistent(landia(who), lovecraftian(who));
                // Insectoids believe in strict hierarchies
                prog.Inconsistent(insectoid(who), pantheist(who));
                // Lovecraftianism is the state religion of cityburgh
                prog.Inconsistent(cityburgh(who), cleric(who), Not(lovecraftian(who)));
            }

            prog.AtMost(1, cast, fighter);
            prog.AtMost(1, cast, magicUser);
            prog.AtMost(1, cast, cleric);
            prog.AtMost(1, cast, thief);


            for (int i = 0; i < 100; i++)
                Console.WriteLine(prog.Solve().Model);
        }

        void Partition(Proposition set, params Literal[] subsets)
        {
            foreach (var subset in subsets)
                Problem.Current.Assert(subset > set);
            Problem.Current.Inconsistent(subsets.Select(Not).Concat(new[]{set}));
            Problem.Current.AtMost(1, subsets);
        }

        [TestMethod]
        public void StructPartyGeneratorTest()
        {
            var prog = new Problem("Struct character generator");
            var party = new[] { "fred", "jenny", "sally" };

            var characterType = new Struct("Character",
                // Members
                new[]{
                    new Member("race", null, "human", "electroid", "insectoid"),
                    new Member("class", null, "fighter", "magic user", "cleric", "thief"),
                    new Member("nationality", "race=human", "landia", "placeville", "cityburgh"),
                    new Member("religion", "class=cleric", "monotheist", "pantheist", "lovecraftian", "dawkinsian")
                },
                // Constraints
                (p, v) =>
                {
                    // Electroids are atheists
                    p.Inconsistent(v["race"] == "electroid", v["class"] == "cleric");
                    // Lovecraftianism is outlawed in Landia
                    p.Inconsistent(v["nationality"] == "landia", v["religion"] == "lovecraftian");
                    // Insectoids believe in strict hierarchies
                    p.Inconsistent(v["race"] == "insectoid", v["religion"] == "pantheist");
                    // Lovecraftianism is the state religion of cityburgh
                    p.Inconsistent(v["nationality"] == "cityburgh", v["class"] == "cleric", v["religion"] != "lovecraftian");
                });

            // Make one for each party member
            var castVars = party.Select(c => (StructVar)prog.Instantiate(c, characterType)).ToArray();
            // All the classes have to be different
            prog.AllDifferent(castVars.Select(c => (FDVariable<string>)c["class"]));
            for (int i = 0; i < 100; i++)
                Console.WriteLine(prog.Solve().Model);
        }

        [TestMethod]
        public void FamilyGeneratorTest()
        {
            var p = new Problem("family generator");

            var FamilySize = 25;
            var generationCount = 5;
            var matriarch = 1;
            var cast = Enumerable.Range(matriarch, FamilySize).ToArray();
            var kids = Enumerable.Range(matriarch+1, FamilySize-1).ToArray();
            var childGenerations = Enumerable.Range(1, generationCount - 1).ToArray();
            var female = Predicate<int>("female");
            var generation = Predicate<int, int>("generation");
            var parent = Predicate<int, int>("parent");
            // Interestingly, this doesn't seem to speed things up.
            //Func<int, int, Proposition> parent = (child, par) =>
            //    child > par ? p.GetProposition(Call.FromArgs(Problem.Current, "parent", child, par)) : false;

            // Make of person # who is person number -who
            int Mate(int who) => -who;

            // Family must be 40-60% female
            p.Quantify((int)(FamilySize*.4), (int)(FamilySize*.6), kids, female);

            // Matriarch is the generation 0 female
            p.Assert(female(matriarch), Not(female(Mate(matriarch))));
            p.Assert(generation(matriarch, 0));
            
            foreach (var child in kids)
            {
                // Everyone has exactly one parent from within the family (and one from outside)
                p.Unique(cast, par => parent(child, par));
                foreach (var par in cast)
                    parent(child, par).InitialProbability = 0;
                // Everyone has a generation number
                p.Unique(childGenerations, g => generation(child, g));
                p.Assert(
                    // Only matriarch and patriarch are generation 0
                    Not(generation(child, 0)),
                    // Heteronormativity
                    female(child) == Not(female(Mate(child))));
                foreach (var par in cast)
                foreach (var g in childGenerations)
                    // Child's generation is one more than parent's generation
                    p.Assert(generation(child, g) <= (parent(child, par) & generation(par, g-1)));
            }
            // Every generation has at least one kid
            foreach (var g in childGenerations)
                p.Exists(kids, k => generation(k, g));
            p.Optimize();

            Console.WriteLine(p.Stats);
            Console.WriteLine(p.PerformanceStatistics);
            Console.WriteLine();

            for (int i = 0; i < 100; i++)
            {
                var s = p.Solve();
                Console.WriteLine(s.PerformanceStatistics);
                void PrintTree(int who, int depth)
                {
                    for (int j = 0; j < depth; j++) Console.Write("    ");
                    Console.WriteLine($"{Gender(who)} {who}: + {Mate(who)}");
                    foreach (var child in kids.Where(k => s[parent(k, who)]))
                        PrintTree(child, depth+1);
                }

                string Gender(int who)
                {
                    return s[female(who)] ? "female" : "male";
                }


                PrintTree(matriarch, 0);
            }
        }
    }
}
