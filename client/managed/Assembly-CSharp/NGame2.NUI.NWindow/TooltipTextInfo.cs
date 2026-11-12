using NGame2.NUtil;

namespace NGame2.NUI.NWindow;

public class TooltipTextInfo
{
	public StringProxy Name = new StringProxy();

	public StringProxy TypeText = new StringProxy();

	public StringProxy Desc = new StringProxy();

	public virtual void Reset()
	{
		Name.Reset();
		TypeText.Reset();
		Desc.Reset();
	}
}
