using UnityEngine;

namespace NVespa.NTask.NCustomYieldInstruction;

public class WaitForSeconds : CustomYieldInstruction
{
	private bool _realtime;

	private float _endTime;

	public override bool keepWaiting => NeedKeepWaiting();

	public WaitForSeconds(float duration, bool realtime = false)
	{
		_realtime = realtime;
		if (realtime)
		{
			_endTime = Time.realtimeSinceStartup + duration;
		}
		else
		{
			_endTime = Time.time + duration;
		}
	}

	private bool NeedKeepWaiting()
	{
		if (_realtime)
		{
			return Time.realtimeSinceStartup < _endTime;
		}
		return Time.time < _endTime;
	}
}
