namespace NShared;

public interface ICreatureDataModifier : ISnapshotObject
{
	CreatureDataFieldType GetFieldType();

	object GetValue();

	int GetValueIndex();
}
