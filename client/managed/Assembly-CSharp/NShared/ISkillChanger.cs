namespace NShared;

public interface ISkillChanger : ISnapshotObject
{
	int FromSkillIndex();

	int ToSkillIndex();

	bool IsReplaced();
}
