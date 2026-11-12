using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NItem;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NDataHelper;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

public class ContentUIGodkingTrialDungeonInfo : BaseWindow, ContentUIGodkingTrialDifficultyNodeComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/GodkingTrial/ContentUIGodkingTrialDungeonInfo";

	private const string LOCALSTORAGE_KEY_GODKING_TRIAL_DECK = "GodkingTrial_Deck_";

	private const string LOCALSTORAGE_KEY_GOKDING_TRIAL_DIFFICULTY = "GodkingTrial_Difficulty_";

	public UILabel LabelDungeonNamePrefix;

	public UILabel LabelDungeonName;

	public UILabelAligner NameAligner;

	public Transform TransformFirstClearRewardAnchor;

	public UILabel LabelFirstClear;

	public UISprite SpriteFirstClearGetMark;

	public UIScrollGrid ScrollGridRewardList;

	public UIScrollGrid ScrollGridDifficultyNodeList;

	public UILabel LabelDifficulty;

	public UIButton ButtonPrev;

	public UIButton ButtonNext;

	public NodeTreeUIPlayer NodeTreePlayer;

	public WindowAnimation WindowAni;

	public TweenListPlayer TweenListPlayer;

	public UIGame3DWidget TrialTouchEffect;

	public float SpringStrength = 13f;

	private GodkingTrialGroupData _groupData;

	private GodkingTrialData _data;

	private GodkingTrialDungeonInfo _info;

	private BanRuleData _banRuleData;

	private CampaignDungeonData _dungeonData;

	private int _difficulty = 1;

	private int _minDifficulty = 1;

	private int _maxDifficulty = 7;

	private RewardElementData _dataFirstClear;

	private ItemIconComponent _compFirstClearReward;

	private List<RewardElementData> _dataListRewards = new List<RewardElementData>();

	private List<ItemIconComponent> _compListRewards = new List<ItemIconComponent>();

	private List<int> _dataListDifficultyNodes = new List<int>();

	private List<ContentUIGodkingTrialDifficultyNodeComponent> _compListDifficultyNodes = new List<ContentUIGodkingTrialDifficultyNodeComponent>();

	private PrefabAuto<ItemIconComponent> _prefabAutoReward = new PrefabAuto<ItemIconComponent>("Prefab2/Component/Icon/ItemIconComponent");

	private PrefabAuto<ContentUIGodkingTrialDifficultyNodeComponent> _prefabAutoDifficultyNode = new PrefabAuto<ContentUIGodkingTrialDifficultyNodeComponent>("Resources/Prefab2/Window/GodkingTrial/ContentUIGodkingTrialDifficultyNodeComponent");

	public const string UI_GodKing_BGM = "EntryMenu.KainAscensionBGM";

	private string _beforeBGM = string.Empty;

	public void Open(int chapterIndex)
	{
		base.Open();
		_minDifficulty = GodkingTrialHelper.GetMinDifficulty(chapterIndex);
		_maxDifficulty = GodkingTrialHelper.GetMaxDifficulty(chapterIndex);
		_difficulty = LoadDifficulty(chapterIndex);
		SetData(chapterIndex);
		UpdateUI();
		PlayBGM();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.Stop();
		}
		if (!SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance || !SceneAutoBehaviour<SoulWeaponManagement>.instance.IsOpened<ContentUIGodkingTrialDungeonList>())
		{
			SingletonAuto<BgmManager2>.instance.StopEvent("EntryMenu.KainAscensionBGM");
			if (_beforeBGM != "EntryMenu.KainAscensionBGM")
			{
				SingletonAuto<BgmManager2>.instance.Request(_beforeBGM, isEvent: true);
			}
		}
	}

	public void SetData(int chapterIndex)
	{
		GodkingTrialGroupData data = GameRoot.Instance.GodkingTrialGroupDataContainer.GetData(chapterIndex);
		if (data != null)
		{
			_groupData = data;
			BanRuleData data2 = GameRoot.Instance.BanRuleDataContainer.GetData(_groupData.BanRuleIndex);
			if (data2 != null)
			{
				_banRuleData = data2;
			}
		}
		GodkingTrialData data3 = GameRoot.Instance.GodkingTrialDataContainer.GetData(chapterIndex, _difficulty);
		if (data3 != null)
		{
			_data = data3;
			_dungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_data.ChapterIndex, _data.DungeonIndex);
		}
		GodkingTrialDungeonInfo godkingTrialDungeonInfo = GlobalAutoBehaviour<AccountManager>.instance.GodkingTrialDungeonInfoManager.Get(chapterIndex);
		if (godkingTrialDungeonInfo != null)
		{
			_info = godkingTrialDungeonInfo;
		}
		UpdateDataFirstClearReward();
		UpdateDataRewardList();
		UpdateDataDifficultyList();
	}

	public void UpdateUI()
	{
		if (NodeTreePlayer != null && _groupData != null)
		{
			NodeTreePlayer.PlayOnce(_groupData.BgNodeTreePath, _groupData.BgNodeTreeCode);
		}
		UpdateDungeonName();
		UpdateUIFirstClearReward();
		UpdateUIFirstClearRewardGetMark();
		UpdateUIRewardList();
		UpdateUIDifficultyList();
		UpdateUIDifficultyLabel();
		UpdateButtonStateDifficulty();
		SetDifficulty(_difficulty);
	}

	public override void Reset()
	{
		_dataFirstClear = null;
		FactoryBase.SafeReleaseComponent(ref _compFirstClearReward);
		_dataListRewards.Clear();
		FactoryBase.ReleaseAndClearList(_compListRewards);
		_dataListDifficultyNodes.Clear();
		FactoryBase.ReleaseAndClearList(_compListDifficultyNodes);
	}

	private void UpdateDungeonName()
	{
		if (_groupData != null)
		{
			UIHelper.SetLabelText(LabelDungeonNamePrefix, LocalizationManager.GetStringDirect(_groupData.DungeonNamePrefixKey));
			UIHelper.SetLabelText(LabelDungeonName, LocalizationManager.GetStringDirect(_groupData.DungeonNameKey));
			if (NameAligner != null)
			{
				NameAligner.Align();
			}
		}
	}

	private void UpdateDataFirstClearReward()
	{
		if (_dungeonData == null)
		{
			return;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_dungeonData.FirstClearRewardIndex_Easy);
		if (data != null)
		{
			List<RewardElementData> rewardItemList = data.GetRewardItemList();
			_dataFirstClear = null;
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(rewardItemList, 0))
			{
				_dataFirstClear = rewardItemList[0];
			}
		}
	}

	private void UpdateDataRewardList()
	{
		if (_data == null)
		{
			return;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_data.RewardIndex);
		if (data == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		_dataListRewards.Clear();
		int i = 0;
		for (int count = rewardItemList.Count; i < count; i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			if (rewardElementData != null)
			{
				_dataListRewards.Add(rewardElementData);
			}
		}
	}

	private void UpdateUIFirstClearReward()
	{
		if (!(TransformFirstClearRewardAnchor == null) && _dataFirstClear != null)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(_dataFirstClear.ItemCode);
			if (dataByCode == null)
			{
				VespaDebug.LogError("ItemData == null");
				return;
			}
			FactoryBase.SafeReleaseComponent(ref _compFirstClearReward);
			ItemIconComponent itemIconComponent = _prefabAutoReward.Factory.Create(TransformFirstClearRewardAnchor);
			itemIconComponent.transform.localPosition = default(Vector3);
			itemIconComponent.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			itemIconComponent.Open(dataByCode);
			int itemMin = _dataFirstClear.ItemMin;
			int itemMax = _dataFirstClear.ItemMax;
			Color? color = null;
			int num = itemMax;
			int num2 = itemMin;
			itemIconComponent.SetItemMinMax(num2, num, color);
			_compFirstClearReward = itemIconComponent;
		}
	}

	private void UpdateUIFirstClearRewardGetMark()
	{
		if (SpriteFirstClearGetMark == null)
		{
			return;
		}
		if (_data == null)
		{
			UIHelper.EnableGameObject(LabelFirstClear, enable: false);
			UIHelper.EnableGameObject(SpriteFirstClearGetMark, enable: false);
			return;
		}
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_data.ChapterIndex, _data.DungeonIndex);
		UIHelper.EnableGameObject(SpriteFirstClearGetMark.gameObject, flag);
		UIHelper.EnableGameObject(LabelFirstClear, !flag);
		if (_compFirstClearReward != null)
		{
			_compFirstClearReward.SetIconGrayScale(flag);
		}
	}

	private void UpdateUIRewardList()
	{
		if (ScrollGridRewardList == null)
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_compListRewards);
		int i = 0;
		for (int count = _dataListRewards.Count; i < count; i++)
		{
			RewardElementData rewardElementData = _dataListRewards[i];
			if (rewardElementData == null)
			{
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = ScrollGridRewardList.AddGridItem(_prefabAutoReward);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(dataByCode);
					int itemMin = rewardElementData.ItemMin;
					int itemMax = rewardElementData.ItemMax;
					Color? color = null;
					int num = itemMax;
					int num2 = itemMin;
					itemIconComponent.SetItemMinMax(num2, num, color);
					_compListRewards.Add(itemIconComponent);
				}
			}
		}
		SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(_data.ChapterIndex, _data.DungeonIndex);
		if (data != null)
		{
			List<SelectRewardData.SelectRewardItemInfo> selectRewardItemList = data.GetSelectRewardItemList();
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(selectRewardItemList); j++)
			{
				SelectRewardData.SelectRewardItemInfo selectRewardItemInfo = selectRewardItemList[j];
				ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(selectRewardItemInfo.Code);
				if (dataByCode2 != null)
				{
					ItemIconComponent itemIconComponent2 = UIHelper.AddGridItem(ScrollGridRewardList, _prefabAutoReward);
					itemIconComponent2.Open(dataByCode2);
					int count2 = selectRewardItemInfo.Count;
					Color? color2 = null;
					int num3 = count2;
					itemIconComponent2.SetItemCount(num3, color2);
					_compListRewards.Add(itemIconComponent2);
				}
			}
		}
		ScrollGridRewardList.Grid.sorting = UIGrid.Sorting.Custom;
		ScrollGridRewardList.Grid.onCustomSort = SortItem;
		ScrollGridRewardList.RepositionGrid();
	}

	private int SortItem(Transform l, Transform r)
	{
		ItemIconComponent component = l.GetComponent<ItemIconComponent>();
		ItemIconComponent component2 = r.GetComponent<ItemIconComponent>();
		int num = ItemComparer.NullCheck(component, component2);
		if (num != 0)
		{
			return num;
		}
		ItemData itemData = component.ItemData;
		ItemData itemData2 = component2.ItemData;
		if (itemData == null || itemData2 == null)
		{
			return num;
		}
		ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
		sortClass.Add(ItemComparer.Compare, itemData.Tier, itemData.Tier, isAscending: false).Add(ItemComparer.Compare_ItemGrade, itemData.Grade, itemData2.Grade, isAscending: false).Add(ItemComparer.Compare, itemData.Index, itemData2.Index, isAscending: true);
		return sortClass.Sort();
	}

	private void UpdateDataDifficultyList()
	{
		_dataListDifficultyNodes.Clear();
		int minDifficulty = GodkingTrialHelper.GetMinDifficulty(_data.ChapterIndex);
		int maxDifficulty = GodkingTrialHelper.GetMaxDifficulty(_data.ChapterIndex);
		int i = minDifficulty;
		for (int num = maxDifficulty; i <= num; i++)
		{
			_dataListDifficultyNodes.Add(i);
		}
	}

	private void UpdateUIDifficultyList()
	{
		if (ScrollGridDifficultyNodeList == null)
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_compListDifficultyNodes);
		int i = 0;
		for (int count = _dataListDifficultyNodes.Count; i < count; i++)
		{
			int difficulty = _dataListDifficultyNodes[i];
			ContentUIGodkingTrialDifficultyNodeComponent contentUIGodkingTrialDifficultyNodeComponent = ScrollGridDifficultyNodeList.AddGridItem(_prefabAutoDifficultyNode);
			if (!(contentUIGodkingTrialDifficultyNodeComponent == null))
			{
				ContentUIGodkingTrialDifficultyNodeComponent.LinkDirection linkDir = ((i != count - 1) ? ContentUIGodkingTrialDifficultyNodeComponent.LinkDirection.Next : ContentUIGodkingTrialDifficultyNodeComponent.LinkDirection.None);
				contentUIGodkingTrialDifficultyNodeComponent.Open(linkDir, difficulty, this);
				_compListDifficultyNodes.Add(contentUIGodkingTrialDifficultyNodeComponent);
			}
		}
		ScrollGridDifficultyNodeList.RepositionGrid();
	}

	private void UpdateUIDifficultyLabel()
	{
		string text = LocalizationManager.GetString(GodkingTrial.DUNGEON_DIFFICULTY_REWARD, _difficulty);
		UIHelper.SetLabelText(LabelDifficulty, text);
	}

	private void UpdateButtonStateDifficulty()
	{
		UIHelper.EnableButton(ButtonPrev, IsPossiblePrev());
		UIHelper.EnableButton(ButtonNext, IsPossibleNext());
	}

	private DefaultBattlePartySettingDataProdiver BuildupDungeonDataProvider()
	{
		if (_data == null)
		{
			return null;
		}
		string storageKey = "GodkingTrial_Deck_" + _data.ChapterIndex;
		EnterDungeonData enterDungeonData = DungeonHelper.GetEnterDungeonData(_data.ChapterIndex, _data.DungeonIndex);
		if (enterDungeonData == null)
		{
			return null;
		}
		if (enterDungeonData.OriginalDungeonData == null)
		{
			return null;
		}
		int reqStamina = enterDungeonData.OriginalDungeonData.ReqStamina;
		object obj = Battle.START_BATTLE;
		BanRuleData banRuleData = _banRuleData;
		EnterDungeonData enterDungeon = enterDungeonData;
		object obj2 = obj;
		int num = reqStamina;
		return new DefaultBattlePartySettingDataProdiver(storageKey, StaminaType.GodkingTrialKey, num, (Enum)obj2, enterDungeon, banRuleData);
	}

	private bool IsPossiblePrev()
	{
		int num = _difficulty - 1;
		if (GodkingTrialHelper.GetMinDifficulty(_data.ChapterIndex) > num)
		{
			return false;
		}
		return true;
	}

	private bool IsPossibleNext()
	{
		int num = _difficulty + 1;
		if (GodkingTrialHelper.GetMaxDifficulty(_data.ChapterIndex) < num)
		{
			return false;
		}
		return true;
	}

	private void SetDifficulty(int difficulty)
	{
		_difficulty = difficulty;
		if (_data != null)
		{
			SaveDifficulty(_difficulty, _data.ChapterIndex);
		}
		GodkingTrialData data = GameRoot.Instance.GodkingTrialDataContainer.GetData(_groupData.ChapterIndex, _difficulty);
		if (data != null)
		{
			_data = data;
			_dungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_data.ChapterIndex, _data.DungeonIndex);
			UpdateDataFirstClearReward();
			UpdateUIFirstClearReward();
			UpdateUIFirstClearRewardGetMark();
			UpdateDataRewardList();
		}
		int i = 0;
		for (int count = _compListDifficultyNodes.Count; i < count; i++)
		{
			ContentUIGodkingTrialDifficultyNodeComponent contentUIGodkingTrialDifficultyNodeComponent = _compListDifficultyNodes[i];
			if (!(contentUIGodkingTrialDifficultyNodeComponent == null))
			{
				contentUIGodkingTrialDifficultyNodeComponent.SetSelect(contentUIGodkingTrialDifficultyNodeComponent.Difficulty == difficulty);
			}
		}
		if (ScrollGridDifficultyNodeList != null)
		{
			ScrollToWithConstraintBounds(_difficulty);
		}
		if (TweenListPlayer != null)
		{
			TweenListPlayer.PlayTween();
		}
		UpdateUIDifficultyLabel();
		UpdateUIRewardList();
		UpdateButtonStateDifficulty();
	}

	private void SaveDifficulty(int difficulty, int chapterIndex)
	{
		LocalAccountInt localAccountInt = new LocalAccountInt(_minDifficulty, "GodkingTrial_Difficulty_" + chapterIndex);
		if (localAccountInt != null)
		{
			localAccountInt.Value = difficulty;
		}
	}

	private int LoadDifficulty(int chapterIndex)
	{
		return new LocalAccountInt(_minDifficulty, "GodkingTrial_Difficulty_" + chapterIndex)?.Value ?? _minDifficulty;
	}

	private void ScrollToWithConstraintBounds(int difficulty)
	{
		if (!ScrollGridDifficultyNodeList.ScrollView.shouldMoveHorizontally)
		{
			return;
		}
		int num = _dataListDifficultyNodes.FindIndex((int value) => value == difficulty);
		if (num != -1)
		{
			ContentUIGodkingTrialDifficultyNodeComponent contentUIGodkingTrialDifficultyNodeComponent = _compListDifficultyNodes[num];
			Transform cachedTransform = ScrollGridDifficultyNodeList.ScrollView.panel.cachedTransform;
			Vector3[] worldCorners = ScrollGridDifficultyNodeList.ScrollView.panel.worldCorners;
			Vector3 vector2;
			if (num <= 2)
			{
				Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[0]);
				vector2 = cachedTransform.InverseTransformPoint(_compListDifficultyNodes[0].transform.position) - vector;
				vector2.x -= ScrollGridDifficultyNodeList.Grid.cellWidth / 2f;
			}
			else if (num >= _dataListDifficultyNodes.Count - 3)
			{
				Vector3 vector3 = cachedTransform.InverseTransformPoint(worldCorners[2]);
				vector2 = cachedTransform.InverseTransformPoint(_compListDifficultyNodes[_dataListDifficultyNodes.Count - 1].transform.position) - vector3;
				vector2.x += ScrollGridDifficultyNodeList.Grid.cellWidth / 2f;
			}
			else
			{
				Vector3 position = (worldCorners[2] + worldCorners[0]) * 0.5f;
				Vector3 vector4 = cachedTransform.InverseTransformPoint(contentUIGodkingTrialDifficultyNodeComponent.transform.position);
				Vector3 vector5 = cachedTransform.InverseTransformPoint(position);
				vector2 = vector4 - vector5;
			}
			if (!ScrollGridDifficultyNodeList.ScrollView.canMoveHorizontally)
			{
				vector2.x = 0f;
			}
			if (!ScrollGridDifficultyNodeList.ScrollView.canMoveVertically)
			{
				vector2.y = 0f;
			}
			vector2.z = 0f;
			Vector3 pos = cachedTransform.localPosition - vector2;
			SpringPanel.Begin(ScrollGridDifficultyNodeList.ScrollView.panel.cachedGameObject, pos, SpringStrength).onFinished = delegate
			{
				ScrollGridDifficultyNodeList.ScrollView.RestrictWithinBounds(instant: true);
			};
		}
	}

	private void PlayBGM()
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		_beforeBGM = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		if (_beforeBGM != "EntryMenu.KainAscensionBGM")
		{
			SingletonAuto<BgmManager2>.instance.Request("EntryMenu.KainAscensionBGM", isEvent: true);
		}
	}

	void ContentUIGodkingTrialDifficultyNodeComponent.IListener.OnClickNode(ContentUIGodkingTrialDifficultyNodeComponent comp)
	{
		SetDifficulty(comp.Difficulty);
	}

	public void OnClickBossInfo()
	{
		if (_groupData != null && _data != null)
		{
			PopupUIGodkingTrialBossInfo.OpenBossInfo(_groupData.ChapterIndex, _data.DungeonIndex, _groupData);
		}
	}

	public void OnClickEnter()
	{
		DefaultBattlePartySettingDataProdiver defaultBattlePartySettingDataProdiver = BuildupDungeonDataProvider();
		if (defaultBattlePartySettingDataProdiver != null)
		{
			SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenBattlePartySetting(defaultBattlePartySettingDataProdiver);
		}
	}

	public void OnClickDifficultyPrev()
	{
		int difficulty = _difficulty - 1;
		SetDifficulty(difficulty);
	}

	public void OnClickDifficultyNext()
	{
		int difficulty = _difficulty + 1;
		SetDifficulty(difficulty);
	}

	public void OnClickTrialTouchEffect()
	{
		UIHelper.Play3DWidgetEffect(TrialTouchEffect, CheckPlay: false);
	}
}
