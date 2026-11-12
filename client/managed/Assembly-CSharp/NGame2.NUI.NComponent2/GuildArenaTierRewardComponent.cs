using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildArenaTierRewardComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	[Serializable]
	public class GuildArenaRewardInfo
	{
		public GameObject Root;

		public GameObject RubyRoot;

		public Transform TierIconParent;

		public UILabel Label_Score;

		public UILabel Label_Rank;

		public UILabel Label_Ruby;

		public UILabel Label_Point;

		public UISpriteSelector BoxRewardIcon;
	}

	public const string PrefabPath = "Resources/Prefab2/Component/GuildArena/GuildArenaTierRewardComponent";

	public static PrefabAuto<GuildArenaTierRewardComponent> PrefabAuto = new PrefabAuto<GuildArenaTierRewardComponent>("Resources/Prefab2/Component/GuildArena/GuildArenaTierRewardComponent");

	public GameObject BronzeName;

	public GameObject SilverName;

	public GameObject GoldName;

	public GameObject PlatinumName;

	public GameObject DiamondName;

	public GameObject MasterName;

	public GameObject RoyalMasterName;

	public GameObject ChallengerName;

	public GameObject ChampionName;

	public GameObject RewardItemsRoot;

	public Transform FirstRewardItemIconTrs;

	public UILabel FirstRewardItemCountLabel;

	public GameObject SecondRewardItemRoot;

	public Transform SecondRewardItemIconTrs;

	public UILabel SecondRewardItemCountLabel;

	public UILabel Label_LeftRank;

	public GuildArenaRewardInfo RewardInfo;

	private List<MatchTierIconComponent> _tierIconList = new List<MatchTierIconComponent>();

	private List<ItemIconComponent> _itemIconList = new List<ItemIconComponent>();

	[SerializeField]
	private GuildArenaTierType _tierType;

	private GuildArenaSeasonRewardData _seasonRewardData;

	private GuildArenaSeasonType _seasonType;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(GuildArenaSeasonRewardData seasonRewardData)
	{
		if (seasonRewardData != null)
		{
			_seasonRewardData = seasonRewardData;
			_tierType = seasonRewardData.ShowTierUI;
			_seasonType = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SeasonType;
			Init();
		}
	}

	public void Init()
	{
		InitName();
		InitScore();
		InitRank();
		InitReward();
		SetTierIcon(RewardInfo);
	}

	public void InitScore()
	{
		if (_seasonRewardData != null)
		{
			int showScoreUI = _seasonRewardData.ShowScoreUI;
			UIHelper.SetLabelText(RewardInfo.Label_Score, showScoreUI);
		}
	}

	public void InitRank()
	{
		if (_seasonRewardData != null)
		{
			UIHelper.EnableGameObject(Label_LeftRank, enable: false);
			int minRank = _seasonRewardData.MinRank;
			int maxRank = _seasonRewardData.MaxRank;
			string text = string.Empty;
			if (minRank > 0 && maxRank > 0)
			{
				text = LocalizationManager.GetStringDirect("WORLDBOSS_RANK_COUNT", minRank, maxRank);
			}
			if (minRank <= 0 && maxRank <= 0)
			{
				text = LocalizationManager.GetStringDirect("MATCH_RANK_UNLIMITED");
			}
			if (_tierType == GuildArenaTierType.Champion)
			{
				text = LocalizationManager.GetStringDirect("GUILD_RANKCOUNT", 1);
			}
			UIHelper.SetLabelText(RewardInfo.Label_Rank, text);
			GuildArenaTierType tierType = _tierType;
			if ((uint)(tierType - 5) <= 3u)
			{
				UIHelper.EnableGameObject(Label_LeftRank, enable: true);
				UIHelper.SetLabelText(Label_LeftRank, text);
			}
		}
	}

	public void InitReward()
	{
		int num = _seasonRewardData.SeasonRubyReward;
		int num2 = _seasonRewardData.SeasonPointReward;
		if (_seasonType == GuildArenaSeasonType.Free)
		{
			num = _seasonRewardData.FreeSeasonRubyReward;
			num2 = _seasonRewardData.FreeSeasonPointReward;
		}
		UIHelper.EnableGameObject(RewardInfo.RubyRoot, _seasonType == GuildArenaSeasonType.Regular);
		UIHelper.SetLabelText(RewardInfo.Label_Ruby, num);
		UIHelper.SetLabelText(RewardInfo.Label_Point, num2);
		int[] rewardItemIndices = _seasonRewardData.RewardItemIndices;
		int[] rewardItemCounts = _seasonRewardData.RewardItemCounts;
		bool flag = !__Extension_Method_ICollection__.IsNullOrEmpty(rewardItemIndices);
		UIHelper.EnableGameObject(RewardItemsRoot, flag);
		if (flag)
		{
			bool enable = __Extension_Method_ICollection__.COUNT(rewardItemIndices) > 1;
			UIHelper.EnableGameObject(SecondRewardItemRoot, enable);
			CreateRewardItem(0, rewardItemIndices, rewardItemCounts, FirstRewardItemIconTrs, FirstRewardItemCountLabel);
			CreateRewardItem(1, rewardItemIndices, rewardItemCounts, SecondRewardItemIconTrs, SecondRewardItemCountLabel);
		}
	}

	private void CreateRewardItem(int index, int[] itemIndices, int[] itemCounts, Transform root, UILabel countLabel)
	{
		int valueInt = ArgParser.GetValueInt(itemIndices, index);
		int valueInt2 = ArgParser.GetValueInt(itemCounts, index);
		if (valueInt == 0 || valueInt2 == 0 || root == null)
		{
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(valueInt);
		if (data != null)
		{
			ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(root);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.ResetPosition();
				itemIconComponent.Open(data);
				_itemIconList.Add(itemIconComponent);
				UIHelper.SetLabelText(countLabel, valueInt2);
			}
		}
	}

	public void Close()
	{
		FactoryBase.ReleaseAndClearList(_tierIconList);
		FactoryBase.ReleaseAndClearList(_itemIconList);
	}

	private new void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			Close();
		}
	}

	private void InitName(bool isChampion = false)
	{
		UIHelper.EnableGameObject(BronzeName, _tierType == GuildArenaTierType.Bronze);
		UIHelper.EnableGameObject(SilverName, _tierType == GuildArenaTierType.Silver);
		UIHelper.EnableGameObject(GoldName, _tierType == GuildArenaTierType.Gold);
		UIHelper.EnableGameObject(PlatinumName, _tierType == GuildArenaTierType.Platinum);
		UIHelper.EnableGameObject(DiamondName, _tierType == GuildArenaTierType.Diamond);
		UIHelper.EnableGameObject(MasterName, _tierType == GuildArenaTierType.Master);
		UIHelper.EnableGameObject(RoyalMasterName, _tierType == GuildArenaTierType.RoyalMaster);
		UIHelper.EnableGameObject(ChallengerName, _tierType == GuildArenaTierType.Challenger);
		UIHelper.EnableGameObject(ChampionName, _tierType == GuildArenaTierType.Champion);
	}

	private void SetTierIcon(GuildArenaRewardInfo rewardInfo)
	{
		MatchTierIconComponent matchTierIconComponent = MatchTierIconComponent.PrefabAuto.Factory.Create(rewardInfo.TierIconParent);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(matchTierIconComponent.transform);
		matchTierIconComponent.Open(_tierType);
		_tierIconList.Add(matchTierIconComponent);
	}
}
