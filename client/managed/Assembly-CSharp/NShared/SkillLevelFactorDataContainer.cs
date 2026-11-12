using NShared.NJit;

namespace NShared;

public class SkillLevelFactorDataContainer : JitDataContainerDefault<int, SkillLevelFactorData>
{
	public const string TABLENAME = "SkillLevelFactorTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SkillLevelFactorTable");

	public int GetLevelFactor(int type, int level)
	{
		if (type == 9999)
		{
			return level * 1000;
		}
		return GetData(level)?.GetFactor(type) ?? 0;
	}
}
