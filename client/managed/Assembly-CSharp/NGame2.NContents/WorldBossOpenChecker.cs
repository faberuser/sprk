using NShared;
using NVespa;

namespace NGame2.NContents;

public class WorldBossOpenChecker : IMenuOpenChecker
{
	bool IMenuOpenChecker.IsOpen()
	{
		return SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.WorldBoss);
	}
}
