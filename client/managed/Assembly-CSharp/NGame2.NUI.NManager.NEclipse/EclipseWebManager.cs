using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetEclipseDeck;
using NShared.GetEclipseInfo;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.NEclipse;

public class EclipseWebManager
{
	public void RequestGetEclipsInfo(bool showLoading, Action<NShared.GetEclipseInfo.Request, NShared.GetEclipseInfo.Response> successAction, Action failAction = null)
	{
		NShared.GetEclipseInfo.Request request = new NShared.GetEclipseInfo.Request();
		WebRequestFlag flag = (showLoading ? WebRequestFlag.ShowLoadingGauge : WebRequestFlag.Ping);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, flag, delegate(NShared.GetEclipseInfo.Response response)
		{
			if (response.Result != NShared.GetEclipseInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				if (failAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(failAction);
				}
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				if (failAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(failAction);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.Apply(response.EclipseInfo);
				if (successAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(successAction, request, response);
				}
			}
		});
	}

	public void RequestGetEclipsDeck(Action<NShared.GetEclipseDeck.Request, NShared.GetEclipseDeck.Response> successAction, Action failAction = null)
	{
		NShared.GetEclipseDeck.Request request = new NShared.GetEclipseDeck.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetEclipseDeck.Response response)
		{
			if (response.Result != NShared.GetEclipseDeck.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.Apply(response.DeckResults);
				if (successAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(successAction, request, response);
				}
			}
		});
	}
}
