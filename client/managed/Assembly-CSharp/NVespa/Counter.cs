using System.Diagnostics;
using UnityEngine;

namespace NVespa;

public static class Counter
{
	private static int _curChainUpdateCount;

	private static int _curUpdateCount;

	private static int _prevFrameCount;

	public static int ChainUpdateCount { get; private set; }

	public static int UpdateCount { get; private set; }

	[Conditional("VESPADEBUG_COUNTER")]
	public static void AddChainUpdate()
	{
		_curChainUpdateCount++;
	}

	[Conditional("VESPADEBUG_COUNTER")]
	public static void AddUpdate()
	{
		_curUpdateCount++;
	}

	[Conditional("VESPADEBUG_COUNTER")]
	public static void FinalizeCurFrame()
	{
		if (_prevFrameCount != Time.frameCount)
		{
			_prevFrameCount = Time.frameCount;
			ChainUpdateCount = _curChainUpdateCount;
			UpdateCount = _curUpdateCount;
			_curChainUpdateCount = 0;
			_curUpdateCount = 0;
		}
	}
}
