using System;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RuneSelectItemFilterMenuComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickFilter(Enum filterType, bool isFilter);
	}

	public const string PrefabPath = "Prefab2/Component/Inventory/RuneSelectItemFilterMenuComponent";

	public static PrefabAuto<RuneSelectItemFilterMenuComponent> PrefabAuto = new PrefabAuto<RuneSelectItemFilterMenuComponent>("Prefab2/Component/Inventory/RuneSelectItemFilterMenuComponent");

	public UIButton Button;

	public UIWidget ButtonWidget;

	public UILabel LbFilterType;

	public GameObject GoSelected;

	public UILabel LbSelectedFilterType;

	private Listener _listener;

	private bool _isFilter;

	private bool _enableButton;

	public Enum FilterType { get; private set; }

	public void Open(Listener listenr, Enum filterType, bool isFilter, bool enableButton = true)
	{
		_listener = listenr;
		FilterType = filterType;
		_enableButton = enableButton;
		SetFilter(isFilter);
		LocalizeFilter(filterType);
		UIHelper.EnableButton(Button, _enableButton);
		UIHelper.SetWidgetGrayScale(ButtonWidget, !_enableButton && !isFilter);
	}

	public void SetFilter(bool isFilter)
	{
		_isFilter = isFilter;
		UIHelper.EnableGameObject(GoSelected, _isFilter);
		UIHelper.EnableGameObject(LbFilterType, !_isFilter);
	}

	private void LocalizeFilter(Enum localizeKey)
	{
		UIHelper.SetLabelText(LbFilterType, LocalizationManager.GetString(localizeKey));
		UIHelper.SetLabelText(LbSelectedFilterType, LocalizationManager.GetString(localizeKey));
	}

	public void OnClickFilter()
	{
		if (_enableButton)
		{
			SetFilter(!_isFilter);
			if (_listener != null)
			{
				_listener.OnClickFilter(FilterType, _isFilter);
			}
		}
	}
}
