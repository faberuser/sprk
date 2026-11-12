namespace NShared.NOperationRuntime;

public class SetCreatureData : BaseOperationRuntime, ICreatureDataModifier, ISnapshotObject
{
	private readonly SnapshotObjectMember<CreatureDataFieldType> _fieldTypeHolder;

	private readonly SnapshotObjectMember<string> _valueHolder;

	private readonly SnapshotObjectMember<int> _valueIndexHolder;

	public SetCreatureData()
	{
		_fieldTypeHolder = new SnapshotObjectMember<CreatureDataFieldType>(this, CreatureDataFieldType.None);
		_valueHolder = new SnapshotObjectMember<string>(this);
		_valueIndexHolder = new SnapshotObjectMember<int>(this, 0);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		_fieldTypeHolder.Set(base.OperationInfo.GetValueEnum<CreatureDataFieldType>(0));
		_valueHolder.Set(base.OperationInfo.GetValueString(1));
		_valueIndexHolder.Set(base.OperationInfo.GetValueInt(2));
		base.OperationInfo.Creature.BattleCreatureData.BeginSetCreatureDataModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.BattleCreatureData.EndSetCreatureDataModifier(this);
	}

	CreatureDataFieldType ICreatureDataModifier.GetFieldType()
	{
		return _fieldTypeHolder.Get();
	}

	object ICreatureDataModifier.GetValue()
	{
		return _valueHolder.Get();
	}

	int ICreatureDataModifier.GetValueIndex()
	{
		return _valueIndexHolder.Get();
	}
}
