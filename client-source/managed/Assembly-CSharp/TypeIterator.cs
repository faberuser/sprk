using NShared;
using NShared.NAi.NTactic;
using NShared.NOperationRuntime;
using NShared.NUtil;

public static class TypeIterator
{
	public static void InitSnapshotObjectFactory()
	{
		SnapshotFactory.Register<AiContext>();
		SnapshotFactory.Register<AiController>();
		SnapshotFactory.Register<BattleCreature>();
		SnapshotFactory.Register<BattleCreatureData>();
		SnapshotFactory.Register<BattleCreatureRestingInfo>();
		SnapshotFactory.Register<BattleInstanceSnapshot>();
		SnapshotFactory.Register<BattleStatistics>();
		SnapshotFactory.Register<BattleStatisticsManager>();
		SnapshotFactory.Register<BlackBoard>();
		SnapshotFactory.Register<BlackBoardValueInfo>();
		SnapshotFactory.Register<BossCcGauge>();
		SnapshotFactory.Register<CastingSkillInfo>();
		SnapshotFactory.Register<CooltimeManager>();
		SnapshotFactory.Register<CooltimeTimer>();
		SnapshotFactory.Register<DelayedOperationInfo>();
		SnapshotFactory.Register<DelegateStateModifier>();
		SnapshotFactory.Register<DungeonSkill>();
		SnapshotFactory.Register<DungeonSkillController>();
		SnapshotFactory.Register<FiredOperationInfo>();
		SnapshotFactory.Register<HpShield>();
		SnapshotFactory.Register<ModCooltimeDurationInfo>();
		SnapshotFactory.Register<ModCooltimeInfoBase>();
		SnapshotFactory.Register<ModCooltimeRatioInfo>();
		SnapshotFactory.Register<MoveHandle>();
		SnapshotFactory.Register<WaitRandomSkillTactic>();
		SnapshotFactory.Register<AddStateStackByDamageR>();
		SnapshotFactory.Register<AddStateStackByHealR>();
		SnapshotFactory.Register<Allure>();
		SnapshotFactory.Register<Banish>();
		SnapshotFactory.Register<BeginResting>();
		SnapshotFactory.Register<Blind>();
		SnapshotFactory.Register<Burn>();
		SnapshotFactory.Register<ChangeSkillByState>();
		SnapshotFactory.Register<ChangeStateValue>();
		SnapshotFactory.Register<Cleanse>();
		SnapshotFactory.Register<ClearSkillOperation>();
		SnapshotFactory.Register<ClearState>();
		SnapshotFactory.Register<CreateProjectile>();
		SnapshotFactory.Register<CreateShieldByDamageR>();
		SnapshotFactory.Register<CreateShieldR>();
		SnapshotFactory.Register<DashFromMyself>();
		SnapshotFactory.Register<DashToTarget>();
		SnapshotFactory.Register<DelegateFilterTargetByState>();
		SnapshotFactory.Register<Die>();
		SnapshotFactory.Register<DisableSkillSlotToTarget>();
		SnapshotFactory.Register<DisableSkillUntilStateCount>();
		SnapshotFactory.Register<DisableSkillUse>();
		SnapshotFactory.Register<Dispel>();
		SnapshotFactory.Register<DodgeDamage>();
		SnapshotFactory.Register<Down>();
		SnapshotFactory.Register<Electronic>();
		SnapshotFactory.Register<EnablePassiveSkill>();
		SnapshotFactory.Register<EscapeDeath>();
		SnapshotFactory.Register<FilterTargetByState>();
		SnapshotFactory.Register<FlyOperation>();
		SnapshotFactory.Register<ForceAutoMode>();
		SnapshotFactory.Register<Frozen>();
		SnapshotFactory.Register<GetChainHealR>();
		SnapshotFactory.Register<GetCriticalDamageR>();
		SnapshotFactory.Register<GetDamage>();
		SnapshotFactory.Register<GetDamageByDamageR>();
		SnapshotFactory.Register<GetDamageByHealR>();
		SnapshotFactory.Register<GetDamageByMaxHpR>();
		SnapshotFactory.Register<GetDamageByStackR>();
		SnapshotFactory.Register<GetDamageNoTrigger>();
		SnapshotFactory.Register<GetDamageR>();
		SnapshotFactory.Register<GetDot>();
		SnapshotFactory.Register<GetDotR>();
		SnapshotFactory.Register<GetHeal>();
		SnapshotFactory.Register<GetHealByDamageR>();
		SnapshotFactory.Register<GetHealByInitialAmount>();
		SnapshotFactory.Register<GetHealByMaxHpR>();
		SnapshotFactory.Register<GetHealNoTrigger>();
		SnapshotFactory.Register<GetHealR>();
		SnapshotFactory.Register<GetHot>();
		SnapshotFactory.Register<GetHotByDamageR>();
		SnapshotFactory.Register<GetHotByMaxHpR>();
		SnapshotFactory.Register<GetHotR>();
		SnapshotFactory.Register<GetMagicalDamage>();
		SnapshotFactory.Register<GetMagicalDamageByMaxHpR>();
		SnapshotFactory.Register<GetMagicalDamageR>();
		SnapshotFactory.Register<GetMagicalDot>();
		SnapshotFactory.Register<GetMagicalDotByMaxHpR>();
		SnapshotFactory.Register<GetMagicalDotR>();
		SnapshotFactory.Register<GetMana>();
		SnapshotFactory.Register<GetManaR>();
		SnapshotFactory.Register<GetMot>();
		SnapshotFactory.Register<GetMotR>();
		SnapshotFactory.Register<GetPhysicalDamage>();
		SnapshotFactory.Register<GetPhysicalDamageByMaxHpR>();
		SnapshotFactory.Register<GetPhysicalDamageR>();
		SnapshotFactory.Register<GetPhysicalDot>();
		SnapshotFactory.Register<GetPhysicalDotByMaxHpR>();
		SnapshotFactory.Register<GetPhysicalDotR>();
		SnapshotFactory.Register<GetStateDamageR>();
		SnapshotFactory.Register<GetThreat>();
		SnapshotFactory.Register<IgnoreDamageByMaxHpR>();
		SnapshotFactory.Register<IgnoreDamageUnderHpR>();
		SnapshotFactory.Register<IgnoreFilterOnCheckOnly>();
		SnapshotFactory.Register<Immortal>();
		SnapshotFactory.Register<ImmuneCc>();
		SnapshotFactory.Register<ImmuneState>();
		SnapshotFactory.Register<Knockback>();
		SnapshotFactory.Register<ModActiveCooltime>();
		SnapshotFactory.Register<ModActiveCooltimeR>();
		SnapshotFactory.Register<ModAllSkillPower>();
		SnapshotFactory.Register<ModAllSkillPowerR>();
		SnapshotFactory.Register<ModAntiCcChance>();
		SnapshotFactory.Register<ModAntiDebuffChance>();
		SnapshotFactory.Register<ModAttackAmount>();
		SnapshotFactory.Register<ModAttackByBoss>();
		SnapshotFactory.Register<ModAttackByRace>();
		SnapshotFactory.Register<ModAttackFlag>();
		SnapshotFactory.Register<ModAttackPower>();
		SnapshotFactory.Register<ModAttackSpeed>();
		SnapshotFactory.Register<ModAttackSpeedR>();
		SnapshotFactory.Register<ModBlockChance>();
		SnapshotFactory.Register<ModBlockPower>();
		SnapshotFactory.Register<ModCcTime>();
		SnapshotFactory.Register<ModCcTimeR>();
		SnapshotFactory.Register<ModComboPowerR>();
		SnapshotFactory.Register<ModCooltime>();
		SnapshotFactory.Register<ModCooltimeR>();
		SnapshotFactory.Register<ModCreatureData>();
		SnapshotFactory.Register<ModCriticalChance>();
		SnapshotFactory.Register<ModCriticalDodgeChance>();
		SnapshotFactory.Register<ModCriticalHealToShieldR>();
		SnapshotFactory.Register<ModCriticalPower>();
		SnapshotFactory.Register<ModCriticalWeakness>();
		SnapshotFactory.Register<ModDamage>();
		SnapshotFactory.Register<ModDamageByState>();
		SnapshotFactory.Register<ModDebuffTime>();
		SnapshotFactory.Register<ModDebuffTimeR>();
		SnapshotFactory.Register<ModDefence>();
		SnapshotFactory.Register<ModDefenceR>();
		SnapshotFactory.Register<ModDefenceRByStageLevel>();
		SnapshotFactory.Register<ModDefensePower>();
		SnapshotFactory.Register<ModDodgeChance>();
		SnapshotFactory.Register<ModDotPower>();
		SnapshotFactory.Register<ModHealPower>();
		SnapshotFactory.Register<ModHitCcChance>();
		SnapshotFactory.Register<ModHitChance>();
		SnapshotFactory.Register<ModHitDebuffChance>();
		SnapshotFactory.Register<ModHpOnAttack>();
		SnapshotFactory.Register<ModHpOnDamage>();
		SnapshotFactory.Register<ModHpOnKill>();
		SnapshotFactory.Register<ModHpOnWave>();
		SnapshotFactory.Register<ModHpRatioOnAttack>();
		SnapshotFactory.Register<ModHpRatioOnDamage>();
		SnapshotFactory.Register<ModHpRatioOnKill>();
		SnapshotFactory.Register<ModHpRatioOnWave>();
		SnapshotFactory.Register<ModHpStealPower>();
		SnapshotFactory.Register<ModIgnoreBlockPower>();
		SnapshotFactory.Register<ModIgnoreDefencePower>();
		SnapshotFactory.Register<ModMagicalAttackPower>();
		SnapshotFactory.Register<ModMagicalBlockChance>();
		SnapshotFactory.Register<ModMagicalBlockPower>();
		SnapshotFactory.Register<ModMagicalCriticalChance>();
		SnapshotFactory.Register<ModMagicalCriticalDodgeChance>();
		SnapshotFactory.Register<ModMagicalCriticalPower>();
		SnapshotFactory.Register<ModMagicalCriticalWeakness>();
		SnapshotFactory.Register<ModMagicalDefence>();
		SnapshotFactory.Register<ModMagicalDefenceR>();
		SnapshotFactory.Register<ModMagicalDodgeChance>();
		SnapshotFactory.Register<ModMagicalDotPower>();
		SnapshotFactory.Register<ModMagicalHitChance>();
		SnapshotFactory.Register<ModMagicalIgnoreBlockPower>();
		SnapshotFactory.Register<ModMagicalIgnoreDefencePower>();
		SnapshotFactory.Register<ModMagicalPiercePower>();
		SnapshotFactory.Register<ModMagicalPower>();
		SnapshotFactory.Register<ModMagicalPowerR>();
		SnapshotFactory.Register<ModMagicalToughness>();
		SnapshotFactory.Register<ModMagicalWeakness>();
		SnapshotFactory.Register<ModMaxHp>();
		SnapshotFactory.Register<ModMaxHpDefence>();
		SnapshotFactory.Register<ModMaxHpDefenceR>();
		SnapshotFactory.Register<ModMaxHpR>();
		SnapshotFactory.Register<ModMaxHpRByStageLevel>();
		SnapshotFactory.Register<ModMaxMp>();
		SnapshotFactory.Register<ModMaxMpR>();
		SnapshotFactory.Register<ModMpOnAttack>();
		SnapshotFactory.Register<ModMpOnAttackR>();
		SnapshotFactory.Register<ModMpOnDamage>();
		SnapshotFactory.Register<ModMpOnDamageR>();
		SnapshotFactory.Register<ModMpOnKill>();
		SnapshotFactory.Register<ModMpOnKillR>();
		SnapshotFactory.Register<ModMpOnTime>();
		SnapshotFactory.Register<ModMpOnTimeR>();
		SnapshotFactory.Register<ModMpOnWave>();
		SnapshotFactory.Register<ModMpOnWaveR>();
		SnapshotFactory.Register<ModMpRegenRatioM>();
		SnapshotFactory.Register<ModMpRegenRatioMR>();
		SnapshotFactory.Register<ModMySkill>();
		SnapshotFactory.Register<ModNextActiveCooltime>();
		SnapshotFactory.Register<ModNextActiveCooltimeR>();
		SnapshotFactory.Register<ModNextBaseAttackR>();
		SnapshotFactory.Register<ModNextCooltime>();
		SnapshotFactory.Register<ModNextCooltimeR>();
		SnapshotFactory.Register<ModOperation>();
		SnapshotFactory.Register<ModPhysicalAttackPower>();
		SnapshotFactory.Register<ModPhysicalBlockChance>();
		SnapshotFactory.Register<ModPhysicalBlockPower>();
		SnapshotFactory.Register<ModPhysicalCriticalChance>();
		SnapshotFactory.Register<ModPhysicalCriticalDodgeChance>();
		SnapshotFactory.Register<ModPhysicalCriticalPower>();
		SnapshotFactory.Register<ModPhysicalCriticalWeakness>();
		SnapshotFactory.Register<ModPhysicalDefence>();
		SnapshotFactory.Register<ModPhysicalDefenceR>();
		SnapshotFactory.Register<ModPhysicalDodgeChance>();
		SnapshotFactory.Register<ModPhysicalDotPower>();
		SnapshotFactory.Register<ModPhysicalHitChance>();
		SnapshotFactory.Register<ModPhysicalIgnoreBlockPower>();
		SnapshotFactory.Register<ModPhysicalIgnoreDefencePower>();
		SnapshotFactory.Register<ModPhysicalPiercePower>();
		SnapshotFactory.Register<ModPhysicalPower>();
		SnapshotFactory.Register<ModPhysicalPowerR>();
		SnapshotFactory.Register<ModPhysicalToughness>();
		SnapshotFactory.Register<ModPhysicalWeakness>();
		SnapshotFactory.Register<ModPiercePower>();
		SnapshotFactory.Register<ModPower>();
		SnapshotFactory.Register<ModPowerR>();
		SnapshotFactory.Register<ModPowerRByStageLevel>();
		SnapshotFactory.Register<ModRecoverPower>();
		SnapshotFactory.Register<ModSetRadiusMm>();
		SnapshotFactory.Register<ModShieldPowerR>();
		SnapshotFactory.Register<ModSkill>();
		SnapshotFactory.Register<ModSkillDisabled>();
		SnapshotFactory.Register<ModSkillEnabled>();
		SnapshotFactory.Register<ModSkillInt>();
		SnapshotFactory.Register<ModSkillLevelAllSlot>();
		SnapshotFactory.Register<ModSkillLevelSlot1>();
		SnapshotFactory.Register<ModSkillLevelSlot2>();
		SnapshotFactory.Register<ModSkillLevelSlot3>();
		SnapshotFactory.Register<ModSkillLevelSlot4>();
		SnapshotFactory.Register<ModSkillPower>();
		SnapshotFactory.Register<ModSkillPowerR>();
		SnapshotFactory.Register<ModSkillValue>();
		SnapshotFactory.Register<ModSoulWeaponMaxUseCount>();
		SnapshotFactory.Register<ModStatByHp>();
		SnapshotFactory.Register<ModStatByHpR>();
		SnapshotFactory.Register<ModStateCount>();
		SnapshotFactory.Register<ModStateTime>();
		SnapshotFactory.Register<ModStateTimeR>();
		SnapshotFactory.Register<ModThreatGain>();
		SnapshotFactory.Register<ModThreatGainR>();
		SnapshotFactory.Register<ModToughness>();
		SnapshotFactory.Register<ModTriggerStat>();
		SnapshotFactory.Register<ModWeakness>();
		SnapshotFactory.Register<MoveRandom>();
		SnapshotFactory.Register<OverrideCalcMaxHpR>();
		SnapshotFactory.Register<OverrideStat>();
		SnapshotFactory.Register<OverrideStatFirst>();
		SnapshotFactory.Register<PropagateDamage>();
		SnapshotFactory.Register<Provoke>();
		SnapshotFactory.Register<Pulling>();
		SnapshotFactory.Register<PullTarget>();
		SnapshotFactory.Register<RaiseEventTrigger>();
		SnapshotFactory.Register<ReflectDamage>();
		SnapshotFactory.Register<RegisterPassiveSkill>();
		SnapshotFactory.Register<RemovePassiveSkill>();
		SnapshotFactory.Register<SetCreatureData>();
		SnapshotFactory.Register<SetDamage>();
		SnapshotFactory.Register<SetDirection>();
		SnapshotFactory.Register<SetMySkill>();
		SnapshotFactory.Register<SetSkill>();
		SnapshotFactory.Register<SetSkillInt>();
		SnapshotFactory.Register<SetSkillPower>();
		SnapshotFactory.Register<SetSkillPowerR>();
		SnapshotFactory.Register<SetSkillValue>();
		SnapshotFactory.Register<SetState>();
		SnapshotFactory.Register<SetTriggerTimer>();
		SnapshotFactory.Register<Shackle>();
		SnapshotFactory.Register<Silence>();
		SnapshotFactory.Register<NShared.NOperationRuntime.SpawnCreature>();
		SnapshotFactory.Register<StartCooltime>();
		SnapshotFactory.Register<Stone>();
		SnapshotFactory.Register<Stun>();
		SnapshotFactory.Register<SubSkill>();
		SnapshotFactory.Register<SuppressSkillCheck>();
		SnapshotFactory.Register<Untargetable>();
		SnapshotFactory.Register<UsePassiveSkill>();
		SnapshotFactory.Register<UseSkill>();
		SnapshotFactory.Register<UseSkillByDamageStackFixed>();
		SnapshotFactory.Register<UseSkillOnTick>();
		SnapshotFactory.Register<UseSommoneeSkill>();
		SnapshotFactory.Register<RandomTable>();
		SnapshotFactory.Register<OnActiveSkillTrigger>();
		SnapshotFactory.Register<OnAttack>();
		SnapshotFactory.Register<OnBaseAttack>();
		SnapshotFactory.Register<OnBeginPlaying>();
		SnapshotFactory.Register<OnBlock>();
		SnapshotFactory.Register<OnBuff>();
		SnapshotFactory.Register<OnCoolTime>();
		SnapshotFactory.Register<OnCreatureKill>();
		SnapshotFactory.Register<OnCriticalSkill>();
		SnapshotFactory.Register<OnDamage>();
		SnapshotFactory.Register<OnDebuff>();
		SnapshotFactory.Register<OnDie>();
		SnapshotFactory.Register<OnDodge>();
		SnapshotFactory.Register<OnEscapeDeath>();
		SnapshotFactory.Register<OnFireSkill>();
		SnapshotFactory.Register<OnFireSlotSkill>();
		SnapshotFactory.Register<OnFriendAttack>();
		SnapshotFactory.Register<OnFriendDamage>();
		SnapshotFactory.Register<OnHeal>();
		SnapshotFactory.Register<OnHealed>();
		SnapshotFactory.Register<OnHpRatio>();
		SnapshotFactory.Register<OnImmune>();
		SnapshotFactory.Register<OnKill>();
		SnapshotFactory.Register<OnMiss>();
		SnapshotFactory.Register<OnMpRatio>();
		SnapshotFactory.Register<OnStart>();
		SnapshotFactory.Register<OnState>();
		SnapshotFactory.Register<OnStateActionSkill>();
		SnapshotFactory.Register<OnTimer>();
		SnapshotFactory.Register<OnUseCombo>();
		SnapshotFactory.Register<OnWaveStart>();
		SnapshotFactory.Register<OperationController>();
		SnapshotFactory.Register<OperationInfo>();
		SnapshotFactory.Register<OperationQueue>();
		SnapshotFactory.Register<PassiveSkillContainer>();
		SnapshotFactory.Register<PassiveSkillInfo>();
		SnapshotFactory.Register<PrevDamageInfo>();
		SnapshotFactory.Register<ProjectileContainer>();
		SnapshotFactory.Register<ReservedOperationInfo>();
		SnapshotFactory.Register<SkillChangerContainer>();
		SnapshotFactory.Register<SkillCheckModifierContainer>();
		SnapshotFactory.Register<SkillController>();
		SnapshotFactory.Register<SkillModifier>();
		SnapshotFactory.Register<SkillModifierContainer>();
		SnapshotFactory.Register<SkillProjectileInfo>();
		SnapshotFactory.Register<SkillTargetController>();
		SnapshotFactory.Register<SkillTickTime>();
		SnapshotFactory.Register<SnapshotObject>();
		SnapshotFactory.Register<SoulWeaponSkillController>();
		SnapshotFactory.Register<StatController>();
		SnapshotFactory.Register<StateController>();
		SnapshotFactory.Register<StateFlagModifier>();
		SnapshotFactory.Register<TacticManager>();
		SnapshotFactory.Register<Team>();
		SnapshotFactory.Register<TeamController>();
		SnapshotFactory.Register<TimerSkill>();
		SnapshotFactory.Register<TransformController>();
		SnapshotFactory.Register<UsingSkillInfo>();
		SnapshotFactory.Register<IndexableSnapshotDictionary<int, CooltimeTimer>>("IndexableSnapshotDictionary_Int32_CooltimeTimer");
		SnapshotFactory.Register<IndexableSnapshotDictionary<int, Vector3>>("IndexableSnapshotDictionary_Int32_Vector3");
		SnapshotFactory.Register<IndexableSnapshotDictionary<int, PassiveSkillInfo>>("IndexableSnapshotDictionary_Int32_PassiveSkillInfo");
		SnapshotFactory.Register<IndexableSnapshotDictionary<long, OperationInfo>>("IndexableSnapshotDictionary_Int64_OperationInfo");
		SnapshotFactory.Register<IndexableSnapshotDictionary<string, BlackBoardValueInfo>>("IndexableSnapshotDictionary_String_BlackBoardValueInfo");
		SnapshotFactory.Register<IndexableSnapshotDictionary<string, int>>("IndexableSnapshotDictionary_String_Int32");
		SnapshotFactory.Register<SnapshotArray<BattleCreature>>("SnapshotArray_BattleCreature");
		SnapshotFactory.Register<SnapshotArray<CreatureRaceType>>("SnapshotArray_CreatureRaceType");
		SnapshotFactory.Register<SnapshotArray<DelegateStateModifier>>("SnapshotArray_DelegateStateModifier");
		SnapshotFactory.Register<SnapshotArray<OperationFlag>>("SnapshotArray_OperationFlag");
		SnapshotFactory.Register<SnapshotArray<StatType>>("SnapshotArray_StatType");
		SnapshotFactory.Register<SnapshotArray<int>>("SnapshotArray_Int32");
		SnapshotFactory.Register<SnapshotArray<string>>("SnapshotArray_String");
		SnapshotFactory.Register<SnapshotDictionary<TeamType, int>>("SnapshotDictionary_TeamType_Int32");
		SnapshotFactory.Register<SnapshotDictionary<int, BattleStatistics>>("SnapshotDictionary_Int32_BattleStatistics");
		SnapshotFactory.Register<SnapshotDictionary<int, ModCooltimeDurationInfo>>("SnapshotDictionary_Int32_ModCooltimeDurationInfo");
		SnapshotFactory.Register<SnapshotDictionary<int, ModCooltimeRatioInfo>>("SnapshotDictionary_Int32_ModCooltimeRatioInfo");
		SnapshotFactory.Register<SnapshotDictionary<int, int>>("SnapshotDictionary_Int32_Int32");
		SnapshotFactory.Register<SnapshotDictionary<int, long>>("SnapshotDictionary_Int32_Int64");
		SnapshotFactory.Register<SnapshotDictionary<int, string>>("SnapshotDictionary_Int32_String");
		SnapshotFactory.Register<SnapshotLinkedList<IAttackModifier>>("SnapshotLinkedList_IAttackModifier");
		SnapshotFactory.Register<SnapshotLinkedList<ICreatureDataModifier>>("SnapshotLinkedList_ICreatureDataModifier");
		SnapshotFactory.Register<SnapshotLinkedList<IDamageDodger>>("SnapshotLinkedList_IDamageDodger");
		SnapshotFactory.Register<SnapshotLinkedList<IDamageFlagModifier>>("SnapshotLinkedList_IDamageFlagModifier");
		SnapshotFactory.Register<SnapshotLinkedList<IDamageModifier>>("SnapshotLinkedList_IDamageModifier");
		SnapshotFactory.Register<SnapshotLinkedList<IStatModifier>>("SnapshotLinkedList_IStatModifier");
		SnapshotFactory.Register<SnapshotLinkedList<IStatOverrider>>("SnapshotLinkedList_IStatOverrider");
		SnapshotFactory.Register<SnapshotLinkedList<ITriggerStatModifier>>("SnapshotLinkedList_ITriggerStatModifier");
		SnapshotFactory.Register<SnapshotLinkedList<ReservedOperationInfo>>("SnapshotLinkedList_ReservedOperationInfo");
		SnapshotFactory.Register<SnapshotList<BattleCreature>>("SnapshotList_BattleCreature");
		SnapshotFactory.Register<SnapshotList<DelayedOperationInfo>>("SnapshotList_DelayedOperationInfo");
		SnapshotFactory.Register<SnapshotList<DungeonSkill>>("SnapshotList_DungeonSkill");
		SnapshotFactory.Register<SnapshotList<IHpObserver>>("SnapshotList_IHpObserver");
		SnapshotFactory.Register<SnapshotList<IPassiveToggler>>("SnapshotList_IPassiveToggler");
		SnapshotFactory.Register<SnapshotList<ISkillChanger>>("SnapshotList_ISkillChanger");
		SnapshotFactory.Register<SnapshotList<ISkillCheckSuppresser>>("SnapshotList_ISkillCheckSuppresser");
		SnapshotFactory.Register<SnapshotList<ISkillDisabler>>("SnapshotList_ISkillDisabler");
		SnapshotFactory.Register<SnapshotList<ISkillModifyListener>>("SnapshotList_ISkillModifyListener");
		SnapshotFactory.Register<SnapshotList<ISkillTargetFilter>>("SnapshotList_ISkillTargetFilter");
		SnapshotFactory.Register<SnapshotList<IStateDurationModifier>>("SnapshotList_IStateDurationModifier");
		SnapshotFactory.Register<SnapshotList<IStateModifier>>("SnapshotList_IStateModifier");
		SnapshotFactory.Register<SnapshotList<IStateObserver>>("SnapshotList_IStateObserver");
		SnapshotFactory.Register<SnapshotList<IStatObserver>>("SnapshotList_IStatObserver");
		SnapshotFactory.Register<SnapshotList<ITeamInverter>>("SnapshotList_ITeamInverter");
		SnapshotFactory.Register<SnapshotList<OperationInfo>>("SnapshotList_OperationInfo");
		SnapshotFactory.Register<SnapshotList<SkillModifier>>("SnapshotList_SkillModifier");
		SnapshotFactory.Register<SnapshotList<SkillProjectileInfo>>("SnapshotList_SkillProjectileInfo");
		SnapshotFactory.Register<SnapshotList<SkillTickTime>>("SnapshotList_SkillTickTime");
		SnapshotFactory.Register<SnapshotList<StateFlagModifier>>("SnapshotList_StateFlagModifier");
		SnapshotFactory.Register<SnapshotList<int>>("SnapshotList_Int32");
	}

	public static bool Contains(this AttackAttrType[] e, AttackAttrType value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this BattleRetryConditionType[] e, BattleRetryConditionType value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this CreatureRaceType[] e, CreatureRaceType value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this CreatureTagType[] e, CreatureTagType value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this OperationFlag[] e, OperationFlag value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this SkillAttrType[] e, SkillAttrType value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this SkillTargetType[] e, SkillTargetType value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this SkillTriggerFlag[] e, SkillTriggerFlag value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this StatType[] e, StatType value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this bool[] e, bool value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this byte[] e, byte value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this char[] e, char value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this short[] e, short value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this int[] e, int value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(this long[] e, long value)
	{
		for (int i = 0; i < e.Length; i++)
		{
			if (e[i] == value)
			{
				return true;
			}
		}
		return false;
	}
}
