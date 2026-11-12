namespace NShared;

public interface IBaseDamageModifier : ISnapshotObject
{
	long GetOperationUid();

	bool IsValid();
}
