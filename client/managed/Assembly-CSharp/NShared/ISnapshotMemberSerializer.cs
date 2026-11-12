namespace NShared;

public interface ISnapshotMemberSerializer
{
	object From(string value, DumpedSnapshotContainer container);

	string To(object value);
}
