namespace NShared.NOperationRuntime;

public class UseSkill : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		bool valueBool = base.OperationInfo.GetValueBool(1);
		bool valueBool2 = base.OperationInfo.GetValueBool(2);
		bool valueBool3 = base.OperationInfo.GetValueBool(3);
		bool valueBool4 = base.OperationInfo.GetValueBool(4);
		bool valueBool5 = base.OperationInfo.GetValueBool(5);
		bool valueBool6 = base.OperationInfo.GetValueBool(6, defaultValue: true);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(valueInt);
		if (data != null)
		{
			SkillController skillController = base.OperationInfo.Creature.SkillController;
			bool isIgnoreCoolTime = valueBool2;
			bool isIgnoreSoloTime = valueBool3;
			bool isCanBeCancel = valueBool4;
			skillController.UseSkill(data, new SkillUseOption(checkOnly: false, valueBool6, valueBool, isEventSkill: false, null, valueBool5, checkReqHpRatio: true, isIgnoreCoolTime, isIgnoreSoloTime, isCanBeCancel), null);
		}
	}
}
