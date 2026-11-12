using NGameAsset;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class CountryListComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnSelectCountry(CountryData seletData);
	}

	public const string PrefabPath = "Prefab2/Component/Login/CountryListComponent";

	public static PrefabAuto<CountryListComponent> PrefabAuto = new PrefabAuto<CountryListComponent>("Prefab2/Component/Login/CountryListComponent");

	public UITexture FlagTexture;

	public UILabel NameLabel;

	private Listener _listener;

	private CountryData _countryData;

	public void Open(Listener listener, CountryData countryData)
	{
		if (countryData != null)
		{
			_listener = listener;
			_countryData = countryData;
			AssetTexture countryFlagIcon = UIHelper.GetCountryFlagIcon(countryData.Code);
			UIHelper.ResetPlaneAssetTextureAsync(this, FlagTexture, countryFlagIcon);
			UIHelper.SetLabelText(NameLabel, countryData.Name);
		}
	}

	public void OnSelectCountry()
	{
		if (_listener != null)
		{
			_listener.OnSelectCountry(_countryData);
		}
	}
}
