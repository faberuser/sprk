namespace NShared;

public class SnapshotObjectSerializer : ISnapshotMemberSerializer
{
	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		long result = 0L;
		long.TryParse(value, out result);
		return container.GetSnapshot(result);
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		if (value == null)
		{
			return null;
		}
		return "U:" + ((ISnapshotObject)value).GetUid();
	}
}
