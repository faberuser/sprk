namespace NShared.NOperationRuntime;

public class UseSommoneeSkill : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		bool valueBool = base.OperationInfo.GetValueBool(1);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(valueInt);
		if (data != null)
		{
			BattleCreatureList childCreatureList = base.OperationInfo.BattleInstance.CreatureContainer.GetChildCreatureList(base.OperationInfo.Creature.Uid, aliveOnly: true);
			for (int i = 0; i < childCreatureList.Count; i++)
			{
				childCreatureList[i]?.SkillController.UseSkill(data, new SkillUseOption(checkOnly: false, checkLevel: true, valueBool), null);
			}
		}
	}
}
