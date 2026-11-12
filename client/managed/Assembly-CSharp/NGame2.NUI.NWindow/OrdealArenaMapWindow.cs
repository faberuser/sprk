using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NOrdeal;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OrdealArenaMapWindow : BaseWindow
{
	public interface IListener
	{
		void OnClickBack();

		void OnClickBattlePartySetting();

		void OnClickRefreshNode(int nodeIndex);

		void OnClickRecievedReward();

		void OnClickStartEvent();
	}

	[Serializable]
	public class Group
	{
		public EOrdealArenaNodeType NodeType;

		public GameObject Root;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/OrdealArena/OrdealArenaMapWindow";

	public Group[] Groups;

	[Header("Group Start")]
	public UILabel LabelStartDesc;

	public Transform SelectorStartTier;

	public UISpriteSelector SelectorStartTierName;

	[Header("Group Event")]
	public UIButton ButtonEventStart;

	public UILabel LabelEventDesc;

	[Header("Group Battle")]
	public UILabel LabelBattleUserLevel;

	public UILabel LabelBattleUserName;

	public UITexture TextureCountryFlag;

	public UIButton ButtonBattleStart;

	public UIButton ButtonBattleRefresh;

	public Transform SelectorBattleTier;

	public UIBundleLoader BundleLoaderUserPortrait;

	public UISimpleGrid GridHeroPotrait;

	public UILabel LabelBattleScore;

	public UILabel LabelBattleWin;

	public UILabel LabelBattleLose;

	[Header("Group End")]
	public UILabel LabelEndDesc;

	public UIButton ButtonEndReward;

	public UISimpleGrid GridEndRewardItem;

	[Header("MyInfo")]
	public UISimpleGrid GridBuff;

	public UILabel LabelMyMatchScore;

	public UILabel LabelMyPoint;

	public UILabel LabelFloor;

	private OrdealArenaNodeData _curNodeData;

	private IListener _listener;

	private List<ItemIconComponent> _listItemIconComponent = new List<ItemIconComponent>();

	private List<HeroPortraitComponent> _listHeroPortraitComponent = new List<HeroPortraitComponent>();

	private List<OrdealArenaBuffIconComponent> _listBuffIconComponents = new List<OrdealArenaBuffIconComponent>();

	private List<MyMatchTierIconComponent> _tierIconList = new List<MyMatchTierIconComponent>();

	public void Open(IListener listener)
	{
		_listener = listener;
		Open(null, null);
		SetBuffList();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		FactoryBase.ReleaseAndClearList(_listHeroPortraitComponent);
		FactoryBase.ReleaseAndClearList(_listBuffIconComponents);
		FactoryBase.ReleaseAndClearList(_listItemIconComponent);
		ClearTierIconComponent();
	}

	public void SetDungeonInfo(int chapterIndex, int dungeonIndex)
	{
		OrdealArenaNodeData data = GameRoot.Instance.OrdealArenaNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null && IsShowMapInfo(data.NodeType))
		{
			_curNodeData = data;
			ClearTierIconComponent();
			SetDungeonInfo(data);
			SetMyInfo(data);
		}
	}

	private void SetMyInfo(OrdealArenaNodeData nodeData)
	{
		int matchScore = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetMatchScore();
		int completeFloor = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetCompleteFloor();
		int maxFloor = GameRoot.Instance.OrdealArenaNodeDataContainer.GetMaxFloor(nodeData.ChapterIndex);
		long num = GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Get(CurrencyType.OrdealArenaPoint)?.Amount ?? 0;
		UIHelper.SetLabelText(LabelMyMatchScore, matchScore);
		UIHelper.SetLabelText(LabelFloor, $"{completeFloor} / {maxFloor}");
		UIHelper.SetLabelText(LabelMyPoint, num);
	}

	private void SetDungeonInfo(OrdealArenaNodeData nodeData)
	{
		OnOffGroup(nodeData.NodeType);
		switch (nodeData.NodeType)
		{
		case EOrdealArenaNodeType.Start:
			SetNodeStart(nodeData);
			break;
		case EOrdealArenaNodeType.Battle:
			SetNodeBattle(nodeData);
			break;
		case EOrdealArenaNodeType.Event:
			SetNodeEvent(nodeData);
			break;
		case EOrdealArenaNodeType.End:
			SetNodeEnd(nodeData);
			break;
		}
	}

	public void SetBuffList()
	{
		FactoryBase.ReleaseAndClearList(_listBuffIconComponents);
		List<int> selectedBuffList = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedBuffList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(selectedBuffList))
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (int item in selectedBuffList)
		{
			if (!dictionary.ContainsKey(item))
			{
				dictionary.Add(item, 1);
			}
			else
			{
				dictionary[item]++;
			}
		}
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			int key = item2.Key;
			int value = item2.Value;
			OrdealArenaBuffData data = GameRoot.Instance.OrdealArenaBuffDataContainer.GetData(key);
			if (data != null)
			{
				OrdealArenaBuffIconComponent ordealArenaBuffIconComponent = GridBuff.AddGridItem(OrdealArenaBuffIconComponent.PrefabAuto);
				if (!(ordealArenaBuffIconComponent == null))
				{
					ordealArenaBuffIconComponent.SetInfo(data, value);
					_listBuffIconComponents.Add(ordealArenaBuffIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(GridBuff);
	}

	private void SetSpriteSelector(UISpriteSelector selector, string tier)
	{
		if (!(selector == null) && !String_Extension_Method.IsNullOrEmpty(tier))
		{
			selector.Select(tier);
		}
	}

	private void CreateTierIconComponent(Transform parentTransform, MatchTierType tierType)
	{
		MyMatchTierIconComponent myMatchTierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(parentTransform);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(myMatchTierIconComponent.transform);
		myMatchTierIconComponent.Open(tierType, 0, VisualTierRank.NoRank, MatchArenaType.Ordeal);
		myMatchTierIconComponent.HideName();
		myMatchTierIconComponent.DisableLevel();
		_tierIconList.Add(myMatchTierIconComponent);
	}

	private void ClearTierIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_tierIconList);
	}

	private void SetNodeStart(OrdealArenaNodeData nodeData)
	{
		MatchTierType selectedTier = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedTier();
		string tier = selectedTier.ToString();
		SetSpriteSelector(SelectorStartTierName, tier);
		CreateTierIconComponent(SelectorStartTier, selectedTier);
		string tierText = OrdealHelper.GetTierText(selectedTier);
		string stringDirect = LocalizationManager.GetStringDirect(nodeData.DescKey, tierText);
		UIHelper.SetLabelText(LabelStartDesc, stringDirect);
	}

	private void SetNodeEvent(OrdealArenaNodeData nodeData)
	{
		int chapterIndex = nodeData.ChapterIndex;
		int dungeonIndex = nodeData.DungeonIndex;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
		UIHelper.EnableButton(ButtonEventStart, !flag);
		string stringDirect = LocalizationManager.GetStringDirect(nodeData.DescKey);
		UIHelper.SetLabelText(LabelEventDesc, stringDirect);
	}

	private void SetNodeBattle(OrdealArenaNodeData nodeData)
	{
		int chapterIndex = nodeData.ChapterIndex;
		int dungeonIndex = nodeData.DungeonIndex;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
		UIHelper.EnableButton(ButtonBattleStart, !flag);
		UIHelper.EnableButton(ButtonBattleRefresh, !flag);
		OrdealNodeInfo ordealNodeInfo = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetOrdealNodeInfo(nodeData.Index);
		if (ordealNodeInfo == null || ordealNodeInfo.AccountInfo == null || __Extension_Method_ICollection__.IsNullOrEmpty(ordealNodeInfo.AccountInfo.HeroInfos))
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_listHeroPortraitComponent);
		foreach (KeyValuePair<int, CachedHeroInfo> heroInfo in ordealNodeInfo.AccountInfo.HeroInfos)
		{
			CachedHeroInfo value = heroInfo.Value;
			if (value != null)
			{
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(GridHeroPotrait, HeroPortraitComponent.SmallPrefabAuto);
				if (heroPortraitComponent == null)
				{
					break;
				}
				heroPortraitComponent.Open(value, null, null, HeroPortraitComponent.EPortraitMode.Icon);
				_listHeroPortraitComponent.Add(heroPortraitComponent);
			}
		}
		CachedAccountInfo accountInfo = ordealNodeInfo.AccountInfo;
		UIHelper.RepositionGrid(GridHeroPotrait);
		UIHelper.SetLabelText(LabelBattleUserLevel, $"Lv.{accountInfo.UserInfo.TeamLevel}");
		UIHelper.SetLabelText(LabelBattleUserName, accountInfo.UserInfo.Nick);
		string countryCode = ((!String_Extension_Method.IsNullOrEmpty(ordealNodeInfo.CountryCode)) ? ordealNodeInfo.CountryCode : "VESPA");
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, TextureCountryFlag, plainTexture);
		}
		UIHelper.SetLabelText(LabelBattleScore, ordealNodeInfo.Rank);
		UIHelper.SetLabelText(LabelBattleWin, accountInfo.UserInfo.MatchScore.ToString());
		int num = accountInfo.UserInfo.SeasonWin + accountInfo.UserInfo.SeasonLose;
		int num2 = ((num != 0) ? ((int)((float)accountInfo.UserInfo.SeasonWin / (float)num * 100f)) : 0);
		UIHelper.SetLabelText(LabelBattleLose, $"{num2}%");
		UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(accountInfo.UserInfo.AvatarHeroIndex);
		if (avatarInfo.GetCreatureData() != null)
		{
			string codeName = avatarInfo.GetCreatureData().CodeName;
			int star = avatarInfo.Star;
			int costumeIndex = avatarInfo.CostumeIndex;
			string battleHeroPortraitPath = UIHelper.GetBattleHeroPortraitPath(codeName, star, costumeIndex);
			UIHelper.SetBundleTexture(BundleLoaderUserPortrait, battleHeroPortraitPath);
		}
		MatchTierType selectedTier = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedTier();
		CreateTierIconComponent(SelectorBattleTier, selectedTier);
	}

	private void SetNodeEnd(OrdealArenaNodeData nodeData)
	{
		int chapterIndex = nodeData.ChapterIndex;
		int dungeonIndex = nodeData.DungeonIndex;
		MatchTierType selectedTier = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedTier();
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
		UIHelper.EnableButton(ButtonEndReward, !flag);
		string tierText = OrdealHelper.GetTierText(selectedTier);
		string stringDirect = LocalizationManager.GetStringDirect(nodeData.DescKey, tierText);
		UIHelper.SetLabelText(LabelEndDesc, stringDirect);
		FactoryBase.ReleaseAndClearList(_listItemIconComponent);
		if (GridEndRewardItem == null || _curNodeData == null || _curNodeData.RewardIndex == 0)
		{
			return;
		}
		int rewardIndex = _curNodeData.RewardIndex;
		OrdealArenaRewardAreaData data = GameRoot.Instance.OrdealArenaRewardAreaDataContainer.GetData(rewardIndex, selectedTier);
		if (data == null)
		{
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.RewardIndex);
		if (data2 == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data2.GetRewardItemList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			if (rewardElementData != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(GridEndRewardItem, ItemIconComponent.PrefabAuto);
				if (itemIconComponent == null)
				{
					break;
				}
				HashString itemCode = rewardElementData.ItemCode;
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
				if (dataByCode != null)
				{
					int itemMin = rewardElementData.ItemMin;
					long count = itemMin;
					itemIconComponent.Open(dataByCode);
					itemIconComponent.SetItemCount(count);
					_listItemIconComponent.Add(itemIconComponent);
				}
			}
		}
	}

	private void OnOffGroup(EOrdealArenaNodeType nodeType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Groups); i++)
		{
			Group obj = Groups[i];
			if (obj == null)
			{
				break;
			}
			bool enable = nodeType == obj.NodeType;
			UIHelper.EnableGameObject(obj.Root, enable);
		}
	}

	private bool IsShowMapInfo(EOrdealArenaNodeType nodeType)
	{
		if (nodeType == EOrdealArenaNodeType.Battle || nodeType == EOrdealArenaNodeType.End || nodeType == EOrdealArenaNodeType.Event || nodeType == EOrdealArenaNodeType.Start)
		{
			return true;
		}
		return false;
	}

	public void OnClickBattlePartySetting()
	{
		if (_listener != null)
		{
			_listener.OnClickBattlePartySetting();
		}
	}

	public void OnClickBack()
	{
		if (_listener != null)
		{
			_listener.OnClickBack();
			_curNodeData = null;
		}
	}

	public void OnClickRefreshNode()
	{
		if (_listener != null && _curNodeData != null)
		{
			_listener.OnClickRefreshNode(_curNodeData.Index);
		}
	}

	public void OnClickReward()
	{
		if (_listener != null && _curNodeData != null)
		{
			_listener.OnClickRecievedReward();
		}
	}

	public void OnClickEventStart()
	{
		if (_listener != null && _curNodeData != null)
		{
			_listener.OnClickStartEvent();
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickBack();
	}

	public override void Close()
	{
		OnClickBack();
	}
}
