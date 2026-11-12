using System;
using System.Text;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NLogin.NState;
using NGame2.NUI.NManager;
using NGameAsset;
using NGameAsset.NPatch;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginAccountInfo : BaseWindow
{
	public enum eMenuMode
	{
		Normal,
		PatchDownloading
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginAccountInfo";

	public UILabel Label_ServerName;

	public UILabel Label_TeamName;

	public UILabel Label_LoginType;

	public UILabel Label_Dev;

	public GameObject NickNameRoot;

	public GameObject LogoutButtonRoot;

	public GameObject LoginButtonRoot;

	public GameObject LoginTypeRoot;

	public GameObject LoginButtonInfoRoot;

	public GameObject LanguageButtonRoot;

	public GameObject VoiceButtonRoot;

	public GameObject PatchNoteButtonRoot;

	public GameObject GameIntroduceButtonRoot;

	public GameObject PatchValidationRoot;

	public GameObject EntrancePTSRoot;

	private Action _onLogout;

	private Action _onChangeLanguage;

	private Action<SystemLanguage> _onChangeVoiceLanguage;

	private Action _onPatchNote;

	private Action _onGameIntroduce;

	private Action _onPatchValidation;

	private Action _onEntrancePTS;

	public UIGrid AccountInfoGrid;

	public UIGrid ButtonGrid;

	private bool _showGameIntroduce;

	private eMenuMode _eMode;

	public void Open(Action onLogout, Action onChangeLanguage, Action<SystemLanguage> onChangeVoiceLanguage, Action onPatchNote = null, Action onGameIntroduce = null, Action onPatchValidation = null, Action onEntrancePTS = null)
	{
		Open(delegate
		{
			Init(onLogout, onChangeLanguage, onChangeVoiceLanguage, onPatchNote, onGameIntroduce, onPatchValidation, onEntrancePTS);
		}, null);
	}

	private void Init(Action onLogout, Action onChangeLanguage, Action<SystemLanguage> onChangeVoiceLanguage, Action onPatchNote, Action onGameIntroduce, Action onPatchValidation, Action onEntrancePTS)
	{
		_onLogout = onLogout;
		_onChangeLanguage = onChangeLanguage;
		_onChangeVoiceLanguage = onChangeVoiceLanguage;
		_onPatchNote = onPatchNote;
		_onGameIntroduce = onGameIntroduce;
		_onPatchValidation = onPatchValidation;
		_onEntrancePTS = onEntrancePTS;
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			Refresh();
		}
	}

	public void SetMode(eMenuMode mode)
	{
		_eMode = mode;
		Refresh();
	}

	public void Refresh()
	{
		switch (_eMode)
		{
		case eMenuMode.Normal:
			Refresh_Noraml();
			break;
		case eMenuMode.PatchDownloading:
			Refresh_PatchDownloading();
			break;
		}
	}

	public void Refresh_Noraml()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance == null)
		{
			SprkAccounts.CleanLoginInfo(this);
			return;
		}
		string text = string.Empty;
		string text2 = (SingletonUpdater<LoginManager>.isValidInstance ? SingletonUpdater<LoginManager>.instance.ServerName : string.Empty);
		if (AccountManager.IsLogin())
		{
			text = GlobalAutoBehaviour<AccountManager>.instance.Nick;
			_ = GlobalAutoBehaviour<AccountManager>.instance.TeamLevel;
		}
		LoginMethodType loginMethodType = LoginData.LastLoginMethod;
		if (loginMethodType == LoginMethodType.None)
		{
			loginMethodType = LoginMethodType.Guest;
		}
		string text3 = loginMethodType.ToString();
		UIHelper.EnableGameObject(NickNameRoot, AccountManager.IsLogin());
		UIHelper.EnableGameObject(LogoutButtonRoot, AccountManager.IsLogin());
		UIHelper.EnableGameObject(LoginButtonRoot, !AccountManager.IsLogin());
		UIHelper.EnableGameObject(LoginButtonInfoRoot, !AccountManager.IsLogin());
		UIHelper.EnableGameObject(LoginTypeRoot, AccountManager.IsLogin());
		UIHelper.EnableGameObject(LanguageButtonRoot, enable: true);
		UIHelper.EnableGameObject(VoiceButtonRoot, enable: true);
		bool enable = false;
		if (AccountManager.IsLogin())
		{
			DateTime o_dateTime;
			if (GlobalAutoBehaviour<AccountManager>.instance.PatchNote != null && GlobalAutoBehaviour<AccountManager>.instance.PatchNote.Index > 0)
			{
				enable = true;
			}
			else if (LoginData.LastPatchNoteIndex > 0 && Vespa_String_Extension_Method2.TryParseNetworkTime2(LoginData.LastPatchNoteEndTime, out o_dateTime) && (int)(o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow()).TotalSeconds >= 0)
			{
				enable = true;
			}
		}
		UIHelper.EnableGameObject(PatchNoteButtonRoot, enable);
		UIHelper.EnableGameObject(GameIntroduceButtonRoot, SystemOptionData.ShowTitleGameIntroduce);
		UIHelper.EnableGameObject(PatchValidationRoot, EnablePatchValidtionCheck());
		bool enable2 = !String_Extension_Method.IsNullOrEmpty(CallManager.MarketURLPTS);
		UIHelper.EnableGameObject(EntrancePTSRoot, enable2);
		UIHelper.SetLabelText(Label_LoginType, text3);
		UIHelper.SetLabelText(Label_TeamName, text);
		UIHelper.SetLabelText(Label_ServerName, text2);
		bool flag = true;
		ClientConfig clientConfig = LoginManager.ClientConfig;
		if (clientConfig != null && !clientConfig.ShowClientConfig)
		{
			flag = false;
		}
		UIHelper.EnableGameObject(Label_Dev, flag);
		if (flag)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("BuildName: " + clientConfig.Name);
			stringBuilder.AppendLine("Platform: " + clientConfig.Platform);
			stringBuilder.AppendLine("ProductName: " + clientConfig.ProductName);
			stringBuilder.AppendLine("BundleIdentifier: " + clientConfig.BundleIdentifier);
			stringBuilder.AppendLine("QueryHost: " + clientConfig.QueryHost);
			stringBuilder.AppendLine("ShowEditorGui: " + clientConfig.ShowEditorGui);
			stringBuilder.AppendLine("ForcedServerName: " + clientConfig.ForcedServerName);
			stringBuilder.AppendLine("Define: " + clientConfig.Define);
			UIHelper.SetLabelText(Label_Dev, stringBuilder.ToString());
		}
		AccountInfoGrid.Reposition();
		ButtonGrid.Reposition();
		SprkAccounts.CleanLoginInfo(this);
	}

	public void Refresh_PatchDownloading()
	{
		UIHelper.EnableGameObject(NickNameRoot, enable: false);
		UIHelper.EnableGameObject(LogoutButtonRoot, enable: false);
		UIHelper.EnableGameObject(LoginButtonRoot, enable: false);
		UIHelper.EnableGameObject(LoginButtonInfoRoot, enable: false);
		UIHelper.EnableGameObject(LoginTypeRoot, enable: false);
		UIHelper.EnableGameObject(PatchNoteButtonRoot, enable: false);
		UIHelper.EnableGameObject(Label_Dev, enable: false);
		UIHelper.EnableGameObject(LanguageButtonRoot, enable: false);
		UIHelper.EnableGameObject(VoiceButtonRoot, enable: false);
		UIHelper.EnableGameObject(GameIntroduceButtonRoot, _showGameIntroduce);
		UIHelper.EnableGameObject(PatchValidationRoot, enable: false);
		UIHelper.EnableGameObject(EntrancePTSRoot, enable: false);
		AccountInfoGrid.Reposition();
		ButtonGrid.Reposition();
	}

	private bool EnablePatchValidtionCheck()
	{
		if (SystemOptionData.Spec == ESpecType.Undefine)
		{
			return false;
		}
		if (SingletonUpdater<LoginManager>.instance.VersionInfo == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<PatchDownloader>.instance.State == PatchDownloader.DownloadState.Started)
		{
			return false;
		}
		return true;
	}

	public void EnableGameIntroduce(bool enable)
	{
		_showGameIntroduce = enable;
	}

	public void OnClickGameIntroduce()
	{
		Vespa_System_Action_Extension_Method.fire(_onGameIntroduce);
	}

	public void OnClickPatchNote()
	{
		Vespa_System_Action_Extension_Method.fire(_onPatchNote);
	}

	public void OnClickLogout()
	{
		Vespa_System_Action_Extension_Method.fire(_onLogout);
	}

	public void OnClickChangeLanguage()
	{
		Vespa_System_Action_Extension_Method.fire(_onChangeLanguage);
	}

	public void OnClickChangeVoiceLanguage()
	{
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<SelectVoiceLanguage>().Open(_onChangeVoiceLanguage);
	}

	public void OnClickAccountConnect()
	{
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginTouchWait>().Close();
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>().Close();
		LoginData.LastLoginMethod = LoginMethodType.None;
		SingletonUpdater<LoginManager>.instance.ResetedLoginData = true;
		SingletonUpdater<LoginManager>.instance.SetNextState(new StateBase_SelectLoginMethod());
	}

	public void OnClickPatchValidation()
	{
		Vespa_System_Action_Extension_Method.fire(_onPatchValidation);
	}

	public void OnClickEntrancePTS()
	{
		Vespa_System_Action_Extension_Method.fire(_onEntrancePTS);
	}
}
