using NGame2.NAccount;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class RightAwayProductEventProtocolHandler : MessageProtocolHandler<RightAwayProductEvent>
{
	public override void Run(long senderId, RightAwayProductEvent protocolData)
	{
		if (AccountManager.IsLogin() && GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.Apply(protocolData.ProductInfos, protocolData.RemovedIndices);
		}
	}
}
