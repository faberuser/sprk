using NGame2.NContentsDefine;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NBattle;

public class PvpResultScore : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PvpResultScore";

	public PvpResultScoreInfo ScoreInfo;

	public static PvpResultScore Open(int matchScore, int matchScoreDiff, VisualTierRank visualTierRank, int rankDiff, int tierIndex, MatchArenaType arenaType)
	{
		WindowPrefab<PvpResultScore> windowPrefab = new WindowPrefab<PvpResultScore>("Resources/Prefab2/Window/Battle/PvpResultScore", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(matchScore, matchScoreDiff, visualTierRank, rankDiff, tierIndex, arenaType);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	private void OpenInternal(int matchScore, int matchScoreDiff, VisualTierRank visualTierRank, int rankDiff, int tierIndex, MatchArenaType arenaType)
	{
		base.Open();
		ScoreInfo.Open(matchScore, matchScoreDiff, visualTierRank, rankDiff, tierIndex, arenaType);
	}

	public override void Reset()
	{
		base.Reset();
		ScoreInfo.Reset();
	}
}
