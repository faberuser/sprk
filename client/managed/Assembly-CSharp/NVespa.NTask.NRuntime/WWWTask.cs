using System;
using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class WWWTask : ITask
{
	private Func<WwwText, ITask> _onEnd;

	private string _url;

	private readonly TaskDebugInfo _debugInfo;

	string ITask.Name
	{
		get
		{
			return _debugInfo.Name;
		}
		set
		{
			_debugInfo.Name = value;
		}
	}

	public WWWTask(string url, Func<WwwText, ITask> onEnd)
	{
		_url = url;
		_onEnd = onEnd;
		_debugInfo = new TaskDebugInfo(this);
	}

	IEnumerator<TaskCurrentData> ITask.Run()
	{
		WwwText www = new WwwText(_url);
		while (www != null && !www.isDone)
		{
			yield return new TaskCurrentData(ETaskResult.Running);
		}
		ITask task = Vespa_System_Action_Extension_Method.fire(_onEnd, www);
		if (task == null)
		{
			yield return new TaskCurrentData(ETaskResult.Success);
			yield break;
		}
		ETaskResult result;
		using (TaskRunner endRunner = new TaskRunner(_debugInfo, task))
		{
			while (endRunner.MoveNextWhileRunning())
			{
				yield return endRunner.Current;
			}
			result = endRunner.SuccessOrFail();
		}
		yield return new TaskCurrentData(result);
	}
}
