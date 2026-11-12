using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPortal;

public class PortalContentsStateIconComponent : UIScrollItemBehaviour
{
	public struct Info
	{
		public ContentsStateIcon IconType;
	}

	public const string PrefabPath = "Prefab2/Component/Portal/PortalContentsStateIconComponent";

	public static PrefabAuto<PortalContentsStateIconComponent> PrefabAuto = new PrefabAuto<PortalContentsStateIconComponent>("Prefab2/Component/Portal/PortalContentsStateIconComponent");

	public UISpriteSelector IconSpriteSelector;

	public void Open(Info info)
	{
		Init(info);
	}

	private void Init(Info? info)
	{
		if (info.HasValue)
		{
			SetIcon(info.Value.IconType);
		}
	}

	private void SetIcon(ContentsStateIcon iconType)
	{
		if (!(IconSpriteSelector == null))
		{
			IconSpriteSelector.SelectEnum(iconType);
		}
	}
}
