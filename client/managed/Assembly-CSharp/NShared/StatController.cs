using System;
using System.Collections.Generic;
using NShared.NOperationRuntime;
using NVespa;

namespace NShared;

public class StatController : SnapshotObject
{
	public const int StatMultiplyFactor = 1000;

	public const long StatMultiplyFactor2 = 1000000L;

	public const long StatMultiplyFactor3 = 1000000000L;

	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IStatModifier>> _statModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, int>> _cachedAddedStatDictionaryHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, int>> _cachedMultedStatDictionaryHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, long>> _cachedFinalStatDictionaryHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<ITriggerStatModifier>> _triggerStatModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IAttackModifier>> _attackModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>> _damageBeginningModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>> _damageAfterDodgeModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>> _damageDamageAmplifierModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>> _damageBeforeFinalModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>> _damageFinalModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>> _damageAfterFinalModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>> _damageEndOfFinalModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageDodger>> _damageDodgerListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IDamageFlagModifier>> _damageFlagModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotList<IHpObserver>> _hpObserverListHolder;

	private readonly SnapshotObjectMember<SnapshotList<IStatObserver>> _statObserverListHolder;

	private readonly SnapshotObjectMember<SnapshotLinkedList<IStatOverrider>> _statOverriderListHolder;

	private readonly SnapshotObjectMember<PrevDamageInfo> _prevDamageInfoHolder;

	private readonly SnapshotObjectMember<PrevDamageInfo> _prevAttackInfoHolder;

	private readonly SnapshotObjectMember<long> _hpHolder;

	private readonly SnapshotObjectMember<int> _mpHolder;

	private readonly SnapshotObjectMember<long> _overridenMaxHpHolder;

	private readonly SnapshotObjectMember<int> _overridenMaxMpHolder;

	private readonly SnapshotObjectMember<long> _lastMpRegenTimeHolder;

	private readonly SnapshotObjectMember<long> _minHpHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public long Hp => _hpHolder.Get();

	public int Mp => _mpHolder.Get();

	public int HpRatioM => GetHpRatioM();

	public int MpRatioM => GetMpRatioM();

	public PrevDamageInfo PrevDamageInfo => _prevDamageInfoHolder.Get();

	public PrevDamageInfo PrevAttackInfo => _prevAttackInfoHolder.Get();

	public SnapshotLinkedList<ITriggerStatModifier> TriggerStatModifierList => _triggerStatModifierListHolder.Get();

