namespace NShared;

[BattleCallback]
public sealed class ChangeState : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public int SkillIndex { get; set; }

	public bool StateAdded { get; set; }

	public string ChangedStateType { get; set; }

	public string[] FinalStateType { get; set; }

	public int StackCount { get; set; }

	public int FiredCreatureUid { get; set; }

	public StateStackDurationRuleType StackDurationRuleType { get; set; }

	public int DurationMs { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is ChangeState changeState))
		{
			return false;
		}
		if (base.GlobalTimestamp == changeState.GlobalTimestamp && CreatureUid == changeState.CreatureUid && OperationUid == changeState.OperationUid && StateAdded == changeState.StateAdded && ChangedStateType == changeState.ChangedStateType && BaseBattleCallback.ArrayIsEquals(FinalStateType, changeState.FinalStateType) && StackCount == changeState.StackCount && FiredCreatureUid == changeState.FiredCreatureUid && StackDurationRuleType == changeState.StackDurationRuleType)
		{
			return DurationMs == changeState.DurationMs;
		}
		return false;
	}
}
