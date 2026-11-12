using System;

namespace NShared;

public class SnapshotBoolSerializer : ISnapshotMemberSerializer
{
	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		return value?.ToString();
	}
}
