using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class CountrySelect : BaseWindow, CountryListComponent.Listener
{
	public interface Listener
	{
		void OnSelect(CountrySelect caller, CountryData countryData);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/CountrySelect";

	public UIScrollGrid CountryGrid;

	private Listener _listener;

	private CountryData[] _countryDatas;

	private List<CountryListComponent> _componentList = new List<CountryListComponent>();

	public void Open(Listener listener, CountryData[] countryDatas)
	{
		base.Open();
		_listener = listener;
		_countryDatas = countryDatas;
		InitCountryList();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_componentList);
		_listener = null;
	}

	private void InitCountryList()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_countryDatas); i++)
		{
			CountryData countryData = _countryDatas[i];
			CountryListComponent countryListComponent = UIHelper.AddGridItem(CountryGrid, CountryListComponent.PrefabAuto);
			if (!(countryListComponent == null))
			{
				countryListComponent.Open(this, countryData);
				_componentList.Add(countryListComponent);
			}
		}
		UIHelper.RepositionGrid(CountryGrid);
	}

	void CountryListComponent.Listener.OnSelectCountry(CountryData countryData)
	{
		if (countryData != null && _listener != null)
		{
			_listener.OnSelect(this, countryData);
		}
	}
}
