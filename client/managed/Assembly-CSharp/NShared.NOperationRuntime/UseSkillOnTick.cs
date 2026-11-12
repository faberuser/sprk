namespace NShared.NOperationRuntime;

public class UseSkillOnTick : BaseOperationRuntime
{
	protected readonly SnapshotObjectMember<int> _skillIndex;

	protected readonly SnapshotObjectMember<bool> _checkMp;

	protected readonly SnapshotObjectMember<bool> _isIgnoreCoolTime;

	protected readonly SnapshotObjectMember<bool> _isIgnoreSoloTime;

	protected readonly SnapshotObjectMember<bool> _isCanBeCancel;

	protected readonly SnapshotObjectMember<bool> _checkBattleEnabled;

	protected readonly SnapshotObjectMember<bool> _checkLevel;

	public UseSkillOnTick()
	{
		_skillIndex = new SnapshotObjectMember<int>(this, 0);
		_checkMp = new SnapshotObjectMember<bool>(this, value: false);
		_isIgnoreCoolTime = new SnapshotObjectMember<bool>(this, value: false);
		_isIgnoreSoloTime = new SnapshotObjectMember<bool>(this, value: false);
		_isCanBeCancel = new SnapshotObjectMember<bool>(this, value: false);
		_checkBattleEnabled = new SnapshotObjectMember<bool>(this, value: false);
		_checkLevel = new SnapshotObjectMember<bool>(this, value: false);
	}

	protected override void OnInit()
	{
		base.OnInit();
		_skillIndex.Set(base.OperationInfo.GetValueInt(0));
		_checkMp.Set(base.OperationInfo.GetValueBool(1));
		_isIgnoreCoolTime.Set(base.OperationInfo.GetValueBool(2));
		_isIgnoreSoloTime.Set(base.OperationInfo.GetValueBool(3));
		_isCanBeCancel.Set(base.OperationInfo.GetValueBool(4));
		_checkBattleEnabled.Set(base.OperationInfo.GetValueBool(5));
		_checkLevel.Set(base.OperationInfo.GetValueBool(6));
	}

	protected override void OnTick()
	{
		base.OnTick();
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(_skillIndex);
		if (data != null)
		{
			base.OperationInfo.Creature.SkillController.UseSkill(data, new SkillUseOption(checkOnly: false, _checkLevel, _checkMp, isEventSkill: false, null, isIgnoreCoolTime: _isIgnoreCoolTime, isIgnoreSoloTime: _isIgnoreSoloTime, isCanBeCancel: _isCanBeCancel, checkBattleEnabled: _checkBattleEnabled), null);
		}
	}
}
