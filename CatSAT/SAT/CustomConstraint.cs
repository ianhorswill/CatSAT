namespace CatSAT.SAT
{
    /// <summary>
    /// A class to represent a constraint with custom fields and properties.
    /// </summary>
    internal abstract class CustomConstraint : Constraint 
    {
        protected CustomConstraint(bool isDisjunction, ushort min, short[] disjuncts, int extraHash) : base(isDisjunction, min, disjuncts, extraHash)
        {
        }
    }
}