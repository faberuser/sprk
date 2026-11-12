using NShared;
using NVespa;

namespace NGame2.NContents;

public class GodkingTrialMenuOpenChecker : IMenuOpenChecker
{
	bool IMenuOpenChecker.IsOpen()
	{
		return SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.GodkingTrial);
	}
}
