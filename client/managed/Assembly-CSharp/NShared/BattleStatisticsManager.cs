using System;
using System.Collections.Generic;

namespace NShared;

public class BattleStatisticsManager : SnapshotObject
{
	public delegate void DamageCallback(int attackerId, int damagedId);

	public delegate void KillCallback(int attackerId, int damagedId);

	public delegate void KillCreatureIndexCallback(int attackerId, int creatureIndex);

	public class CreatureInfo
	{
		public long EnterTimeMs;

		public long PlayerId { get; set; }

		public int CreatureUid { get; set; }

		public int WaveIndex { get; set; }

		public CachedHeroInfo HeroInfo { get; set; }

		public string VisualInfo { get; set; }

		public bool IsBoss { get; set; }
	}

	public DamageCallback OnDamageActions;

	public KillCallback OnKillActions;

	public KillCreatureIndexCallback OnKillCreatureIndexActions;

	private BattleInstance _battleInstance;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _damageMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _damagedMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _skillDamageMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _healMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _healedMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _skillHealMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _threatMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _skillTheatMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _manaHealMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _manaRestoredMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _skillManaHealMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _killMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> _killCreatureIndexHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<TeamType, int>> _KillTeamMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<TeamType, int>> _currentWaveTeamKillMeterHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<TeamType, int>> _totalKillTeamMeterHolder;

	private readonly Dictionary<int, CreatureInfo> _creatureInfoDictionary = new Dictionary<int, CreatureInfo>();

	public BattleStatisticsManager()
	{
		_damageMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_damagedMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_skillDamageMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_healMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_healedMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_skillHealMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_threatMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_skillTheatMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_manaHealMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_manaRestoredMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_skillManaHealMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_killMeterHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_killCreatureIndexHolder = new SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>>(this);
		_KillTeamMeterHolder = new SnapshotObjectMember<SnapshotDictionary<TeamType, int>>(this);
		_currentWaveTeamKillMeterHolder = new SnapshotObjectMember<SnapshotDictionary<TeamType, int>>(this);
		_totalKillTeamMeterHolder = new SnapshotObjectMember<SnapshotDictionary<TeamType, int>>(this);
	}

	public void Init(BattleInstance battleInstance)
	{
		_battleInstance = battleInstance;
		_damageMeterHolder.Alloc();
		_damagedMeterHolder.Alloc();
		_skillDamageMeterHolder.Alloc();
		_healMeterHolder.Alloc();
		_healedMeterHolder.Alloc();
		_skillHealMeterHolder.Alloc();
		_threatMeterHolder.Alloc();
		_skillTheatMeterHolder.Alloc();
		_manaHealMeterHolder.Alloc();
		_manaRestoredMeterHolder.Alloc();
		_skillManaHealMeterHolder.Alloc();
		_killMeterHolder.Alloc();
		_killCreatureIndexHolder.Alloc();
		_KillTeamMeterHolder.Alloc();
		_currentWaveTeamKillMeterHolder.Alloc();
		_totalKillTeamMeterHolder.Alloc();
		OnDamageActions = delegate
		{
		};
		OnKillActions = delegate
		{
		};
		OnKillCreatureIndexActions = delegate
		{
		};
	}

	public void AddCreature(int uid, long playerId, int waveIndex, BattleCreatureCreateInfo createInfo, long currentTimeMs)
	{
		_creatureInfoDictionary.Remove(uid);
		_creatureInfoDictionary.Add(uid, new CreatureInfo
		{
			PlayerId = playerId,
			CreatureUid = uid,
			WaveIndex = waveIndex,
			HeroInfo = createInfo.HeroInfo,
			VisualInfo = createInfo.VisualInfo,
			IsBoss = createInfo.IsBoss,
			EnterTimeMs = currentTimeMs
		});
	}

	public Dictionary<int, CreatureInfo> GetCreatureInfoDictionary()
	{
		return _creatureInfoDictionary;
	}

