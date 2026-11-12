namespace NGame2.NUI.NComponent2.NItemIconSub;

public abstract class ProxyBase
{
	public ItemIconComponent Owner;

	public abstract bool IsActive();

	public abstract void Release();

	public abstract void OnDisable();

	public abstract void ChainUpdate();

	public abstract void RefreshInfo();

	public abstract void DestroyThis();

	protected static void RearrangeDepth(ItemIconComponent owner, UIWidget subRoot)
	{
		if (owner.WidgetThis.depth != subRoot.depth)
		{
			subRoot.RearrangeWidgetDepth(owner.WidgetThis.depth);
		}
	}
}
