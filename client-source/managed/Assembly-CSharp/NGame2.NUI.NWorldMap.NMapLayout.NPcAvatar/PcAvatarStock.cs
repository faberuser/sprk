using NGame2.N3D;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public struct PcAvatarStock
{
	public static PcAvatarStock EmptyStock;

	public RenderFacadeBuilder RenderFacadeBulider;

	public BasePcMediator Mediator;

	public BasePcAvatar Avatar;

	public IInteractiveAction InteractionAction;

	public long Uid;

	public string Name;

	public bool IsValid()
	{
		if (RenderFacadeBulider != null && Mediator != null)
		{
			return Avatar != null;
		}
		return false;
	}
}
