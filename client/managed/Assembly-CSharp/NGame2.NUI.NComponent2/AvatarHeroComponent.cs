using NGameAsset;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class AvatarHeroComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(int heroIndex);
	}

	public const string PrefabPath = "Prefab2/Component/GameOption/AvatarHeroComponent";

	public static PrefabAuto<AvatarHeroComponent> PrefabAuto = new PrefabAuto<AvatarHeroComponent>("Prefab2/Component/GameOption/AvatarHeroComponent");

	public UITexture Texture_Hero;

	private Listener _listener;

	private int _avatarHeroIndex;

	public int HeroIndex { get; private set; }

	public void Open(int avatarHeroIndex, int heroIndex, Listener listener)
	{
		_avatarHeroIndex = avatarHeroIndex;
		HeroIndex = heroIndex;
		_listener = listener;
		SetHeroIcon();
	}

	private void SetHeroIcon()
	{
		string iconPath = UIAvatarHelper.GetIconPath(_avatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Hero, plainTexture);
		}
	}

	public void OnClickHero()
	{
		if (_listener != null)
		{
			_listener.OnClick(_avatarHeroIndex);
		}
	}
}
