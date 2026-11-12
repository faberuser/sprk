namespace NShared;

public interface ISnapshotMember
{
	void Set(object value);

	void Reset();

	object Get();

	ISnapshotMemberSerializer GetSerializer();
}
