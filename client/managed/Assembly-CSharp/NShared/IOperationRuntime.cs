namespace NShared;

public interface IOperationRuntime : ISnapshotObject
{
	void Init(OperationInfo operationInfo);

	void OnBegin();

	void OnEnd();

	void OnTick();
}
