using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NConfig;
using NGame2.NLogin;
using NGame2.NPet;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NShared.ChangeNick;
using NShared.NLocalization;
using NShared.QueryNick;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MyInfoWindow : BaseWindow, LoginCreateNick.Listener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>
{
	[Serializable]
	public class GameOptionMyInfo
	{
		public UILabel Label_Nick;

		public UILabel Label_TeamLevelNum;

		public UISprite Sprite_TeamExp;

		public UILabel Label_TeamExp;

		public UIBundleLoader BundleTex_Avatar;

		public UIBundleLoader BundleTex_Pet;

		public GameObject PetLockedState;

		public UIButton Button_SelectPet;

		public TeamLevelupPopup.TeamLevelEffectLabels TeamLevelEffectLabels;

		public UITexture Texture_CountryFlag;

		public UILabel Label_CountryFlag;

		public UILabel Label_ServerTime;

		public UILabel Label_PetLockedState;
	}

	public interface Listener
	{
		void OnChangeAvatar();

		void OnChangeCountryFlag();

		void OpenPetHouseWindow();

		void OnChangeAvatarPet();
	}

	public interface IDataProvider
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/MyInfoWindow";

	public GameOptionMyInfo MyInfo = new GameOptionMyInfo();

	public HeroPortraitComponent.PrefabSelector PortraitSelector = new HeroPortraitComponent.PrefabSelector(HeroPortraitComponent.PrefabSelector.EPrefabType.Short);

	private List<PetInfo> _petInfoList;

	private Listener _listener;

	private IDataProvider _dataProvider;

	private int _clickDevelop;

	private ESpecType _prevSpecType;

	private HeroPortraitComponent _avatarHeroPortrait;

	private CountryData _countryData;

	private CountryServerData _countryServerData;

	private const int COMMON_LAW_1_INDEX = 0;

	private const int COMMON_LAW_2_INDEX = 1;

	private const int LOCALIZED_LAW_1_INDEX = 0;

	private const int LOCALIZED_LAW_2_INDEX = 1;

	private const int LOCALIZED_LAW_3_INDEX = 2;

	public override void ReOpen()
	{
		base.ReOpen();
	}

	public override void Awake()
	{
		base.Awake();
	}

	public void Open(Listener listener, IDataProvider dataProvider)
	{
		base.Open();
		_listener = listener;
		_dataProvider = dataProvider;
		SetupMyInfo();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateServerTime();
	}

	public void SetupMyInfo()
	{
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		ChangeNick(userManager.Nick);
		UIHelper.SetLabelText(MyInfo.Label_TeamLevelNum, userManager.TeamLevel);
		NGame2.NAccount.Helper.LevelInfo accountTeamLevelInfo = NGame2.NAccount.Helper.GetAccountTeamLevelInfo();
		UIHelper.SetLabelText(MyInfo.Label_TeamExp, $"{accountTeamLevelInfo.CurExp}/{accountTeamLevelInfo.LocalExp}");
		UIHelper.SetLabelText(MyInfo.Label_PetLockedState, LocalizationManager.GetStringDirect("PET_MAINPET_UNABLE_CHAPTER"));
		UIHelper.SetSpriteFillAmount(MyInfo.Sprite_TeamExp, accountTeamLevelInfo.ExpPercent);
		UpdateAvatarHero();
		UpdateCountryFlag();
		UpdateServerTime();
		UpdateAvatarPet();
		TeamLevelupPopup.SetTeamLevelEffectLabels(GameRoot.Instance.TeamLevelDataContainer.GetData(userManager.TeamLevel), MyInfo.TeamLevelEffectLabels);
		SprkAccounts.LabelNickname(this);
		SprkPetPresentation.Setup(this);
	}

	private void ChangeNick(string nick)
	{
		UIHelper.SetLabelText(MyInfo.Label_Nick, nick);
	}

	public void UpdateAvatarHero()
	{
		string profilePortraitPath = SprkLobbyAvatar.GetProfilePortraitPath(GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(profilePortraitPath))
		{
			UIHelper.SetBundleTexture(MyInfo.BundleTex_Avatar, profilePortraitPath, UIHelper.EAssetTextureAssign.ClipCenter);
		}
		SprkLobbyAvatar.WatchMyInfoPortrait(this);
	}

	public void UpdateCountryFlag()
	{
		string countryFlagCode = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetCountryFlagCode();
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryFlagCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, MyInfo.Texture_CountryFlag, plainTexture);
			UIHelper.SetLabelText(MyInfo.Label_CountryFlag, dataByCode.CountryName);
		}
	}

	public void UpdateServerTime()
	{
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		int serverUTCOffsetHour = GlobalAutoBehaviour<AccountManager>.instance.GetServerUTCOffsetHour();
		string text = ((serverUTCOffsetHour > 0) ? "+" : "-");
		string text2 = $"{currentServerLocalTime.Year}/{currentServerLocalTime.Month:00}/{currentServerLocalTime.Day:00} {currentServerLocalTime.Hour:00}:{currentServerLocalTime.Minute:00} UTC{text}{System.Math.Abs(serverUTCOffsetHour)}";
		UIHelper.SetLabelText(MyInfo.Label_ServerTime, text2);
	}

	public void UpdateAvatarPet()
	{
		string assetPath = "Portrait/Pet/CREATURE_PET_EXPONENT_UNSELECTED";
		SetPetLockedState();
		PetMiscInfo petMiscInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.GetPetMiscInfo(PetMiscType.AvatarPetIndex);
		if (petMiscInfo != null)
		{
			assetPath = UIHelper.GetPetPortraitPath(Vespa_String_Extension_Method.TOINT(petMiscInfo.MiscValue, 0));
		}
		UIHelper.SetBundleTexture(MyInfo.BundleTex_Pet, assetPath, UIHelper.EAssetTextureAssign.ClipCenter);
	}

	public bool SetPetLockedState()
	{
		bool flag = false;
		_petInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetPetInfoList(sort: true, PetManager.EPetCompareType.PetManagement);
		if (__Extension_Method_ICollection__.COUNT(_petInfoList) <= 0)
		{
			UIHelper.SetLabelText(MyInfo.Label_PetLockedState, LocalizationManager.GetStringDirect("PET_HAVE_NOT_WARNING_TEXT"));
			flag = false;
		}
		else
		{
			flag = true;
		}
		UIHelper.EnableButton(MyInfo.Button_SelectPet, flag);
		UIHelper.EnableGameObject(MyInfo.PetLockedState, !flag);
		return flag;
	}

	public void OnClickChangeAvatar()
	{
		if (_listener != null)
		{
			_listener.OnChangeAvatar();
		}
	}

	public void OnClickChangePet()
	{
		if (_listener != null)
		{
			_listener.OnChangeAvatarPet();
		}
	}

	public void OnClickChangeCountryFlag()
	{
		if (_listener != null)
		{
			_listener.OnChangeCountryFlag();
		}
	}

	public void OnClickTeamLevelBuff()
	{
		OnClickExitButton();
		SceneAutoBehaviour<TeamLevelBuffManagement>.instance.OpenTeamLevelBuffWindow();
	}

	public new void Reset()
	{
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
	}

	public void OnClickExitButton()
	{
		Reset();
		Close();
	}

	public void OnClickChangeNickName()
	{
		SprkAccounts.OpenFreeNickname(this);
	}

	public void OnClickDevelop()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.AdminLevel != 0)
		{
			_clickDevelop++;
			if (SingletonAuto<Config>.isValidInstance && _clickDevelop == 20)
			{
				GlobalAutoBehaviour<GuiManager>.instance.SetEnable(enabled: true);
			}
		}
	}

	void LoginCreateNick.Listener.IsValidNick(LoginCreateNick caller, string nick, Action<ResultType> onCheck)
	{
		ResultType resultType = SingletonAuto<NickChecker>.instance.Check(nick);
		if (resultType != ResultType.Success)
		{
			Vespa_System_Action_Extension_Method.fire(onCheck, resultType);
			return;
		}
		NShared.QueryNick.Request request = new NShared.QueryNick.Request
		{
			Nick = nick
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.QueryNick.Response response)
		{
			if (response == null || response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onCheck, ResultType.Fail);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onCheck, response.Result);
			}
		});
	}

	void LoginCreateNick.Listener.UseNick(LoginCreateNick caller, string nick, string countryCode, Action<ResultType> onUse)
	{
		string language = Application.systemLanguage.ToString();
		NShared.ChangeNick.Request request = new NShared.ChangeNick.Request
		{
			Nick = nick,
			CountryCode = countryCode,
			Language = language
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ChangeNick.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onUse, ResultType.Fail);
			}
			else if (response.Result != ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onUse, response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.NickChange = 0;
				if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetNick(nick);
				}
				if (response.CreatedTime != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.SetCreateTime(response.CreatedTime);
				}
				_ = (object)null;
				_ = 4;
				_ = 1;
				_ = 1;
				Array.Empty<object>();
				Vespa_System_Action_Extension_Method.fire(onUse, response.Result);
				SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LoginCreateNick>().Close();
				CenterMessage.Open(LocalizationManager.GetString(Item.USE_NICK_CHANGE_MSG, nick));
				SetupMyInfo();
			}
		});
	}

	void LoginCreateNick.Listener.OnSelectCountry(LoginCreateNick caller)
	{
	}

	void LoginCreateNick.Listener.OnClickBack(LoginCreateNick caller)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LoginCreateNick>().Close();
	}

	void LoginCreateNick.Listener.OpenConfirmPopup(LoginCreateNick caller, string title, string content, Action onYes, Action onNo)
	{
		LoginMethodType lastLoginMethod = LoginData.LastLoginMethod;
		NickMessagePopup.Open(title, content, lastLoginMethod, onYes, onNo);
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.Nick)
		{
			ChangeNick(changedInfo.AfterValue3);
		}
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		UpdateAvatarPet();
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
