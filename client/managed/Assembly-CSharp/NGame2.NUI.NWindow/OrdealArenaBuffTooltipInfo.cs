using NGame2.NUtil;

namespace NGame2.NUI.NWindow;

public class OrdealArenaBuffTooltipInfo : TooltipTextInfo
{
	public StringProxy Desc1 = new StringProxy();

	public StringProxy Desc2 = new StringProxy();

	public override void Reset()
	{
		base.Reset();
		Desc1.Reset();
		Desc2.Reset();
	}
}
