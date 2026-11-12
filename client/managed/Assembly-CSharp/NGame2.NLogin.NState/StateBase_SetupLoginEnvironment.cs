using System.Collections;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_SetupLoginEnvironment : StateBase
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
		StateOwner.SetNextState(new StateBase_QueryHost());
		StateOwner.SetSubState(new StateBase_LoadResource());
	}

	public override IEnumerator coRun()
	{
		yield return new WaitForSeconds(0.1f);
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		requester.Reset();
		requester.SetRequestHeader("X-Vespa-KR-Version", StateBase.ClientConfig.Version);
		requester.SetRequestHeader("X-Vespa-KR-Market", QueryMarketInfo.GetMarketTypeName());
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		LoginBackground instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>();
		instance.HideMessage();
		WaitingPopup.BlockWaitingPopup = true;
		VespaCoroutine.Start(coRunBackground(instance));
		yield return new WaitUntil(() => SceneAutoBehaviour<LoginManagement>.instance.IsBIOncePlayed());
	}

	private IEnumerator coRunBackground(LoginBackground loginBackgroundWindow)
	{
		yield return new WaitUntil(() => LogoScene.IsLogoPlayed);
		if (!SceneAutoBehaviour<LoginManagement>.instance.IsBIOncePlayed())
		{
			yield return new WaitForSeconds(1f);
		}
		loginBackgroundWindow.Open(StateBase.ClientConfig);
		loginBackgroundWindow.ShowMessage(showUnsuitableMsg: false, showGuestLoginMsg: false);
		WaitingPopup.BlockWaitingPopup = false;
		loginBackgroundWindow.PlayEffectPhase();
	}
}
