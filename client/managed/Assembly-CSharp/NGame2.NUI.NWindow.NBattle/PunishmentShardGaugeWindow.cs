using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PunishmentShardGaugeWindow : BaseWindow, IInspectorGuiDrawer
{
	private class ExpCalculator
	{
		private int _punishmentGroupIndex;

		private int _cachedWaveIndex;

		private int _cachedNextCleareWaveIndex;

		private int _cachedTotalMonsterCount;

		private float _cachedClearWaveExp;

		private float _cachedCurrentWaveExp;

		private int _initialExp;

		private KarmaDungeonData _karmaDungeonData;

		private int _karmaDungeonDataChangeWaveIndex;

		public ExpCalculator(int puishmentGroupIndex)
		{
			_punishmentGroupIndex = puishmentGroupIndex;
			_cachedNextCleareWaveIndex = 1;
			_cachedWaveIndex = 1;
			_karmaDungeonData = GetKarmaData(1);
			_karmaDungeonDataChangeWaveIndex += GetTotalWaveCount(_karmaDungeonData);
			_initialExp = GetInitialExp(_punishmentGroupIndex);
			_cachedCurrentWaveExp = GetCurrentKarmaExp(_initialExp, _cachedNextCleareWaveIndex);
			_cachedTotalMonsterCount = GetCurrentTotalMonsterCount(_karmaDungeonData, _cachedWaveIndex);
		}

		private int GetTotalWaveCount(KarmaDungeonData karmaDungeonData)
		{
			if (karmaDungeonData == null)
			{
				return 0;
			}
			int result = 1;
			int chapterIndex = karmaDungeonData.ChapterIndex;
			int dungeonIndex = karmaDungeonData.DungeonIndex;
			for (int i = 2; i < 100 && GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false, i) != null; i++)
			{
				result = i;
			}
			return result;
		}

		private int GetInitialExp(int punishmentGroupIndex)
		{
			return GameRoot.Instance.PunishmentGroupDataContainer.GetData(punishmentGroupIndex, EPunishmentDungeonType.Karma)?.InitialShardPoint ?? 0;
		}

		private float GetCurrentKarmaExp(int initialExp, int waveIndex)
		{
			if (initialExp <= 0)
			{
				return 1f;
			}
			if (waveIndex == 1)
			{
				return initialExp;
			}
			float karmagaugeExponentialValue = GetKarmagaugeExponentialValue(waveIndex);
			return _cachedCurrentWaveExp * karmagaugeExponentialValue;
		}

		private int GetCurrentTotalMonsterCount(KarmaDungeonData karmaDungeonData, int waveIndex)
		{
			if (karmaDungeonData == null)
			{
				return 1;
			}
			int chapterIndex = karmaDungeonData.ChapterIndex;
			int dungeonIndex = karmaDungeonData.DungeonIndex;
			CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false, waveIndex);
			if (data == null)
			{
				return 1;
			}
			return __Extension_Method_ICollection__.COUNT(data.GetCreatureIndexList());
		}

		private float GetKarmagaugeExponentialValue(int waveIndex)
		{
			return GameRoot.Instance.KarmaGaugeExponentialDataContainer.GetExponentValue(_punishmentGroupIndex, waveIndex);
		}

		private KarmaDungeonData GetKarmaData(int stageLevel)
		{
			return GameRoot.Instance.KarmaDungeonDataContainer.GetData(_punishmentGroupIndex, stageLevel);
		}

		private bool IsChangeClearWaveIndex(int cachedClearWaveIndex, int clearWaveIndex)
		{
			return cachedClearWaveIndex == clearWaveIndex;
		}

		private bool IsChangeKarmaDungeonData()
		{
			return _karmaDungeonDataChangeWaveIndex == _cachedNextCleareWaveIndex - 1;
		}

		private int GetKarmaDugeonNextStageLevel(KarmaDungeonData karmaDungeonData)
		{
			return karmaDungeonData?.NextLevel ?? 0;
		}

		public float CalculatorTotalExp(int clearWaveIndex, int endKillCount)
		{
			if (IsChangeClearWaveIndex(_cachedNextCleareWaveIndex, clearWaveIndex))
			{
				_cachedNextCleareWaveIndex++;
				_cachedWaveIndex++;
				_cachedClearWaveExp += _cachedCurrentWaveExp;
				_cachedCurrentWaveExp = GetCurrentKarmaExp(_initialExp, _cachedNextCleareWaveIndex);
				if (IsChangeKarmaDungeonData())
				{
					_cachedWaveIndex = 1;
					int karmaDugeonNextStageLevel = GetKarmaDugeonNextStageLevel(_karmaDungeonData);
					_karmaDungeonData = GetKarmaData(karmaDugeonNextStageLevel);
					_karmaDungeonDataChangeWaveIndex += GetTotalWaveCount(_karmaDungeonData);
				}
				_cachedTotalMonsterCount = GetCurrentTotalMonsterCount(_karmaDungeonData, _cachedWaveIndex);
			}
			if (endKillCount > _cachedTotalMonsterCount)
			{
				endKillCount = _cachedTotalMonsterCount;
			}
			return _cachedClearWaveExp + (float)endKillCount / (float)_cachedTotalMonsterCount * _cachedCurrentWaveExp;
		}
	}

	public static int ClearWaveIndex;

	public static int EndKillCount;

	public static string PrefabPath = "Resources/Prefab2/Window/Battle/PunishmentShardGaugeWindow";

	public UILabel DebugLabel;

	public UIGrid KarmaGrid;

	public UIGrid PunishmentGrid;

	private List<ShardGaugeComponent> _listComponent = new List<ShardGaugeComponent>();

	private ExpCalculator _expCalculator;

	private int _groupIndex;

	private bool _isPunishmentBoss;

	private float _flaskGague;

	private bool _showDebug;

	private bool _test;

	private void DrawGui()
	{
	}

	public void Open(int chatperIndex, int dungeonIndex, BattleType battleType, BattleInstanceExtraData extraData)
	{
		Open(delegate
		{
			UIHelper.EnableGameObject(DebugLabel, enable: false);
			_isPunishmentBoss = IsPunishmentBoss(battleType, chatperIndex, dungeonIndex);
			_groupIndex = GetGroupIndex(chatperIndex, dungeonIndex);
			Set(battleType, extraData);
		}, null);
	}

	private int GetGroupIndex(int chaterIndex, int dungeonIndex)
	{
		return GameRoot.Instance.KarmaDungeonDataContainer.GetDataByChapterDungeon(chaterIndex, dungeonIndex)?.PunishmentGroupIndex ?? 0;
	}

	private bool IsPunishmentBoss(BattleType battleType, int chapterIndex, int dungeonIndex)
	{
		if (battleType != BattleType.PunishmentRaid)
		{
			return false;
		}
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(chapterIndex, dungeonIndex);
		if (raidDataByDungeon == null)
		{
			return false;
		}
		int index = raidDataByDungeon.Index;
		int level = raidDataByDungeon.Level;
		return GameRoot.Instance.PunishmentRaidDataContainer.GetData(index, level)?.UsePunishmentRaidBossStatus ?? false;
	}

	private void Set(BattleType battleType, BattleInstanceExtraData extraData)
	{
		if (extraData != null && !(KarmaGrid == null) && !(PunishmentGrid == null))
		{
			SetGaugeComponent(battleType, extraData.FlaskItemIndex, extraData.FlaskItemCount);
		}
	}

	private void SetGaugeComponent(BattleType battleType, int index, int totalCount)
	{
		if ((battleType != BattleType.PunishmentRaid && battleType != BattleType.KarmaRaid) || index == 0)
		{
			return;
		}
		switch (battleType)
		{
		case BattleType.PunishmentRaid:
		{
			UIGrid uIGrid = (_isPunishmentBoss ? PunishmentGrid : KarmaGrid);
			ShardGaugeComponent shardGaugeComponent2 = CreateShardGaugeComp(uIGrid.transform);
			if (shardGaugeComponent2 == null)
			{
				return;
			}
			shardGaugeComponent2.OpenShardItem(index, totalCount, _isPunishmentBoss);
			_listComponent.Add(shardGaugeComponent2);
			break;
		}
		case BattleType.KarmaRaid:
		{
			_expCalculator = new ExpCalculator(_groupIndex);
			totalCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(index)?.Count ?? 0;
			int defaultShardFlaskIndex = GetDefaultShardFlaskIndex(_groupIndex);
			List<Tuple<int, int>> list = new List<Tuple<int, int>>();
			if (defaultShardFlaskIndex != index && totalCount > 0)
			{
				list.Add(new Tuple<int, int>(index, totalCount));
				list.Add(new Tuple<int, int>(defaultShardFlaskIndex, 0));
			}
			else
			{
				list.Add(new Tuple<int, int>(defaultShardFlaskIndex, 0));
			}
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				Tuple<int, int> tuple = list[i];
				ShardGaugeComponent shardGaugeComponent = CreateShardGaugeComp(KarmaGrid.transform);
				if (shardGaugeComponent == null)
				{
					break;
				}
				int item = tuple.Item1;
				int item2 = tuple.Item2;
				shardGaugeComponent.OpenShardFlask(item, item2, num);
				ShardFlaskItemData data = GameRoot.Instance.ShardFlaskItemDataContainer.GetData(item);
				if (data != null)
				{
					num += data.FlaskGauge * item2;
				}
				_listComponent.Add(shardGaugeComponent);
			}
			break;
		}
		}
		KarmaGrid.Reposition();
		PunishmentGrid.Reposition();
	}

	private int GetDefaultShardFlaskIndex(int groupIndex)
	{
		return GameRoot.Instance.ShardFlaskItemDataContainer.GetDefaultFlaskIndex(groupIndex);
	}

	private ShardGaugeComponent CreateShardGaugeComp(Transform transform)
	{
		ShardGaugeComponent shardGaugeComponent = ShardGaugeComponent.PrefabAuto.Factory.Create(transform);
		if (shardGaugeComponent == null)
		{
			return null;
		}
		return shardGaugeComponent;
	}

	public override void OnClosing()
	{
		base.OnClosing();
		FactoryBase.ReleaseAndClearList(_listComponent);
		_expCalculator = null;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_expCalculator != null)
		{
			if (!_test)
			{
				int clearWaveCount = SingletonAuto<BattleFacade>.instance.KarmaCache.ClearWaveCount;
				int currentWaveKillCount = SingletonAuto<BattleFacade>.instance.KarmaCache.CurrentWaveKillCount;
				_flaskGague = _expCalculator.CalculatorTotalExp(clearWaveCount, currentWaveKillCount);
			}
			else
			{
				_flaskGague = _expCalculator.CalculatorTotalExp(ClearWaveIndex, EndKillCount);
			}
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComponent); i++)
		{
			ShardGaugeComponent shardGaugeComponent = _listComponent[i];
			if (!(shardGaugeComponent == null))
			{
				shardGaugeComponent.OnUpdate(_flaskGague);
			}
		}
	}
}
