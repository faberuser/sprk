using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_QueryMarketInfo : StateBase
{
	public override void OnBegin()
	{
		base.OnBegin();
		StartCoroutine(coEntry());
	}

	public override void OnEnd()
	{
		base.OnEnd();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		StateOwner.SetNextState(new StateBase_Notification());
	}

	public override IEnumerator coRun()
	{
		StateBase.SetBgMessage("Query MarketInfo", animate: true);
		AppServerInfo serverInfo = SingletonUpdater<LoginManager>.instance.ServerInfo;
		if (serverInfo == null)
		{
			yield return new WaitQuitError("ServerInfo is NULL");
		}
		if (!string.IsNullOrEmpty(serverInfo.ShutdownMessage))
		{
			yield return new WaitForSeconds(3f);
			if (!GuiManager.IsShowDebugButton())
			{
				yield return new WaitQuitError(serverInfo.ShutdownMessage);
			}
		}
		// SPRK: honor the server's Https flag. Without a scheme the requester used http://,
		// which Android refuses for remote servers ("Insecure connection not allowed").
		string host = NVespa.NWeb.WebServiceRequester.WithScheme(serverInfo.Host, serverInfo.Https);
		_ = serverInfo.CookieKeyword;
		SingletonUpdater<WebServiceManager>.instance.Requester.SetHost(host);
		bool queried = false;
		QueryMarketInfo queryMarketInfo = new QueryMarketInfo();
		queryMarketInfo.Query(delegate
		{
			queried = true;
		});
		while (!queried)
		{
			yield return null;
		}
		CallManager.MarketURLPTS = queryMarketInfo.Response.PtsMarketUrl;
		bool isWaiting = false;
		if (!queryMarketInfo.Verify())
		{
			isWaiting = true;
		}
		while (isWaiting)
		{
			yield return null;
		}
	}
}
