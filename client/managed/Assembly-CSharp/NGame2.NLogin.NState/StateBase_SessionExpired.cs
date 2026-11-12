using System.Collections;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NSocket;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NLocalization;
using NVespa;
using NVespa.NSingleton;
using UnityEngine.SceneManagement;

namespace NGame2.NLogin.NState;

public class StateBase_SessionExpired : StateBase
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
	}

	public override IEnumerator coRun()
	{
		if (LoginManagement.IsNowSignOutProgressed)
		{
			StateOwner.SetNextState(new StateBase_SignOut());
			yield break;
		}
		if (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsMultiplay())
		{
			SingletonAuto<CampaignContext>.instance.SetOnlineBattleCallbackIsBattleEnd(IsBattleEnd: true);
			GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Close();
		}
		GlobalMessagePopup.OpenGlobalError(Account.SESSION_EXPIRED_AND_ENTER_LOGIN, delegate
		{
			AccountManager.Logout();
			if (SceneManager.GetActiveScene().name == "Login")
			{
				StateOwner.SetNextState(new StateBase_WaitTouch());
			}
			else
			{
				SingletonUpdater<LoginManager>.instance.ReturnToLogin();
			}
		});
	}
}
