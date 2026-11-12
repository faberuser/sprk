using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NLogin;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NShared.QueryNick;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class LoginCreateNick : BaseWindow
{
	public interface Listener
	{
		void IsValidNick(LoginCreateNick caller, string nick, Action<ResultType> onCheck);

		void UseNick(LoginCreateNick caller, string nick, string countryCode, Action<ResultType> onUse);

		void OnSelectCountry(LoginCreateNick caller);

		void OnClickBack(LoginCreateNick caller);

		void OpenConfirmPopup(LoginCreateNick caller, string title, string content, Action onYes, Action onNo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginCreateNick";

	public UIInput NickInput;

	public UILabel DescLabel;

	public UILabel ChangeDescLabel;

	public UILabel CountryNameLabel;

	public UITexture CountryFlagTexture;

	private Listener _listener;

	private CountryData _countryData;

	public void Open(Listener listener)
	{
		Open(delegate
		{
			Init(listener);
		}, null);
	}

	public void Init(Listener listener)
	{
		_listener = listener;
		NickInput.value = string.Empty;
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance != null && GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.NickChange >= 1)
		{
			UIHelper.EnableGameObject(ChangeDescLabel, enable: true);
		}
		else
		{
			UIHelper.EnableGameObject(ChangeDescLabel, enable: false);
		}
		InitCurrentCountry();
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
	}

	public override void OnSystemBackButton()
	{
		OnClickBack();
	}

	public void OnClickBack()
	{
		if (_listener != null)
		{
			_listener.OnClickBack(this);
		}
	}

	private void InitCurrentCountry()
	{
		string countryCode = SingletonUpdater<LoginManager>.instance.CountryCode;
		CountryData countryData = GameRoot.Instance.CountryDataContainer.GetData(countryCode);
		if (countryData == null)
		{
			countryData = GameRoot.Instance.CountryDataContainer.GetFirstData();
		}
		if (countryData == null)
		{
			VespaDebug.LogError("Failed to find country data.");
		}
		else
		{
			SetCountry(countryData);
		}
	}

	private void ShowResult(ResultType result)
	{
		if (result != ResultType.Success)
		{
			string text = LocalizationManager.GetString(result);
			UIHelper.SetLabelText(DescLabel, "[FF0000]" + text);
		}
		else
		{
			UIHelper.SetLabelText(DescLabel, LocalizationManager.GetString(Nick.NICK_VALID_MESSAGE));
		}
	}

	public void OnClickConfirm()
	{
		if (!(NickInput == null) && _listener != null)
		{
			VespaCoroutine.Start(this, coUseNick());
		}
	}

	private IEnumerator coUseNick()
	{
		string nick = NickInput.value;
		bool isComplete = false;
		ResultType validNickResult = ResultType.Fail;
		_listener.IsValidNick(this, nick, delegate(ResultType result)
		{
			isComplete = true;
			validNickResult = result;
		});
		while (!isComplete)
		{
			yield return null;
		}
		ShowResult(validNickResult);
		if (validNickResult == ResultType.Success)
		{
			string text = LocalizationManager.GetString(Nick.NICK_VALID_MESSAGE);
			UIHelper.SetLabelText(DescLabel, text);
			string text2 = LocalizationManager.GetString(Nick.NICK_CONFIRM_TITLE);
			string content = LocalizationManager.GetString(Nick.NICK_CONFIRM_MESSAGE, nick);
			isComplete = false;
			bool useThisNick = false;
			_listener.OpenConfirmPopup(this, text2, content, delegate
			{
				isComplete = true;
				useThisNick = true;
			}, delegate
			{
				isComplete = true;
				useThisNick = false;
			});
			while (!isComplete)
			{
				yield return null;
			}
			if (useThisNick)
			{
				string countryCode = ((_countryData != null) ? _countryData.Code : "");
				_listener.UseNick(this, nick, countryCode, OnUseNick);
			}
		}
	}

	private void OnUseNick(ResultType result)
	{
		ShowResult(result);
	}

	public void OnClickSelectCountry()
	{
		if (_listener != null)
		{
			_listener.OnSelectCountry(this);
		}
	}

	public void SetCountry(CountryData countryData)
	{
		_countryData = countryData;
		AssetTexture countryFlagIcon = UIHelper.GetCountryFlagIcon(countryData.Code);
		UIHelper.ResetPlaneAssetTextureAsync(this, CountryFlagTexture, countryFlagIcon);
		UIHelper.SetLabelText(CountryNameLabel, countryData.Name);
	}
}
