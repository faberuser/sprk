using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EquipmentItemFilterMenuComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickFilter(int filterType, bool isFilter);
	}

	public const string PrefabPath = "Prefab2/Component/Inventory/EquipmentItemFilterMenuComponent";

	public const string PrefabPathDetailOption = "Prefab2/Component/Inventory/EquipmentItemDetailOptionComponent";

	public static PrefabAuto<EquipmentItemFilterMenuComponent> PrefabAuto = new PrefabAuto<EquipmentItemFilterMenuComponent>("Prefab2/Component/Inventory/EquipmentItemFilterMenuComponent");

	public static PrefabAuto<EquipmentItemFilterMenuComponent> PrefabAutoDetailOption = new PrefabAuto<EquipmentItemFilterMenuComponent>("Prefab2/Component/Inventory/EquipmentItemDetailOptionComponent");

	[SerializeField]
	private UIButton Button;

	[SerializeField]
	private UIWidget ButtonWidget;

	[SerializeField]
	private UILabel LbFilterType;

	[SerializeField]
	private GameObject GoSelected;

	[SerializeField]
	private UILabel LbSelectedFilterType;

	private Listener _listener;

	private int _filterType;

	private bool _isFilter;

	private bool _enableButton;

	public int FilterType => _filterType;

	public void Open(Listener listenr, int filterType, bool isFilter, string localizeKey, bool enableButton = true)
	{
		_listener = listenr;
		_filterType = filterType;
		_enableButton = enableButton;
		SetFilter(isFilter);
		LocalizeFilter(localizeKey);
		UIHelper.EnableButton(Button, _enableButton);
		UIHelper.SetWidgetGrayScale(ButtonWidget, !_enableButton && !isFilter);
	}

	public void SetFilter(bool isFilter)
	{
		_isFilter = isFilter;
		UIHelper.EnableGameObject(GoSelected, _isFilter);
		UIHelper.EnableGameObject(LbFilterType, !_isFilter);
	}

	private void LocalizeFilter(string localizeKey)
	{
		UIHelper.SetLabelText(LbFilterType, LocalizationManager.GetStringDirect(localizeKey));
		UIHelper.SetLabelText(LbSelectedFilterType, LocalizationManager.GetStringDirect(localizeKey));
	}

	public void OnClickFilter()
	{
		if (_enableButton)
		{
			SetFilter(!_isFilter);
			if (_listener != null)
			{
				_listener.OnClickFilter(_filterType, _isFilter);
			}
		}
	}
}
