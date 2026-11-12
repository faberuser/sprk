namespace NShared;

public class SnapshotStringSerializer : ISnapshotMemberSerializer
{
	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		return value;
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		return value?.ToString();
	}
}
