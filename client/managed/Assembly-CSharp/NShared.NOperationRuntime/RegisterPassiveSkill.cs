namespace NShared.NOperationRuntime;

public class RegisterPassiveSkill : BaseOperationRuntime
{
	protected readonly SnapshotObjectMember<PassiveSkillInfo> _passiveSkillInfoHolder;

	public RegisterPassiveSkill()
	{
		_passiveSkillInfoHolder = new SnapshotObjectMember<PassiveSkillInfo>(this);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(valueInt);
		if (data != null)
		{
			valueInt2 = ((valueInt2 != 0) ? valueInt2 : base.OperationInfo.Creature.CreateInfo.HeroInfo.Level);
			PassiveSkillInfo passiveSkillInfo = base.OperationInfo.Creature.SkillController.PassiveSkillContainer.Register(data, valueInt2);
			if (passiveSkillInfo != null)
			{
				_passiveSkillInfoHolder.Set(passiveSkillInfo);
			}
		}
	}
}
