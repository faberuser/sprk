using NGame2.NAccount;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class RequestFriendCanceledProtocolHandler : MessageProtocolHandler<RequestFriendCanceled>
{
	public override void Run(long senderId, RequestFriendCanceled protocolData)
	{
		GlobalAutoBehaviour<AccountManager>.instance.FriendManager.RemoveFriend(senderId);
	}
}
