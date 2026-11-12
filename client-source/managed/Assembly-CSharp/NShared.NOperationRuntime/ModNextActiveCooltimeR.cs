namespace NShared.NOperationRuntime;

public class ModNextActiveCooltimeR : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int ratioM = base.OperationInfo.GetValueInt(1);
		int count = base.OperationInfo.GetValueInt(2);
		bool checkExist = base.OperationInfo.GetValueBool(3);
		BattleCreature creature = base.OperationInfo.Creature;
		if (creature != null)
		{
			ModActiveCooltime.ForEachActiveSkillIndex(creature, valueInt, ModActiveCooltime.ActiveSkillFlag.None, delegate(int skillIndex)
			{
				creature.SkillController.CooltimeManager.ModNextCooltimeRatio(skillIndex, ratioM, count, checkExist);
			});
		}
	}
}
