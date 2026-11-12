using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUI.NHelper.NItem;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ShakmehDungeonWindow : BaseWindow, LobbyTopmostMenu.ICustomStaminaObserver, DifficultyNodeComponent.IListener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, ISweepDungeonWindow
{
	public interface Listener
	{
		void OnClickEnter(int chapterIndex, int index, int level, ShakmehBossType shakmehBossType);

		void OnClickInfo(int chapterIndex, int dungeonIndex, ShakmehBossData shakmehBossData);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/ShakmehDungeon/ShakmehDungeonWindow";

	public UILabel BossNamePrefixLabel;

	public UILabel BossNameLabel;

	public UILabelAligner NameAligner;

	public NodeTreeUIPlayer NodeTreePlayer;

	public Transform FirstClearRewardRoot;

	public UILabel FirstClearLabel;

	public UISprite FirstClearSprite;

	public UILabel DifficultyLabel;

	public UIButton PrevButton;

	public UIButton NextButton;

	public UIScrollGrid RewardListGrid;

	public UIScrollGrid DifficultyNodeListGrid;

	public UIScrollGrid BuffListGrid;

	public UIButton BossInfoButton;

	public UIButton EnterButton;

	public UIButton SweepButton;

	public float SpringStrength = 13f;

	private Listener _listener;

	private ShakmehDungeonData _shakmehDungeonData;

	private ShakmehBossData _shakmehBossData;

	private ItemIconComponent _firstClearReward;

	private List<ItemIconComponent> _rewardList = new List<ItemIconComponent>();

	private List<DifficultyNodeComponent> _diffCompList = new List<DifficultyNodeComponent>();

	private List<SkillIconComponent> _buffIconList = new List<SkillIconComponent>();

	private int _level = 1;

	private int _minLevel = 1;

	private int _maxLevel = 7;

	private StaminaType _staminaType;

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType => _staminaType;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.Stamina && changedInfo.StaminaType == StaminaType.ShakmehMiddleBossKey)
		{
			SetButton();
		}
	}

	public void Open(Listener listener, ShakmehBossData shakmehBossData)
	{
		if (shakmehBossData == null)
		{
			VespaDebug.LogError("ShakmehBossData not found ");
			return;
		}
		_listener = listener;
		_shakmehBossData = shakmehBossData;
		_staminaType = ((_shakmehBossData.BossType == ShakmehBossType.Middle) ? StaminaType.ShakmehMiddleBossKey : StaminaType.Empty);
		_minLevel = GameRoot.Instance.ShakmehDungeonDataContainer.GetMinLevel(shakmehBossData.ShakmehIndex);
		_maxLevel = GameRoot.Instance.ShakmehDungeonDataContainer.GetMaxLevel(shakmehBossData.ShakmehIndex);
		_level = LoadLevel(shakmehBossData.ShakmehIndex);
		_shakmehDungeonData = GameRoot.Instance.ShakmehDungeonDataContainer.GetData(shakmehBossData.ShakmehIndex, _level);
		if (_shakmehDungeonData == null)
		{
			VespaDebug.LogError("ShakmehDungeonData not found ");
			return;
		}
		title.Key = shakmehBossData.DungeonNameKey;
		Open(Init, null);
	}

	public void Init()
	{
		InitBossName();
		InitNodeTreePlayer();
		InitLevelNode();
		InitBuffList();
		SetLevel(_level);
		SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyTopmostMenu(this);
		OpenShakmehDungeonWindow triggerEvent = new OpenShakmehDungeonWindow();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_diffCompList);
		ClearFirstClearReward();
		ClearRewardList();
		ClearBuffList();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.Stop();
		}
	}

	private void ClearFirstClearReward()
	{
		FactoryBase.ReleaseComponent(_firstClearReward);
		_firstClearReward = null;
	}

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
	}

	private void ClearBuffList()
	{
		FactoryBase.ReleaseAndClearList(_buffIconList);
	}

	private void InitBossName()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_shakmehBossData.DungeonPrefixKey);
		string stringDirect2 = LocalizationManager.GetStringDirect(_shakmehBossData.DungeonNameKey);
		UIHelper.SetLabelText(BossNamePrefixLabel, stringDirect);
		UIHelper.SetLabelText(BossNameLabel, stringDirect2);
		if (NameAligner != null)
		{
			NameAligner.Align();
		}
	}

	private void InitNodeTreePlayer()
	{
		string bgNodeTreePath = _shakmehBossData.BgNodeTreePath;
		string bgNodeTreeCode = _shakmehBossData.BgNodeTreeCode;
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.PlayOnce(bgNodeTreePath, bgNodeTreeCode);
		}
	}

	private void InitLevelNode()
	{
		if (DifficultyNodeListGrid == null)
		{
			return;
		}
		for (int i = 0; i < _maxLevel; i++)
		{
			int num = i + 1;
			DifficultyNodeComponent difficultyNodeComponent = UIHelper.AddGridItem(DifficultyNodeListGrid, DifficultyNodeComponent.PrefabAuto);
			if (!(difficultyNodeComponent == null))
			{
				DifficultyNodeComponent.LinkDirection linkDir = ((num != _maxLevel) ? DifficultyNodeComponent.LinkDirection.Next : DifficultyNodeComponent.LinkDirection.None);
				difficultyNodeComponent.Open(linkDir, num, this);
				_diffCompList.Add(difficultyNodeComponent);
			}
		}
		UIHelper.RepositionGrid(DifficultyNodeListGrid);
	}

	private void InitBuffList()
	{
		int[] projectileGroupIndexFromServer = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetProjectileGroupIndexFromServer(BattleType.Shakmeh);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(projectileGroupIndexFromServer); i++)
		{
			int key = projectileGroupIndexFromServer[i];
			SkillProjectileGroupData data = GameRoot.Instance.SkillProjectileGroupDataContainer.GetData(key);
			if (data == null)
			{
				return;
			}
			foreach (SkillProjectileGroupData.SkillProjectileInfo skillProjectileInfo in data.GetSkillProjectileInfos())
			{
				SkillProjectileData data2 = GameRoot.Instance.SkillProjectileDataContainer.GetData(skillProjectileInfo.Index);
				if (data2 != null)
				{
					SkillData data3 = GameRoot.Instance.SkillDataContainer.GetData(data2.SkillIndex);
					if (data3 != null)
					{
						SkillIconComponent skillIconComponent = BuffListGrid.AddGridItem(SkillIconComponent.PrefabAuto);
						skillIconComponent.Open(data3.Index);
						_buffIconList.Add(skillIconComponent);
					}
				}
			}
		}
		BuffListGrid.RepositionGrid();
	}

	private void RefreshUI()
	{
		SetLevelText();
		SetFirstClearReward();
		SetRewardList();
		SetButton();
	}

	private void SetLevel(int lev)
	{
		_level = lev;
		_shakmehDungeonData = GameRoot.Instance.ShakmehDungeonDataContainer.GetData(_shakmehBossData.ShakmehIndex, _level);
		if (_shakmehDungeonData == null)
		{
			VespaDebug.LogError("ShakmehDungeonData not found ");
			return;
		}
		SaveLevel(_shakmehBossData.ShakmehIndex, _level);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_diffCompList); i++)
		{
			DifficultyNodeComponent difficultyNodeComponent = _diffCompList[i];
			if (!(difficultyNodeComponent == null))
			{
				difficultyNodeComponent.SetSelect(difficultyNodeComponent.Difficulty == lev);
			}
		}
		if (DifficultyNodeListGrid != null)
		{
			ScrollToWithConstraintBounds(_level);
		}
		RefreshUI();
	}

	private void SetLevelText()
	{
		string text = LocalizationManager.GetString(GodkingTrial.DUNGEON_DIFFICULTY_REWARD, _level);
		UIHelper.SetLabelText(DifficultyLabel, text);
	}

	private void SetFirstClearReward()
	{
		ClearFirstClearReward();
		int firstClearRewardIndex = _shakmehDungeonData.FirstClearRewardIndex;
		_firstClearReward = UIHelper.CreateRewardMainItemIconComponent(firstClearRewardIndex, FirstClearRewardRoot);
		int chapterIndex = _shakmehBossData.ChapterIndex;
		int dungeonIndex = _shakmehDungeonData.DungeonIndex;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
		UIHelper.EnableGameObject(FirstClearSprite, flag);
		UIHelper.EnableGameObject(FirstClearLabel, !flag);
		if (_firstClearReward != null)
		{
			_firstClearReward.SetIconGrayScale(flag);
		}
	}

	private void SetRewardList()
	{
		ClearRewardList();
		int rewardIndex = _shakmehDungeonData.RewardIndex;
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardListGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(dataByCode);
					int itemMin = rewardElementData.ItemMin;
					Color? color = null;
					int num = itemMin;
					itemIconComponent.SetItemCount(num, color);
					_rewardList.Add(itemIconComponent);
				}
			}
		}
		SelectRewardData data2 = GameRoot.Instance.SelectRewardDataContainer.GetData(_shakmehBossData.ChapterIndex, _shakmehDungeonData.DungeonIndex);
		if (data2 != null)
		{
			List<SelectRewardData.SelectRewardItemInfo> selectRewardItemList = data2.GetSelectRewardItemList();
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(selectRewardItemList); j++)
			{
				SelectRewardData.SelectRewardItemInfo selectRewardItemInfo = selectRewardItemList[j];
				ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(selectRewardItemInfo.Code);
				if (dataByCode2 != null)
				{
					ItemIconComponent itemIconComponent2 = UIHelper.AddGridItem(RewardListGrid, ItemIconComponent.PrefabAuto);
					if (!(itemIconComponent2 == null))
					{
						itemIconComponent2.Open(dataByCode2);
						int count = selectRewardItemInfo.Count;
						Color? color2 = null;
						int num2 = count;
						itemIconComponent2.SetItemCount(num2, color2);
						_rewardList.Add(itemIconComponent2);
					}
				}
			}
		}
		RewardListGrid.RepositionGrid();
	}

	private void SetButton()
	{
		bool enable = IsPossibleEnter();
		bool enable2 = IsPossiblePrev();
		bool enable3 = IsPossibleNext();
		UIHelper.EnableButton(EnterButton, enable);
		UIHelper.EnableButton(PrevButton, enable2);
		UIHelper.EnableButton(NextButton, enable3);
		int chapterIndex = _shakmehBossData.ChapterIndex;
		int dungeonIndex = _shakmehDungeonData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		bool flag = SweepHelper.IsRemainStamina(data);
		bool flag2 = SweepHelper.IsPossibleSweepStep(data);
		UIHelper.EnableButton(SweepButton, flag && flag2, grayscaleIfDisable: true);
	}

	public void RefreshBySweep()
	{
		if (_shakmehBossData.BossType == ShakmehBossType.Final)
		{
			Close();
		}
		else if (SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.IsEnoughShakmehBossGauge)
		{
			Close();
		}
		else
		{
			SetButton();
		}
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

	private bool IsPossibleEnter()
	{
		if (_shakmehDungeonData == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(_shakmehDungeonData.OpenCondition) < 2)
		{
			return true;
		}
		int chapterIndex = _shakmehDungeonData.OpenCondition[0];
		int dungeonIndex = _shakmehDungeonData.OpenCondition[1];
		return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
	}

	private bool IsPossiblePrev()
	{
		int num = _level - 1;
		if (_minLevel > num)
		{
			return false;
		}
		return true;
	}

	private bool IsPossibleNext()
	{
		int num = _level + 1;
		if (_maxLevel < num)
		{
			return false;
		}
		return true;
	}

	private void SaveLevel(int ShakmehIndex, int level)
	{
		LocalAccountInt localAccountInt = new LocalAccountInt(_minLevel, "ShakmehDungeon_Level_" + ShakmehIndex);
		if (localAccountInt != null)
		{
			localAccountInt.Value = level;
		}
	}

	private int LoadLevel(int ShakmehIndex)
	{
		return new LocalAccountInt(_minLevel, "ShakmehDungeon_Level_" + ShakmehIndex)?.Value ?? _minLevel;
	}

	private void ScrollToWithConstraintBounds(int level)
	{
		if (!DifficultyNodeListGrid.ScrollView.shouldMoveHorizontally)
		{
			return;
		}
		int num = _diffCompList.FindIndex((DifficultyNodeComponent value) => value.Difficulty == level);
		if (num != -1)
		{
			DifficultyNodeComponent difficultyNodeComponent = _diffCompList[num];
			Transform cachedTransform = DifficultyNodeListGrid.ScrollView.panel.cachedTransform;
			Vector3[] worldCorners = DifficultyNodeListGrid.ScrollView.panel.worldCorners;
			Vector3 vector2;
			if (num <= 2)
			{
				Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[0]);
				vector2 = cachedTransform.InverseTransformPoint(_diffCompList[0].transform.position) - vector;
				vector2.x -= DifficultyNodeListGrid.Grid.cellWidth / 2f;
			}
			else if (num >= _diffCompList.Count - 3)
			{
				Vector3 vector3 = cachedTransform.InverseTransformPoint(worldCorners[2]);
				vector2 = cachedTransform.InverseTransformPoint(_diffCompList[_diffCompList.Count - 1].transform.position) - vector3;
				vector2.x += DifficultyNodeListGrid.Grid.cellWidth / 2f;
			}
			else
			{
				Vector3 position = (worldCorners[2] + worldCorners[0]) * 0.5f;
				Vector3 vector4 = cachedTransform.InverseTransformPoint(difficultyNodeComponent.transform.position);
				Vector3 vector5 = cachedTransform.InverseTransformPoint(position);
				vector2 = vector4 - vector5;
			}
			if (!DifficultyNodeListGrid.ScrollView.canMoveHorizontally)
			{
				vector2.x = 0f;
			}
			if (!DifficultyNodeListGrid.ScrollView.canMoveVertically)
			{
				vector2.y = 0f;
			}
			vector2.z = 0f;
			Vector3 pos = cachedTransform.localPosition - vector2;
			SpringPanel.Begin(DifficultyNodeListGrid.ScrollView.panel.cachedGameObject, pos, SpringStrength).onFinished = delegate
			{
				DifficultyNodeListGrid.ScrollView.RestrictWithinBounds(instant: true);
			};
		}
	}

	void DifficultyNodeComponent.IListener.OnClickNode(DifficultyNodeComponent comp)
	{
		SetLevel(comp.Difficulty);
	}

	public void OnClickSweep()
	{
		int chapterIndex = _shakmehBossData.ChapterIndex;
		int dungeonIndex = _shakmehDungeonData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			SweepHelper.OpenSweepPopup(data, this);
		}
	}

	public void OnClickEnter()
	{
		if (_listener != null && _shakmehDungeonData != null && _shakmehBossData != null)
		{
			int chapterIndex = _shakmehBossData.ChapterIndex;
			int shakmehIndex = _shakmehDungeonData.ShakmehIndex;
			int level = _shakmehDungeonData.Level;
			ShakmehBossType bossType = _shakmehBossData.BossType;
			_listener.OnClickEnter(chapterIndex, shakmehIndex, level, bossType);
		}
	}

	public void OnClickBossInfo()
	{
		if (_listener != null && _shakmehDungeonData != null && _shakmehBossData != null)
		{
			_listener.OnClickInfo(_shakmehBossData.ChapterIndex, _shakmehDungeonData.DungeonIndex, _shakmehBossData);
		}
	}

	public void OnClickNext()
	{
		int level = _level + 1;
		SetLevel(level);
	}

	public void OnClickPrev()
	{
		int level = _level - 1;
		SetLevel(level);
	}
}
