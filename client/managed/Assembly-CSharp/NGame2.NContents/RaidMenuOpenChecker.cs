using NShared;
using NVespa;

namespace NGame2.NContents;

public class RaidMenuOpenChecker : IMenuOpenChecker
{
	bool IMenuOpenChecker.IsOpen()
	{
		return SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.Raid);
	}
}
