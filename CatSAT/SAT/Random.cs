#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Random.cs" company="Ian Horswill">
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
#define XORSHIFT

using System.Collections.Generic;

namespace CatSAT
{
    /// <summary>
    /// A fast RNG
    /// </summary>
    public static class Random
    {
#if XORSHIFT
        private static uint state = 234923840;

        /// <summary>
        /// Set the seed to a specified value.
        /// </summary>
        public static void SetSeed(uint seed)
        {
            state = seed;
        }

        /// <summary>
        /// Set the seed to the current time (System.DateTime.Now.Ticks)
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static void SetSeed()
        {
            SetSeed((uint)System.DateTime.Now.Ticks);
        }

        /// <summary>
        /// Returns a randon uint.
        /// </summary>
        public static uint Next()
        {
            /* Algorithm "xor" from p. 4 of Marsaglia, "Xorshift RNGs" */
            // Cribbed from Wikipedia
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }
#else
        private static System.Random rand = new System.Random();

        /// <summary>
        /// Return a random integer
        /// </summary>
        /// <returns></returns>
        public static uint Next()
        {
            return (uint)rand.Next();
        }
#endif

        /// <summary>
        /// Return a random integer in [0, max)
        /// </summary>
        public static uint InRange(uint max)
        {
            return Next() % max;
        }

        /// <summary>
        /// Return a random integer in [min, max]
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static uint InRange(uint min, uint max)
        {
            return min + Next() % (max - min + 1);
        }

        /// <summary>
        /// Return a random element of list
        /// </summary>
        public static T RandomElement<T>(this List<T> list)
        {
            return list[(int) InRange((uint) list.Count)];
        }

        /// <summary>
        /// Return a random element of array
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static T RandomElement<T>(this T[] array)
        {
            return array[Next() % array.Length];
        }


        /// <summary>
        /// Returns a random prime number from a list.
        /// </summary>
        public static uint Prime()
        {
            return Primes.RandomElement();
        }

        private static readonly uint[] Primes = new uint[]
        {
            2,
            3,
            5,
            7,
            11,
            13,
            17,
            19,
            23,
            29,
            31,
            37,
            41,
            43,
            47,
            53,
            59,
            61,
            67,
            71,
            73,
            79,
            83,
            89,
            97,
            101,
            103,
            107,
            109,
            113,
            127,
            131,
            137,
            139,
            149,
            151,
            157,
            163,
            167,
            173,
            179,
            181,
            191,
            193,
            197,
            199,
            211,
            223,
            227,
            229,
            233,
            239,
            241,
            251,
            257,
            263,
            269,
            271,
            277,
            281,
            283,
            293,
            307,
            311,
            313,
            317,
            331,
            337,
            347,
            349,
            353,
            359,
            367,
            373,
            379,
            383,
            389,
            397,
            401,
            409,
            419,
            421,
            431,
            433,
            439,
            443,
            449,
            457,
            461,
            463,
            467,
            479,
            487,
            491,
            499,
            503,
            509,
            521,
            523,
            541,
            547,
            557,
            563,
            569,
            571,
            577,
            587,
            593,
            599,
            601,
            607,
            613,
            617,
            619,
            631,
            641,
            643,
            647,
            653,
            659,
            661,
            673,
            677,
            683,
            691,
            701,
            709,
            719,
            727,
            733,
            739,
            743,
            751,
            757,
            761,
            769,
            773,
            787,
            797,
            809,
            811,
            821,
            823,
            827,
            829,
            839,
            853,
            857,
            859,
            863,
            877,
            881,
            883,
            887,
            907,
            911,
            919,
            929,
            937,
            941,
            947,
            953,
            967,
            971,
            977,
            983,
            991,
            997,
            1009,
            1013,
            1019,
            1021,
            1031,
            1033,
            1039,
            1049,
            1051,
            1061,
            1063,
            1069,
            1087,
            1091,
            1093,
            1097,
            1103,
            1109,
            1117,
            1123,
            1129,
            1151,
            1153,
            1163,
            1171,
            1181,
            1187,
            1193,
            1201,
            1213,
            1217,
            1223,
            1229,
            1231,
            1237,
            1249,
            1259,
            1277,
            1279,
            1283,
            1289,
            1291,
            1297,
            1301,
            1303,
            1307,
            1319,
            1321,
            1327,
            1361,
            1367,
            1373,
            1381,
            1399,
            1409,
            1423,
            1427,
            1429,
            1433,
            1439,
            1447,
            1451,
            1453,
            1459,
            1471,
            1481,
            1483,
            1487,
            1489,
            1493,
            1499,
            1511,
            1523,
            1531,
            1543,
            1549,
            1553,
            1559,
            1567,
            1571,
            1579,
            1583,
            1597,
            1601,
            1607,
            1609,
            1613,
            1619,
            1621,
            1627,
            1637,
            1657
        };

        /// <summary>
        /// Returns a random single-precision float in the specified range.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns></returns>
        public static float Float(float min, float max)
        {
            double unitInterval = Next() / ((double) uint.MaxValue);
            return min + (max - min) * (float) unitInterval;
        }
    }
}
