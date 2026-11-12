using System;
using System.Collections.Generic;
using NShared.NOperationRuntime;
using NVespa;

namespace NShared;

public class OperationRuntimeFactory
{
	public const int InitialPoolCapacity = 10;

	private readonly Dictionary<OperationType, Func<IOperationRuntime>> _creatorDictionary = new Dictionary<OperationType, Func<IOperationRuntime>>();

	private readonly Dictionary<Type, OperationType> _operationTypeMap = new Dictionary<Type, OperationType>();

	public OperationRuntimeFactory()
	{
		RegisterCreator<GetDamage>(OperationType.GetDamage);
		RegisterCreator<GetDamageR>(OperationType.GetDamageR);
		RegisterCreator<GetDamageByMaxHpR>(OperationType.GetDamageByMaxHpR);
		RegisterCreator<GetStateDamageR>(OperationType.GetStateDamageR);
		RegisterCreator<GetCriticalDamageR>(OperationType.GetCriticalDamageR);
		RegisterCreator<GetPhysicalDamage>(OperationType.GetPhysicalDamage);
		RegisterCreator<GetPhysicalDamageR>(OperationType.GetPhysicalDamageR);
		RegisterCreator<GetPhysicalDamageByMaxHpR>(OperationType.GetPhysicalDamageByMaxHpR);
		RegisterCreator<GetMagicalDamage>(OperationType.GetMagicalDamage);
		RegisterCreator<GetMagicalDamageR>(OperationType.GetMagicalDamageR);
		RegisterCreator<GetMagicalDamageByMaxHpR>(OperationType.GetMagicalDamageByMaxHpR);
		RegisterCreator<GetDot>(OperationType.GetDot);
		RegisterCreator<GetDotR>(OperationType.GetDotR);
		RegisterCreator<GetPhysicalDot>(OperationType.GetPhysicalDot);
		RegisterCreator<GetPhysicalDotR>(OperationType.GetPhysicalDotR);
		RegisterCreator<GetPhysicalDotByMaxHpR>(OperationType.GetPhysicalDotByMaxHpR);
		RegisterCreator<GetMagicalDot>(OperationType.GetMagicalDot);
		RegisterCreator<GetMagicalDotR>(OperationType.GetMagicalDotR);
		RegisterCreator<GetMagicalDotByMaxHpR>(OperationType.GetMagicalDotByMaxHpR);
		RegisterCreator<GetHeal>(OperationType.GetHeal);
		RegisterCreator<GetHealR>(OperationType.GetHealR);
		RegisterCreator<GetHot>(OperationType.GetHot);
		RegisterCreator<GetHotR>(OperationType.GetHotR);
		RegisterCreator<GetHealByMaxHpR>(OperationType.GetHealByMaxHpR);
		RegisterCreator<GetHotByMaxHpR>(OperationType.GetHotByMaxHpR);
		RegisterCreator<GetMana>(OperationType.GetMana);
		RegisterCreator<GetManaR>(OperationType.GetManaR);
		RegisterCreator<GetMot>(OperationType.GetMot);
		RegisterCreator<GetMotR>(OperationType.GetMotR);
		RegisterCreator<ModMaxHp>(OperationType.ModMaxHp);
		RegisterCreator<ModMaxHpR>(OperationType.ModMaxHpR);
		RegisterCreator<ModMaxMp>(OperationType.ModMaxMp);
		RegisterCreator<ModMaxMpR>(OperationType.ModMaxMpR);
		RegisterCreator<ModDefence>(OperationType.ModDefence);
		RegisterCreator<ModDefenceR>(OperationType.ModDefenceR);
		RegisterCreator<ModMaxHpDefence>(OperationType.ModMaxHpDefence);
		RegisterCreator<ModMaxHpDefenceR>(OperationType.ModMaxHpDefenceR);
		RegisterCreator<ModPhysicalDefence>(OperationType.ModPhysicalDefence);
		RegisterCreator<ModPhysicalDefenceR>(OperationType.ModPhysicalDefenceR);
		RegisterCreator<ModMagicalDefence>(OperationType.ModMagicalDefence);
		RegisterCreator<ModMagicalDefenceR>(OperationType.ModMagicalDefenceR);
		RegisterCreator<ModPower>(OperationType.ModPower);
		RegisterCreator<ModPowerR>(OperationType.ModPowerR);
		RegisterCreator<ModPhysicalPower>(OperationType.ModPhysicalPower);
		RegisterCreator<ModPhysicalPowerR>(OperationType.ModPhysicalPowerR);
		RegisterCreator<ModMagicalPower>(OperationType.ModMagicalPower);
		RegisterCreator<ModMagicalPowerR>(OperationType.ModMagicalPowerR);
		RegisterCreator<ModHealPower>(OperationType.ModHealPower);
		RegisterCreator<ModRecoverPower>(OperationType.ModRecoverPower);
		RegisterCreator<ModBlockChance>(OperationType.ModBlockChance);
		RegisterCreator<ModPhysicalBlockChance>(OperationType.ModPhysicalBlockChance);
		RegisterCreator<ModMagicalBlockChance>(OperationType.ModMagicalBlockChance);
		RegisterCreator<ModBlockPower>(OperationType.ModBlockPower);
		RegisterCreator<ModPhysicalBlockPower>(OperationType.ModPhysicalBlockPower);
		RegisterCreator<ModMagicalBlockPower>(OperationType.ModMagicalBlockPower);
		RegisterCreator<ModHitChance>(OperationType.ModHitChance);
		RegisterCreator<ModPhysicalHitChance>(OperationType.ModPhysicalHitChance);
		RegisterCreator<ModMagicalHitChance>(OperationType.ModMagicalHitChance);
		RegisterCreator<ModAllSkillPower>(OperationType.ModAllSkillPower);
		RegisterCreator<ModAllSkillPowerR>(OperationType.ModAllSkillPowerR);
		RegisterCreator<ModComboPowerR>(OperationType.ModComboPowerR);
		RegisterCreator<ModPiercePower>(OperationType.ModPiercePower);
		RegisterCreator<ModPhysicalPiercePower>(OperationType.ModPhysicalPiercePower);
		RegisterCreator<ModMagicalPiercePower>(OperationType.ModMagicalPiercePower);
		RegisterCreator<ModAntiDebuffChance>(OperationType.ModAntiDebuffChance);
		RegisterCreator<ModAntiCcChance>(OperationType.ModAntiCcChance);
		RegisterCreator<ModHitCcChance>(OperationType.ModHitCcChance);
		RegisterCreator<ModHitDebuffChance>(OperationType.ModHitDebuffChance);
		RegisterCreator<ModCriticalPower>(OperationType.ModCriticalPower);
		RegisterCreator<ModCriticalChance>(OperationType.ModCriticalChance);
		RegisterCreator<ModCriticalDodgeChance>(OperationType.ModCriticalDodgeChance);
		RegisterCreator<ModCriticalWeakness>(OperationType.ModCriticalWeakness);
		RegisterCreator<ModPhysicalCriticalPower>(OperationType.ModPhysicalCriticalPower);
		RegisterCreator<ModPhysicalCriticalChance>(OperationType.ModPhysicalCriticalChance);
		RegisterCreator<ModPhysicalCriticalWeakness>(OperationType.ModPhysicalCriticalWeakness);
		RegisterCreator<ModPhysicalCriticalDodgeChance>(OperationType.ModPhysicalCriticalDodgeChance);
		RegisterCreator<ModMagicalCriticalPower>(OperationType.ModMagicalCriticalPower);
		RegisterCreator<ModMagicalCriticalChance>(OperationType.ModMagicalCriticalChance);
		RegisterCreator<ModMagicalCriticalWeakness>(OperationType.ModMagicalCriticalWeakness);
		RegisterCreator<ModMagicalCriticalDodgeChance>(OperationType.ModMagicalCriticalDodgeChance);
		RegisterCreator<ModDodgeChance>(OperationType.ModDodgeChance);
		RegisterCreator<ModPhysicalDodgeChance>(OperationType.ModPhysicalDodgeChance);
		RegisterCreator<ModMagicalDodgeChance>(OperationType.ModMagicalDodgeChance);
		RegisterCreator<ModToughness>(OperationType.ModToughness);
		RegisterCreator<ModPhysicalToughness>(OperationType.ModPhysicalToughness);
		RegisterCreator<ModMagicalToughness>(OperationType.ModMagicalToughness);
		RegisterCreator<ModWeakness>(OperationType.ModWeakness);
		RegisterCreator<ModPhysicalWeakness>(OperationType.ModPhysicalWeakness);
		RegisterCreator<ModMagicalWeakness>(OperationType.ModMagicalWeakness);
		RegisterCreator<ModDebuffTime>(OperationType.ModDebuffTime);
		RegisterCreator<ModDebuffTimeR>(OperationType.ModDebuffTimeR);
		RegisterCreator<ModCcTime>(OperationType.ModCcTime);
		RegisterCreator<ModCcTimeR>(OperationType.ModCcTimeR);
		RegisterCreator<ModCooltime>(OperationType.ModCooltime);
		RegisterCreator<ModCooltimeR>(OperationType.ModCooltimeR);
		RegisterCreator<ModAttackSpeed>(OperationType.ModAttackSpeed);
		RegisterCreator<ModAttackSpeedR>(OperationType.ModAttackSpeedR);
		RegisterCreator<ModHpStealPower>(OperationType.ModHpStealPower);
		RegisterCreator<ModHpOnDamage>(OperationType.ModHpOnDamage);
		RegisterCreator<ModHpOnAttack>(OperationType.ModHpOnAttack);
		RegisterCreator<ModHpOnKill>(OperationType.ModHpOnKill);
		RegisterCreator<ModHpOnWave>(OperationType.ModHpOnWave);
		RegisterCreator<ModHpRatioOnDamage>(OperationType.ModHpRatioOnDamage);
		RegisterCreator<ModHpRatioOnAttack>(OperationType.ModHpRatioOnAttack);
		RegisterCreator<ModHpRatioOnKill>(OperationType.ModHpRatioOnKill);
		RegisterCreator<ModHpRatioOnWave>(OperationType.ModHpRatioOnWave);
		RegisterCreator<ModMpOnAttack>(OperationType.ModMpOnAttack);
		RegisterCreator<ModMpOnAttackR>(OperationType.ModMpOnAttackR);
		RegisterCreator<ModMpOnDamage>(OperationType.ModMpOnDamage);
		RegisterCreator<ModMpOnDamageR>(OperationType.ModMpOnDamageR);
		RegisterCreator<ModMpOnKill>(OperationType.ModMpOnKill);
		RegisterCreator<ModMpOnKillR>(OperationType.ModMpOnKillR);
		RegisterCreator<ModMpOnTime>(OperationType.ModMpOnTime);
		RegisterCreator<ModMpOnTimeR>(OperationType.ModMpOnTimeR);
		RegisterCreator<ModMpOnWave>(OperationType.ModMpOnWave);
		RegisterCreator<ModMpOnWaveR>(OperationType.ModMpOnWaveR);
		RegisterCreator<ModMpRegenRatioM>(OperationType.ModMpRegenRatioM);
		RegisterCreator<ModMpRegenRatioMR>(OperationType.ModMpRegenRatioMR);
		RegisterCreator<ModStatByHp>(OperationType.ModStatByHp);
		RegisterCreator<ModStatByHpR>(OperationType.ModStatByHpR);
		RegisterCreator<ModAttackPower>(OperationType.ModAttackPower);
		RegisterCreator<ModDefensePower>(OperationType.ModDefensePower);
		RegisterCreator<ModPhysicalAttackPower>(OperationType.ModPhysicalAttackPower);
		RegisterCreator<ModMagicalAttackPower>(OperationType.ModMagicalAttackPower);
		RegisterCreator<ModSkillLevelAllSlot>(OperationType.ModSkillLevelAllSlot);
		RegisterCreator<ModSkillLevelSlot1>(OperationType.ModSkillLevelSlot1);
		RegisterCreator<ModSkillLevelSlot2>(OperationType.ModSkillLevelSlot2);
		RegisterCreator<ModSkillLevelSlot3>(OperationType.ModSkillLevelSlot3);
		RegisterCreator<ModSkillLevelSlot4>(OperationType.ModSkillLevelSlot4);
		RegisterCreator<ModSoulWeaponMaxUseCount>(OperationType.ModSoulWeaponMaxUseCount);
		RegisterCreator<ModShieldPowerR>(OperationType.ModShieldPowerR);
		RegisterCreator<ModDotPower>(OperationType.ModDotPower);
		RegisterCreator<ModPhysicalDotPower>(OperationType.ModPhysicalDotPower);
		RegisterCreator<ModMagicalDotPower>(OperationType.ModMagicalDotPower);
		RegisterCreator<ModIgnoreBlockPower>(OperationType.ModIgnoreBlockPower);
		RegisterCreator<ModPhysicalIgnoreBlockPower>(OperationType.ModPhysicalIgnoreBlockPower);
		RegisterCreator<ModMagicalIgnoreBlockPower>(OperationType.ModMagicalIgnoreBlockPower);
		RegisterCreator<ModIgnoreDefencePower>(OperationType.ModIgnoreDefencePower);
		RegisterCreator<ModPhysicalIgnoreDefencePower>(OperationType.ModPhysicalIgnoreDefencePower);
		RegisterCreator<ModMagicalIgnoreDefencePower>(OperationType.ModMagicalIgnoreDefencePower);
		RegisterCreator<ModMaxHpRByStageLevel>(OperationType.ModMaxHpRByStageLevel);
		RegisterCreator<ModPowerRByStageLevel>(OperationType.ModPowerRByStageLevel);
		RegisterCreator<ModDefenceRByStageLevel>(OperationType.ModDefenceRByStageLevel);
		RegisterCreator<SetState>(OperationType.SetState);
		RegisterCreator<ImmuneCc>(OperationType.ImmuneCc);
		RegisterCreator<ImmuneState>(OperationType.ImmuneState);
		RegisterCreator<Immortal>(OperationType.Immortal);
		RegisterCreator<Knockback>(OperationType.Knockback);
		RegisterCreator<Pulling>(OperationType.Pulling);
		RegisterCreator<Stun>(OperationType.Stun);
		RegisterCreator<Down>(OperationType.Down);
		RegisterCreator<Shackle>(OperationType.Shackle);
		RegisterCreator<Silence>(OperationType.Silence);
		RegisterCreator<Burn>(OperationType.Burn);
		RegisterCreator<Blind>(OperationType.Blind);
		RegisterCreator<Banish>(OperationType.Banish);
		RegisterCreator<Frozen>(OperationType.Frozen);
		RegisterCreator<Electronic>(OperationType.Electronic);
		RegisterCreator<Stone>(OperationType.Stone);
		RegisterCreator<Untargetable>(OperationType.Untargetable);
		RegisterCreator<ModStateTime>(OperationType.ModStateTime);
		RegisterCreator<ModStateTimeR>(OperationType.ModStateTimeR);
		RegisterCreator<ModStateCount>(OperationType.ModStateCount);
		RegisterCreator<DashToTarget>(OperationType.DashToTarget);
		RegisterCreator<DashFromMyself>(OperationType.DashFromMyself);
		RegisterCreator<GetChainHealR>(OperationType.GetChainHealR);
		RegisterCreator<ModCriticalHealToShieldR>(OperationType.ModCriticalHealToShieldR);
		RegisterCreator<Cleanse>(OperationType.Cleanse);
		RegisterCreator<Dispel>(OperationType.Dispel);
		RegisterCreator<ClearSkillOperation>(OperationType.ClearSkillOperation);
		RegisterCreator<ClearState>(OperationType.ClearState);
		RegisterCreator<CreateShieldR>(OperationType.CreateShieldR);
		RegisterCreator<CreateProjectile>(OperationType.CreateProjectile);
		RegisterCreator<EscapeDeath>(OperationType.EscapeDeath);
		RegisterCreator<ModAttackAmount>(OperationType.ModAttackAmount);
		RegisterCreator<ModAttackFlag>(OperationType.ModAttackFlag);
		RegisterCreator<ModAttackByBoss>(OperationType.ModAttackByBoss);
		RegisterCreator<ModAttackByRace>(OperationType.ModAttackByRace);
		RegisterCreator<ModDamage>(OperationType.ModDamage);
		RegisterCreator<ModTriggerStat>(OperationType.ModTriggerStat);
		RegisterCreator<DodgeDamage>(OperationType.DodgeDamage);
		RegisterCreator<FlyOperation>(OperationType.FlyOperation);
		RegisterCreator<GetHealByDamageR>(OperationType.GetHealByDamageR);
		RegisterCreator<GetHotByDamageR>(OperationType.GetHotByDamageR);
		RegisterCreator<NShared.NOperationRuntime.SpawnCreature>(OperationType.SpawnCreature);
		RegisterCreator<ModDamageByState>(OperationType.ModDamageByState);
		RegisterCreator<ModNextCooltime>(OperationType.ModNextCooltime);
		RegisterCreator<ModNextCooltimeR>(OperationType.ModNextCooltimeR);
		RegisterCreator<ModNextActiveCooltime>(OperationType.ModNextActiveCooltime);
		RegisterCreator<ModNextActiveCooltimeR>(OperationType.ModNextActiveCooltimeR);
		RegisterCreator<ModActiveCooltime>(OperationType.ModActiveCooltime);
		RegisterCreator<ModActiveCooltimeR>(OperationType.ModActiveCooltimeR);
		RegisterCreator<ModNextBaseAttackR>(OperationType.ModNextBaseAttackR);
		RegisterCreator<ModSkillValue>(OperationType.ModSkillValue);
		RegisterCreator<ModSkillEnabled>(OperationType.ModSkillEnabled);
		RegisterCreator<ModSkillDisabled>(OperationType.ModSkillDisabled);
		RegisterCreator<ModSkillPower>(OperationType.ModSkillPower);
		RegisterCreator<ModSkillPowerR>(OperationType.ModSkillPowerR);
		RegisterCreator<ModOperation>(OperationType.ModOperation);
		RegisterCreator<ModSetRadiusMm>(OperationType.ModSetRadiusMm);
		RegisterCreator<ModSkillInt>(OperationType.ModSkillInt);
		RegisterCreator<ModSkill>(OperationType.ModSkill);
		RegisterCreator<SetSkillValue>(OperationType.SetSkillValue);
		RegisterCreator<SetSkillPower>(OperationType.SetSkillPower);
		RegisterCreator<SetSkillPowerR>(OperationType.SetSkillPowerR);
		RegisterCreator<StartCooltime>(OperationType.StartCooltime);
		RegisterCreator<SetSkillInt>(OperationType.SetSkillInt);
		RegisterCreator<SetSkill>(OperationType.SetSkill);
		RegisterCreator<UseSkill>(OperationType.UseSkill);
		RegisterCreator<UseSkillOnTick>(OperationType.UseSkillOnTick);
		RegisterCreator<UsePassiveSkill>(OperationType.UsePassiveSkill);
		RegisterCreator<RegisterPassiveSkill>(OperationType.RegisterPassiveSkill);
		RegisterCreator<RemovePassiveSkill>(OperationType.RemovePassiveSkill);
		RegisterCreator<UseSommoneeSkill>(OperationType.UseSommoneeSkill);
		RegisterCreator<MoveRandom>(OperationType.MoveRandom);
		RegisterCreator<PullTarget>(OperationType.PullTarget);
		RegisterCreator<ReflectDamage>(OperationType.ReflectDamage);
		RegisterCreator<ModMySkill>(OperationType.ModMySkill);
		RegisterCreator<SetMySkill>(OperationType.SetMySkill);
		RegisterCreator<EnablePassiveSkill>(OperationType.EnablePassiveSkill);
		RegisterCreator<BeginResting>(OperationType.BeginResting);
		RegisterCreator<IgnoreDamageByMaxHpR>(OperationType.IgnoreDamageByMaxHpR);
		RegisterCreator<IgnoreDamageUnderHpR>(OperationType.IgnoreDamageUnderHpR);
		RegisterCreator<IgnoreFilterOnCheckOnly>(OperationType.IgnoreFilterOnCheckOnly);
		RegisterCreator<RaiseEventTrigger>(OperationType.RaiseEventTrigger);
		RegisterCreator<SetDirection>(OperationType.SetDirection);
		RegisterCreator<OverrideStat>(OperationType.OverrideStat);
		RegisterCreator<OverrideStatFirst>(OperationType.OverrideStatFirst);
		RegisterCreator<SetDamage>(OperationType.SetDamage);
		RegisterCreator<PropagateDamage>(OperationType.PropagateDamage);
		RegisterCreator<ChangeStateValue>(OperationType.ChangeStateValue);
		RegisterCreator<SubSkill>(OperationType.SubSkill);
		RegisterCreator<Allure>(OperationType.Allure);
		RegisterCreator<SetTriggerTimer>(OperationType.SetTriggerTimer);
		RegisterCreator<FilterTargetByState>(OperationType.FilterTargetByState);
		RegisterCreator<ForceAutoMode>(OperationType.ForceAutoMode);
		RegisterCreator<ChangeSkillByState>(OperationType.ChangeSkillByState);
		RegisterCreator<SetCreatureData>(OperationType.SetCreatureData);
		RegisterCreator<ModCreatureData>(OperationType.ModCreatureData);
		RegisterCreator<SuppressSkillCheck>(OperationType.SuppressSkillCheck);
		RegisterCreator<DisableSkillUse>(OperationType.DisableSkillUse);
		RegisterCreator<Die>(OperationType.Die);
		RegisterCreator<DisableSkillUntilStateCount>(OperationType.DisableSkillUntilStateCount);
		RegisterCreator<DisableSkillSlotToTarget>(OperationType.DisableSkillSlotToTarget);
		RegisterCreator<GetDamageNoTrigger>(OperationType.GetDamageNoTrigger);
		RegisterCreator<GetDamageByDamageR>(OperationType.GetDamageByDamageR);
		RegisterCreator<GetDamageByStackR>(OperationType.GetDamageByStackR);
		RegisterCreator<AddStateStackByDamageR>(OperationType.AddStateStackByDamageR);
		RegisterCreator<AddStateStackByHealR>(OperationType.AddStateStackByHealR);
		RegisterCreator<CreateShieldByDamageR>(OperationType.CreateShieldByDamageR);
		RegisterCreator<GetHealByInitialAmount>(OperationType.GetHealByInitialAmount);
		RegisterCreator<ModThreatGain>(OperationType.ModThreatGain);
		RegisterCreator<ModThreatGainR>(OperationType.ModThreatGainR);
		RegisterCreator<GetThreat>(OperationType.GetThreat);
		RegisterCreator<Provoke>(OperationType.Provoke);
		RegisterCreator<UseSkillByDamageStackFixed>(OperationType.UseSkillByDamageStackFixed);
		RegisterCreator<OverrideCalcMaxHpR>(OperationType.OverrideCalcMaxHpR);
	}

