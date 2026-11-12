using NGameAsset;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildLogoComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickGuildLogoComponent(int LogoIndex);
	}

	public const string PrefabPath = "Prefab2/Component/Guild/GuildLogoComponent";

	public static PrefabAuto<GuildLogoComponent> PrefabAuto = new PrefabAuto<GuildLogoComponent>("Prefab2/Component/Guild/GuildLogoComponent");

	public int _LogoIndex;

	public UITexture Texture_Logo;

	public UISprite SelectSprite;

	private Listener _listener;

	public bool IsSelected { get; set; }

	public void Open(int LogoIndex, Listener listener)
	{
		_LogoIndex = LogoIndex;
		_listener = listener;
		UIHelper.EnableGameObject(SelectSprite, enable: false);
		SetupLogo();
	}

	private void SetupLogo()
	{
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(_LogoIndex);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Logo, guildLogoIcon);
	}

	public void OnClickGuildLogo()
	{
		if (_listener != null)
		{
			_listener.OnClickGuildLogoComponent(_LogoIndex);
		}
	}

	public void SetSelected(int selectedIdx)
	{
		IsSelected = selectedIdx == _LogoIndex;
		UIHelper.EnableGameObject(SelectSprite, IsSelected);
	}
}
