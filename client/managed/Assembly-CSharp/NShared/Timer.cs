namespace NShared;

public class Timer
{
	private long _currentTimeMs;

	public void Set(long currentTimeMs)
	{
		_currentTimeMs = currentTimeMs;
	}

	public void Advance(long advancedTimeMs)
	{
		_currentTimeMs += advancedTimeMs;
	}

	public long GetCurrentTimeMs()
	{
		return _currentTimeMs;
	}
}