	public void AddDamageMeter(int attacker, int receiver, long value, int skillIndex)
	{
		AddScore(_damageMeterHolder, attacker, receiver, value);
		AddScore(_damagedMeterHolder, receiver, attacker, value);
		AddScore(_skillDamageMeterHolder, attacker, skillIndex, value);
		OnDamageActions(attacker, receiver);
	}

	public long TotalDamageMeter(int attacker)
	{
		return TotalScore(_damageMeterHolder, attacker);
	}

	public long TotalDamagedMeter(int receiver)
	{
		return TotalScore(_damagedMeterHolder, receiver);
	}

	public BattleStatistics GetSkillDamageStatistics(int attacker)
	{
		return GetBattleStatistics(_skillDamageMeterHolder, attacker, allocate: false);
	}

	public long GetTotalDamageOfAccount(long playerId)
	{
		long num = 0L;
		foreach (KeyValuePair<int, CreatureInfo> item in _creatureInfoDictionary)
		{
			int key = item.Key;
			CreatureInfo value = item.Value;
			if (playerId == value.PlayerId)
			{
				num += TotalDamageMeter(key);
			}
		}
		return num;
	}

	public long GetBossTotalDamagedOfAccount()
	{
		long num = 0L;
		foreach (KeyValuePair<int, CreatureInfo> item in _creatureInfoDictionary)
		{
			int key = item.Key;
			if (item.Value.IsBoss)
			{
				num += TotalDamagedMeter(key);
			}
		}
		return num;
	}

	public void AddHealMeter(int healer, int receiver, long value, int skillIndex)
	{
		AddScore(_healMeterHolder, healer, receiver, value);
		AddScore(_healedMeterHolder, receiver, healer, value);
		AddScore(_skillHealMeterHolder, healer, skillIndex, value);
		OnDamageActions(healer, receiver);
	}

	public void AddThreatMeter(int threatener, int receiver, long value, int skillIndex)
	{
		AddScore(_threatMeterHolder, threatener, receiver, value);
		AddScore(_skillTheatMeterHolder, threatener, skillIndex, value);
	}

	public void AddManaHealMeter(int healer, int receiver, long value, int skillIndex)
	{
		AddScore(_manaRestoredMeterHolder, receiver, healer, value);
	}

	public void AddManaRestoredMeter(int healer, int receiver, long value)
	{
		AddScore(_manaRestoredMeterHolder, receiver, healer, value);
	}

	public long TotalHealMeter(int healer)
	{
		return TotalScore(_healMeterHolder, healer);
	}

	public long TotalHealedMeter(int receiver)
	{
		return TotalScore(_healedMeterHolder, receiver);
	}

	public BattleStatistics GetSkillHealStatistics(int healer)
	{
		return GetBattleStatistics(_skillHealMeterHolder, healer, allocate: false);
	}

	public long TotalThreatMeter(int threatener)
	{
		return TotalScore(_threatMeterHolder, threatener);
	}

	public BattleStatistics GetSkillThreatStatistics(int healer)
	{
		return GetBattleStatistics(_skillTheatMeterHolder, healer, allocate: false);
	}

	public long TotalManaHealMeter(int healer)
	{
		return TotalScore(_manaHealMeterHolder, healer);
	}

	public long TotalManaRestoredMeter(int receiver)
	{
		return TotalScore(_manaRestoredMeterHolder, receiver);
	}

	public BattleStatistics GetSkillManaHealStatistics(int healer)
	{
		return GetBattleStatistics(_skillManaHealMeterHolder, healer, allocate: false);
	}

	public void AddKillMeter(int from, int to, int value)
	{
		AddScore(_killMeterHolder, from, to, value);
		OnKillActions(from, to);
	}

	public int GetKillMeter(int from, int to)
	{
		return (int)GetScore(_killMeterHolder, from, to);
	}

	public int TotalKillMeter(int from)
	{
		return (int)TotalScore(_killMeterHolder, from);
	}

	public void AddKillTeamMeter(TeamType from, int value)
	{
		SnapshotDictionary<TeamType, int> snapshotDictionary = _KillTeamMeterHolder.Get();
		int value2 = 0;
		if (snapshotDictionary.TryGetValue(from, out value2))
		{
			snapshotDictionary[from] = value2 + value;
		}
		else
		{
			snapshotDictionary[from] = value;
		}
	}

