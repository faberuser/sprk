namespace NShared;

public interface ISkillModifyListener : ISnapshotObject
{
	void OnModified(int skillIndex, int operationNo, OperationFieldType fieldType, int fieldIndex);
}
