using NGame2.NAccount;
using NGame2.NSocket;
using NShared.NMessage;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class RequestFriendProtocolHandler : MessageProtocolHandler<RequestFriend>
{
	public override void Run(long senderId, RequestFriend protocolData)
	{
		UserFriendManager friendManager = GlobalAutoBehaviour<AccountManager>.instance.FriendManager;
		protocolData.InvitorInfo.IsOnline = true;
		friendManager.AddInvitor(protocolData.InvitorInfo);
		long accountId = protocolData.InvitorInfo.AccountId;
		if (friendManager.GetFriendInfo(accountId) != null)
		{
			long[] friendIds = friendManager.GetFriendIds();
			GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.Send(new ChangeFriendReq
			{
				FriendIds = friendIds
			});
			SingletonAuto<MessageLogHandler>.instance.SendToClient(new long[1] { accountId }, new Login());
		}
	}
}
