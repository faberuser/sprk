using NGame2.NUtil;
using NShared;

namespace NGame2.NUI.NWindow;

public class CurrencyTooltipTextInfo : TooltipTextInfo
{
	public CurrencyType CurrencyType;

	public StringProxy Desc1 = new StringProxy();

	public override void Reset()
	{
		base.Reset();
		Desc1.Reset();
	}
}
