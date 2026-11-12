using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class CreateGuild2 : BaseWindow, GuildLogoComponent.Listener, GuildBGComponent.Listener, GuildLogoEditPopup.Listener
{
	public interface Listener
	{
		void OnClickCreate(string name, int logo, int background, EGuildJoinWay joinWay, int joinLevel, string countryCode, string introduce);

		void OnClickCheckName(string name);

		void OnChangeCountryFlag(string guildCountryCode);

		void OnClickGuildEdit();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Guild/CreateGuild2";

	public UILabel Label_GuildName;

	public UILabel Label_Introduce;

	public UIInput Input_GuildName;

	public UIInput Input_Introduce;

	public UIButton Button_Search;

	public UILabel Label_Registerable;

	public UILabel Label_NotRegisterable;

	public UILabel Label_Level;

	public UILabel Label_CreationCost;

	public UIToggle Toggle_JoinFree;

	public UIToggle Toggle_JoinAccept;

	public UIButton Button_AddLevel;

	public UIButton Button_SubtractLevel;

	public UIButton Button_CreateGuild;

	public UITexture Texture_CountryFlag;

	public UIScrollGrid Grid_GuildLogoIcon;

	private List<GuildLogoComponent> _GuildLogoIconList = new List<GuildLogoComponent>();

	private Listener _listener;

	private EGuildJoinWay _joinWay;

	private int _joinLevel;

	private int _iconIndex;

	private int _bgIndex;

	private string _guildCountryCode;

	public UISprite SpriteEmptyLogo;

	public GuildLogoComponent _guildLogoComponent;

	public GuildBGComponent _guildBGComponent;

	public int CurrentIconIndex => _iconIndex;

	public int CurrentBGIndex => _bgIndex;

	public void Open(Listener listener)
	{
		base.Open();
		Init(listener);
	}

	public void Init(Listener listener)
	{
		_listener = listener;
		InitGuildCreateWidget();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_GuildLogoIconList);
	}

	private void InitGuildCreateWidget()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildNameLength");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildIntroduceLength");
		UIHelper.SetLabelText(Label_GuildName, LocalizationManager.GetStringDirect("GUILD_CREATE_NAME_LIMIT", valueInt));
		UIHelper.SetLabelText(Label_Introduce, LocalizationManager.GetStringDirect("GUILD_CREATE_INTRODUCE_LIMIT", valueInt2));
		_joinWay = EGuildJoinWay.FreeJoin;
		_joinLevel = 1;
		_iconIndex = 1;
		_guildCountryCode = "VESPA";
		SetLevel(_joinLevel);
		Input_GuildName.value = "";
		Input_Introduce.value = "";
		Input_GuildName.characterLimit = valueInt;
		Input_Introduce.characterLimit = valueInt2;
		int valueInt3 = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildCreateGold");
		long i_value = valueInt3;
		UIHelper.SetLabelText(Label_CreationCost, Vespa_Int_Extension_Method.ToCommaString(i_value));
		RestoreCheckNameButton();
		UIHelper.EnableGameObject(SpriteEmptyLogo, enable: true);
		SetGuildLogo(1, 1);
		UpdateCountryFlag();
	}

	private void UpdateSelected()
	{
		foreach (GuildLogoComponent guildLogoIcon in _GuildLogoIconList)
		{
			guildLogoIcon.SetSelected(_iconIndex);
		}
	}

	private void UpdateCountryFlag()
	{
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(_guildCountryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CountryFlag, plainTexture);
		}
	}

	private void RestoreCheckNameButton()
	{
		UIHelper.EnableGameObject(Button_Search, enable: true);
		UIHelper.EnableGameObject(Label_Registerable, enable: false);
		UIHelper.EnableGameObject(Label_NotRegisterable, enable: false);
		UIHelper.EnableButton(Button_CreateGuild, enable: false);
	}

	public void OnClickExitButton()
	{
		Close();
	}

	public void OnClickCreateButton()
	{
		if (_listener != null)
		{
			_listener.OnClickCreate(Input_GuildName.value, _iconIndex, _bgIndex, _joinWay, _joinLevel, _guildCountryCode, Input_Introduce.value);
		}
	}

	public void OnClickCheckGuildName()
	{
		if (!String_Extension_Method.IsNullOrEmpty(Input_GuildName.value))
		{
			if (Input_GuildName.value.Length <= 2)
			{
				CenterMessage.Open(LocalizationManager.GetString(Guild.GUILD_CHECK_NAME_MIN_LENGTH));
			}
			else if (_listener != null)
			{
				_listener.OnClickCheckName(Input_GuildName.value);
			}
		}
	}

	private void SetLevel(int newLevel)
	{
		int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
		if (newLevel >= 1 && newLevel <= teamLevel)
		{
			_joinLevel = newLevel;
			UIHelper.SetLabelText(Label_Level, _joinLevel.ToString());
			UIHelper.EnableButton(Button_AddLevel, _joinLevel < teamLevel);
			UIHelper.EnableButton(Button_SubtractLevel, _joinLevel > 1);
		}
	}

	public void OnClickAddLevel()
	{
		SetLevel(_joinLevel + 1);
	}

	public void OnClickSubtractLevel()
	{
		SetLevel(_joinLevel - 1);
	}

	public void OnToggleChanged()
	{
		if (Toggle_JoinFree.value)
		{
			_joinWay = EGuildJoinWay.FreeJoin;
		}
		else if (Toggle_JoinAccept.value)
		{
			_joinWay = EGuildJoinWay.AcceptJoin;
		}
	}

	public void SetGuildNameChecked(bool usable)
	{
		UIHelper.EnableGameObject(Button_Search, enable: false);
		UIHelper.EnableGameObject(Label_Registerable, usable);
		UIHelper.EnableGameObject(Label_NotRegisterable, !usable);
		UIHelper.EnableButton(Button_CreateGuild, usable);
	}

	public void OnChangeInputGuildName()
	{
		if (!UIHelper.IsEnabledGameObjet(Button_Search))
		{
			RestoreCheckNameButton();
		}
		GuildManagement2.CheckInput(Input_GuildName, GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildNameLength"));
	}

	public void OnSubmitInputGuildName()
	{
		RestoreCheckNameButton();
	}

	public void OnChangeInputIntroduce()
	{
		GuildManagement2.CheckInput(Input_Introduce, GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildIntroduceLength"));
	}

	void GuildLogoComponent.Listener.OnClickGuildLogoComponent(int LogoIndex)
	{
	}

	void GuildBGComponent.Listener.OnClickGuildBGComponent(int index)
	{
	}

	public void OnClickChangeCountryFlag()
	{
		if (_listener != null)
		{
			_listener.OnChangeCountryFlag(_guildCountryCode);
		}
	}

	public void SetGuildCountryCode(string countryCode)
	{
		_guildCountryCode = countryCode;
		UpdateCountryFlag();
	}

	public void OnClickGuildEdit()
	{
		if (_listener != null)
		{
			_listener.OnClickGuildEdit();
		}
	}

	public void SetGuildLogo(int iconInex, int bgIndex)
	{
		UIHelper.EnableGameObject(SpriteEmptyLogo, enable: false);
		_iconIndex = iconInex;
		_bgIndex = bgIndex;
		_guildLogoComponent.Open(iconInex, this);
		_guildBGComponent.Open(bgIndex, this);
	}

	void GuildLogoEditPopup.Listener.OnClickReset()
	{
		SetGuildLogo(1, 1);
	}

	void GuildLogoEditPopup.Listener.OnClickApply(int iconIndex, int bgIndex)
	{
		SetGuildLogo(iconIndex, bgIndex);
	}
}
