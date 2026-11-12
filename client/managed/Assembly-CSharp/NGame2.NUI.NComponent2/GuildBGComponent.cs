using NGameAsset;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildBGComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickGuildBGComponent(int index);
	}

	public const string PrefabPath = "Prefab2/Component/Guild/GuildBGComponent";

	public static PrefabAuto<GuildBGComponent> PrefabAuto = new PrefabAuto<GuildBGComponent>("Prefab2/Component/Guild/GuildBGComponent");

	public int _index;

	public UITexture Texture_Logo;

	public UISprite SelectSprite;

	private Listener _listener;

	public bool IsSelected { get; set; }

	public void Open(int index, Listener listener)
	{
		_index = index;
		_listener = listener;
		UIHelper.EnableGameObject(SelectSprite, enable: false);
		SetupLogo();
	}

	private void SetupLogo()
	{
		AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(_index);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Logo, guildBackGround2D);
	}

	public void OnClickGuildLogo()
	{
		if (_listener != null)
		{
			_listener.OnClickGuildBGComponent(_index);
		}
	}

	public void SetSelected(int selectedIdx)
	{
		IsSelected = selectedIdx == _index;
		UIHelper.EnableGameObject(SelectSprite, IsSelected);
	}
}
