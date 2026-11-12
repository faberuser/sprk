using NShared.NJit;

namespace NShared;

public class SkillProjectileGroupDataContainer : JitDataContainerDefault<int, SkillProjectileGroupData>
{
	public const string TABLENAME = "SkillProjectileGroupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SkillProjectileGroupTable");
}
