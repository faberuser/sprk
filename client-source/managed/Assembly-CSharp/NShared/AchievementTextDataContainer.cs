using NShared.NJit;

namespace NShared;

public class AchievementTextDataContainer : JitDataContainerDefault<AchievementKey2, AchievementTextData>
{
	public const string TABLENAME = "AchievementTextTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AchievementTextTable");

	public AchievementTextData GetData(int AchievementIndex, int Step = 1)
	{
		return GetData(new AchievementKey2(AchievementIndex, Step));
	}
}