	public StatController()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_statModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IStatModifier>>(this);
		_cachedAddedStatDictionaryHolder = new SnapshotObjectMember<SnapshotDictionary<int, int>>(this);
		_cachedMultedStatDictionaryHolder = new SnapshotObjectMember<SnapshotDictionary<int, int>>(this);
		_cachedFinalStatDictionaryHolder = new SnapshotObjectMember<SnapshotDictionary<int, long>>(this);
		_triggerStatModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<ITriggerStatModifier>>(this);
		_attackModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IAttackModifier>>(this);
		_damageBeginningModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>>(this);
		_damageAfterDodgeModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>>(this);
		_damageDamageAmplifierModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>>(this);
		_damageBeforeFinalModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>>(this);
		_damageFinalModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>>(this);
		_damageAfterFinalModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>>(this);
		_damageEndOfFinalModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageModifier>>(this);
		_damageDodgerListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageDodger>>(this);
		_damageFlagModifierListHolder = new SnapshotObjectMember<SnapshotLinkedList<IDamageFlagModifier>>(this);
		_hpObserverListHolder = new SnapshotObjectMember<SnapshotList<IHpObserver>>(this);
		_statObserverListHolder = new SnapshotObjectMember<SnapshotList<IStatObserver>>(this);
		_statOverriderListHolder = new SnapshotObjectMember<SnapshotLinkedList<IStatOverrider>>(this);
		_prevDamageInfoHolder = new SnapshotObjectMember<PrevDamageInfo>(this);
		_prevAttackInfoHolder = new SnapshotObjectMember<PrevDamageInfo>(this);
		_hpHolder = new SnapshotObjectMember<long>(this, 0L);
		_mpHolder = new SnapshotObjectMember<int>(this, 0);
		_overridenMaxHpHolder = new SnapshotObjectMember<long>(this, 0L);
		_overridenMaxMpHolder = new SnapshotObjectMember<int>(this, 0);
		_lastMpRegenTimeHolder = new SnapshotObjectMember<long>(this, -1L);
		_minHpHolder = new SnapshotObjectMember<long>(this, 0L);
	}

	public void Init(BattleCreature owner)
	{
		_ownerHolder.Set(owner);
		_statModifierListHolder.Alloc();
		_cachedAddedStatDictionaryHolder.Alloc();
		_cachedMultedStatDictionaryHolder.Alloc();
		_cachedFinalStatDictionaryHolder.Alloc();
		_triggerStatModifierListHolder.Alloc();
		_attackModifierListHolder.Alloc();
		_damageBeginningModifierListHolder.Alloc();
		_damageBeforeFinalModifierListHolder.Alloc();
		_damageAfterDodgeModifierListHolder.Alloc();
		_damageDamageAmplifierModifierListHolder.Alloc();
		_damageFinalModifierListHolder.Alloc();
		_damageAfterFinalModifierListHolder.Alloc();
		_damageEndOfFinalModifierListHolder.Alloc();
		_damageDodgerListHolder.Alloc();
		_damageFlagModifierListHolder.Alloc();
		_hpObserverListHolder.Alloc();
		_statObserverListHolder.Alloc();
		_statOverriderListHolder.Alloc();
	}

	public void AddHpObserver(IHpObserver observer)
	{
		if (observer != null && !_hpObserverListHolder.Get().Contains(observer))
		{
			_hpObserverListHolder.Get().Add(observer);
		}
	}

	public bool RemoveHpObserver(IHpObserver observer)
	{
		return _hpObserverListHolder.Get().Remove(observer);
	}

	private void AnnounceHpChangedToObserver(long oldHp, long newHp)
	{
		for (int i = 0; i < _hpObserverListHolder.Get().Count; i++)
		{
			_hpObserverListHolder.Get()[i].OnHpChanged(oldHp, newHp);
		}
	}

	public void AddStatObserver(IStatObserver observer)
	{
		if (observer != null && !_statObserverListHolder.Get().Contains(observer))
		{
			_statObserverListHolder.Get().Add(observer);
		}
	}

	public bool RemoveStatObserver(IStatObserver observer)
	{
		return _statObserverListHolder.Get().Remove(observer);
	}

	private void AnnounceStatChangedToObserver(StatType statType)
	{
		for (int i = 0; i < _statObserverListHolder.Get().Count; i++)
		{
			_statObserverListHolder.Get()[i].OnStatChanged(statType);
		}
	}

	public void Update()
	{
		if (!Owner.IsResting)
		{
			UpdateDamageModifier(Owner, _attackModifierListHolder.Get());
			UpdateDamageModifier(Owner, _damageBeginningModifierListHolder.Get());
			UpdateDamageModifier(Owner, _damageDamageAmplifierModifierListHolder.Get());
			UpdateDamageModifier(Owner, _damageAfterDodgeModifierListHolder.Get());
			UpdateDamageModifier(Owner, _damageBeforeFinalModifierListHolder.Get());
			UpdateDamageModifier(Owner, _damageFinalModifierListHolder.Get());
			UpdateDamageModifier(Owner, _damageAfterFinalModifierListHolder.Get());
			UpdateDamageModifier(Owner, _damageEndOfFinalModifierListHolder.Get());
			UpdateDamageModifier(Owner, _damageDodgerListHolder.Get());
			UpdateDamageModifier(Owner, _damageFlagModifierListHolder.Get());
			UpdateRegen();
		}
	}

	private void UpdateRegen()
	{
		if (IsDead() || Owner.BattleInstance.IsBattleClosingOrDisabled())
		{
			return;
		}
		int deltaTimeMs = Owner.BattleInstance.DeltaTimeMs;
		if (deltaTimeMs <= 0)
		{
			return;
		}
		long num = _lastMpRegenTimeHolder.Get();
		long currentTimeMs = Owner.BattleInstance.MainTimer.GetCurrentTimeMs();
		if (num <= -1)
		{
			num = currentTimeMs;
			_lastMpRegenTimeHolder.Set(currentTimeMs);
		}
		long num2 = (currentTimeMs - num) / deltaTimeMs;
		if (num2 > 0)
		{
			num += deltaTimeMs * num2;
			_lastMpRegenTimeHolder.Set(num);
			long finalStat = Owner.StatController.GetFinalStat(StatType.HpOnTime);
			long finalStat2 = Owner.StatController.GetFinalStat(StatType.HpRatioOnTime);
			int hpRegenM = Owner.BattleInstance.InstanceConstant.HpRegenM;
			int num3 = (int)((GetMaxMp() * (hpRegenM + finalStat2) / 1000 + finalStat) * num2 * deltaTimeMs / 1000);
			if (num3 > 0)
			{
				HealFlag healFlag = HealFlag.HpOnTime | HealFlag.Supplemently;
				GiveHeal(num3, null, 0L, 0, 0, 0, ref healFlag, 0, 0);
			}
			long finalStat3 = Owner.StatController.GetFinalStat(StatType.MpOnTime);
			long finalStat4 = Owner.StatController.GetFinalStat(StatType.MpRatioOnTime);
			long finalStat5 = Owner.StatController.GetFinalStat(StatType.MpRegenRatioM);
			int mpRegenM = Owner.BattleInstance.InstanceConstant.MpRegenM;
			int num4 = (int)((finalStat3 + mpRegenM + GetMaxMp() * finalStat4 / 1000) * num2 * deltaTimeMs * finalStat5 / 1000000);
			if (num4 > 0)
			{
				AddMp(num4, 0L, ChangeMp.ReasonType.MpOnTime, Owner, 0L);
			}
		}
	}

	public bool IsAlive()
	{
		return Hp > 0;
	}

	public bool IsDead()
	{
		return !IsAlive();
	}

	private bool GetCustomStat(StatType statType, out long stat)
	{
		switch (statType)
		{
		case StatType.Hp:
			stat = Hp;
			return true;
		case StatType.HpRatioM:
			stat = HpRatioM;
			return true;
		case StatType.Mp:
			stat = Mp;
			return true;
		case StatType.MpRatioM:
			stat = MpRatioM;
			return true;
		case StatType.AllyCount:
			stat = Owner.BattleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(Owner.TeamController.TeamId);
			return true;
		case StatType.EnemyCount:
			stat = Owner.BattleInstance.CreatureContainer.GetAliveCreatureCountOfOtherTeam(Owner.TeamController.TeamId);
			return true;
		case StatType.TargetableEnemyCount:
			stat = Owner.BattleInstance.CreatureContainer.GetAliveCreatureCountOfOtherTeam(Owner.TeamController.TeamId, targetableOnly: true);
			return true;
		case StatType.BossCcCurGauge:
			stat = GetBossCcCurGauge();
			return true;
		case StatType.BossCcCurGaugeR:
			stat = GetBossCcCurGaugeRatio();
			return true;
		case StatType.MaxHp:
			if (_overridenMaxHpHolder.Get() > 0)
			{
				stat = _overridenMaxHpHolder.Get();
				return true;
			}
			break;
		}
		if (statType == StatType.MaxMp && _overridenMaxMpHolder.Get() > 0)
		{
			stat = _overridenMaxMpHolder.Get();
			return true;
		}
		stat = 0L;
		return false;
	}

	private long GetBossCcCurGauge()
	{
		if (!(Owner.BattleInstance.Snapshot.TacticManager.Tactic is BossCcGauge bossCcGauge))
		{
			return 0L;
		}
		return bossCcGauge.CurGauge;
	}

	private long GetBossCcCurGaugeRatio()
	{
		if (!(Owner.BattleInstance.Snapshot.TacticManager.Tactic is BossCcGauge bossCcGauge))
		{
			return 0L;
		}
		return bossCcGauge.CurGauge * 1000 / bossCcGauge.MaxGauge;
	}

	public void DirtyFinalStat(StatType statType)
	{
		_cachedFinalStatDictionaryHolder.Get().Remove((int)statType);
	}

	public void DirtyFinalStatAll()
	{
		_cachedFinalStatDictionaryHolder.Get().Clear();
	}

	public long GetBaseStat(StatType statType)
	{
		long num = ((Owner.CreateInfo.StatData != null) ? Owner.CreateInfo.StatData.GetStat(statType) : 0);
		BattleDefineData battleDefineData = ((Owner.BattleInstance != null) ? Owner.BattleInstance.BattleDefine : null);
		if (battleDefineData != null)
		{
			int modStatRatioM = battleDefineData.GetModStatRatioM(statType);
			num = num * modStatRatioM / 1000;
		}
		return num;
	}

	public long GetFinalStatNoCache(StatType statType, bool allowNegative, bool applyAttenuation)
	{
		long baseStat = GetBaseStat(statType);
		int num = CalculateMultedStat(statType, applyAttenuation);
		int num2 = CalculateAddedStat(statType, applyAttenuation);
		long num3 = MultAndAddStat(baseStat, num, num2, allowNegative);
		if (applyAttenuation)
		{
			num3 = AttenuateStat(statType, num3);
		}
		return num3;
	}

	public bool GetOverrideStat(StatType statType, out long statValue)
	{
		statValue = 0L;
		bool result = false;
		LinkedListNode<IStatOverrider> linkedListNode = _statOverriderListHolder.Get().First;
		while (linkedListNode != null)
		{
			IStatOverrider value = linkedListNode.Value;
			linkedListNode = linkedListNode.Next;
			if (value != null && value.GetStatType() == statType)
			{
				statValue = value.GetOverridenStatValue();
				result = true;
				break;
			}
		}
		return result;
	}

	public SnapshotDictionary<int, long> GetFinalStatDictionary()
	{
		return _cachedFinalStatDictionaryHolder.Get();
	}

	public long GetFinalStat(StatType statType, bool applyAttenuation = true)
	{
		bool allowNegative = StatTypeAttribute.IsAllowNegativeStat(statType);
		long stat = 0L;
		if (GetCustomStat(statType, out stat))
		{
			return stat;
		}
		long value = 0L;
		if (_cachedFinalStatDictionaryHolder.Get().TryGetValue((int)statType, out value))
		{
			return value;
		}
		long statValue = 0L;
		if (!GetOverrideStat(statType, out statValue))
		{
			long baseStat = GetBaseStat(statType);
			statValue = MultAndAddStat(statType, baseStat, allowNegative);
			if (applyAttenuation)
			{
				statValue = AttenuateStat(statType, statValue);
			}
		}
		_cachedFinalStatDictionaryHolder.Get().Remove((int)statType);
		_cachedFinalStatDictionaryHolder.Get().Add((int)statType, statValue);
		return statValue;
	}

	public static long GetFinalStat(BattleCreature creature, StatType statType, bool applyAttenuation = true)
	{
		return creature?.StatController.GetFinalStat(statType, applyAttenuation) ?? 0;
	}

	public int GetAddedStat(StatType statType)
	{
		int value = 0;
		if (_cachedAddedStatDictionaryHolder.Get().TryGetValue((int)statType, out value))
		{
			return value;
		}
		int num = CalculateAddedStat(statType);
		_cachedAddedStatDictionaryHolder.Get().Remove((int)statType);
		_cachedAddedStatDictionaryHolder.Get().Add((int)statType, num);
		return num;
	}

	private int CalculateAddedStat(StatType statType, bool applyAttenuation = true)
	{
		int num = 0;
		for (LinkedListNode<IStatModifier> linkedListNode = _statModifierListHolder.Get().First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			IStatModifier value = linkedListNode.Value;
			StatType[] statTypes = value.GetStatTypes();
			if (statTypes != null && statTypes.Contains(statType))
			{
				num += value.GetAddedStat();
			}
		}
		if (applyAttenuation)
		{
			StatAttenuationData data = GameRoot.Instance.AddedStatAttenuationDataContainer.GetData(statType);
			if (data != null)
			{
				num = (int)data.Attenuate(num);
			}
		}
		return num;
	}

	public static long GetBaseStat(BattleCreature creature, StatType statType)
	{
		return creature?.StatController.GetBaseStat(statType) ?? 0;
	}

	public static int GetAddedStat(BattleCreature creature, StatType statType)
	{
		return creature?.StatController.GetAddedStat(statType) ?? 0;
	}

	public int GetMultedStat(StatType statType)
	{
		int value = 0;
		if (_cachedMultedStatDictionaryHolder.Get().TryGetValue((int)statType, out value))
		{
			return value;
		}
		int num = CalculateMultedStat(statType);
		_cachedMultedStatDictionaryHolder.Get().Remove((int)statType);
		_cachedMultedStatDictionaryHolder.Get().Add((int)statType, num);
		return num;
	}

	private int CalculateMultedStat(StatType statType, bool applyAttenuation = true)
	{
		int num = 0;
		for (LinkedListNode<IStatModifier> linkedListNode = _statModifierListHolder.Get().First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			IStatModifier value = linkedListNode.Value;
			StatType[] statTypes = value.GetStatTypes();
			if (statTypes != null && statTypes.Contains(statType))
			{
				num += value.GetMultedStat();
			}
		}
		if (GameRoot.Instance.StatAttenuationDataContainer.GetData(statType) != null || statType == StatType.PhysicalAttack || statType == StatType.MagicalAttack || statType == StatType.PhysicalDefense || statType == StatType.MagicalDefense)
		{
			num = (int)AttenuateStatUnderMinX(num);
		}
		else if (applyAttenuation)
		{
			StatAttenuationData data = GameRoot.Instance.MultedStatAttenuationDataContainer.GetData(statType);
			if (data != null)
			{
				num = (int)data.Attenuate(num);
			}
		}
		return num;
	}

	public static int GetMultedStat(BattleCreature creature, StatType statType)
	{
		return creature?.StatController.GetMultedStat(statType) ?? 0;
	}

	public static int GetModified(BattleCreature creature, int skillIndex, int operationNo, OperationFieldType fieldType)
	{
		if (creature == null)
		{
			return 0;
		}
		if (skillIndex <= 0)
		{
			return 0;
		}
		return creature.SkillController.ModifierContainer.GetModifiedInt(skillIndex, operationNo, fieldType, 0, 0);
	}

	public void MakeStatDirty(StatType statType)
	{
		_cachedAddedStatDictionaryHolder.Get().Remove((int)statType);
		_cachedMultedStatDictionaryHolder.Get().Remove((int)statType);
		_cachedFinalStatDictionaryHolder.Get().Remove((int)statType);
		AnnounceStatChangedToObserver(statType);
	}

	public long MultAndAddStat(StatType statType, long stat, bool allowNegative = false)
	{
		int multedStat = GetMultedStat(statType);
		int addedStat = GetAddedStat(statType);
		return MultAndAddStat(stat, multedStat, addedStat, allowNegative);
	}

	public static long MultAndAddStat(long baseStat, long multedStat, long addedStat, bool allowNegative = false)
	{
		long num = baseStat * System.Math.Max(1000 + multedStat, 0L) / 1000 + addedStat;
		if (!allowNegative)
		{
			num = System.Math.Max(num, 0L);
		}
		return num;
	}

	public static long MultStat(long baseStat, int multedStat)
	{
		return System.Math.Max(baseStat * multedStat / 1000, 0L);
	}

	public static long AttenuateStatUnderMinX(long stat)
	{
		if (stat < BattleConstant.MultedStatAttenuationMinX && BattleConstant.MultedStatAttenuationValue != null && BattleConstant.MultedStatAttenuationValue.Length >= 3)
		{
			int k = BattleConstant.MultedStatAttenuationValue[0];
			int a = BattleConstant.MultedStatAttenuationValue[1];
			int b = BattleConstant.MultedStatAttenuationValue[2];
			stat = StatAttenuationData.AttenuateInv(stat, k, a, b);
		}
		return stat;
	}

	public static long AttenuateStat(StatType statType, long stat)
	{
		StatAttenuationData data = GameRoot.Instance.StatAttenuationDataContainer.GetData(statType);
		if (data != null)
		{
			stat = data.Attenuate(stat);
		}
		return stat;
	}

	public void OverrideMaxHp(long maxHp)
	{
		_overridenMaxHpHolder.Set(maxHp);
	}

	public void OverrideMaxMp(int maxMp)
	{
		_overridenMaxMpHolder.Set(maxMp);
	}

	public void SetHp(long hp)
	{
		_hpHolder.Set(hp);
		MakeStatDirty(StatType.Hp);
		MakeStatDirty(StatType.HpRatioM);
	}

	public void SetMp(int mp, bool raiseEvent = false)
	{
		_mpHolder.Set(mp);
		MakeStatDirty(StatType.Mp);
		MakeStatDirty(StatType.MpRatioM);
		if (raiseEvent)
		{
			Owner.RaiseBattleEvent(new ChangeMp
			{
				CreatureUid = Owner.Uid,
				OperationUid = 0L,
				Amount = 0,
				FinalMp = mp,
				Reason = ChangeMp.ReasonType.None
			});
		}
	}

	public void SetMinHp(long minHp)
	{
		_minHpHolder.Set(minHp);
	}

	public int GetHpRatioM()
	{
		long maxHp = GetMaxHp();
		if (maxHp <= 0)
		{
			return 0;
		}
		return (int)((decimal)Hp / (decimal)maxHp * 1000m);
	}

	public long GetMaxHp()
	{
		return GetFinalStat(StatType.MaxHp);
	}

	public int GetMaxMp()
	{
		return (int)GetFinalStat(StatType.MaxMp);
	}

	public long GetMinHp()
	{
		return _minHpHolder.Get();
	}

	public int GetMpRatioM()
	{
		long finalStat = GetFinalStat(StatType.MaxMp);
		if (finalStat <= 0)
		{
			return 0;
		}
		return (int)(Mp * 1000 / finalStat);
	}

	public int MultiplyAttackSpeed(int deltaTimeMs, int extraFactor = 1000)
	{
		long num = GetFinalStat(StatType.AttackSpeed);
		if (num < BattleConstant.MinAttackSpeed)
		{
			num = BattleConstant.MinAttackSpeed;
		}
		return (int)(deltaTimeMs * num * extraFactor / 1000000);
	}

	public int GetSkillSlotAddedLevel(int slotIndex)
	{
		if (slotIndex > 4)
		{
			return 0;
		}
		int num = 0;
		return slotIndex switch
		{
			1 => (int)(GetFinalStat(StatType.SkillLevelSlot1) + GetFinalStat(StatType.SkillLevelAllSlot)), 
			2 => (int)(GetFinalStat(StatType.SkillLevelSlot2) + GetFinalStat(StatType.SkillLevelAllSlot)), 
			3 => (int)(GetFinalStat(StatType.SkillLevelSlot3) + GetFinalStat(StatType.SkillLevelAllSlot)), 
			4 => (int)(GetFinalStat(StatType.SkillLevelSlot4) + GetFinalStat(StatType.SkillLevelAllSlot)), 
			_ => 0, 
		};
	}

	public int GetSoulWeaponAddedMaxUseCount()
	{
		return (int)GetFinalStat(StatType.SoulWeaponMaxUseCount);
	}

	public void MakeDirtyModifier(IStatModifier statModifier)
	{
		if (statModifier != null)
		{
			StatType[] statTypes = statModifier.GetStatTypes();
			for (int i = 0; i < statTypes.Length; i++)
			{
				MakeStatDirty(statTypes[i]);
			}
		}
	}

	public void AddModifier(IStatModifier statModifier)
	{
		if (statModifier != null)
		{
			_statModifierListHolder.Get().AddLast(statModifier);
			MakeDirtyModifier(statModifier);
		}
	}

	public bool RemoveModifier(IStatModifier statModifier)
	{
		if (statModifier == null)
		{
			return false;
		}
		if (_statModifierListHolder.Get().Remove(statModifier))
		{
			MakeDirtyModifier(statModifier);
			return true;
		}
		return false;
	}

	public void AddOverriderFirst(IStatOverrider overrider)
	{
		if (overrider != null)
		{
			_statOverriderListHolder.Get().AddFirst(overrider);
			MakeStatDirty(overrider.GetStatType());
		}
	}

	public void AddOverrider(IStatOverrider overrider)
	{
		if (overrider != null)
		{
			_statOverriderListHolder.Get().AddLast(overrider);
			MakeStatDirty(overrider.GetStatType());
		}
	}

	public bool RemoveOverrider(IStatOverrider overrider)
	{
		if (overrider == null)
		{
			return false;
		}
		if (_statOverriderListHolder.Get().Remove(overrider))
		{
			MakeStatDirty(overrider.GetStatType());
			return true;
		}
		return false;
	}

	private static void AddDamageModifier<T>(BattleCreature creature, SnapshotLinkedList<T> list, T modifier) where T : IBaseDamageModifier
	{
		if (!list.Contains(modifier))
		{
			list.AddLast(modifier);
		}
	}

	private static bool RemoveDamageModifier<T>(BattleCreature creature, SnapshotLinkedList<T> list, T modifier) where T : IBaseDamageModifier
	{
		bool num = list.Remove(modifier);
		if (num)
		{
			long operationUid = modifier.GetOperationUid();
			if (operationUid > 0)
			{
				creature.OperationController.RemoveOperationInfo(operationUid);
			}
		}
		return num;
	}

	private static void ForEachDamageModifier<T>(SnapshotLinkedList<T> list, Action<T> action) where T : IBaseDamageModifier
	{
		LinkedListNode<T> linkedListNode = list.First;
		while (linkedListNode != null)
		{
			LinkedListNode<T> next = linkedListNode.Next;
			T value = linkedListNode.Value;
			if (value.IsValid())
			{
				action(value);
			}
			linkedListNode = next;
		}
	}

	private static void UpdateDamageModifier<T>(BattleCreature creature, SnapshotLinkedList<T> list) where T : IBaseDamageModifier
	{
		List<T> list2 = null;
		foreach (T item in (IEnumerable<T>)list)
		{
			if (!item.IsValid())
			{
				if (list2 == null)
				{
					list2 = new List<T>();
				}
				list2.Add(item);
			}
		}
		if (list2 == null)
		{
			return;
		}
		foreach (T item2 in list2)
		{
			RemoveDamageModifier(creature, list, item2);
		}
	}

	public void AddAttackModifier(IAttackModifier modifier)
	{
		AddDamageModifier(Owner, _attackModifierListHolder.Get(), modifier);
	}

	public bool RemoveAttackModifier(IAttackModifier modifier)
	{
		return RemoveDamageModifier(Owner, _attackModifierListHolder.Get(), modifier);
	}

	public SnapshotLinkedList<IDamageModifier> GetDamageModifierList(DamagePipelinePosition pipelinePos)
	{
		switch (pipelinePos)
		{
		case DamagePipelinePosition.DamageAmplifier:
			return _damageDamageAmplifierModifierListHolder.Get();
		case DamagePipelinePosition.BeforeFinal:
			return _damageBeforeFinalModifierListHolder.Get();
		case DamagePipelinePosition.Final:
			return _damageFinalModifierListHolder.Get();
		case DamagePipelinePosition.AfterDodge:
			return _damageAfterDodgeModifierListHolder.Get();
		case DamagePipelinePosition.AfterFinal:
			return _damageAfterFinalModifierListHolder.Get();
		case DamagePipelinePosition.EndOfFinal:
			return _damageEndOfFinalModifierListHolder.Get();
		case DamagePipelinePosition.Beginning:
			return _damageBeginningModifierListHolder.Get();
		default:
			VespaDebug.LogError($"Not Defined DamageModifyPipelinePosition.{pipelinePos.ToString()} Yet! ");
			return _damageEndOfFinalModifierListHolder.Get();
		}
	}

	public void AddDamageModifier(IDamageModifier modifier)
	{
		AddDamageModifier(Owner, GetDamageModifierList(modifier.GetPipelinePosition()), modifier);
	}

	public bool RemoveDamageModifier(IDamageModifier modifier)
	{
		return RemoveDamageModifier(Owner, GetDamageModifierList(modifier.GetPipelinePosition()), modifier);
	}

	public void AddModifier(IDamageDodger modifier)
	{
		AddDamageModifier(Owner, _damageDodgerListHolder.Get(), modifier);
	}

	public bool RemoveModifier(IDamageDodger modifier)
	{
		return RemoveDamageModifier(Owner, _damageDodgerListHolder.Get(), modifier);
	}

	public void AddFlagModifier(IDamageFlagModifier modifier)
	{
		AddDamageModifier(Owner, _damageFlagModifierListHolder.Get(), modifier);
	}

	public bool RemoveFlagModifier(IDamageFlagModifier modifier)
	{
		return RemoveDamageModifier(Owner, _damageFlagModifierListHolder.Get(), modifier);
	}

	public static DamageFlag GetDefaultDamageFlag(SkillData skillData)
	{
		if (skillData != null)
		{
			if (skillData.AttrType == SkillAttrType.Physical)
			{
				return DamageFlag.Physical;
			}
			if (skillData.AttrType == SkillAttrType.Magical)
			{
				return DamageFlag.Magical;
			}
		}
		return DamageFlag.None;
	}

	public long GetAttackAmountByPower(int ratio, DamageFlag damageFlag)
	{
		long num = 0L;
		if ((damageFlag & DamageFlag.Physical) == DamageFlag.Physical)
		{
			num = GetFinalStat(StatType.PhysicalAttack);
		}
		else
		{
			if ((damageFlag & DamageFlag.Magical) != DamageFlag.Magical)
			{
				return 0L;
			}
			num = GetFinalStat(StatType.MagicalAttack);
		}
		return MultStat(num, ratio);
	}

	public void GetAttackAmountAndFlagByPower(SkillData skillData, int ratio, out DamageFlag damageFlag, out long amount)
	{
		damageFlag = GetDefaultDamageFlag(skillData);
		amount = GetAttackAmountByPower(ratio, damageFlag);
	}

	public long GetHealAmountByPower(int ratio)
	{
		return MultStat(GetFinalStat(StatType.MagicalAttack), ratio);
	}

	public long GetHpAmountByMaxHp(int ratio)
	{
		return MultStat(GetMaxHp(), ratio);
	}

	public bool AddMp(int amount, long operationUid, ChangeMp.ReasonType reason, BattleCreature caster = null, long buffUid = 0L, int skillId = 0, int stackCount = 0)
	{
		if (!Owner.BattleInstance.IsBattleEnabled())
		{
			return false;
		}
		if (amount <= 0)
		{
			return false;
		}
		long finalStat = GetFinalStat(StatType.MaxMp);
		int num = (int)System.Math.Min(Mp + amount, finalStat);
		if (Owner.ManaHistoryTracker != null)
		{
			Owner.ManaHistoryTracker.Record(amount);
		}
		int healer = 0;
		if (caster != null)
		{
			healer = caster.Uid;
		}
		Owner.BattleInstance.Snapshot.StatisticsManager.AddManaHealMeter(healer, Owner.Uid, amount, skillId);
		amount = num - Mp;
		if (amount <= 0)
		{
			return false;
		}
		SetMp(num);
		UseSkillByMpRatio();
		Owner.RaiseBattleEvent(new ChangeMp
		{
			CreatureUid = Owner.Uid,
			OperationUid = operationUid,
			Amount = amount,
			FinalMp = num,
			Reason = reason
		});
		return true;
	}

	public bool ConsumeMp(int amount, long operationUid = 0L, ChangeMp.ReasonType reason = ChangeMp.ReasonType.Consume)
	{
		if (Owner.BattleInstance.IsBattleClosingOrDisabled())
		{
			return false;
		}
		if (amount <= 0)
		{
			return false;
		}
		int num = System.Math.Max(Mp - amount, 0);
		amount = Mp - num;
		if (amount <= 0)
		{
			return false;
		}
		SetMp(num);
		UseSkillByMpRatio();
		Owner.RaiseBattleEvent(new ChangeMp
		{
			CreatureUid = Owner.Uid,
			OperationUid = operationUid,
			Amount = -amount,
			FinalMp = num,
			Reason = reason
		});
		return true;
	}

	private void UseSkillByMpRatio()
	{
		OnMpRatio onMpRatio = SnapshotObjectPool<OnMpRatio>.Alloc();
		onMpRatio.TriggerCreature = Owner;
		onMpRatio.TargetCreatures = new BattleCreature[1] { Owner };
		onMpRatio.MpRatioM = MpRatioM;
		Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onMpRatio);
	}

	public bool ConsumeHpRatio(int ratio, long operationUid = 0L)
	{
		if (Owner.BattleInstance.IsBattleClosingOrDisabled())
		{
			return false;
		}
		if (ratio <= 0)
		{
			return false;
		}
		long num = Hp * ratio / 1000;
		if (num <= 0)
		{
			return false;
		}
		long hp = System.Math.Max(Hp - num, 1L);
		SetHp(hp);
		return true;
	}

	public bool TestChanceRateResult(BattleCreature creature, StatType statType, string desc)
	{
		if (creature == null)
		{
			return false;
		}
		long finalStat = creature.StatController.GetFinalStat(statType);
		return Owner.TestChanceRateResult(finalStat, desc);
	}

	public bool TestHitChanceRateResult(long hitChance, long dodgeChance, int attackerLevel, string desc)
	{
		long num = System.Math.Min(hitChance, 1000L);
		long num2 = hitChance - num;
		long ratio = num * (1000 - dodgeChance) / 1000 + num2;
		ratio = Owner.GetAccumulatedHitRatio(attackerLevel, ratio);
		return Owner.TestChanceRateResult(ratio, desc);
	}

	public DamageResultType GiveDamage_Season1(long amount, BattleCreature attacker, OperationInfo operationInfo, long operationUid, int operationNo, SkillData skillData, SkillData firedSkillData, int skillLevel, int stackCount, string damageStateType, ref DamageFlag damageFlag, out long finalDamage)
	{
		finalDamage = 0L;
		long initialAmount = amount;
		if (Owner.BattleInstance.IsBattleClosingOrDisabled())
		{
			return DamageResultType.BattleClosing;
		}
		_prevDamageInfoHolder.Set(null);
		_hpHolder.Get();
		if (IsDead())
		{
			return DamageResultType.AlreadyDied;
		}
		if (Owner.BattleInstance.DisableGiveDamage)
		{
			return DamageResultType.Immune;
		}
		if (attacker != null)
		{
			DamageFlag copiedDamageFlag = damageFlag;
			ForEachDamageModifier(attacker.StatController._damageFlagModifierListHolder.Get(), delegate(IDamageFlagModifier modifier)
			{
				modifier.Modify(Owner, ref copiedDamageFlag);
			});
			damageFlag = copiedDamageFlag;
		}
		ModifyDamage(attacker, DamagePipelinePosition.Beginning, ref amount, ref damageFlag);
		int num = skillData?.Index ?? 0;
		if (firedSkillData != null)
		{
			_ = firedSkillData.Index;
		}
		int num2 = ((int?)attacker?.CreateInfo.HeroInfo.Level) ?? skillLevel;
		short level = Owner.CreateInfo.HeroInfo.Level;
		bool flag = (damageFlag & DamageFlag.Physical) != 0;
		bool flag2 = !flag;
		bool flag3 = (damageFlag & DamageFlag.Dot) != 0;
		bool flag4 = operationInfo?.IsBaseAttack ?? false;
		bool flag5 = (flag && Owner.StateController.IsImmuneToPhysicalAttack()) || (flag2 && Owner.StateController.IsImmuneToMagicalAttack());
		if (flag5 && (damageFlag & DamageFlag.IgnoreImmune) != DamageFlag.None)
		{
			flag5 = false;
		}
		if (flag5)
		{
			OnImmune onImmune = SnapshotObjectPool<OnImmune>.Alloc();
			onImmune.TriggerCreature = attacker;
			onImmune.TargetCreatures = new BattleCreature[1] { Owner };
			onImmune.SkillIndex = num;
			onImmune.DamageFlag = damageFlag;
			Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onImmune);
			Owner.RaiseBattleEvent(new Damage
			{
				CreatureUid = Owner.Uid,
				OperationUid = operationUid,
				Amount = 0L,
				FinalHp = 0L,
				FinalPercentage = 0,
				DamageFlag = (DamageFlag.Immune | damageFlag),
				DamageStateType = damageStateType,
				IsDead = false
			});
			return DamageResultType.Immune;
		}
		if ((damageFlag & DamageFlag.Unavoidable) == 0)
		{
			bool dodged = false;
			if ((attacker != null && flag && attacker.StateController.IsPhysicalAttackBlinded()) || (flag2 && attacker.StateController.IsMagicalAttackBlinded()))
			{
				dodged = true;
			}
			if (!dodged && ((flag && Owner.StateController.CanPhysicalDodgeAll()) || (flag2 && Owner.StateController.CanMagicalDodgeAll())))
			{
				dodged = true;
			}
			if (!dodged)
			{
				ForEachDamageModifier(_damageDodgerListHolder.Get(), delegate(IDamageDodger modifier)
				{
					dodged = (dodged ? dodged : modifier.Dodge());
				});
			}
			if (!dodged)
			{
				long num3 = (flag ? BattleConstant.PhysicalDefaultHitChance : BattleConstant.MagicalDefaultHitChance);
				long hitChance = num3;
				if (attacker != null && !flag3)
				{
					hitChance = num3 + GetFinalStat(attacker, flag ? StatType.PhysicalHitChance : StatType.MagicalHitChance);
					hitChance = System.Math.Max(hitChance, 0L);
				}
				long finalStat = GetFinalStat(flag ? StatType.PhysicalDodgeChance : StatType.MagicalDodgeChance);
				bool flag6 = TestHitChanceRateResult(hitChance, finalStat, num2, "GiveDamage.hit");
				dodged = !flag6;
			}
			if (dodged)
			{
				Owner.RaiseBattleEvent(new Damage
				{
					CreatureUid = Owner.Uid,
					OperationUid = operationUid,
					Amount = 0L,
					FinalHp = 0L,
					FinalPercentage = 0,
					DamageFlag = DamageFlag.Dodged,
					DamageStateType = damageStateType,
					IsDead = false
				});
				if (num != 0)
				{
					OnDodge onDodge = SnapshotObjectPool<OnDodge>.Alloc();
					onDodge.TriggerCreature = attacker;
					onDodge.TargetCreatures = new BattleCreature[1] { Owner };
					onDodge.SkillIndex = num;
					Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onDodge);
					if (attacker != null)
					{
						OnMiss onMiss = SnapshotObjectPool<OnMiss>.Alloc();
						onMiss.TriggerCreature = attacker;
						onMiss.TargetCreatures = new BattleCreature[1] { Owner };
						onMiss.SkillIndex = num;
						attacker.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onMiss);
					}
				}
				return DamageResultType.Dodged;
			}
		}
		ModifyDamage(attacker, DamagePipelinePosition.AfterDodge, ref amount, ref damageFlag);
		StatType statType = (flag ? StatType.PhysicalPiercePower : StatType.MagicalPiercePower);
		long num4 = 0L;
		int num5 = 0;
		if (attacker != null)
		{
			long baseStat = GetBaseStat(attacker, statType);
			int addedStat = GetAddedStat(attacker, statType);
			int modified = GetModified(attacker, num, operationNo, OperationFieldType.PiercePower);
			num4 = System.Math.Max((int)baseStat + addedStat + modified, 0);
			num5 = GetMultedStat(attacker, statType);
		}
		long num6 = amount;
		long val = num6 * BattleConstant.MinAttackDamageFactor / 1000;
		long finalStat2 = GetFinalStat(flag ? StatType.PhysicalDefense : StatType.MagicalDefense);
		if (finalStat2 > 0 && num5 != 0)
		{
			num4 += finalStat2 * num5 / 1000;
		}
		if (num4 != 0L)
		{
			num4 = (int)AttenuateStat(statType, num4);
		}
		finalStat2 = MultAndAddStat(finalStat2, -num4, 0L);
		bool flag7 = (damageFlag & DamageFlag.IgnoreDefence) != 0;
		if (flag7)
		{
			finalStat2 = 0L;
		}
		long baseStat2 = RestoredPortal.PunishmentDamage(num6, finalStat2, num2, level, this);
		long val2 = (((damageFlag & DamageFlag.IgnoreToughness) != DamageFlag.None) ? 0 : GetFinalStat(flag ? StatType.PhysicalToughness : StatType.MagicalToughness));
		val2 = System.Math.Min(val2, BattleConstant.MaxToughness);
		long num7 = (((damageFlag & DamageFlag.IgnoreWeakness) != DamageFlag.None) ? 0 : GetFinalStat(flag ? StatType.PhysicalWeakness : StatType.MagicalWeakness));
		baseStat2 = MultAndAddStat(baseStat2, (int)(num7 - val2), 0L);
		bool flag8 = false;
		bool flag9 = (damageFlag & DamageFlag.NoCritical) != 0;
		if (flag9)
		{
			damageFlag &= ~DamageFlag.Critical;
		}
		else
		{
			flag8 = (damageFlag & DamageFlag.Critical) != 0;
		}
		if (!flag8 && !flag9 && attacker != null)
		{
			StatType statType2 = (flag ? StatType.PhysicalCriticalChance : StatType.MagicalCriticalChance);
			long finalStat3 = GetFinalStat(attacker, statType2, applyAttenuation: false);
			finalStat3 += GetModified(attacker, num, operationNo, OperationFieldType.CriticalChance);
			finalStat3 = AttenuateStat(statType2, finalStat3);
			long finalStat4 = GetFinalStat(flag ? StatType.PhysicalCriticalDodgeChance : StatType.MagicalCriticalDodgeChance);
			long ratio = System.Math.Max(1000 - (finalStat3 - finalStat4), 0L);
			ratio = Owner.GetAccumulatedDodgeRatio(num2, ratio);
			long chanceRate = System.Math.Max(1000 - ratio, 0L);
			flag8 = Owner.TestChanceRateResult(chanceRate, "GiveDamage.critical");
		}
		if (flag8)
		{
			StatType statType3 = (flag ? StatType.PhysicalCriticalPower : StatType.MagicalCriticalPower);
			int num8 = (flag ? BattleConstant.PhysicalCriticalDamageFactor : BattleConstant.MagicalCriticalDamageFactor);
			num8 += (int)GetFinalStat(attacker, statType3, applyAttenuation: false);
			num8 += GetModified(attacker, num, operationNo, OperationFieldType.CriticalPower);
			num8 = (int)AttenuateStat(statType3, num8);
			int num9 = (int)GetFinalStat(flag ? StatType.PhysicalCriticalWeakness : StatType.MagicalCriticalWeakness);
			int multedStat = System.Math.Max(num8 + num9, 0);
			baseStat2 = MultStat(baseStat2, multedStat);
			damageFlag |= DamageFlag.Critical;
		}
		if (attacker != null)
		{
			int num10 = 0;
			int num11 = 0;
			if (!flag4)
			{
				num10 = GetMultedStat(attacker, StatType.SkillPower);
				num11 = GetAddedStat(attacker, StatType.SkillPower);
			}
			num10 += GetModified(attacker, num, operationNo, OperationFieldType.SkillPowerR);
			num11 += GetModified(attacker, num, operationNo, OperationFieldType.SkillPower);
			baseStat2 = MultAndAddStat(baseStat2, num10, num11);
		}
		if (stackCount > 1)
		{
			baseStat2 *= stackCount;
		}
		baseStat2 = System.Math.Max(baseStat2, val);
		bool flag10 = false;
		if ((flag && Owner.StateController.CanPhysicalBlockAll()) || (flag2 && Owner.StateController.CanMagicalBlockAll()))
		{
			flag10 = true;
		}
		if (!flag10)
		{
			long finalStat5 = GetFinalStat(flag ? StatType.PhysicalBlockChance : StatType.MagicalBlockChance);
			finalStat5 = Owner.GetAccumulatedDodgeRatio(num2, finalStat5);
			flag10 = Owner.TestChanceRateResult(finalStat5, "GiveDamage.block");
		}
		bool flag11 = (damageFlag & DamageFlag.IgnoreBlock) != 0;
		if (flag10 && flag11)
		{
			flag10 = false;
		}
		if (flag10)
		{
			long val3 = BattleConstant.DefaultBlockPower + GetFinalStat(flag ? StatType.PhysicalBlockPower : StatType.MagicalBlockPower);
			val3 = System.Math.Max(val3, 0L);
			val3 = System.Math.Min(val3, BattleConstant.MaxBlockPower);
			baseStat2 = baseStat2 * System.Math.Max(1000 - val3, 0L) / 1000;
			damageFlag |= DamageFlag.Blocked;
		}
		if (flag11)
		{
			StatType statType4 = (flag ? StatType.PhysicalIgnoreBlockPower : StatType.MagicalIgnoreBlockPower);
			int num12 = (int)GetFinalStat(attacker, statType4, applyAttenuation: false);
			num12 += GetModified(attacker, num, operationNo, OperationFieldType.IgnoreBlockPower);
			num12 = (int)AttenuateStat(statType4, num12);
			baseStat2 = MultStat(baseStat2, num12);
		}
		if (flag7)
		{
			StatType statType5 = (flag ? StatType.PhysicalIgnoreDefensePower : StatType.MagicalIgnoreDefensePower);
			int num13 = (int)GetFinalStat(attacker, statType5, applyAttenuation: false);
			num13 += GetModified(attacker, num, operationNo, OperationFieldType.IgnoreDefencePower);
			num13 = (int)AttenuateStat(statType5, num13);
			baseStat2 = MultStat(baseStat2, num13);
		}
		if (flag3)
		{
			StatType statType6 = (flag ? StatType.PhysicalDotPower : StatType.MagicalDotPower);
			int num14 = (int)GetFinalStat(attacker, statType6, applyAttenuation: false);
			num14 += GetModified(attacker, num, operationNo, OperationFieldType.DotPower);
			num14 = (int)AttenuateStat(statType6, num14);
			baseStat2 = MultStat(baseStat2, num14);
		}
		int num15 = (int)((attacker != null) ? attacker.StatController.GetFinalStat(flag ? StatType.PhysicalAttackPower : StatType.MagicalAttackPower) : 0);
		long num16 = (((damageFlag & DamageFlag.IgnoreDefensePower) != DamageFlag.None) ? 0 : GetFinalStat(flag ? StatType.PhysicalDefensePower : StatType.MagicalDefensePower));
		baseStat2 = baseStat2 * System.Math.Max(1000 + num15, 0) / 1000;
		baseStat2 = baseStat2 * System.Math.Max(1000 - num16, 0L) / 1000;
		if (attacker != null && baseStat2 > 0 && Owner.CreatureData.AttackWeakType != null)
		{
			AttackAttrType attackAttrType = ((firedSkillData != null) ? firedSkillData.AttackType : AttackAttrType.None);
			if (attackAttrType == AttackAttrType.None)
			{
				attackAttrType = attacker.CreatureData.AttackType;
			}
			if (attackAttrType != AttackAttrType.None && Owner.CreatureData.AttackWeakType.Contains(attackAttrType))
			{
				baseStat2 = baseStat2 * BattleConstant.WeakAttackAttrDamageFactor / 1000;
			}
		}
		int num17 = level - num2;
		if (num17 > 0 && Owner.BattleInstance.BattleDefine != null && Owner.BattleInstance.BattleDefine.ApplyLevelPenalty && Owner.CreateInfo.IsAi && Owner.PlayerId == 0L)
		{
			int levelDamageAttenuationFactor = BattleConstant.LevelDamageAttenuationFactor;
			if (levelDamageAttenuationFactor > 0 && levelDamageAttenuationFactor != 1000)
			{
				long accRatio = GameRoot.Instance.ExponentialTableK.GetAccRatio(levelDamageAttenuationFactor, num17);
				baseStat2 = (int)(baseStat2 * accRatio / 1000);
			}
		}
		if (attacker != null && firedSkillData != null)
		{
			DamageFlag copiedDamageFlag2 = damageFlag;
			long copiedDamage = baseStat2;
			ForEachDamageModifier(attacker.StatController._attackModifierListHolder.Get(), delegate(IAttackModifier modifier)
			{
				modifier.Modify(Owner, firedSkillData, ref copiedDamage, ref copiedDamageFlag2);
			});
			damageFlag = copiedDamageFlag2;
			baseStat2 = copiedDamage;
		}
		ModifyDamage(attacker, DamagePipelinePosition.Final, ref baseStat2, ref damageFlag);
		ModifyDamage(attacker, DamagePipelinePosition.AfterFinal, ref baseStat2, ref damageFlag);
		ModifyDamage(attacker, DamagePipelinePosition.EndOfFinal, ref baseStat2, ref damageFlag);
		finalDamage = baseStat2;
		SetHpFromDamage(finalDamage);
		DamageEvent(initialAmount, attacker, operationInfo, operationUid, operationNo, skillData, firedSkillData, skillLevel, stackCount, damageStateType, damageFlag, finalDamage);
		return DamageResultType.Success;
	}

	public DamageResultType GiveDamageNoTrigger_Season1(long amount, BattleCreature attacker, OperationInfo operationInfo, long operationUid, int operationNo, SkillData skillData, SkillData firedSkillData, int skillLevel, int stackCount, string damageStateType, ref DamageFlag damageFlag, out long finalDamage)
	{
		finalDamage = amount;
		if (Owner.BattleInstance.IsBattleClosingOrDisabled())
		{
			return DamageResultType.BattleClosing;
		}
		_hpHolder.Get();
		if (IsDead())
		{
			return DamageResultType.AlreadyDied;
		}
		if (Owner.BattleInstance.DisableGiveDamage)
		{
			return DamageResultType.Immune;
		}
		if (attacker != null)
		{
			DamageFlag copiedDamageFlag = damageFlag;
			ForEachDamageModifier(attacker.StatController._damageFlagModifierListHolder.Get(), delegate(IDamageFlagModifier modifier)
			{
				modifier.Modify(Owner, ref copiedDamageFlag);
			});
			damageFlag = copiedDamageFlag;
		}
		ModifyDamage(attacker, DamagePipelinePosition.Beginning, ref finalDamage, ref damageFlag);
		ModifyDamage(attacker, DamagePipelinePosition.AfterDodge, ref finalDamage, ref damageFlag);
		if (attacker != null && firedSkillData != null)
		{
			DamageFlag copiedDamageFlag2 = damageFlag;
			long copiedDamage = finalDamage;
			ForEachDamageModifier(attacker.StatController._attackModifierListHolder.Get(), delegate(IAttackModifier modifier)
			{
				_ = copiedDamage;
				modifier.Modify(Owner, firedSkillData, ref copiedDamage, ref copiedDamageFlag2);
			});
			damageFlag = copiedDamageFlag2;
			finalDamage = copiedDamage;
		}
		ModifyDamage(attacker, DamagePipelinePosition.Final, ref finalDamage, ref damageFlag);
		ModifyDamage(attacker, DamagePipelinePosition.AfterFinal, ref finalDamage, ref damageFlag);
		ModifyDamage(attacker, DamagePipelinePosition.EndOfFinal, ref finalDamage, ref damageFlag);
		SetHpFromDamage(finalDamage);
		ExcuteCustomeDamageEvent(amount, attacker, operationInfo, operationUid, operationNo, skillData, firedSkillData, skillLevel, stackCount, damageStateType, damageFlag, finalDamage);
		return DamageResultType.Success;
	}

	public DamageResultType GiveDamage(long amount, BattleCreature attacker, OperationInfo operationInfo, long operationUid, int operationNo, SkillData skillData, SkillData firedSkillData, int skillLevel, int stackCount, string damageStateType, ref DamageFlag damageFlag, out long finalDamage)
	{
		return GiveDamage_Season1(amount, attacker, operationInfo, operationUid, operationNo, skillData, firedSkillData, skillLevel, stackCount, damageStateType, ref damageFlag, out finalDamage);
	}

	public DamageResultType GiveDamageNoTrigger(long amount, BattleCreature attacker, OperationInfo operationInfo, long operationUid, int operationNo, SkillData skillData, SkillData firedSkillData, int skillLevel, int stackCount, string damageStateType, ref DamageFlag damageFlag, out long finalDamage)
	{
		return GiveDamageNoTrigger_Season1(amount, attacker, operationInfo, operationUid, operationNo, skillData, firedSkillData, skillLevel, stackCount, damageStateType, ref damageFlag, out finalDamage);
	}

	public void SetHpFromDamage(long damage)
	{
		long hp = Hp;
		long maxHp = GetMaxHp();
		long num = System.Math.Min(System.Math.Max(Hp - damage, GetMinHp()), maxHp);
		if (Owner.CreateInfo.ForceNoDie && num <= 0)
		{
			num = 1L;
		}
		SetHp(num);
		AnnounceHpChangedToObserver(hp, num);
	}

	public void DamageModifiedEventLogOnly(long operationUid, DamagePipelinePosition pipelinePosition, int mult, int add, long beforeDamage, long afterDamage)
	{
	}

	public void DamageEvent(long initialAmount, BattleCreature attacker, OperationInfo operationInfo, long operationUid, int operationNo, SkillData skillData, SkillData firedSkillData, int skillLevel, int stackCount, string damageStateType, DamageFlag damageFlag, long damage)
	{
		long maxHp = GetMaxHp();
		long hp = Hp;
		bool flag = Hp <= 0;
		int finalPercentage = (int)((maxHp > 0) ? (hp * 100 / maxHp) : 0);
		SavePrevDamageInfo(attacker, operationInfo, initialAmount, damage, damageFlag);
		Owner.RaiseBattleEvent(new Damage
		{
			CreatureUid = Owner.Uid,
			OperationUid = operationUid,
			Amount = damage,
			FinalHp = hp,
			FinalPercentage = finalPercentage,
			DamageFlag = damageFlag,
			DamageStateType = damageStateType,
			IsDead = flag
		});
		if (!flag)
		{
			int num = (int)((maxHp > 0) ? (damage * 1000 / maxHp) : 0);
			RecoverMpByStat(StatType.MpOnDamage, StatType.MpRatioOnDamage, ChangeMp.ReasonType.MpOnDamage, num);
			RecoverHpByStat(StatType.HpOnDamage, StatType.HpRatioOnDamage, HealFlag.HpOnDamage | HealFlag.Supplemently);
		}
		HpDrain(attacker, damage, damageFlag);
		RecoredDamageMeter(attacker, damage, damageFlag, firedSkillData);
		int skillIndex = skillData?.Index ?? 0;
		_ = firedSkillData?.Index;
		DamageFlag damageFlagCopy = damageFlag;
		Action action = delegate
		{
			BattleCreature[] targetCreatures = new BattleCreature[1] { Owner };
			OnDamage(attacker, targetCreatures, skillIndex, initialAmount, damage, damageFlagCopy);
			OnHpRatio(attacker, targetCreatures, skillIndex);
			OnBlock(attacker, targetCreatures, skillIndex, damageFlagCopy);
			bool flag2 = (damageFlagCopy & DamageFlag.DerivedDamage) != 0;
			bool flag3 = (damageFlagCopy & DamageFlag.IgnoreAttackersTrigger) == 0;
			if (attacker != null && flag3 && !flag2)
			{
				OnAttack(attacker, targetCreatures, skillIndex, initialAmount, damage, damageFlagCopy);
				OnBaseAttack(attacker, targetCreatures, operationInfo, initialAmount, damage, damageFlagCopy);
				OnCritical(attacker, targetCreatures, skillIndex, initialAmount, damage, damageFlagCopy);
			}
			if (skillIndex > 0 && !flag2)
			{
				OnFriendDamage_In_DefenderTeam(attacker, targetCreatures, skillIndex, initialAmount, damage, damageFlagCopy);
				OnFriendAttack_In_AttackerTeam(attacker, targetCreatures, skillIndex, initialAmount, damage, damageFlagCopy);
			}
		};
		if (skillData != null)
		{
			action();
		}
		if (flag)
		{
			Owner.OnDied(attacker, skillIndex, operationUid);
			return;
		}
		int num2 = BattleConstant.ShortKnockbackDamageMinRate / 10;
		if (damage * 100 / maxHp >= num2)
		{
			string name = typeof(Knockback).Name;
			StateData data = GameRoot.Instance.StateDataContainer.GetData(name);
			Owner.StateController.AddState(data, BattleConstant.ShortKnockbackDurationMs, attacker);
		}
	}

	public void ExcuteCustomeDamageEvent(long initialAmount, BattleCreature attacker, OperationInfo operationInfo, long operationUid, int operationNo, SkillData skillData, SkillData firedSkillData, int skillLevel, int stackCount, string damageStateType, DamageFlag damageFlag, long damage)
	{
		long maxHp = GetMaxHp();
		long hp = Hp;
		bool flag = Hp <= 0;
		int finalPercentage = (int)((maxHp > 0) ? (hp * 100 / maxHp) : 0);
		Owner.RaiseBattleEvent(new Damage
		{
			CreatureUid = Owner.Uid,
			OperationUid = operationUid,
			Amount = damage,
			FinalHp = hp,
			FinalPercentage = finalPercentage,
			DamageFlag = damageFlag,
			DamageStateType = damageStateType,
			IsDead = flag
		});
		if (!flag)
		{
			int num = (int)((maxHp > 0) ? (damage * 1000 / maxHp) : 0);
			RecoverMpByStat(StatType.MpOnDamage, StatType.MpRatioOnDamage, ChangeMp.ReasonType.MpOnDamage, num);
			RecoverHpByStat(StatType.HpOnDamage, StatType.HpRatioOnDamage, HealFlag.HpOnDamage | HealFlag.Supplemently);
		}
		RecoredDamageMeter(attacker, damage, damageFlag, firedSkillData);
		int skillIndex = skillData?.Index ?? 0;
		_ = firedSkillData?.Index;
		DamageFlag damageFlagCopy = damageFlag;
		Action action = delegate
		{
			BattleCreature[] targetCreatures = new BattleCreature[1] { Owner };
			OnDamage(attacker, targetCreatures, skillIndex, initialAmount, damage, damageFlagCopy);
			OnHpRatio(attacker, targetCreatures, skillIndex);
			OnBlock(attacker, targetCreatures, skillIndex, damageFlagCopy);
		};
		if (skillData != null)
		{
			action();
		}
		if (flag)
		{
			Owner.OnDied(attacker, skillIndex, operationUid);
		}
	}

	private void SavePrevDamageInfo(BattleCreature attacker, OperationInfo operationInfo, long initAmount, long damage, DamageFlag damageFlag)
	{
		PrevDamageInfo prevDamageInfo = SnapshotObjectPool<PrevDamageInfo>.Alloc();
		prevDamageInfo.OperationUid = operationInfo.Uid;
		prevDamageInfo.Attacker = attacker;
		prevDamageInfo.Damaged = Owner;
		prevDamageInfo.SkillData = operationInfo.SkillData;
		prevDamageInfo.FiredSkillData = operationInfo.FiredSkillData;
		prevDamageInfo.SkillLevel = operationInfo.SkillLevel;
		prevDamageInfo.StackCount = operationInfo.StackCount;
		prevDamageInfo.StateType = operationInfo.State;
		prevDamageInfo.DamageFlag = damageFlag;
		prevDamageInfo.InitialAmount = initAmount;
		prevDamageInfo.FinalAmount = damage;
		_prevDamageInfoHolder.Set(prevDamageInfo);
		attacker?.StatController._prevAttackInfoHolder.Set(prevDamageInfo);
	}

	private void HpDrain(BattleCreature attacker, long damage, DamageFlag damageFlag)
	{
		if (attacker != null)
		{
			long finalStat = GetFinalStat(attacker, StatType.HpStealPower);
			if (finalStat > 0)
			{
				long num = damage * finalStat / 1000;
				HealFlag healFlag = HealFlag.HpSteal | HealFlag.Supplemently;
				attacker.StatController.GiveHeal((int)num, attacker, 0L, 0, 0, 1, ref healFlag, 0, 0);
			}
		}
	}

	private void RecoredDamageMeter(BattleCreature attacker, long damage, DamageFlag damageFlag, SkillData firedSkillData)
	{
		if (attacker != null)
		{
			Owner.BattleInstance.Snapshot.StatisticsManager.AddDamageMeter(attacker.Uid, Owner.Uid, damage, firedSkillData.Index);
			long val = Owner.StatController.GetFinalStat(StatType.ThreatGain) / 1000;
			val = System.Math.Max(val, 0L);
			int threatMultiplier = GameRoot.Instance.ThreatDataContainer.GetThreatMultiplier(attacker.CreatureData.TagType, ThreatType.Damage);
			long amount = damage * threatMultiplier * val / 1000;
			attacker.ThreatHistoryTracker.Record(amount);
			if ((damageFlag & DamageFlag.Critical) > DamageFlag.None)
			{
				int threatMultiplier2 = GameRoot.Instance.ThreatDataContainer.GetThreatMultiplier(attacker.CreatureData.TagType, ThreatType.CriticalDamage);
				long amount2 = damage * threatMultiplier2 * val / 1000;
				attacker.ThreatHistoryTracker.Record(amount2);
			}
		}
	}

	private void OnDamage(BattleCreature triggerTarget, BattleCreature[] targetCreatures, int skillIndex, long initialAmount, long damage, DamageFlag damageFlag)
	{
		OnDamage onDamage = SnapshotObjectPool<NShared.OnDamage>.Alloc();
		onDamage.TriggerCreature = triggerTarget;
		onDamage.TargetCreatures = targetCreatures;
		onDamage.SkillIndex = skillIndex;
		onDamage.FinalHp = Hp;
		onDamage.InitialAmount = initialAmount;
		onDamage.Amount = damage;
		onDamage.DamageFlag = damageFlag;
		Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onDamage);
	}

	private void OnHpRatio(BattleCreature triggerTarget, BattleCreature[] targetCreatures, int skillIndex)
	{
		OnHpRatio onHpRatio = SnapshotObjectPool<NShared.OnHpRatio>.Alloc();
		onHpRatio.TriggerCreature = triggerTarget;
		onHpRatio.TargetCreatures = targetCreatures;
		onHpRatio.HpRatioM = HpRatioM;
		onHpRatio.SkillIndex = skillIndex;
		Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onHpRatio);
	}

	private void OnBlock(BattleCreature triggerTarget, BattleCreature[] targetCreatures, int skillIndex, DamageFlag damageFlag)
	{
		if ((damageFlag & DamageFlag.Blocked) != DamageFlag.None)
		{
			OnBlock onBlock = SnapshotObjectPool<NShared.OnBlock>.Alloc();
			onBlock.TriggerCreature = triggerTarget;
			onBlock.TargetCreatures = targetCreatures;
			onBlock.SkillIndex = skillIndex;
			Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onBlock);
		}
	}

	private void OnAttack(BattleCreature triggerTarget, BattleCreature[] targetCreatures, int skillIndex, long initialAmount, long damage, DamageFlag damageFlag)
	{
		OnAttack onAttack = SnapshotObjectPool<NShared.OnAttack>.Alloc();
		onAttack.TriggerCreature = triggerTarget;
		onAttack.TargetCreatures = targetCreatures;
		onAttack.SkillIndex = skillIndex;
		onAttack.InitialAmount = initialAmount;
		onAttack.Amount = damage;
		onAttack.DamageFlag = damageFlag;
		triggerTarget.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onAttack);
	}

	private void OnBaseAttack(BattleCreature attacker, BattleCreature[] targetCreatures, OperationInfo operationInfo, long initialAmount, long damage, DamageFlag damageFlag)
	{
		if (operationInfo != null && operationInfo.IsBaseAttack)
		{
			OnBaseAttack onBaseAttack = SnapshotObjectPool<NShared.OnBaseAttack>.Alloc();
			onBaseAttack.TriggerCreature = attacker;
			onBaseAttack.TargetCreatures = targetCreatures;
			onBaseAttack.SkillIndex = operationInfo.SkillIndex;
			onBaseAttack.InitialAmount = initialAmount;
			onBaseAttack.Amount = damage;
			onBaseAttack.DamageFlag = damageFlag;
			attacker.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onBaseAttack);
		}
	}

	private void OnCritical(BattleCreature attacker, BattleCreature[] targetCreatures, int skillIndex, long initialAmount, long damage, DamageFlag damageFlag)
	{
		if (skillIndex > 0 && (damageFlag & DamageFlag.Critical) != DamageFlag.None)
		{
			OnCriticalSkill onCriticalSkill = SnapshotObjectPool<OnCriticalSkill>.Alloc();
			onCriticalSkill.TriggerCreature = attacker;
			onCriticalSkill.TargetCreatures = targetCreatures;
			onCriticalSkill.SkillIndex = skillIndex;
			onCriticalSkill.InitialAmount = initialAmount;
			onCriticalSkill.Amount = damage;
			onCriticalSkill.DamageFlag = damageFlag;
			attacker.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onCriticalSkill);
		}
	}

	private void OnFriendDamage_In_DefenderTeam(BattleCreature attacker, BattleCreature[] targetCreatures, int skillIndex, long initialAmount, long damage, DamageFlag damageFlag)
	{
		BattleCreatureList aliveCreaturesOfTeam = Owner.BattleInstance.CreatureContainer.GetAliveCreaturesOfTeam(Owner.TeamController.TeamId, BattleCreatureList.AllocWithCreature(Owner));
		int num = 0;
		while (aliveCreaturesOfTeam != null && num < aliveCreaturesOfTeam.Count)
		{
			OnFriendDamage onFriendDamage = SnapshotObjectPool<OnFriendDamage>.Alloc();
			onFriendDamage.TriggerCreature = attacker;
			onFriendDamage.TargetCreatures = targetCreatures;
			onFriendDamage.SkillIndex = skillIndex;
			onFriendDamage.InitialAmount = initialAmount;
			onFriendDamage.Amount = damage;
			onFriendDamage.DamageFlag = damageFlag;
			aliveCreaturesOfTeam[num].SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onFriendDamage);
			num++;
		}
	}

	private void OnFriendAttack_In_AttackerTeam(BattleCreature attacker, BattleCreature[] targetCreatures, int skillIndex, long initialAmount, long damage, DamageFlag damageFlag)
	{
		if (attacker != null)
		{
			BattleCreatureList aliveCreaturesOfTeam = Owner.BattleInstance.CreatureContainer.GetAliveCreaturesOfTeam(attacker.TeamController.TeamId, BattleCreatureList.AllocWithCreature(attacker));
			int num = 0;
			while (aliveCreaturesOfTeam != null && num < aliveCreaturesOfTeam.Count)
			{
				OnFriendAttack onFriendAttack = SnapshotObjectPool<OnFriendAttack>.Alloc();
				onFriendAttack.TriggerCreature = attacker;
				onFriendAttack.TargetCreatures = targetCreatures;
				onFriendAttack.SkillIndex = skillIndex;
				onFriendAttack.InitialAmount = initialAmount;
				onFriendAttack.Amount = damage;
				onFriendAttack.DamageFlag = damageFlag;
				aliveCreaturesOfTeam[num].SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onFriendAttack);
				num++;
			}
		}
	}

	private void ModifyDamage(BattleCreature attacker, DamagePipelinePosition pipelinePosition, ref long amount, ref DamageFlag damageFlag)
	{
		DamageFlag copiedDamageFlag = damageFlag;
		long copiedAmount = amount;
		ForEachDamageModifier(GetDamageModifierList(pipelinePosition), delegate(IDamageModifier modifier)
		{
			modifier.Modify(attacker, ref copiedAmount, ref copiedDamageFlag);
		});
		damageFlag = copiedDamageFlag;
		amount = copiedAmount;
	}

	public static long CalculateDamage_Season1(long attack, long defence, int attackerLevel, int damagedLevel)
	{
		return RestoredPortal.LegacyPunishmentDamage(attack, defence, attackerLevel, damagedLevel);
	}

	public static long CalculateDamage(long attack, long defence, int attackerLevel, int damagedLevel)
	{
		return RestoredPortal.LegacyPunishmentDamage(attack, defence, attackerLevel, damagedLevel);
	}

	public HealResultType GiveHeal(long amount, BattleCreature healer, long buffUid, int operationNo, int skillId, int stackCount, ref HealFlag healFlag, int overHealShieldRatio, int overHealShieldMinHpRatio)
	{
		if (!Owner.BattleInstance.IsBattleEnabled())
		{
			return HealResultType.BattleClosing;
		}
		if (IsDead())
		{
			return HealResultType.AlreadyDied;
		}
		bool flag = (healFlag & HealFlag.Supplemently) == 0;
		long oldHp = _hpHolder.Get();
		int hpRatioM = HpRatioM;
		long baseStat = amount;
		if (flag)
		{
			if (healer != null)
			{
				int num = (int)healer.StatController.GetFinalStat(StatType.HealPower);
				baseStat = MultAndAddStat(baseStat, num, 0L);
			}
			bool flag2 = (healFlag & HealFlag.Critical) != 0;
			bool flag3 = (healFlag & HealFlag.NoCritical) != 0;
			if (healer != null && !flag2 && !flag3)
			{
				flag2 = TestChanceRateResult(healer, StatType.MagicalCriticalChance, "GiveHeal.critical");
			}
			if (flag2 && healer != null)
			{
				int multedStat = healer.StatController.GetMultedStat(StatType.CriticalHealToShield);
				if (multedStat > 0)
				{
					long shieldHp = MultStat(baseStat, multedStat);
					HpShield hpShield = SnapshotObjectPool<HpShield>.Alloc();
					hpShield.Init(shieldHp, 0, DamageFlag.Physical | DamageFlag.Magical, 0L);
					AddDamageModifier(hpShield);
					healFlag |= HealFlag.CriticalHealToShield;
				}
				else
				{
					int num2 = (int)healer.StatController.GetFinalStat(StatType.MagicalCriticalPower);
					int multedStat2 = System.Math.Max(BattleConstant.MagicalCriticalDamageFactor + num2, 0);
					baseStat = MultStat(baseStat, multedStat2);
					healFlag |= HealFlag.Critical;
				}
			}
		}
		int num3 = (int)GetFinalStat(StatType.RecoverPower);
		baseStat = MultAndAddStat(baseStat, num3, 0L);
		if (healer != null && flag)
		{
			int multedStat3 = GetMultedStat(healer, StatType.SkillPower);
			int addedStat = GetAddedStat(healer, StatType.SkillPower);
			multedStat3 += GetModified(healer, skillId, operationNo, OperationFieldType.SkillPowerR);
			addedStat += GetModified(healer, skillId, operationNo, OperationFieldType.SkillPower);
			baseStat = MultAndAddStat(baseStat, multedStat3, addedStat);
		}
		if (stackCount > 1)
		{
			baseStat *= stackCount;
		}
		long num4 = baseStat;
		long maxHp = GetMaxHp();
		long num5 = System.Math.Max(Hp + num4 - maxHp, 0L);
		long num6 = System.Math.Max(System.Math.Min(Hp + num4, maxHp), 1L);
		int finalPercentage = (int)((maxHp > 0) ? (num6 * 100 / maxHp) : 0);
		SetHp(num6);
		if (num5 > 0 && overHealShieldRatio > 0 && hpRatioM >= overHealShieldMinHpRatio)
		{
			long num7 = num5 * overHealShieldRatio / 1000;
			if (num7 > 0)
			{
				HpShield hpShield2 = SnapshotObjectPool<HpShield>.Alloc();
				hpShield2.Init(num7, 0, DamageFlag.Physical | DamageFlag.Magical, 0L);
				AddDamageModifier(hpShield2);
			}
		}
		if (healer != null)
		{
			Owner.BattleInstance.Snapshot.StatisticsManager.AddHealMeter(healer.Uid, Owner.Uid, num4, skillId);
			int threatMultiplier = GameRoot.Instance.ThreatDataContainer.GetThreatMultiplier(healer.CreatureData.TagType, ThreatType.Heal);
			int threatMultiplier2 = GameRoot.Instance.ThreatDataContainer.GetThreatMultiplier(healer.CreatureData.TagType, ThreatType.OverHeal);
			long val = Owner.StatController.GetFinalStat(StatType.ThreatGain) / 1000;
			val = System.Math.Max(val, 0L);
			long amount2 = num4 * threatMultiplier * val / 1000;
			long amount3 = num4 * threatMultiplier2 * val / 1000;
			healer.ThreatHistoryTracker.Record(amount2);
			healer.ThreatHistoryTracker.Record(amount3);
		}
		AnnounceHpChangedToObserver(oldHp, num6);
		Owner.RaiseBattleEvent(new Heal
		{
			CreatureUid = Owner.Uid,
			OperationUid = buffUid,
			Amount = num4,
			FinalHp = num6,
			FinalPercentage = finalPercentage,
			HealFlag = healFlag
		});
		bool flag4 = num4 == 0L && (healFlag & HealFlag.NoTriggerOnZeroHeal) != 0;
		bool flag5 = (healFlag & HealFlag.DerivedHeal) != 0;
		if (healer != null && flag && !flag4 && !flag5)
		{
			OnHeal onHeal = SnapshotObjectPool<OnHeal>.Alloc();
			onHeal.TriggerCreature = healer;
			onHeal.TargetCreatures = new BattleCreature[1] { Owner };
			onHeal.SkillIndex = skillId;
			onHeal.Amount = num4;
			onHeal.FinalHp = num6;
			onHeal.HealFlag = healFlag;
			onHeal.OverHeal = num5;
			OnHealed onHealed = SnapshotObjectPool<OnHealed>.Alloc();
			onHealed.TriggerCreature = healer;
			onHealed.TargetCreatures = new BattleCreature[1] { Owner };
			onHealed.SkillIndex = skillId;
			onHealed.Amount = num4;
			onHealed.FinalHp = num6;
			onHealed.HealFlag = healFlag;
			onHealed.OverHeal = num5;
			healer.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onHeal);
			Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onHealed);
		}
		return HealResultType.Success;
	}

	public HealResultType GiveHealNoTrigger(long amount, BattleCreature healer, long buffUid, int operationNo, int skillId, int stackCount, ref HealFlag healFlag, int overHealShieldRatio, int overHealShieldMinHpRatio)
	{
		if (!Owner.BattleInstance.IsBattleEnabled())
		{
			return HealResultType.BattleClosing;
		}
		if (IsDead())
		{
			return HealResultType.AlreadyDied;
		}
		long oldHp = _hpHolder.Get();
		int hpRatioM = HpRatioM;
		long maxHp = GetMaxHp();
		long num = System.Math.Max(Hp + amount - maxHp, 0L);
		long num2 = System.Math.Max(System.Math.Min(Hp + amount, maxHp), 1L);
		int finalPercentage = (int)((maxHp > 0) ? (num2 * 100 / maxHp) : 0);
		SetHp(num2);
		if (num > 0 && overHealShieldRatio > 0 && hpRatioM >= overHealShieldMinHpRatio)
		{
			long num3 = num * overHealShieldRatio / 1000;
			if (num3 > 0)
			{
				HpShield hpShield = SnapshotObjectPool<HpShield>.Alloc();
				hpShield.Init(num3, 0, DamageFlag.Physical | DamageFlag.Magical, 0L);
				AddDamageModifier(hpShield);
			}
		}
		if (healer != null)
		{
			Owner.BattleInstance.Snapshot.StatisticsManager.AddHealMeter(healer.Uid, Owner.Uid, amount, skillId);
			int threatMultiplier = GameRoot.Instance.ThreatDataContainer.GetThreatMultiplier(healer.CreatureData.TagType, ThreatType.Heal);
			int threatMultiplier2 = GameRoot.Instance.ThreatDataContainer.GetThreatMultiplier(healer.CreatureData.TagType, ThreatType.OverHeal);
			long val = Owner.StatController.GetFinalStat(StatType.ThreatGain) / 1000;
			val = System.Math.Max(val, 0L);
			long amount2 = amount * threatMultiplier * val / 1000;
			long amount3 = amount * threatMultiplier2 * val / 1000;
			healer.ThreatHistoryTracker.Record(amount2);
			healer.ThreatHistoryTracker.Record(amount3);
		}
		AnnounceHpChangedToObserver(oldHp, num2);
		Owner.RaiseBattleEvent(new Heal
		{
			CreatureUid = Owner.Uid,
			OperationUid = buffUid,
			Amount = amount,
			FinalHp = num2,
			FinalPercentage = finalPercentage,
			HealFlag = healFlag
		});
		return HealResultType.Success;
	}

	public void GainThreat(long amount, int skillIndex)
	{
		if (Owner.BattleInstance.IsBattleEnabled() && !IsDead())
		{
			long finalStat = GetFinalStat(StatType.ThreatGain);
			long amount2 = amount * finalStat / 1000;
			Owner.ThreatHistoryTracker.Record(amount2);
		}
	}

	public void RecoverHpByStat(StatType addStatType, StatType multStatType, HealFlag healFlag)
	{
		long num = GetFinalStat(addStatType);
		long finalStat = GetFinalStat(multStatType);
		if (finalStat > 0)
		{
			num += (long)((decimal)GetMaxHp() * ((decimal)finalStat / 1000m));
		}
		if (num > 0)
		{
			GiveHeal(num, Owner, 0L, 0, 0, 1, ref healFlag, 0, 0);
		}
	}

	public void RecoverMpByStat(StatType addStatType, StatType statByMaxType, ChangeMp.ReasonType reasonType, long ratio = 1000L)
	{
		long finalStat = GetFinalStat(addStatType);
		long finalStat2 = Owner.StatController.GetFinalStat(StatType.MpRegenRatioM);
		long num = finalStat * ratio * finalStat2 / 1000000;
		if (statByMaxType != StatType.None)
		{
			long num2 = GetMaxMp() * GetFinalStat(statByMaxType) * ratio * finalStat2 / 1000000000;
			num += num2;
		}
		if (num != 0L)
		{
			AddMp((int)num, 0L, reasonType, Owner, 0L);
		}
	}

	public void InitExtraStats()
	{
		InitExtraStat();
		InitPetStat();
		InitOrdealArenaStats();
		InitAffixStats();
	}

	private void InitExtraStat()
	{
		StatCalculator.ApplyExtraStat(Owner.CreateInfo.StatData, Owner.CreateInfo.ExtraStats);
	}

	private void InitPetStat()
	{
		InitConditionalExtraStats(Owner.CreateInfo.PetStats);
	}

	private void InitOrdealArenaStats()
	{
		InitConditionalExtraStats(Owner.CreateInfo.OrdealArenaStats);
	}

	private void InitAffixStats()
	{
		int uid = Owner.Uid;
		TeamType teamType = Owner.TeamController.TeamType;
		int index = Owner.CreatureData.Index;
		List<IConditionalExtraStatData> statAffixDataBase = Owner.BattleInstance.AffixContainer.GetStatAffixDataBase(teamType, index, uid);
		InitConditionalExtraStats(statAffixDataBase);
	}

	private void InitConditionalExtraStats(IList<IConditionalExtraStatData> extraStatDatas)
	{
		if (extraStatDatas == null || extraStatDatas.Count == 0)
		{
			return;
		}
		List<IConditionalExtraStatData> list = null;
		for (int i = 0; i < extraStatDatas.Count; i++)
		{
			IConditionalExtraStatData conditionalExtraStatData = extraStatDatas[i];
			if (conditionalExtraStatData != null && conditionalExtraStatData.OptionType != OperationType.UseSkill && conditionalExtraStatData.CheckActiveCondition(Owner.BattleInstance.DungeonData))
			{
				if (list == null)
				{
					list = new List<IConditionalExtraStatData>();
				}
				list.Add(conditionalExtraStatData);
			}
		}
		if (list != null && list.Count != 0)
		{
			StatCalculator.ApplyExtraStat(Owner.CreateInfo.StatData, list);
		}
	}
}
