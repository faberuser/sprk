namespace NShared.NOperationRuntime;

public class Allure : BaseOperationRuntime, ITeamInverter, ISnapshotObject
{
	protected override void OnBegin()
	{
		base.OnBegin();
		AddInverter(base.OperationInfo.Creature);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		if (RemoveInverter(base.OperationInfo.Creature))
		{
			base.OperationInfo.Creature.SkillController.StopUsingDurationalSkill(isCancel: true);
		}
	}

	private void AddInverter(BattleCreature creature)
	{
		if (!IsDisabledInverter(creature))
		{
			creature.TeamController.AddInverter(this);
		}
	}

	private bool RemoveInverter(BattleCreature creature)
	{
		if (IsDisabledInverter(creature))
		{
			return false;
		}
		return creature.TeamController.RemoveInveter(this);
	}

	private bool IsDisabledInverter(BattleCreature creature)
	{
		if (creature == null || creature.CreateInfo.IsBoss)
		{
			return true;
		}
		return false;
	}
}