	private void RegisterCreator<T>(OperationType operationType) where T : class, IOperationRuntime, new()
	{
		if (_creatorDictionary.ContainsKey(operationType))
		{
			VespaDebug.LogError("Already registered operation runtime : " + operationType);
			return;
		}
		_ = operationType.ToString() != typeof(T).Name;
		_creatorDictionary.Add(operationType, () => SnapshotObjectPool<T>.Alloc());
		_operationTypeMap.Add(typeof(T), operationType);
	}

	private void TestUnregisteredOperationRuntime()
	{
		foreach (OperationType value2 in Enum.GetValues(typeof(OperationType)))
		{
			if (value2 != OperationType.None)
			{
				_creatorDictionary.TryGetValue(value2, out var value);
				if (value == null)
				{
					VespaDebug.LogError("Unregistered operation runtime : " + value2);
					continue;
				}
				string name = value().GetType().Name;
				string.Equals(value2.ToString(), name);
			}
		}
	}

	public IOperationRuntime Create(OperationType operationType)
	{
		Func<IOperationRuntime> value = null;
		_creatorDictionary.TryGetValue(operationType, out value);
		return value?.Invoke();
	}

	public IOperationRuntime CreateForStatCalculation(OperationType operationType, int optionValue)
	{
		if (!(Create(operationType) is SetStatBase setStatBase))
		{
			return null;
		}
		OperationInfo operationInfo = new OperationInfo();
		operationInfo.FiredInfo = new FiredOperationInfo();
		operationInfo.FiredInfo.OperationData = OperationData.CreateDummy(new string[1] { optionValue.ToString() });
		operationInfo.FiredInfo.Init();
		operationInfo.Init();
		((IOperationRuntime)setStatBase).Init(operationInfo);
		((IOperationRuntime)setStatBase).OnBegin();
		return setStatBase;
	}
}
