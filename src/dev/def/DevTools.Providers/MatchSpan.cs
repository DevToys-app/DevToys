namespace DevTools.Providers
{
    public struct MatchSpan
    {
        public int StartPosition { get; }

        public int Length { get; }

        public MatchSpan(int startPosition, int length)
        {
            StartPosition = startPosition;
            Length = length;
        }
    }
}
