using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPortrait;
using NGame2.NUI.NWindow.NSelectRewardPopup;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NMultipleDungeon;

public class DungeonDiffSelect : BaseWindow, LobbyTopmostMenu.ICustomStaminaObserver, MultipleDungeonDifficulty.IListener
{
	public interface Listener
	{
		bool OnCloseRequest(DungeonDiffSelect closeThis);

		void EnterDungeon(int chapterIndex, int dungeonIndex, int Diff);

		void SelectDungeonDiff(int Diff);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventDungeon/DungeonDiffSelect2";

	public UILabel Label_Title;

	public UIScrollGrid Grid_Enemies;

	public UIScrollGrid Grid_Reward;

	public UILabel Label_Tip;

	public UILabel RewardDescLabel;

	public UIButton Button_ReadyBattle;

	public MultipleDungeonDifficulty MultipleDungeonDifficulty;

	private List<MonsterComponent> _ListEnemies = new List<MonsterComponent>();

	private List<UIScrollItemBehaviour> _rewardList = new List<UIScrollItemBehaviour>();

	private Listener _listener;

	private UnderPrisonData _underPrisonData;

	private List<UnderPrisonDungeonData> _underPrisonDungeonDataList = new List<UnderPrisonDungeonData>();

	private int _selectDifficulty;

	public StaminaType StaminaType => StaminaType.None;

	public override void Reset()
	{
		base.Reset();
		ResetLists();
	}

	public override void Close()
	{
		base.Close();
		if (MultipleDungeonDifficulty != null)
		{
			MultipleDungeonDifficulty.Close();
		}
	}

	private void ResetLists()
	{
		FactoryBase.ReleaseAndClearList(_ListEnemies);
		FactoryBase.ReleaseAndClearList(_rewardList);
	}

	public void Open(Listener listener, UnderPrisonData underPrisonData, int diff)
	{
		Open(delegate
		{
			Init(listener, underPrisonData, diff);
		}, null);
	}

	public void Init(Listener listener, UnderPrisonData underPrisonData, int diff)
	{
		_listener = listener;
		_underPrisonData = underPrisonData;
		SetUnderPrisonDungeonDataList();
		InitDifficulty(diff);
		SetupDungeonBasicInfo();
		SetupDungeonDiffInfo();
	}

	private void SetUnderPrisonDungeonDataList()
	{
		_underPrisonDungeonDataList.Clear();
		if (_underPrisonData != null)
		{
			_underPrisonDungeonDataList = GameRoot.Instance.UnderPrisonDungeonDataContainer.GetUnderPrisonDungeonDataListByChpaterIndex(_underPrisonData.ChapterIndex);
		}
	}

	private void InitDifficulty(int diff)
	{
		SetDifficulty(diff);
		if (_underPrisonData != null && !(MultipleDungeonDifficulty == null))
		{
			MultipleDungeonDifficulty.Init(this, _selectDifficulty);
			MultipleDungeonDifficulty.SetDifficulty(_underPrisonData.ChapterIndex, _underPrisonDungeonDataList);
		}
	}

	private void SetDifficulty(int diff)
	{
		if (diff >= 0)
		{
			_selectDifficulty = diff;
			return;
		}
		_selectDifficulty = 1;
		if (_underPrisonData != null)
		{
			_selectDifficulty = GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.GetOpenDiff(_underPrisonData.ChapterIndex);
			int lastDiffByChapterIndex = GameRoot.Instance.UnderPrisonDungeonDataContainer.GetLastDiffByChapterIndex(_underPrisonData.ChapterIndex);
			if (_selectDifficulty > lastDiffByChapterIndex)
			{
				_selectDifficulty = lastDiffByChapterIndex;
			}
		}
	}

	private void SetDungeonTitle()
	{
		if (_underPrisonData != null)
		{
			UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(_underPrisonData.Name));
		}
	}

	private void SetDungeonDesc()
	{
		if (_underPrisonData != null)
		{
			UIHelper.SetLabelText(Label_Tip, LocalizationManager.GetStringDirect(_underPrisonData.Desc));
		}
	}

	private void SetLabelDungeonRewardDesc()
	{
		if (_underPrisonData != null)
		{
			UIHelper.SetLabelText(RewardDescLabel, LocalizationManager.GetStringDirect(_underPrisonData.IsSelectReward ? "UNDERGROUND_CHOICE_REWARD" : "UNDERGROUND_GET_ALL_REWARD"));
		}
	}

	private void SetupDungeonBasicInfo()
	{
		SetDungeonTitle();
		SetDungeonDesc();
		SetLabelDungeonRewardDesc();
	}

