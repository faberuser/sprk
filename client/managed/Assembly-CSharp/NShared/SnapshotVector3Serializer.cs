using NShared.NUtil;

namespace NShared;

public class SnapshotVector3Serializer : ISnapshotMemberSerializer
{
	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		string[] array = value.Substring(1, value.Length - 2).Split(new char[1] { ',' });
		string s = ((array.Length != 0) ? array[0] : "0");
		string s2 = ((array.Length > 1) ? array[1] : "0");
		string s3 = ((array.Length > 2) ? array[2] : "0");
		return new Vector3
		{
			mx = long.Parse(s),
			my = long.Parse(s2),
			mz = long.Parse(s3)
		};
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		if (value == null)
		{
			return null;
		}
		Vector3 vector = (Vector3)value;
		return "(" + vector.mx + "," + vector.my + "," + vector.mz + ")";
	}
}
