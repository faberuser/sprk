using NShared.NJit;

namespace NShared;

public class LCSkillNameDataContainer : JitDataContainerDefault<int, LCSkillNameData>
{
	public const string TABLENAME = "LCSkillNameTable";

	public override TablePathDefine PathDefine => new TablePathDefine("LCSkillNameTable");
}
