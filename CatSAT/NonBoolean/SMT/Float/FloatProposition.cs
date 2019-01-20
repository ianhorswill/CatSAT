#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatProposition.cs" company="Ian Horswill">
// Copyright (C) 2018, 2019 Ian Horswill
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

namespace CatSAT.NonBoolean.SMT.Float
{
    class FloatProposition : TheoryProposition
    {
        public override void Initialize(Problem p)
        {
            p.GetSolver<FloatSolver>().Propositions.Add(this);
        }

        /// <summary>
        /// Sanity check the proposition
        /// </summary>
        public virtual void Validate()
        { }

        /// <summary>
        /// Check that other propositions don't depend on this one.
        /// This is needed for propositions for which we can't implement their inverses as constraints.
        /// </summary>
        protected void ValidateNotDependency()
        {
            if (IsDependency)
                throw new InvalidOperationException(
                    $"{Name}: this proposition can't be used to infer other propositions.");
        }
    }
}
