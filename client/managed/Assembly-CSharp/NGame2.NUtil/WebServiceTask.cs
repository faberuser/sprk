using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NTask;

namespace NGame2.NUtil;

public class WebServiceTask<TRequest, TResponse> : ITask where TRequest : Request, IServiceRequest where TResponse : Response, new()
{
	private TRequest _request;

	private TResponse _response;

	private Func<TRequest, TResponse, ITask> _onResponse;

	private ITask _onBaseError;

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

	public WebServiceTask(TRequest request, Func<TRequest, TResponse, ITask> onResponse, ITask onBaseError = null)
	{
		_request = request;
		_onResponse = onResponse;
		_onBaseError = onBaseError;
		_debugInfo = new TaskDebugInfo(this);
	}

	IEnumerator<TaskCurrentData> ITask.Run()
	{
		bool isComplete = false;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(_request, delegate(TResponse response)
		{
			isComplete = true;
			_response = response;
		});
		while (!isComplete)
		{
			yield return TaskCurrentData.RUNNING;
		}
		if (_response == null)
		{
			yield return TaskCurrentData.FAIL;
			yield break;
		}
		if (_response.BaseResult != BaseResultType.Success)
		{
			if (_onBaseError != null)
			{
				using TaskRunner baseErrorRunner = new TaskRunner(_debugInfo, _onBaseError);
				while (baseErrorRunner.MoveNextWhileRunning())
				{
					yield return baseErrorRunner.Current;
				}
				baseErrorRunner.SuccessOrFail();
			}
			yield return TaskCurrentData.FAIL;
			yield break;
		}
		ITask task = Vespa_System_Action_Extension_Method.fire(_onResponse, _request, _response);
		if (task == null)
		{
			yield return TaskCurrentData.SUCCESS;
			yield break;
		}
		ETaskResult result;
		using (TaskRunner baseErrorRunner = new TaskRunner(_debugInfo, task))
		{
			while (baseErrorRunner.MoveNextWhileRunning())
			{
				yield return baseErrorRunner.Current;
			}
			result = baseErrorRunner.SuccessOrFail();
		}
		yield return new TaskCurrentData(result);
	}
}
