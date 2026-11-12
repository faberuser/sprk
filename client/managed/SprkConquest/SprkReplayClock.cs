using System;

public sealed class SprkReplayClock
{
	private readonly int bufferMs;

	private long latest = -1L;

	public SprkReplayClock(int bufferMs)
	{
		this.bufferMs = Math.Max(0, Math.Min(200, bufferMs));
	}

	public bool Accept(ref long time)
	{
		lock (this)
		{
			if (time < 0 || time <= latest)
			{
				return false;
			}
			latest = time;
			time = Math.Max(0L, time - bufferMs);
			return true;
		}
	}
}
