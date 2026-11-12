using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_WaitTouch : StateBase, SelectLanguage.Listener
{
	private enum WaitTouchState
	{
		Null,
		Logined,
		Logout,
		NewGuest,
		Login,
		GameServerLogin,
		PatchNote,
		ClickedPatchNote
	}

	private string _loginVerificationInfo = string.Empty;

	private WaitTouchState _loginState;

	public override void OnBegin()
	{
		base.OnBegin();
		_ = (object)null;
		_ = 6;
		_ = 1;
		_ = 1;
		Array.Empty<object>();
		StateBase.SetBgMessage(string.Empty, animate: false);
		_loginState = WaitTouchState.Null;
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
		if (_loginState != WaitTouchState.PatchNote && _loginState != WaitTouchState.ClickedPatchNote)
		{
			SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginTouchWait>().Close();
			SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>().Close();
		}
		if (_loginState == WaitTouchState.Logined)
		{
			StateOwner.SetNextState(new StateBase_PatchDownload());
		}
		else if (_loginState == WaitTouchState.PatchNote)
		{
			StateOwner.SetNextState(new StateBase_PatchNote());
		}
		else if (_loginState == WaitTouchState.ClickedPatchNote)
		{
			StateOwner.SetNextState(new StateBase_PatchNote(isClickPopupOpen: true));
		}
		else if (_loginState == WaitTouchState.NewGuest)
		{
			StateOwner.SetNextState(new StateBase_SelectLoginMethod());
		}
		else if (_loginState == WaitTouchState.GameServerLogin)
		{
			StateOwner.SetNextState(new StateBase_LoginGameServer(LoginData.LastLoginUserId, LoginData.LastLoginMethod, SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value));
		}
		else if (_loginState == WaitTouchState.Login)
		{
			StateOwner.SetNextState(new StateBase_Login());
		}
		else if (_loginState == WaitTouchState.Logout)
		{
			StateOwner.SetNextState(new StateBase_Logout(() => new StateBase_WaitTouch()));
		}
		else
		{
			ErrorAndQuit("Error Login Flow.");
		}
	}

	public override IEnumerator coRun()
	{
		bool isComplete = false;
		LoginTouchWait loginTouch = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginTouchWait>();
		Action onTouched = delegate
		{
			if (AccountManager.IsLogin())
			{
				_loginState = WaitTouchState.Logined;
			}
			else
			{
				_loginState = WaitTouchState.NewGuest;
			}
			isComplete = true;
		};
		loginTouch.Open(onTouched);
		LoginAccountInfo instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>();
		instance.Open(delegate
		{
			_loginState = WaitTouchState.Logout;
		}, delegate
		{
			SceneAutoBehaviour<LoginManagement>.instance.GetInstance<SelectLanguage>().Open(this);
		}, delegate(SystemLanguage language)
		{
			PlayBgmInResource playBgmInResource = UnityEngine.Object.FindObjectOfType<PlayBgmInResource>();
			if (playBgmInResource != null)
			{
				playBgmInResource.PlayBgm(language);
			}
		}, delegate
		{
			_loginState = WaitTouchState.ClickedPatchNote;
		}, delegate
		{
			LoginIntroduceHero instance5 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHero>();
			if (instance5 != null)
			{
				instance5.Open(null);
			}
		}, delegate
		{
			PatchValidation instance5 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<PatchValidation>();
			if (instance5 != null)
			{
				instance5.Open(null);
			}
		}, delegate
		{
			MessageOpenCS.OpenGlobal(LocalizationManager.GetStringDirect("PTS_LINK_POPUP_TITLE"), LocalizationManager.GetStringDirect("PTS_LINK_POPUP_DESC"), changedCheckBoxTitle: LocalizationManager.GetStringDirect("PTS_LINK_POPUP_CHECKBOX"), onYes: delegate
			{
				if (!CallManager.OpenAnotherKingsRaidApp(CallManager.PackageSuffixPTS))
				{
					Application.OpenURL(CallManager.MarketURLPTS);
				}
			}, onNo: null, isHideComboBox: true);
		});
		instance.SetMode(LoginAccountInfo.eMenuMode.Normal);
		if (AccountManager.IsLogin() && GlobalAutoBehaviour<AccountManager>.instance.PatchNote != null && GlobalAutoBehaviour<AccountManager>.instance.PatchNote.Index > 0 && LoginData.LastPatchNoteIndex < GlobalAutoBehaviour<AccountManager>.instance.PatchNote.Index)
		{
			LoginData.LastPatchNoteIndex = GlobalAutoBehaviour<AccountManager>.instance.PatchNote.Index;
			LoginData.LastPatchNoteTitle = GlobalAutoBehaviour<AccountManager>.instance.PatchNote.Title;
			LoginData.LastPatchNoteMessage = GlobalAutoBehaviour<AccountManager>.instance.PatchNote.Message;
			LoginData.LastPatchNoteEndTime = GlobalAutoBehaviour<AccountManager>.instance.PatchNote.EndTime;
			_loginState = WaitTouchState.PatchNote;
		}
		while (!isComplete)
		{
			yield return null;
			if (_loginState == WaitTouchState.Null)
			{
				bool flag = false;
				LoginIntroduceHero instance2 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHero>();
				if (instance2 != null && instance2.IsOpen())
				{
					flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
				}
				LoginIntroduceHeroPopup instance3 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHeroPopup>();
				if (instance3 != null && instance3.IsOpen())
				{
					flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
				}
				LoginIntroduceWorldPopup instance4 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceWorldPopup>();
				if (instance4 != null && instance4.IsOpen())
				{
					flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
				}
				if (!loginTouch.IsOpen() && !flag)
				{
					loginTouch.Open(onTouched);
				}
				continue;
			}
			break;
		}
	}

	void SelectLanguage.Listener.OnChangeLanguage(SystemLanguage language)
	{
		if (SingletonAuto<LocalizationManager>.instance.IsValidLanguage(language))
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(GameOption.LANGUAGE_CHANGE_CONFIRM, language);
			MessagePopup.Open(title, message, delegate
			{
				OnEnd();
				SingletonAuto<LocalizationManager>.instance.ChangeLanguage(language);
				SingletonUpdater<LoginManager>.instance.ReturnToLogin(LoadingManager.ELoadingType.Black);
			}, null);
		}
	}
}
