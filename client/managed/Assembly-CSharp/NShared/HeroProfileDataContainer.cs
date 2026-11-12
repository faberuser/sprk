using NShared.NJit;

namespace NShared;

public class HeroProfileDataContainer : JitDataContainerDefault<int, HeroProfileData>
{
	public const string TABLENAME = "HeroProfileTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HeroProfileTable");
}
