using NShared.NJit;

namespace NShared;

public class SkillPriceDataContainer : JitDataContainerDefault<SkillPriceKey, SkillPriceData>
{
	public const string TABLENAME = "SkillPriceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SkillPriceTable");
}
