namespace Chess;
using System;

internal static class SpanExtensions
{
    public static SpanSplitEnumerator Split(this ReadOnlySpan<char> span)
    {
        return new SpanSplitEnumerator(span);
    }

    public ref struct SpanSplitEnumerator
    {
        private ReadOnlySpan<char> span;
        private ReadOnlySpan<char> split;

        public SpanSplitEnumerator(ReadOnlySpan<char> span)
        {
            this.span = span;
            this.split = default;
        }

        public ReadOnlySpan<char> Current => this.split;

        public SpanSplitEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (this.span.IsEmpty)
                return false;

            var spacePos = this.span.IndexOf(' ');
            while (spacePos == 0)
            {
                this.span = this.span[1..];
                spacePos = this.span.IndexOf(' ');
            }

            var spaceIdx = spacePos < 0 ? this.span.Length : spacePos;
            this.split = this.span[..spaceIdx];
            this.span = this.span[spaceIdx..];

            return this.split.Length > 0;
        }
    }
}
