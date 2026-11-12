using NGameAsset;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class CountryFlagComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnClickCountryFlag(string CountryCode);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/GameOption/CountryFlagComponent";

	public static PrefabAuto<CountryFlagComponent> PrefabAuto = new PrefabAuto<CountryFlagComponent>("Resources/Prefab2/Component/GameOption/CountryFlagComponent");

	public UISprite Sprite_Selected;

	public UILabel Label_CountryName;

	public UITexture Texture_CountryFlag;

	private Listener _listener;

	private bool _selected;

	public string CountryCode { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(CountryCodeData codeData, Listener listener)
	{
		_listener = listener;
		_selected = false;
		CountryCode = codeData.Index;
		UIHelper.SetLabelText(Label_CountryName, codeData.CountryName);
		UIHelper.EnableGameObject(Sprite_Selected, _selected);
		SetCountryFlagImage(codeData.IconPath);
	}

	public void SetSelected(string index)
	{
		_selected = CountryCode.Equals(index);
		UIHelper.EnableGameObject(Sprite_Selected, _selected);
	}

	private void SetCountryFlagImage(string path)
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(path);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CountryFlag, plainTexture);
	}

	public void OnClickItem()
	{
		if (_listener != null)
		{
			_listener.OnClickCountryFlag(CountryCode);
		}
	}
}
