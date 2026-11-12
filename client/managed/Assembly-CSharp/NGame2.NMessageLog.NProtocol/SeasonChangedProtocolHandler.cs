using NGame2.NContentsInfo;
using NGame2.NUI.NManager;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class SeasonChangedProtocolHandler : MessageProtocolHandler<SeasonChanged>
{
	public override void Run(long senderId, SeasonChanged protocolData)
	{
		MatchSeasonData seasonData = protocolData.SeasonData;
		if (seasonData != null)
		{
			MatchPvpType pvpType = GetPvpType(protocolData);
			SingletonAuto<MatchArena>.instance.ApplySeasonData(pvpType, seasonData);
			if (SceneAutoBehaviour<MatchManagement>.isValidInstance)
			{
				SceneAutoBehaviour<MatchManagement>.instance.UpdateSeasonDataIfSame(pvpType);
				SceneAutoBehaviour<MatchManagement>.instance.RefreshRankers();
			}
		}
	}

	private static MatchPvpType GetPvpType(SeasonChanged protocolData)
	{
		if (protocolData != null && protocolData.SeasonData != null && protocolData.SeasonData.ArenaType == MatchArenaType.BanPick)
		{
			return MatchPvpType.BanPick;
		}
		return MatchPvpType.Normal;
	}
}
