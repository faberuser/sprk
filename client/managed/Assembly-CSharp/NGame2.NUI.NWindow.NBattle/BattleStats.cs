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
		foreach (KeyValuePair<int, BattleStatisticsManager.CreatureInfo> item in _battleInstance.Snapshot.StatisticsManager.GetCreatureInfoDictionary())
		{
			_ = item.Key;
			BattleStatisticsManager.CreatureInfo value = item.Value;
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
			CreateCreatureStatistics(Left, list, _leftStatComponentList, HeroBattleStatComponent.LeftPrefabAuto, func, getter);
		}
		if (right)
		{
			List<BattleStatisticsManager.CreatureInfo> creatures = (flag ? _rightCreatureInfoList : _rightCreatureInfoList.Where((BattleStatisticsManager.CreatureInfo e) => e.PlayerId != 0L || e.WaveIndex == _waveIndex).ToList());
			CreateCreatureStatistics(Right, creatures, _rightStatComponentList, HeroBattleStatComponent.RightPrefabAuto, func, getter);
		}
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
			long num2 = Vespa_System_Action_Extension_Method.fire(getter, creature.CreatureUid);
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
		UIHelper.SetLabelText(statsGroup.CountLabel, creatures.Count);
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
