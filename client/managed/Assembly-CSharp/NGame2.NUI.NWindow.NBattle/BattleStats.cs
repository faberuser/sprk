using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NDescription;
using NShared;
using NVespa;
using NVespa.NGui;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BattleStats : BaseWindow
{
	public enum EStatCategory
	{
		Damage,
		Heal,
		Damaged
	}

	[Serializable]
	public class StatsGroup
	{
		public UIScrollGrid Grid;

		public UILabel TitleLabel;

		public UILabel CountLabel;

		public string DebugSkillDetail;
	}

	[Serializable]
	public class StatLabelGroup
	{
		public EStatCategory Category;

		public GameObject LabelObject;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BattleStats";

	public static LocalStorageBool ShowBattleDetail = new LocalStorageBool(i_default: false, typeof(BattleStats).FullName + "ShowBattleDetail");

	public StatsGroup Left;

	public StatsGroup Right;

	public StatLabelGroup[] StatLabels;

	public GameObject WaveRoot;

	public UILabel WaveLabel;

	private List<BattleStatisticsManager.CreatureInfo> _leftCreatureInfoList = new List<BattleStatisticsManager.CreatureInfo>();

	private List<BattleStatisticsManager.CreatureInfo> _rightCreatureInfoList = new List<BattleStatisticsManager.CreatureInfo>();

	private List<HeroBattleStatComponent> _leftStatComponentList = new List<HeroBattleStatComponent>();

	private List<HeroBattleStatComponent> _rightStatComponentList = new List<HeroBattleStatComponent>();

	private BattleInstance _battleInstance;

	private EStatCategory _category;

	private int _waveIndex;

	private int _totalWaveIndex;

	private bool CheckEachWaveEachTeam
	{
		get
		{
			if (_battleInstance == null)
			{
				return false;
			}
			return _battleInstance.CheckEachWaveEachTeam;
		}
	}

	public void Open(BattleInstance battleInstance)
	{
		Open(delegate
		{
			Init(battleInstance);
		}, null);
		SceneAutoBehaviour<GuiManager>.instance.Add("BattleStats", this, DrawGui);
	}

	public void Init(BattleInstance battleInstance)
	{
		_battleInstance = battleInstance;
		_category = EStatCategory.Damage;
		_waveIndex = 1;
		_totalWaveIndex = _battleInstance.Snapshot.TotalWaveIndex;
		InitCreatureInfoList();
		Refresh(left: true, right: true);
	}

	public override void Reset()
	{
		base.Reset();
		ClearStatComponent();
		_leftCreatureInfoList.Clear();
		_rightCreatureInfoList.Clear();
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	public void OnClickPrevCategry()
	{
		EnumLookup<EStatCategory>.ValueData[] valueArray = EnumLookup<EStatCategory>.ValueArray;
		int num = (int)(_category - 1);
		_category = (EStatCategory)(Enum.IsDefined(typeof(EStatCategory), num) ? num : (__Extension_Method_ICollection__.COUNT(valueArray) - 1));
		Refresh(left: true, right: true);
	}

	public void OnClickNextCategry()
	{
		int num = (int)(_category + 1);
		_category = (Enum.IsDefined(typeof(EStatCategory), num) ? ((EStatCategory)num) : EStatCategory.Damage);
		Refresh(left: true, right: true);
	}

	public void OnClickPrevWave()
	{
		_waveIndex = ((_waveIndex > 1) ? (_waveIndex - 1) : _totalWaveIndex);
		Refresh(CheckEachWaveEachTeam, right: true);
	}

	public void OnClickNextWave()
	{
		_waveIndex = ((_waveIndex >= _totalWaveIndex) ? 1 : (_waveIndex + 1));
		Refresh(CheckEachWaveEachTeam, right: true);
	}

	private void ClearStatComponent()
	{
		FactoryBase.ReleaseAndClearList(_leftStatComponentList);
		FactoryBase.ReleaseAndClearList(_rightStatComponentList);
	}

	private void InitCreatureInfoList()
	{
		_leftCreatureInfoList.Clear();
		_rightCreatureInfoList.Clear();
		SprkGroupSummons();
		foreach (KeyValuePair<int, BattleStatisticsManager.CreatureInfo> item in _battleInstance.Snapshot.StatisticsManager.GetCreatureInfoDictionary())
		{
			_ = item.Key;
			BattleStatisticsManager.CreatureInfo value = item.Value;
			if (_sprkFoldedSummons.Contains(value.CreatureUid))
			{
				continue;
			}
			long playerId = value.PlayerId;
			BattlePlayer player = _battleInstance.PlayerContainer.GetPlayer(playerId);
			if (player != null && player.TeamType == TeamType.Left)
			{
				_leftCreatureInfoList.Add(value);
			}
			else
			{
				_rightCreatureInfoList.Add(value);
			}
		}
	}

	private void Refresh(bool left, bool right)
	{
		SetupCreatureStatistics(left, right);
		UpdateWaveLabel();
	}

	private void UpdateWaveLabel()
	{
		if (_totalWaveIndex <= 1)
		{
			UIHelper.EnableGameObject(WaveRoot, enable: false);
			return;
		}
		UIHelper.EnableGameObject(WaveRoot, enable: true);
		UIHelper.SetLabelText(WaveLabel, SprkText.Format("Wave {0}", _waveIndex));
	}

	private void SetupCreatureStatistics(bool left, bool right)
	{
		Func<int, long> func = null;
		Func<int, BattleStatistics> getter = null;
		switch (_category)
		{
		case EStatCategory.Damage:
			func = _battleInstance.Snapshot.StatisticsManager.TotalDamageMeter;
			getter = _battleInstance.Snapshot.StatisticsManager.GetSkillDamageStatistics;
			break;
		case EStatCategory.Damaged:
			func = _battleInstance.Snapshot.StatisticsManager.TotalDamagedMeter;
			break;
		case EStatCategory.Heal:
			func = _battleInstance.Snapshot.StatisticsManager.TotalHealMeter;
			getter = _battleInstance.Snapshot.StatisticsManager.GetSkillHealStatistics;
			break;
		}
		if (func == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StatLabels); i++)
		{
			UIHelper.EnableGameObject(StatLabels[i].LabelObject, StatLabels[i].Category == _category);
		}
		bool flag = _totalWaveIndex <= 1;
		if (left)
		{
			List<BattleStatisticsManager.CreatureInfo> list = (flag ? _leftCreatureInfoList : _leftCreatureInfoList.Where((BattleStatisticsManager.CreatureInfo e) => e.PlayerId != 0L && e.WaveIndex + 1 == _waveIndex).ToList());
			if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
			{
				list = _leftCreatureInfoList.Where((BattleStatisticsManager.CreatureInfo e) => e.PlayerId != 0L || e.WaveIndex == _waveIndex).ToList();
			}
			list = SprkAttachSummons(list, _leftCreatureInfoList);
			CreateCreatureStatistics(Left, list, _leftStatComponentList, HeroBattleStatComponent.LeftPrefabAuto, func, getter);
		}
		if (right)
		{
			List<BattleStatisticsManager.CreatureInfo> creatures = (flag ? _rightCreatureInfoList : _rightCreatureInfoList.Where((BattleStatisticsManager.CreatureInfo e) => e.PlayerId != 0L || e.WaveIndex == _waveIndex).ToList());
			creatures = SprkAttachSummons(creatures, _rightCreatureInfoList);
			CreateCreatureStatistics(Right, creatures, _rightStatComponentList, HeroBattleStatComponent.RightPrefabAuto, func, getter);
		}
	}

	// SPRK: summons (Maya's Seves) by summoner uid, and each summoner's re-summons of the same
	// creature folded into one row: _sprkSummonGroups maps the row's uid (the first summon) to every
	// summon uid it stands for; the other uids are in _sprkFoldedSummons and get no row of their own.
	private Dictionary<int, int> _sprkSummonerOf = new Dictionary<int, int>();

	private Dictionary<int, List<int>> _sprkSummonGroups = new Dictionary<int, List<int>>();

	private HashSet<int> _sprkFoldedSummons = new HashSet<int>();

	private void SprkGroupSummons()
	{
		_sprkSummonerOf.Clear();
		_sprkSummonGroups.Clear();
		_sprkFoldedSummons.Clear();
		Dictionary<(int, int), int> rowOf = new Dictionary<(int, int), int>();
		foreach (int uid in _battleInstance.Snapshot.StatisticsManager.GetCreatureInfoDictionary().Keys.OrderBy((int uid) => uid))
		{
			BattleCreature creature = _battleInstance.CreatureContainer?.GetCreature(uid);
			if (creature == null || creature.SummonerUid == 0)
			{
				continue;
			}
			_sprkSummonerOf[uid] = creature.SummonerUid;
			(int, int) key = (creature.SummonerUid, creature.CreatureIndex);
			if (rowOf.TryGetValue(key, out var rowUid))
			{
				_sprkSummonGroups[rowUid].Add(uid);
				_sprkFoldedSummons.Add(uid);
			}
			else
			{
				rowOf.Add(key, uid);
				_sprkSummonGroups.Add(uid, new List<int> { uid });
			}
		}
	}

	// SPRK: summons follow their summoner instead of the per-wave filter, which compares the wave a
	// creature entered with and dropped Seves (summoned mid-wave) from multi-wave battles like
	// dragon raids. Summons whose summoner has no entry keep the filter's verdict.
	private List<BattleStatisticsManager.CreatureInfo> SprkAttachSummons(List<BattleStatisticsManager.CreatureInfo> filtered, List<BattleStatisticsManager.CreatureInfo> all)
	{
		if (_sprkSummonerOf.Count == 0)
		{
			return filtered;
		}
		HashSet<int> allUids = new HashSet<int>(all.Select((BattleStatisticsManager.CreatureInfo e) => e.CreatureUid));
		HashSet<int> shown = new HashSet<int>(filtered.Where((BattleStatisticsManager.CreatureInfo e) => !_sprkSummonerOf.ContainsKey(e.CreatureUid)).Select((BattleStatisticsManager.CreatureInfo e) => e.CreatureUid));
		List<BattleStatisticsManager.CreatureInfo> result = new List<BattleStatisticsManager.CreatureInfo>();
		foreach (BattleStatisticsManager.CreatureInfo info in all)
		{
			bool include;
			if (!_sprkSummonerOf.TryGetValue(info.CreatureUid, out var summonerUid))
			{
				include = shown.Contains(info.CreatureUid);
			}
			else if (allUids.Contains(summonerUid))
			{
				include = shown.Contains(summonerUid);
			}
			else
			{
				include = filtered.Contains(info);
			}
			if (include)
			{
				result.Add(info);
			}
		}
		return result;
	}

	private long SprkGroupTotal(Func<int, long> getter, int creatureUid)
	{
		if (!_sprkSummonGroups.TryGetValue(creatureUid, out var uids))
		{
			return Vespa_System_Action_Extension_Method.fire(getter, creatureUid);
		}
		long total = 0L;
		foreach (int uid in uids)
		{
			total += Vespa_System_Action_Extension_Method.fire(getter, uid);
		}
		return total;
	}

	private string BuildSkillDebugString(List<BattleStatisticsManager.CreatureInfo> creatures, Dictionary<int, long> valueByCreatures, long sum, Func<int, BattleStatistics> getter2)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (BattleStatisticsManager.CreatureInfo item in creatures.OrderByDescending((BattleStatisticsManager.CreatureInfo c) => valueByCreatures[c.CreatureUid]))
		{
			int creatureUid = item.CreatureUid;
			long num = valueByCreatures[creatureUid];
			BattleStatistics battleStatistics = Vespa_System_Action_Extension_Method.fire(getter2, creatureUid);
			CreatureData creatureData = ((item.HeroInfo == null) ? null : GameRoot.Instance.CreatureDataContainer.GetData(item.HeroInfo.HeroIndex));
			if (creatureData != null)
			{
				stringBuilder.Append($"{creatureData.Name}: ");
			}
			else
			{
				stringBuilder.Append($"Creature {creatureUid}: ");
			}
			if (sum > 0)
			{
				stringBuilder.Append($"{num} ({num * 100 / sum}%)");
			}
			stringBuilder.AppendLine("");
			if (num > 0)
			{
				Dictionary<int, long> skillDamages = new Dictionary<int, long>();
				battleStatistics.ForEachIrregular(delegate(KeyValuePair<int, long> pair)
				{
					skillDamages[pair.Key] = pair.Value;
				});
				foreach (KeyValuePair<int, long> item2 in skillDamages.OrderByDescending(delegate(KeyValuePair<int, long> pair)
				{
					KeyValuePair<int, long> keyValuePair = pair;
					return keyValuePair.Value;
				}))
				{
					SkillData data = GameRoot.Instance.SkillDataContainer.GetData(item2.Key);
					if (data != null)
					{
						string skillName = SkillDescContainer.GetSkillName(item.HeroInfo, data);
						stringBuilder.Append($" {skillName} ({item2.Key})");
					}
					else
					{
						stringBuilder.Append($" ({item2.Key})");
					}
					stringBuilder.AppendLine($" = {item2.Value} ({item2.Value * 100 / num}%)");
				}
			}
			stringBuilder.AppendLine("");
		}
		return stringBuilder.ToString();
	}

	private void CreateCreatureStatistics(StatsGroup statsGroup, List<BattleStatisticsManager.CreatureInfo> creatures, List<HeroBattleStatComponent> statComponentList, PrefabAuto<HeroBattleStatComponent> prefabAuto, Func<int, long> getter, Func<int, BattleStatistics> getter2)
	{
		FactoryBase.ReleaseAndClearList(statComponentList);
		long num = 0L;
		Dictionary<int, long> dictionary = new Dictionary<int, long>();
		foreach (BattleStatisticsManager.CreatureInfo creature in creatures)
		{
			long num2 = SprkGroupTotal(getter, creature.CreatureUid);
			dictionary[creature.CreatureUid] = num2;
			num += num2;
		}
		if (getter2 != null && ShowBattleDetail.Value)
		{
			statsGroup.DebugSkillDetail = BuildSkillDebugString(creatures, dictionary, num, getter2);
		}
		List<HeroBattleStatComponent> list = new List<HeroBattleStatComponent>();
		foreach (BattleStatisticsManager.CreatureInfo creature2 in creatures)
		{
			long damage = dictionary[creature2.CreatureUid];
			HeroBattleStatComponent heroBattleStatComponent = UIHelper.AddGridItem(statsGroup.Grid, prefabAuto);
			heroBattleStatComponent.Open(creature2);
			heroBattleStatComponent.SetDamage(damage, num);
			statComponentList.Add(heroBattleStatComponent);
			list.Add(heroBattleStatComponent);
		}
		list.Sort((HeroBattleStatComponent l, HeroBattleStatComponent r) => r.Value.CompareTo(l.Value));
		for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(list); num3++)
		{
			list[num3].SetRank(new VisualTierRank(num3 + 1));
		}
		statsGroup.Grid.Grid.sorting = UIGrid.Sorting.Custom;
		statsGroup.Grid.Grid.onCustomSort = delegate(Transform l, Transform r)
		{
			HeroBattleStatComponent component = l.GetComponent<HeroBattleStatComponent>();
			return r.GetComponent<HeroBattleStatComponent>().Value.CompareTo(component.Value);
		};
		UIHelper.RepositionGrid(statsGroup.Grid);
		SprkEnableRowScrolling(statsGroup.Grid, list);
		UIHelper.SetLabelText(statsGroup.CountLabel, creatures.Count);
	}

	// SPRK: rows (and their portraits) take input but never forwarded drags to the list, so once
	// summons push a side past four rows (Maya's Seves in arena) the extra rows could not be
	// scrolled into view. Forward drags from every collider in a row and refresh the scroll bounds.
	private static void SprkEnableRowScrolling(UIScrollGrid grid, List<HeroBattleStatComponent> rows)
	{
		if (grid == null || grid.ScrollView == null)
		{
			return;
		}
		foreach (HeroBattleStatComponent row in rows)
		{
			if (row == null)
			{
				continue;
			}
			foreach (Collider collider in row.GetComponentsInChildren<Collider>(includeInactive: true))
			{
				UIDragScrollView drag = collider.GetComponent<UIDragScrollView>() ?? collider.gameObject.AddComponent<UIDragScrollView>();
				drag.scrollView = grid.ScrollView;
			}
		}
		grid.ScrollView.InvalidateBounds();
		grid.ScrollView.ResetPosition();
	}

	private void DrawGui()
	{
		ShowBattleDetail.Value = GUILayout.Toggle(ShowBattleDetail.Value, "Show Skill Debug on Battle Statistics");
		if (ShowBattleDetail.Value)
		{
			GUILayout.Label("Left");
			using (new BlockManager().VerticalBox(Color.black))
			{
				GUILayout.Label(Left.DebugSkillDetail);
			}
			GUILayout.Label("Right");
			using (new BlockManager().VerticalBox(Color.black))
			{
				GUILayout.Label(Right.DebugSkillDetail);
			}
		}
	}
}
