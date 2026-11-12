namespace NShared;

public interface IPassiveToggler : ISnapshotObject
{
	int GetSkillIndex();

	bool GetEnabled();
}
