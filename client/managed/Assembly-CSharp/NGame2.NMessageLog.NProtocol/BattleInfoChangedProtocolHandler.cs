using NGame2.NContentsInfo;
using NGame2.NUI.NManager;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class BattleInfoChangedProtocolHandler : MessageProtocolHandler<BattleInfoChanged>
{
	public override void Run(long senderId, BattleInfoChanged protocolData)
	{
		PlayerBattleInfo battleInfo = protocolData.BattleInfo;
		if (battleInfo != null)
		{
			SingletonAuto<MatchArena>.instance.ApplyBattleInfo(MatchPvpType.Normal, battleInfo);
			if (SceneAutoBehaviour<MatchManagement>.isValidInstance)
			{
				SceneAutoBehaviour<MatchManagement>.instance.UpdateMyBattleInfoIfSame(MatchPvpType.Normal);
			}
		}
	}
}
