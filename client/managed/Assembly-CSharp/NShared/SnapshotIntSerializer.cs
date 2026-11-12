namespace NShared;

public class SnapshotIntSerializer : ISnapshotMemberSerializer
{
	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		int result = 0;
		int.TryParse(value, out result);
		return result;
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		return value?.ToString();
	}
}
