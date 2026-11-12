using NGame2.NUI;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

public class EquipFilterOptionComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClickOption(EquipFilterOptionComponent comp, int groupIndex, int filterType, bool value);

		bool IsFilterActive(int groupIndex, int filterType);
	}

	public UISprite SpriteBg;

	public GameObject GameObjectActiveRoot;

	public GameObject GameObjectInactiveRoot;

	public UIWidget[] ResizeTagetWidgetList;

	public UILabel LabelActiveName;

	public UILabel LabelInactiveName;

	private bool _isLocked;

	private bool _isActive;

	private EquipFilterOptionGroupComponent.SelectMode _selectMode;

	private EquipFilterOptionData _data;

	private int _filterType;

	private IListener _listener;

	private UIWidget _inactiveWidget;

	private UILabel _currentStateNameLabel
	{
		get
		{
			if (_isActive)
			{
				return LabelActiveName;
			}
			return LabelInactiveName;
		}
	}

	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			_isActive = value;
			EquipFilterOptionGroupData data = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData(_data.Group);
			if (data != null && _listener != null)
			{
				_listener.OnClickOption(this, data.Index, _filterType, _isActive);
			}
			SetActive(_isActive);
		}
	}

	public bool IsLocked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			_isLocked = value;
			UIHelper.SetWidgetGrayScale(_inactiveWidget, _isLocked);
		}
	}

	public int FilterType => _filterType;

	public void Open(EquipFilterOptionData data, IListener listener, EquipFilterOptionGroupComponent.SelectMode mode = EquipFilterOptionGroupComponent.SelectMode.MultiSelect)
	{
		_data = data;
		_filterType = EquipFilterOptionGroupData.GetFilterType(_data);
		_listener = listener;
		_inactiveWidget = GameObjectInactiveRoot.GetComponent<UIWidget>();
		ChangeMode(mode);
		SetActive(IsActive);
		UpdateUI();
	}

	public void OnClickOption()
	{
		if (!IsLocked && (_selectMode != EquipFilterOptionGroupComponent.SelectMode.SingleSelect || !_isActive))
		{
			IsActive = !IsActive;
		}
	}

	public void UpdateFilterActive()
	{
		if (_listener != null)
		{
			EquipFilterOptionGroupData data = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData(_data.Group);
			if (data != null && _listener != null)
			{
				IsActive = _listener.IsFilterActive(data.Index, _filterType);
			}
		}
	}

	private void SetActive(bool isActive)
	{
		UpdateUI();
		if (GameObjectActiveRoot != null)
		{
			GameObjectActiveRoot.SetActive(isActive);
		}
		if (GameObjectInactiveRoot != null)
		{
			GameObjectInactiveRoot.SetActive(!isActive);
		}
	}

	private void UpdateUI()
	{
		if (_data != null && _currentStateNameLabel != null)
		{
			_currentStateNameLabel.text = LocalizationManager.GetStringDirect(_data.LocalKey);
		}
	}

	private void ChangeMode(EquipFilterOptionGroupComponent.SelectMode mode)
	{
		_selectMode = mode;
		switch (_selectMode)
		{
		case EquipFilterOptionGroupComponent.SelectMode.Disable:
			IsLocked = true;
			break;
		case EquipFilterOptionGroupComponent.SelectMode.SingleSelect:
			IsLocked = false;
			break;
		case EquipFilterOptionGroupComponent.SelectMode.MultiSelect:
			IsLocked = false;
			break;
		}
	}
}
