namespace PicoSAT
{
    public struct Interval
    {
        public Interval(float lowerBound, float upperBound)
        {
            Lower = lowerBound;
            Upper = upperBound;
        }

        public float Lower;
        public float Upper;

        public bool IsEmpty => Upper < Lower;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public bool IsUnique => Upper == Lower;

        public void BoundAbove(float upper)
        {
            if (upper < Upper)
                Upper = upper;
        }

        public void BoundBelow(float lower)
        {
            if (lower > Lower)
                Lower = lower;
        }
    }
}
