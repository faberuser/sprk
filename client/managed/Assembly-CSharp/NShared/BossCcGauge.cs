using System;
using NShared.NBattleTacticEvent;

namespace NShared;

public class BossCcGauge : BaseBattleTactic
{
	public const int RegenWaitPerDecRatioM = 1000;

	public const int RegenIntervalTimeMs = 100;

	public const int RecoverFromGroggyTimeMs = 1000;

	private readonly SnapshotObjectMember<int> _maxGaugeHolder;

	private readonly SnapshotObjectMember<int> _decHolder;

	private readonly SnapshotObjectMember<int> _regenHolder;

	private readonly SnapshotObjectMember<int> _regenDisableTimeHolder;

	private readonly SnapshotObjectMember<SkillData> _groggySkillDataHolder;

	private readonly SnapshotObjectMember<int> _curGaugeHolder;

	private readonly SnapshotObjectMember<long> _lastRegenTimeMsHolder;

	private readonly SnapshotObjectMember<long> _regenEnableTimeMsHolder;

	private readonly SnapshotObjectMember<long> _recoverStartTimeMsHolder;

	private readonly SnapshotObjectMember<BattleCreature> _groggyCreatureHolder;

	public int MaxGauge => _maxGaugeHolder.Get();

	public int CurGauge => _curGaugeHolder.Get();

	public BossCcGauge()
	{
		_maxGaugeHolder = new SnapshotObjectMember<int>(this, 0);
		_decHolder = new SnapshotObjectMember<int>(this, 0);
		_regenHolder = new SnapshotObjectMember<int>(this, 0);
		_regenDisableTimeHolder = new SnapshotObjectMember<int>(this, 0);
		_groggySkillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_curGaugeHolder = new SnapshotObjectMember<int>(this, 0);
		_lastRegenTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_regenEnableTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_recoverStartTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_groggyCreatureHolder = new SnapshotObjectMember<BattleCreature>(this);
	}

	public override void OnInit()
	{
		int valueInt = ArgParser.GetValueInt(base.TacticData.TacticValue, 0);
		int valueInt2 = ArgParser.GetValueInt(base.TacticData.TacticValue, 1);
		int valueInt3 = ArgParser.GetValueInt(base.TacticData.TacticValue, 2);
		int valueInt4 = ArgParser.GetValueInt(base.TacticData.TacticValue, 3);
		int valueInt5 = ArgParser.GetValueInt(base.TacticData.TacticValue, 4);
		_maxGaugeHolder.Set(valueInt);
		_decHolder.Set(valueInt2);
		_regenHolder.Set(valueInt3);
		_regenDisableTimeHolder.Set(valueInt5);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(valueInt4);
		_groggySkillDataHolder.Set(data);
		MakeBossCcCurGaugeDirty(_curGaugeHolder.Get(), _maxGaugeHolder.Get());
		_curGaugeHolder.Set(_maxGaugeHolder.Get());
		_lastRegenTimeMsHolder.Set(0L);
		_regenEnableTimeMsHolder.Set(0L);
		_recoverStartTimeMsHolder.Set(0L);
	}

	public override void OnUpdate(long currentGlobalTimeMs)
	{
		Regen(currentGlobalTimeMs);
		RecoverFromGroggy(currentGlobalTimeMs);
	}

	public override void OnEvent(IBattleTacticEvent tacticEvent, long currentGlobalTimeMs)
	{
		BaseBattleTactic.HandleEvent<OperationPreBeginEvent>(tacticEvent, currentGlobalTimeMs, Handle);
	}

	private bool IsRecovering(long currentGlobalTimeMs)
	{
		return currentGlobalTimeMs - _recoverStartTimeMsHolder.Get() <= 1000;
	}

