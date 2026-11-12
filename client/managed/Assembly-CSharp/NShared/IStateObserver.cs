namespace NShared;

public interface IStateObserver : ISnapshotObject
{
	void OnStateChanged(string stateType);
}
