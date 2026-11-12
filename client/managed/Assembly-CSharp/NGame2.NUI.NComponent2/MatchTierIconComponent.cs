using NGame2.NContentsDefine;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchTierIconComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/Match/MatchTierIconComponent";

	public static PrefabAuto<MatchTierIconComponent> PrefabAuto = new PrefabAuto<MatchTierIconComponent>("Prefab2/Component/Match/MatchTierIconComponent");

	public UISpriteSelector IconSelector_LocalTier;

	public UISpriteSelector IconSelector_WorldTier;

	public UISpriteSelector NameSelector;

	public GameObject[] BronzeLevel;

	public GameObject[] SilverLevel;

	public GameObject[] GoldLevel;

	public GameObject[] PlatinumLevel;

	public GameObject[] DiamondLevel;

	public GameObject[] MasterLevel;

	public GameObject[] RoyalMasterLevel;

	private MatchSeasonType _seasonType;

	private MatchTierType _tierType;

	private VisualTierRank _visualTierRank;

	private MatchArenaType _arenaType;

	public bool Open(int tierIndex, VisualTierRank visualTierRank, MatchArenaType arenaType)
	{
		MatchTierData tierData = MatchArena.GetTierData(arenaType, tierIndex);
		if (tierData != null)
		{
			return Open(tierData.Tier, tierData.Level, visualTierRank, arenaType);
		}
		return false;
	}

	public bool Open(MatchTierType tierType, int tierLevel, VisualTierRank visualTierRank, MatchArenaType arenaType)
	{
		_tierType = tierType;
		_visualTierRank = visualTierRank;
		_arenaType = arenaType;
		SetTierIcon();
		ShowLevel(BronzeLevel, tierType == MatchTierType.Bronze, tierLevel);
		ShowLevel(SilverLevel, tierType == MatchTierType.Silver, tierLevel);
		ShowLevel(GoldLevel, tierType == MatchTierType.Gold, tierLevel);
		ShowLevel(PlatinumLevel, tierType == MatchTierType.Platinum, tierLevel);
		ShowLevel(DiamondLevel, tierType == MatchTierType.Diamond, tierLevel);
		ShowLevel(MasterLevel, tierType == MatchTierType.Master, tierLevel);
		ShowLevel(RoyalMasterLevel, tierType == MatchTierType.RoyalMaster, tierLevel);
		ShowName();
		return true;
	}

	public bool Open(MatchSeasonType seasonType, int seasonIndex)
	{
		_seasonType = seasonType;
		UIHelper.EnableGameObject(IconSelector_LocalTier, enable: true);
		UIHelper.EnableGameObject(IconSelector_WorldTier, enable: false);
		IconSelector_LocalTier.SelectEnum(seasonType);
		ShowLevel(BronzeLevel, isShow: false);
		ShowLevel(SilverLevel, isShow: false);
		ShowLevel(GoldLevel, isShow: false);
		ShowLevel(PlatinumLevel, isShow: false);
		ShowLevel(DiamondLevel, isShow: false);
		ShowLevel(MasterLevel, isShow: false);
		ShowSeasonName(show: true);
		return true;
	}

	public void Open(GuildArenaTierType tierType)
	{
		HideName();
		DisableLevel();
		SetTierIcon(tierType.ToString());
	}

	public void ShowLevel(GameObject[] rankLevel, bool isShow, int tierLevel = 0)
	{
		for (int i = 0; i < rankLevel.Length; i++)
		{
			bool enable = isShow && tierLevel == i;
			UIHelper.EnableGameObject(rankLevel[i], enable);
		}
	}

	public void DisableLevel()
	{
		ShowLevel(BronzeLevel, isShow: false);
		ShowLevel(SilverLevel, isShow: false);
		ShowLevel(GoldLevel, isShow: false);
		ShowLevel(PlatinumLevel, isShow: false);
		ShowLevel(DiamondLevel, isShow: false);
		ShowLevel(MasterLevel, isShow: false);
		ShowLevel(RoyalMasterLevel, isShow: false);
	}

	public void HideName()
	{
		UIHelper.EnableGameObject(NameSelector, enable: false);
	}

	private void ShowName()
	{
		UIHelper.EnableGameObject(NameSelector, enable: true);
		int championRank = MatchArena.GetChampionRank(_arenaType);
		if (_tierType == MatchTierType.Challenger && _visualTierRank.Value <= championRank)
		{
			NameSelector.Select("Champion");
		}
		else
		{
			NameSelector.SelectEnum(_tierType);
		}
	}

	public void ShowSeasonName(bool show)
	{
		UIHelper.EnableGameObject(NameSelector, show);
		if (show)
		{
			NameSelector.SelectEnum(_seasonType);
		}
	}

	private void SetTierIcon()
	{
		if (_arenaType == MatchArenaType.Normal || _arenaType == MatchArenaType.BanPick)
		{
			SetTierIcon_Pvp();
		}
		else
		{
			SetTierIcon_Etc();
		}
	}

	private void SetTierIcon_Pvp()
	{
		UIHelper.EnableGameObject(IconSelector_LocalTier, enable: false);
		UIHelper.EnableGameObject(IconSelector_WorldTier, enable: true);
		if (_tierType == MatchTierType.Challenger && _visualTierRank.Value <= 3)
		{
			switch (_visualTierRank.Value)
			{
			case 1:
				IconSelector_WorldTier.Select("Challenger_World_1st");
				break;
			case 2:
				IconSelector_WorldTier.Select("Challenger_World_2nd");
				break;
			case 3:
				IconSelector_WorldTier.Select("Challenger_World_3rd");
				break;
			}
		}
		else
		{
			IconSelector_WorldTier.SelectEnum(_tierType);
		}
	}

	private void SetTierIcon_Etc()
	{
		UIHelper.EnableGameObject(IconSelector_LocalTier, enable: true);
		UIHelper.EnableGameObject(IconSelector_WorldTier, enable: false);
		if (_tierType == MatchTierType.Challenger && _visualTierRank.Value == 1)
		{
			IconSelector_LocalTier.Select("Champion");
		}
		else
		{
			IconSelector_LocalTier.SelectEnum(_tierType);
		}
	}

	public void SetTierIcon(string tierTypeStr)
	{
		UIHelper.EnableGameObject(IconSelector_LocalTier, enable: true);
		UIHelper.EnableGameObject(IconSelector_WorldTier, enable: false);
		IconSelector_LocalTier.Select(tierTypeStr);
	}
}
