using NGame2.NAccount;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class AvatarChangeNotProtocolHandler : MessageProtocolHandler<AvatarChangeNot>
{
	public override void Run(long senderId, AvatarChangeNot protocolData)
	{
		GlobalAutoBehaviour<AccountManager>.instance.FriendManager.SetAvatarIndex(senderId, protocolData.HeroIndex);
	}
}
