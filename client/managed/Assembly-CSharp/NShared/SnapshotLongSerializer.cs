namespace NShared;

public class SnapshotLongSerializer : ISnapshotMemberSerializer
{
	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		long result = 0L;
		long.TryParse(value, out result);
		return result;
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		return value.ToString();
	}
}