	public int GetKillTeamMeter(TeamType from)
	{
		int value = 0;
		_KillTeamMeterHolder.Get().TryGetValue(from, out value);
		return value;
	}

	public void AddCurrentWaveKillTeamMeter(TeamType killerTeam, int killCount)
	{
		SnapshotDictionary<TeamType, int> snapshotDictionary = _currentWaveTeamKillMeterHolder.Get();
		int value = 0;
		if (snapshotDictionary.TryGetValue(killerTeam, out value))
		{
			snapshotDictionary[killerTeam] = value + killCount;
		}
		else
		{
			snapshotDictionary[killerTeam] = killCount;
		}
	}

	public int GetCurrentWaveKillMeter(TeamType from)
	{
		int value = 0;
		_currentWaveTeamKillMeterHolder.Get().TryGetValue(from, out value);
		return value;
	}

	public void AddTotalKillTeamMeter(TeamType killerTeam, int killCount)
	{
		SnapshotDictionary<TeamType, int> snapshotDictionary = _totalKillTeamMeterHolder.Get();
		int value = 0;
		if (snapshotDictionary.TryGetValue(killerTeam, out value))
		{
			snapshotDictionary[killerTeam] = value + killCount;
		}
		else
		{
			snapshotDictionary[killerTeam] = killCount;
		}
	}

	public int GetTotalKillTeamMeter(TeamType from)
	{
		int value = 0;
		_totalKillTeamMeterHolder.Get().TryGetValue(from, out value);
		return value;
	}

	public void ResetCurrentWaveKill()
	{
		_currentWaveTeamKillMeterHolder.Get().Clear();
	}

	public void AddKillCreatureIndex(int from, int killedCreatureIndex, int value)
	{
		AddScore(_killCreatureIndexHolder, from, killedCreatureIndex, value);
		OnKillCreatureIndexActions(from, killedCreatureIndex);
	}

	public int GetKillCreatureIndex(int from, int killedCreatureIndex)
	{
		return (int)GetScore(_killCreatureIndexHolder, from, killedCreatureIndex);
	}

	public int GetKillCreatureIndex(int killedCreatureIndex)
	{
		int totalCount = 0;
		_killCreatureIndexHolder.Get().ForEachIrregular(delegate(KeyValuePair<int, BattleStatistics> pair)
		{
			_ = pair.Key;
			BattleStatistics value = pair.Value;
			totalCount += (int)value.Get(killedCreatureIndex);
		});
		return totalCount;
	}

	public BattleStatistics GetKillCreatureIndexStatistics(int from)
	{
		return GetBattleStatistics(_killCreatureIndexHolder, from, allocate: false);
	}

	private BattleStatistics GetBattleStatistics(SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> holder, int from, bool allocate)
	{
		BattleStatistics value = null;
		holder.Get().TryGetValue(from, out value);
		if (value == null && allocate)
		{
			value = SnapshotObjectPool<BattleStatistics>.Alloc();
			value.Init();
			holder.Get().Add(from, value);
		}
		return value;
	}

	private void AddScore(SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> holder, int from, int to, long value)
	{
		GetBattleStatistics(holder, from, allocate: true)?.Add(to, value);
	}

	private long GetScore(SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> holder, int from, int to)
	{
		return GetBattleStatistics(holder, from, allocate: false)?.Get(to) ?? 0;
	}

	private long TotalScore(SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> holder, int from)
	{
		return GetBattleStatistics(holder, from, allocate: false)?.Sum() ?? 0;
	}

	private long MaxScore(SnapshotObjectMember<SnapshotDictionary<int, BattleStatistics>> holder)
	{
		long max = long.MinValue;
		holder.Get().ForEachIrregular(delegate(KeyValuePair<int, BattleStatistics> pair)
		{
			long val = pair.Value.Sum();
			max = Math.Max(max, val);
		});
		return max;
	}
}
