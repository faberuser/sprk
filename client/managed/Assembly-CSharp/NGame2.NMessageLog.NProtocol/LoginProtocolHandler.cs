using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class LoginProtocolHandler : MessageProtocolHandler<Login>
{
	public override void Run(long senderId, Login protocolData)
	{
		FriendUserInfo friendInfo = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfo(senderId);
		if (friendInfo != null)
		{
			string lastOnlineTime = (friendInfo.LastLoginTime = Vespa_DateTime_Extension_Method.ToNetworkString2(Common.GetUTCNow()));
			friendInfo.LastOnlineTime = lastOnlineTime;
			friendInfo.IsOnline = true;
			if (SceneAutoBehaviour<MyFriendManagement>.isValidInstance)
			{
				SceneAutoBehaviour<MyFriendManagement>.instance.UpdateOnlineStatus(senderId);
			}
		}
	}
}
