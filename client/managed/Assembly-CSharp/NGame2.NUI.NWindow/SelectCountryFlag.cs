using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SelectCountryFlag : BaseWindow, CountryFlagComponent.Listener
{
	public interface Listener
	{
		void OnConfirmCountryFlag(string countryCode);
	}

	public const string PrefabPath = "Prefab2/Window/GameOption/SelectCountryFlag";

	public UIButton Button_Confirm;

	public UIScrollGrid CountryFlagGrid;

	public int ComponentOffset;

	private List<CountryFlagComponent> _countryFlagComponentList = new List<CountryFlagComponent>();

	private Listener _listener;

	private string _selectedCountryCode = string.Empty;

	private CountryFlagComponent _selectedCountryFlagComponent;

	[SerializeField]
	protected SelectFilter Filter_CountryGroup;

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_countryFlagComponentList);
	}

	public void Open(string countryCode, Listener listener)
	{
		Open();
		_listener = listener;
		_selectedCountryCode = countryCode;
		if (String_Extension_Method.IsNullOrEmpty(_selectedCountryCode))
		{
			_selectedCountryCode = "VESPA";
		}
		if (Filter_CountryGroup != null)
		{
			Filter_CountryGroup.InitFilter(SelectFilter.SelectType.SELECT_REGION, AddCountryFlagByFilter, null, useSubFilter: false);
		}
		else
		{
			UIHelper.EnableGameObject(Filter_CountryGroup, enable: false);
		}
	}

	private void AddCountryFlagByFilter()
	{
		FactoryBase.ReleaseAndClearList(_countryFlagComponentList);
		CountryCodeDataContainer countryCodeDataContainer = GameRoot.Instance.CountryCodeDataContainer;
		if (countryCodeDataContainer == null)
		{
			return;
		}
		foreach (CountryCodeData allDatum in countryCodeDataContainer.GetAllData())
		{
			if (Filter_CountryGroup.MainComboBoxCurrentKey == SelectFilter.ComboBoxType.ALL || Filter_CountryGroup.MainComboBoxCurrentKey == (SelectFilter.ComboBoxType)allDatum.GroupIndex)
			{
				CountryFlagComponent countryFlagComponent = UIHelper.AddGridItem(CountryFlagGrid, CountryFlagComponent.PrefabAuto);
				countryFlagComponent.Open(allDatum, this);
				countryFlagComponent.SetSelected(_selectedCountryCode);
				_countryFlagComponentList.Add(countryFlagComponent);
				if (_selectedCountryCode == allDatum.Index)
				{
					_selectedCountryFlagComponent = countryFlagComponent;
				}
			}
		}
		UIHelper.RepositionGrid(CountryFlagGrid);
	}

	public void ScrollToSelectedComponent()
	{
		if (_selectedCountryFlagComponent != null)
		{
			CountryFlagGrid.ScrollTo(_selectedCountryFlagComponent, ComponentOffset, 0f);
		}
	}

	void CountryFlagComponent.Listener.OnClickCountryFlag(string countryFlagIndex)
	{
		if (_selectedCountryCode.Equals(countryFlagIndex))
		{
			return;
		}
		_selectedCountryCode = countryFlagIndex;
		foreach (CountryFlagComponent countryFlagComponent in _countryFlagComponentList)
		{
			countryFlagComponent.SetSelected(_selectedCountryCode);
			if (_selectedCountryCode.Equals(countryFlagComponent.CountryCode))
			{
				_selectedCountryFlagComponent = countryFlagComponent;
			}
		}
	}

	public void OnClickConfirm()
	{
		if (_listener != null)
		{
			_listener.OnConfirmCountryFlag(_selectedCountryCode);
			Close();
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}
}
