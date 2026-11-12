using NGame2.NAccount;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class SentFriendshipPointProtocolHandler : MessageProtocolHandler<SentFriendshipPoint>
{
	public override void Run(long senderId, SentFriendshipPoint protocolData)
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance.FriendManager.GetFriendInfo(senderId) != null)
		{
			instance.SendRecvFriendshipPointManager.AddRecvFriendshipPointInfo(protocolData.SentFriendshipPointInfo);
		}
	}
}
