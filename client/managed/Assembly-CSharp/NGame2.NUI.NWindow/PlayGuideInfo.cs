using NShared;

namespace NGame2.NUI.NWindow;

public class PlayGuideInfo
{
	public PlayGuideData PlayGuideData { get; private set; }

	public PlayGuideStatus Status { get; set; }

	public PlayGuideInfo(PlayGuideData playGuideData, PlayGuideStatus status)
	{
		PlayGuideData = playGuideData;
		Status = status;
	}
}
