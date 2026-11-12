using NGame2.NUI.NWindow.NValance;
using NGame2.NUI.NWindow.NValance.NExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.Nvalance;

public class ValanceFilterComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickEquipFilter(ValanceFilterComponent comp, IValanceEquipFilterInfo info);

		void OnClickCreatureFilter(ValanceFilterComponent comp, IValanceCreatureFilterInfo info);
	}

	public const string PrefabPath = "Prefab2/Component/Valance/ValanceFilterComponent";

	public const string PrefabPath2 = "Prefab2/Component/Valance/ValanceFilterComponent2";

	public static PrefabAuto<ValanceFilterComponent> PrefabAuto = new PrefabAuto<ValanceFilterComponent>("Prefab2/Component/Valance/ValanceFilterComponent");

	public static PrefabAuto<ValanceFilterComponent> PrefabAuto2 = new PrefabAuto<ValanceFilterComponent>("Prefab2/Component/Valance/ValanceFilterComponent2");

	public GameObject GoNotice;

	public UIButton ButtonSelected;

	public UIButton ButtonNoSelected;

	public UILabel LabelName;

	public Color ColorButtonOn;

	public Color ColorButtonOff;

	private IListener _listener;

	private ValanceEquipFilterData _equipInfo;

	private ValanceCreatureFilterData _creatureInfo;

	public EValanceFilterSelected FilterType { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_listener = null;
		Close();
	}

	public void Open(IValanceFilterInfo info, IListener listener)
	{
		if (info == null)
		{
			return;
		}
		ShowNotice(isShow: false);
		if (info is IValanceEquipFilterInfo)
		{
			_equipInfo = info as ValanceEquipFilterData;
			FilterType = _equipInfo.GetFilterType();
			if (_equipInfo.DisplayEquipOptionIndex == 0)
			{
				SetLabelLocalKey(_equipInfo.LocalKey);
			}
			else
			{
				EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(_equipInfo.DisplayEquipOptionIndex);
				if (data == null)
				{
					SetLabel("옵션데이터 없음");
					return;
				}
				OperationType optionType = data.OptionType;
				int optionValue = data.GetOptionValue(0, 0);
				string optionDisplay = StatData.GetOptionDisplay(optionType, optionValue, opened: true, 0, appendColor: false);
				SetLabel(optionDisplay);
			}
		}
		else
		{
			if (!(info is IValanceCreatureFilterInfo))
			{
				return;
			}
			_creatureInfo = info as ValanceCreatureFilterData;
			FilterType = _creatureInfo.GetFilterType();
			SetLabelLocalKey(_creatureInfo.LocalKey);
		}
		_listener = listener;
		SetSelected(isSelect: false);
	}

	public void SetLabelLocalKey(string key)
	{
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.SetLabelText(LabelName, stringDirect);
	}

	private void SetLabel(string text)
	{
		UIHelper.SetLabelText(LabelName, text);
	}

	public void Close()
	{
		_listener = null;
		_equipInfo = null;
		_creatureInfo = null;
		SetSelected(isSelect: false);
		SetEnable(isEnable: true);
		ShowNotice(isShow: false);
	}

	public void SetEnable(bool isEnable)
	{
		UIHelper.SetWidgetGrayScale(ButtonSelected, !isEnable, setColliders: true);
		UIHelper.SetWidgetGrayScale(ButtonNoSelected, !isEnable, setColliders: true);
		UIHelper.SetWidgetGrayScale(LabelName, !isEnable);
		if (!isEnable)
		{
			SetSelected(isSelect: false);
		}
	}

	public void SetSelected(bool isSelect)
	{
		UIHelper.EnableGameObject(ButtonSelected, isSelect);
		UIHelper.EnableGameObject(ButtonNoSelected, !isSelect);
		Color color = (isSelect ? ColorButtonOn : ColorButtonOff);
		UIHelper.SetLabelColor(LabelName, color);
	}

	public void OnClickFilterInfo()
	{
		if (_listener != null)
		{
			if (_equipInfo != null)
			{
				_listener.OnClickEquipFilter(this, _equipInfo);
			}
			if (_creatureInfo != null)
			{
				_listener.OnClickCreatureFilter(this, _creatureInfo);
			}
		}
	}

	public IValanceFilterInfo GetFilterInfo()
	{
		if (_equipInfo != null)
		{
			return _equipInfo;
		}
		if (_creatureInfo != null)
		{
			return _creatureInfo;
		}
		return null;
	}

	public void ShowNotice(bool isShow)
	{
		UIHelper.EnableGameObject(GoNotice, isShow);
	}
}
