namespace NShared;

public class SnapshotShortSerializer : ISnapshotMemberSerializer
{
	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		short result = 0;
		short.TryParse(value, out result);
		return result;
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		return value?.ToString();
	}
}
