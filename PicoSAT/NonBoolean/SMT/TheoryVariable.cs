namespace PicoSAT
{
    public abstract class TheoryVariable<T> : DomainVariable<T>
    {
        protected TheoryVariable(object name, Problem problem, Literal condition) : base(name, problem, condition)
        { }
    }
}
