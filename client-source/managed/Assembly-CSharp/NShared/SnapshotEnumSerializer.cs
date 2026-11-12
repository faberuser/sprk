using System;
using NVespa;

namespace NShared;

public class SnapshotEnumSerializer : ISnapshotMemberSerializer
{
	private readonly Type _type;

	public SnapshotEnumSerializer(Type type)
	{
		_type = type;
	}

	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		try
		{
			return Enum.Parse(_type, value);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to parse enum : " + _type.Name + " value=" + value + " error=" + ex);
			return 0;
		}
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		return value?.ToString();
	}
}
