using NShared.NJit;

namespace NShared;

public class SkillProjectileDataContainer : JitDataContainerDefault<int, SkillProjectileData>, ISnapshotMemberSerializer
{
	public const string TABLENAME = "SkillProjectileTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SkillProjectileTable");

	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		if (!int.TryParse(value, out var result))
		{
			return null;
		}
		return GetData(result);
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		if (!(value is SkillProjectileData skillProjectileData))
		{
			return null;
		}
		return skillProjectileData.GETKEY().ToString();
	}
}
