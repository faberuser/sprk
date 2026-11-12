namespace NShared.StateAction;

public class UseSkill : IStateAction
{
	void IStateAction.Run(BattleCreature creature, string triggerStateType, string[] values, IStateModifier stateModifier)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(valueInt);
		if (data != null)
		{
			SkillUseOption skillUseOption = new SkillUseOption();
			skillUseOption.CheckOnly = true;
			skillUseOption.CheckLevel = false;
			OnStateActionSkill onStateActionSkill = new OnStateActionSkill();
			onStateActionSkill.TriggerCreature = ((stateModifier != null) ? stateModifier.GetFiredCreature() : creature);
			onStateActionSkill.TargetCreatures = new BattleCreature[1] { creature };
			onStateActionSkill.SkillIndex = valueInt;
			creature.SkillController.UseSkill(data, skillUseOption, onStateActionSkill);
		}
	}
}
