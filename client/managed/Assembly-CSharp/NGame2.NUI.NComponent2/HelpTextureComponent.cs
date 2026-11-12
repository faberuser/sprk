using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HelpTextureComponent : BaseHelpComponent
{
	public const string PrefabPathHorizontal = "Prefab2/Component/Help/HelpTextureHorizontalComponent";

	public static PrefabAuto<HelpTextureComponent> PrefabAutoHorizontal = new PrefabAuto<HelpTextureComponent>("Prefab2/Component/Help/HelpTextureHorizontalComponent");

	public const string PrefabPathVertical = "Prefab2/Component/Help/HelpTextureVerticalComponent";

	public static PrefabAuto<HelpTextureComponent> PrefabAutoVertical = new PrefabAuto<HelpTextureComponent>("Prefab2/Component/Help/HelpTextureVerticalComponent");

	public UIBundleLoader BundleLoader;

	public UITexture Texture;

	public override void Open(HelpListPageElement elementData)
	{
		base.Open(elementData);
		SetTexuture(elementData.TexturePath, elementData.TextureWidth, elementData.TextureHeight);
	}

	public override void Close()
	{
		base.Close();
	}

	private void SetTexuture(string texturePath, int width, int height)
	{
		if (!(Texture == null))
		{
			if (Movement == UIScrollView.Movement.Horizontal)
			{
				Texture.width = width;
			}
			else if (Movement == UIScrollView.Movement.Vertical)
			{
				Texture.height = height;
			}
			UIHelper.SetBundleTexture(BundleLoader, texturePath);
		}
	}
}