	private void Regen(long currentGlobalTimeMs)
	{
		if (_regenHolder.Get() <= 0)
		{
			return;
		}
		long num = _lastRegenTimeMsHolder.Get();
		long num2 = currentGlobalTimeMs - num;
		if (num2 >= 100)
		{
			_lastRegenTimeMsHolder.Set(currentGlobalTimeMs);
			if (currentGlobalTimeMs >= _regenEnableTimeMsHolder.Get() && _groggyCreatureHolder.Get() == null)
			{
				int num3 = 0;
				num3 = (int)((!IsRecovering(currentGlobalTimeMs)) ? (_regenHolder.Get() * num2 / 1000) : (_maxGaugeHolder.Get() * num2 / 1000));
				int oldValue = _curGaugeHolder.Get();
				int num4 = Math.Min(_curGaugeHolder.Get() + num3, _maxGaugeHolder.Get());
				_curGaugeHolder.Set(num4);
				MakeBossCcCurGaugeDirty(oldValue, num4);
			}
		}
	}

	private void RecoverFromGroggy(long currentGlobalTimeMs)
	{
		BattleCreature battleCreature = _groggyCreatureHolder.Get();
		if (battleCreature != null)
		{
			SkillData skillData = _groggySkillDataHolder.Get();
			if (skillData != null && (battleCreature.SkillController.UsingSkillInfo == null || battleCreature.SkillController.UsingSkillInfo.SkillData != skillData) && battleCreature.OperationController.FindOperationInfo(skillData.Index) == null && battleCreature.SkillController.OperationQueue.GetReservedOperationInfoOfSkill(skillData.Index) == null)
			{
				_groggyCreatureHolder.Set(null);
				_regenEnableTimeMsHolder.Set(0L);
				_recoverStartTimeMsHolder.Set(currentGlobalTimeMs);
			}
		}
	}

	private void Handle(OperationPreBeginEvent tacticEvent, long currentGlobalTimeMs)
	{
		if (!tacticEvent.Owner.CreateInfo.IsBoss || !(tacticEvent.OperationInfo.Runtime is IStateModifier stateModifier))
		{
			return;
		}
		StateData stateData = stateModifier.GetStateData();
		if (stateData == null || !StateController.IsCcState(stateModifier.GetFiredCreature(), stateData))
		{
			return;
		}
		int ccPower = stateData.CcPower;
		if (ccPower <= 0)
		{
			return;
		}
		int num = _curGaugeHolder.Get();
		if (num <= 0 || IsRecovering(currentGlobalTimeMs))
		{
			return;
		}
		if (tacticEvent.OperationInfo.HasOperationFlag(OperationFlag.CheckConditionOnBegin))
		{
			FiredOperationInfo firedInfo = tacticEvent.OperationInfo.FiredInfo;
			if (!SkillController.CheckMeetCondition(firedInfo.FiredCreature.SkillController.ModifierContainer, tacticEvent.OperationInfo.OperationData, firedInfo, tacticEvent.Owner))
			{
				return;
			}
		}
		int durationMs = tacticEvent.OperationInfo.DurationMs;
		int num2 = (int)((long)_decHolder.Get() * (long)durationMs * ccPower / 1000 / 1000);
		int oldValue = _curGaugeHolder.Get();
		int num3 = Math.Max(num - num2, 0);
		_curGaugeHolder.Set(num3);
		MakeBossCcCurGaugeDirty(oldValue, num3);
		int num4 = num2 * 1000 / 1000;
		long num5 = currentGlobalTimeMs + num4;
		if (num5 > _regenEnableTimeMsHolder.Get())
		{
			_regenEnableTimeMsHolder.Set(num5);
		}
		if (num3 != 0)
		{
			return;
		}
		SkillData skillData = _groggySkillDataHolder.Get();
		if (skillData != null)
		{
			_groggyCreatureHolder.Set(tacticEvent.Owner);
			if (_regenDisableTimeHolder.Get() > 0)
			{
				_regenEnableTimeMsHolder.Set(currentGlobalTimeMs + _regenDisableTimeHolder.Get());
			}
			tacticEvent.Owner.SkillController.UseSkill(skillData, new SkillUseOption(checkOnly: false, checkLevel: false, checkMp: false, isEventSkill: true), null);
		}
	}

	private void MakeBossCcCurGaugeDirty(int oldValue, int newValue)
	{
		if (oldValue != newValue)
		{
			BattleCreatureList aliveCreatures = _battleInstance.CreatureContainer.GetAliveCreatures();
			_battleInstance.MakeCretureSatatDirty(aliveCreatures, StatType.BossCcCurGauge);
		}
	}
}
