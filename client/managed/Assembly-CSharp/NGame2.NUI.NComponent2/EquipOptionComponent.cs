using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EquipOptionComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnSelectOption(EquipOptionData optionData);
	}

	public const string PrefabPath = "Prefab2/Component/Stat/EquipOptionComponent";

	public static PrefabAuto<EquipOptionComponent> PrefabAuto = new PrefabAuto<EquipOptionComponent>("Prefab2/Component/Stat/EquipOptionComponent");

	public UISprite Spr_Selected;

	public UISprite Spr_Disabled;

	public UILabel Lbl_Name;

	public UILabel Lbl_Value;

	private int _optionIndex;

	private Listener _listener;

	private long _totalValue;

	private bool _selected;

	private bool _disabled;

	public EquipOptionData EquipOptionData { get; private set; }

	public void Open(EquipOptionData equipOptionData, Listener listener)
	{
		_totalValue = 0L;
		_listener = listener;
		EquipOptionData = equipOptionData;
		SetSelected(selected: false);
		SetDisable(disabled: false);
		int optionValueMin = EquipOptionData.GetOptionValueMin(0);
		int optionValueMax = EquipOptionData.GetOptionValueMax(0);
		string optionDisplay = StatData.GetOptionDisplay(EquipOptionData.OptionType, optionValueMin, optionValueMax, "~");
		UIHelper.SetLabelText(Lbl_Name, optionDisplay);
		UIHelper.SetLabelText(Lbl_Value, string.Empty);
	}

	public void SetSelected(bool selected)
	{
		_selected = selected;
		UIHelper.EnableGameObject(Spr_Selected, _selected);
	}

	public void SetDisable(bool disabled)
	{
		_disabled = disabled;
		UIHelper.EnableGameObject(Spr_Disabled, _disabled);
	}

	public void SetStatColor(Color color)
	{
		Lbl_Name.color = color;
	}

	public void OnSelectOption()
	{
		if (!_disabled && _listener != null)
		{
			_listener.OnSelectOption(EquipOptionData);
		}
	}
}
