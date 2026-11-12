using System;
using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NManager;

public class BaseWebManager
{
	private Dictionary<Type, Request> _cachedRequestDic = new Dictionary<Type, Request>();

	protected void Request<T, V>(Action<V> responseAction = null) where T : Request, IServiceRequest where V : Response, new()
	{
		Type typeFromHandle = typeof(T);
		if (!_cachedRequestDic.ContainsKey(typeFromHandle))
		{
			_cachedRequestDic.Add(typeFromHandle, Activator.CreateInstance<T>());
		}
		if (!(_cachedRequestDic[typeFromHandle] is T request))
		{
			return;
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(V response)
		{
			if (CheckBaseResult(response))
			{
				Vespa_System_Action_Extension_Method.fire(responseAction, response);
			}
		});
	}

	private bool CheckBaseResult(Response response)
	{
		if (response == null)
		{
			MessagePopup.OpenError(BaseResultType.Fail);
			return false;
		}
		if (response.BaseResult != BaseResultType.Success)
		{
			MessagePopup.OpenError(response.BaseResult);
			return false;
		}
		return true;
	}
}
