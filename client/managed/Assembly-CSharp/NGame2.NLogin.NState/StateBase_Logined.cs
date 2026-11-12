using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGameAsset;
using NGameAsset.NPatch;
using NShared.FirstLobby;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_Logined : StateBase
{
	public override void OnBegin()
	{
		base.OnBegin();
		StateBase.SetBgMessage("Logined", animate: true);
		StartCoroutine(FirstLobby());
	}

	private IEnumerator FirstLobby()
	{
		if (!PatchDownloader.IsBackgorundDownloading())
		{
			SingletonUpdater<AssetCacheService>.instance.StartService();
		}
		Request request = new Request();
		int retry = 5;
		bool succeeded = false;
		while (retry > 0)
		{
			bool responded = false;
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.SilenceError, delegate(Response response)
			{
				responded = true;
				if (response.Result == ResultType.Success)
				{
					GlobalAutoBehaviour<AccountManager>.instance.HandleFirstLobby(response);
					succeeded = true;
				}
			});
			yield return new WaitUntil(() => responded);
			if (succeeded)
			{
				yield break;
			}
			int num = retry - 1;
			retry = num;
		}
		MessagePopup.OpenError(Account.CANNOT_INITIALIZE_ACCOUNT);
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
	}
}
