using NGame2.NAccount;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class StaminaInfoChangedProtocolHandler : MessageProtocolHandler<StaminaInfoChanged>
{
	public override void Run(long senderId, StaminaInfoChanged protocolData)
	{
		if (AccountManager.IsLogined())
		{
			StaminaType staminaType = protocolData.StaminaType;
			long staminaCount = protocolData.StaminaCount;
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetStamina(staminaType, staminaCount);
		}
	}
}
