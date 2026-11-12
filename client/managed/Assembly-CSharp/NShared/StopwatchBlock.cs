using System;
using System.Diagnostics;

namespace NShared;

public class StopwatchBlock : IDisposable
{
	private Action<Stopwatch> _onEnd;

	private Stopwatch _stopwatch;

	public StopwatchBlock(Action<Stopwatch> onEnd = null)
	{
		_onEnd = onEnd;
		_stopwatch = new Stopwatch();
		_stopwatch.Start();
	}

	void IDisposable.Dispose()
	{
		_stopwatch.Stop();
		if (_onEnd != null)
		{
			_onEnd(_stopwatch);
		}
	}
}
