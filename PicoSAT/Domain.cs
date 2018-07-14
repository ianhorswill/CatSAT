namespace PicoSAT
{
    /// <summary>
    /// Base class for domains of Variables.
    /// A Domain defines the set of possible values of a Variable.
    /// </summary>
    /// <typeparam name="T">Underlying data type of values</typeparam>
    public abstract class Domain<T> : VariableType
    {
        protected Domain(string name) : base(name)
        { }
    }
}
