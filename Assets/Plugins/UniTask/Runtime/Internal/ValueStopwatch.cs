using System;
using System.Diagnostics;

namespace Cysharp.Threading.Tasks.Internal
{
	readonly struct ValueStopwatch
	{
		static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

		readonly long startTimestamp;

		public static ValueStopwatch StartNew()
		{
			return new ValueStopwatch(Stopwatch.GetTimestamp());
		}

		ValueStopwatch(long startTimestamp)
		{
			this.startTimestamp = startTimestamp;
		}

		public TimeSpan Elapsed
		{
			get { return TimeSpan.FromTicks(ElapsedTicks); }
		}

		public bool IsInvalid
		{
			get { return startTimestamp == 0; }
		}

		public long ElapsedTicks
		{
			get
			{
				if (startTimestamp == 0)
				{
					throw new InvalidOperationException("Detected invalid initialization(use 'default'), only to create from StartNew().");
				}

				long delta = Stopwatch.GetTimestamp() - startTimestamp;
				return (long)(delta * TimestampToTicks);
			}
		}
	}
}