using NGame2.NAccount;
using NGame2.NSocket;
using NShared.NMessage;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class RemoveFriendProtocolHandler : MessageProtocolHandler<RemoveFriend>
{
	public override void Run(long senderId, RemoveFriend protocolData)
	{
		UserFriendManager friendManager = GlobalAutoBehaviour<AccountManager>.instance.FriendManager;
		friendManager.RemoveFriend(senderId);
		long[] friendIds = friendManager.GetFriendIds();
		GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.Send(new ChangeFriendReq
		{
			FriendIds = friendIds
		});
	}
}