	private void SetupMonsterList()
	{
		FactoryBase.ReleaseAndClearList(_ListEnemies);
		if (_underPrisonData == null)
		{
			return;
		}
		int chapterIndex = _underPrisonData.ChapterIndex;
		int dungeonIndex = GetDungeonIndex();
		EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, _selectDifficulty, DungeonDifficultyType.Easy);
		if (enterDungeonData != null)
		{
			List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(enterDungeonData);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(monsterInfoFromDungeon); i++)
			{
				MonsterComponent monsterComponent = UIHelper.AddGridItem(Grid_Enemies, MonsterComponent.PrefabAuto);
				monsterComponent.Open(monsterInfoFromDungeon[i], default(MonsterComponent.StockData));
				_ListEnemies.Add(monsterComponent);
			}
			UIHelper.RepositionGrid(Grid_Enemies);
		}
	}

	private void SetupRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		if (!(Grid_Reward == null))
		{
			if (_underPrisonData.IsSelectReward)
			{
				SetSelectReward();
			}
			SetReward();
			UIHelper.RepositionGrid(Grid_Reward);
		}
	}

	private void SetReward()
	{
		if (_underPrisonData == null)
		{
			return;
		}
		int chapterIndex = _underPrisonData.ChapterIndex;
		int dungeonIndex = GetDungeonIndex();
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.DropRewardIndex_Easy);
		if (data2 == null)
		{
			return;
		}
		_ = data?.BattleType;
		List<RewardElementData> rewardItemList = data2.GetRewardItemList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(rewardItemList))
		{
			return;
		}
		RewardElementData rewardElementData = null;
		for (int i = 0; i < rewardItemList.Count; i++)
		{
			rewardElementData = rewardItemList[i];
			if (rewardElementData == null)
			{
				continue;
			}
			CurrencyType currencyType = rewardElementData.GetCurrencyType();
			if (currencyType != CurrencyType.None)
			{
				CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(Grid_Reward, CurrencyIconComponent.PrefabAuto);
				int itemMin = rewardElementData.ItemMin;
				CurrencyIconComponent.Listener listener = null;
				int num = itemMin;
				currencyIconComponent.Open(currencyType, num, listener);
				_rewardList.Add(currencyIconComponent);
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode != null)
			{
				int itemMin2 = rewardElementData.ItemMin;
				long count = itemMin2;
				int itemMax = rewardElementData.ItemMax;
				long maxCount = itemMax;
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_Reward, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode);
				if (dataByCode.Type != ItemType.Equip)
				{
					itemIconComponent.SetItemMinMax(count, maxCount);
				}
				_rewardList.Add(itemIconComponent);
			}
		}
	}

	private void SetSelectReward()
	{
		if (_underPrisonData == null)
		{
			return;
		}
		int chapterIndex = _underPrisonData.ChapterIndex;
		int dungeonIndex = GetDungeonIndex();
		SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return;
		}
		CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data2 == null)
		{
			return;
		}
		BaseSelectRewardPopup.ApplyBonusItemCount(data2?.BattleType ?? BattleType.None, data.GetSelectRewardItemList()).ForEach(delegate(BaseSelectRewardPopup.BoostedSelectRewardItemInfo info)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(info.ItemInfo.Code);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_Reward, ItemIconComponent.PrefabAuto);
				if (itemIconComponent != null)
				{
					itemIconComponent.Open(dataByCode);
					int boostedCount = info.BoostedCount;
					Color? color = null;
					int num = boostedCount;
					itemIconComponent.SetItemCount(num, color);
					_rewardList.Add(itemIconComponent);
				}
			}
		});
	}

	private void SetupDungeonDiffInfo()
	{
		SetupMonsterList();
		SetupRewardList();
	}

	public void OnClickExitButton()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public void OnClickBattleReadyButton()
	{
		if (_underPrisonData != null && _listener != null)
		{
			_listener.EnterDungeon(_underPrisonData.ChapterIndex, GetDungeonIndex(), _selectDifficulty);
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	private int GetDungeonIndex()
	{
		if (_underPrisonData == null)
		{
			return 0;
		}
		return GameRoot.Instance.UnderPrisonDungeonDataContainer.GetData(new UnderPrisonDungeonKey(_underPrisonData.ChapterIndex, _selectDifficulty))?.DungeonIndex ?? 0;
	}

	void MultipleDungeonDifficulty.IListener.SelectDiff(int diff)
	{
		_selectDifficulty = diff;
		SetupDungeonDiffInfo();
	}
}
