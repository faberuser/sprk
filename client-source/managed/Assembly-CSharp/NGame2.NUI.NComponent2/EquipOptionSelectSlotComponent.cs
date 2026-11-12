using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EquipOptionSelectSlotComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnSelectSlot(int _slotIndex);
	}

	private enum SprState
	{
		Normal,
		Select,
		Complete
	}

	public const string PrefabPath = "Resources/Prefab2/Component/ShopItem/EquipOptionSelectSlotComponent";

	public static PrefabAuto<EquipOptionSelectSlotComponent> PrefabAuto = new PrefabAuto<EquipOptionSelectSlotComponent>("Resources/Prefab2/Component/ShopItem/EquipOptionSelectSlotComponent");

	private SprState _sprState;

	public Color DefaultColor = Color.yellow;

	public Color SelectedColor = Color.white;

	public UISprite Spr_Normal;

	public UISprite Spr_Selected;

	public UISprite Spr_Complete;

	public UILabel Lbl_Pre;

	public UILabel Lbl_Option;

	private Listener _listener;

	private bool _completed;

	public int SlotIndex { get; private set; }

	public void Open(int slotIndex, Listener listener)
	{
		SlotIndex = slotIndex;
		_listener = listener;
		_completed = false;
		_sprState = SprState.Normal;
		UIHelper.EnableGameObject(Lbl_Pre, enable: false);
		UIHelper.EnableGameObject(Spr_Normal, enable: false);
		UIHelper.EnableGameObject(Spr_Selected, enable: false);
		UIHelper.EnableGameObject(Spr_Complete, enable: false);
		string selectOption = LocalizationManager.GetString(Equip.EQUIP_OPTION_STAT_TEXT);
		SetSlotLabel(selectOption, DefaultColor);
	}

	private void SetSelectSprite(SprState sprState)
	{
		UIHelper.EnableGameObject(Spr_Normal, sprState == SprState.Normal);
		UIHelper.EnableGameObject(Spr_Selected, sprState == SprState.Select);
		UIHelper.EnableGameObject(Spr_Complete, sprState == SprState.Complete);
	}

	private void SetSlotLabel(string selectOption, Color slotColor)
	{
		UIHelper.SetLabelText(Lbl_Option, selectOption);
		UIHelper.SetLabelColor(Lbl_Option, DefaultColor);
	}

	public void SetSelected(int index)
	{
		_sprState = ((SlotIndex == index) ? SprState.Select : (_completed ? SprState.Complete : SprState.Normal));
		SetSelectSprite(_sprState);
	}

	public void OnOptionSelected(EquipOptionData equipOptionData)
	{
		UIHelper.EnableGameObject(Lbl_Pre, enable: false);
		int optionValueMin = equipOptionData.GetOptionValueMin(0);
		int optionValueMax = equipOptionData.GetOptionValueMax(0);
		string optionDisplay = StatData.GetOptionDisplay(equipOptionData.OptionType, optionValueMin, optionValueMax, "~");
		SetSlotLabel(optionDisplay, SelectedColor);
		_completed = true;
		_sprState = SprState.Complete;
	}

	public void OnOptionChanged(EquipOptionData equipOptionData)
	{
		UIHelper.EnableGameObject(Lbl_Pre, enable: false);
		int optionValueMin = equipOptionData.GetOptionValueMin(0);
		int optionValueMax = equipOptionData.GetOptionValueMax(0);
		string optionDisplay = StatData.GetOptionDisplay(equipOptionData.OptionType, optionValueMin, optionValueMax, "~");
		SetSlotLabel(optionDisplay, SelectedColor);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	public void Close()
	{
		Reset();
	}

	private void Reset()
	{
	}

	public void OnClickItem()
	{
		if (_listener != null)
		{
			_listener.OnSelectSlot(SlotIndex);
		}
	}
}
