using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class LogoutProtocolHandler : MessageProtocolHandler<Logout>
{
	public override void Run(long senderId, Logout protocolData)
	{
		FriendUserInfo friendInfo = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfo(senderId);
		if (friendInfo != null && friendInfo.IsOnline)
		{
			string lastOnlineTime = (friendInfo.LastLogoutTime = Vespa_DateTime_Extension_Method.ToNetworkString2(Common.GetUTCNow()));
			friendInfo.LastOnlineTime = lastOnlineTime;
			friendInfo.IsOnline = false;
			if (SceneAutoBehaviour<MyFriendManagement>.isValidInstance)
			{
				SceneAutoBehaviour<MyFriendManagement>.instance.UpdateOnlineStatus(senderId);
			}
		}
	}
}
