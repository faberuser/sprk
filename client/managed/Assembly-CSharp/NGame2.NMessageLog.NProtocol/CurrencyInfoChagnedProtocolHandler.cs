using NGame2.NAccount;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class CurrencyInfoChagnedProtocolHandler : MessageProtocolHandler<CurrencyInfoChanged>
{
	public override void Run(long senderId, CurrencyInfoChanged protocolData)
	{
		if (AccountManager.IsLogined() && protocolData != null && protocolData.CurrencyType != CurrencyType.None)
		{
			CurrencyType currencyType = protocolData.CurrencyType;
			long amount = protocolData.Amount;
			CurrencyResultInfo3 info = new CurrencyResultInfo3
			{
				CurrencyType = currencyType,
				NewValue = amount
			};
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(info);
		}
	}
}
