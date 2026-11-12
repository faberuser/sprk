using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ChallengeRaidBanInfoComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnShowTooltip(ChallengeRaidBanInfoComponent comp);

		void OnHideTooltip(ChallengeRaidBanInfoComponent comp);
	}

	public const string PrefabPath = "Prefab2/Component/Raid/ChallengeRaidBanInfoComponent";

	public static PrefabAuto<ChallengeRaidBanInfoComponent> PrefabAuto = new PrefabAuto<ChallengeRaidBanInfoComponent>("Prefab2/Component/Raid/ChallengeRaidBanInfoComponent");

	private Listener _listener;

	public UISprite Spr_ClassTag;

	public UITexture Tex_Attr;

	public UITexture Tex_Hero;

	private RaidBanType _banType;

	private string _banValue;

	private const string LOCALE_CREATURE_TAG = "CHALLENGERAID_BAN_TOOLTIP_CLASS";

	private const string LOCALE_CREATURE_TAG_DESC = "CLASS_DESC_";

	private const string LOCALE_ATTR_TAG = "CHALLENGERAID_BAN_TOOLTIP_ATTRTYPE";

	private const string LOCALE_ATTR_TAG_DESC = "SKILL_ATTR_TYPE.";

	private const string LOCALE_HERO_TAG = "CHALLENGERAID_BAN_TOOLTIP_HERO";

	private const string LOCALE_HERO_TAG_NAME = "CREATURE_NAME_";

	public RaidBanType BanType => _banType;

	public string BanValue => _banValue;

	private void Init()
	{
		UIHelper.EnableGameObject(Spr_ClassTag, enable: false);
		UIHelper.EnableGameObject(Tex_Attr, enable: false);
		UIHelper.EnableGameObject(Tex_Hero, enable: false);
	}

	private void ShowClassIcon(CreatureTagType type)
	{
		UIHelper.EnableGameObject(Spr_ClassTag, enable: true);
		Spr_ClassTag.spriteName = type.ToString();
	}

	private void ShowAttr(SkillAttrType type)
	{
		UIHelper.EnableGameObject(Tex_Attr, enable: true);
		AssetTexture attrTypeIcon = UIHelper.GetAttrTypeIcon(type.ToString());
		UIHelper.ResetPlaneAssetTextureAsync(this, Tex_Attr, attrTypeIcon);
	}

	private void ShowHero(string codeName)
	{
		UIHelper.EnableGameObject(Tex_Hero, enable: true);
		CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(codeName);
		if (dataByCode == null)
		{
			VespaDebug.LogError("creatureData == null /  codeName : " + codeName);
			return;
		}
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(dataByCode.Index, 3);
		UIHelper.ResetPlaneAssetTextureAsync(this, Tex_Hero, heroIcon);
	}

	public void Open(Listener listener, RaidBanType banType, string banValue)
	{
		_listener = listener;
		Init();
		_banType = banType;
		_banValue = banValue;
		switch (banType)
		{
		case RaidBanType.CreatureTagType:
		{
			if (BanRuleData.GetCreatureTagType(banValue, out var type2))
			{
				ShowClassIcon(type2);
			}
			else
			{
				VespaDebug.LogError("EnumParse Fail / GetCreatureTagType / banValue : " + banValue);
			}
			break;
		}
		case RaidBanType.SkillAttrType:
		{
			if (BanRuleData.GetSkillAttrType(banValue, out var type))
			{
				ShowAttr(type);
			}
			else
			{
				VespaDebug.LogError("EnumParse Fail / GetCreatureTagType / banValue : " + banValue);
			}
			break;
		}
		case RaidBanType.CodeName:
			ShowHero(banValue);
			break;
		}
	}

	private void InitIconSprite(CreatureTagType tagType)
	{
		Spr_ClassTag.spriteName = tagType.ToString();
	}

	public void OnShowTooltip()
	{
		TooltipTextInfo tooltipTextInfo = new TooltipTextInfo();
		string title = string.Empty;
		string desc = string.Empty;
		GetTooltipLocale(out title, out desc);
		tooltipTextInfo.Name.Value = title;
		tooltipTextInfo.Desc.Value = desc;
		ChallengeRaidBanTooltip.Open(GetComponent<UIWidget>(), tooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		ChallengeRaidBanTooltip.Hide(GetComponent<UIWidget>());
	}

	private void GetTooltipLocale(out string title, out string desc)
	{
		title = string.Empty;
		desc = string.Empty;
		string empty = string.Empty;
		if (_banType == RaidBanType.CreatureTagType)
		{
			title = LocalizationManager.GetStringDirect("CHALLENGERAID_BAN_TOOLTIP_CLASS");
			if (BanRuleData.GetCreatureTagType(_banValue, out var type))
			{
				empty = "CLASS_DESC_" + type.ToString().ToUpper();
				desc = LocalizationManager.GetStringDirect(empty);
			}
		}
		else if (_banType == RaidBanType.SkillAttrType)
		{
			title = LocalizationManager.GetStringDirect("CHALLENGERAID_BAN_TOOLTIP_ATTRTYPE");
			if (BanRuleData.GetSkillAttrType(_banValue, out var type2))
			{
				empty = "SKILL_ATTR_TYPE." + type2.ToString().ToUpper();
				desc = LocalizationManager.GetStringDirect(empty);
			}
		}
		else if (_banType == RaidBanType.CodeName)
		{
			title = LocalizationManager.GetStringDirect("CHALLENGERAID_BAN_TOOLTIP_HERO");
			CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(_banValue);
			if (dataByCode != null)
			{
				empty = "CREATURE_NAME_" + dataByCode.Index;
				desc = LocalizationManager.GetStringDirect(empty);
			}
		}
	}
}
