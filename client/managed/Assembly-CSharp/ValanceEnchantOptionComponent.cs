using System.Collections.Generic;
using NGame2.NUI;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

public class ValanceEnchantOptionComponent : MonoBehaviour, InitOnCreate
{
	public interface IListener
	{
		void OnClickEnchantConfirm(bool isNew);
	}

	public enum EnchantPanelState
	{
		Hide,
		Show,
		ShowEmpty,
		Out,
		In,
		ShowChance,
		NewOption
	}

	public Animator anim;

	public Transform optionSlotRoot;

	public UIButton optionConfirmButton;

	public UILabel titleLabel;

	public UIGrid grid;

	public ValanceEnchantOptionSlot template;

	private bool isNew;

	private IListener _listener;

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceEnchantOptionComponent";

	public static PrefabAuto<ValanceEnchantOptionComponent> PrefabAuto = new PrefabAuto<ValanceEnchantOptionComponent>("Resources/Prefab2/Window/Valance/ValanceEnchantOptionComponent");

	private List<ValanceEnchantOptionSlot> slotItems = new List<ValanceEnchantOptionSlot>();

	public void OnCreate()
	{
	}

	public void OnRelease()
	{
		Clear();
	}

	public void Open(bool isNewOption, IListener listener)
	{
		isNew = isNewOption;
		_listener = listener;
		string stringDirect = LocalizationManager.GetStringDirect(isNewOption ? "VALANCE_ENCHANT_CHANGE_OPTION_TITLE" : "VALANCE_ENCHANT_APPICATION_OPTION_TITLE");
		UIHelper.SetLabelText(titleLabel, stringDirect);
		HideConfirmButton();
	}

	public void HideConfirmButton()
	{
		UIHelper.EnableGameObject(optionConfirmButton, enable: false);
	}

	public void DisableConfirmButton()
	{
		UIHelper.EnableButton(optionConfirmButton, enable: false);
	}

	public void ShowConfirmButton()
	{
		UIHelper.EnableButton(optionConfirmButton, enable: true);
		UIHelper.EnableGameObject(optionConfirmButton, enable: true);
	}

	public void ConfirmEnchantOption()
	{
		_listener.OnClickEnchantConfirm(isNew);
	}

	public void ShowNewOptionList(ValanceEnchantOptionInfo info)
	{
		Clear();
		SetPanelState(EnchantPanelState.NewOption);
		AddOptionSlot(info.EnchantOptionIndex1, info.EnchantOptionStep1);
		AddOptionSlot(info.EnchantOptionIndex2, info.EnchantOptionStep2);
		AddOptionSlot(info.EnchantOptionIndex3, info.EnchantOptionStep3);
		ShowConfirmButton();
		Reposition();
	}

	public void ShowCurrentOptionList(EquipItemInfo info, bool skipAnimation = true)
	{
		Clear();
		if (info.EnchantOptionIndex1 == 0)
		{
			SetPanelState(EnchantPanelState.ShowEmpty);
		}
		else
		{
			SetPanelState(EnchantPanelState.Show, skipAnimation);
			AddOptionSlot(info.EnchantOptionIndex1, info.EnchantOptionStep1);
			AddOptionSlot(info.EnchantOptionIndex2, info.EnchantOptionStep2);
			AddOptionSlot(info.EnchantOptionIndex3, info.EnchantOptionStep3);
		}
		HideConfirmButton();
		Reposition();
	}

	internal void ShowOptionChanceList(int min, int max)
	{
		Clear();
		for (int i = 1; i <= 3; i++)
		{
			if (i <= min)
			{
				string stringDirect = LocalizationManager.GetStringDirect("VALANCE_ENCHANT_OPTIONSLOT_FIX");
				AddOptionSlot(stringDirect, ValanceEnchantOptionSlot.ValanceEnchantSlotState.Fix);
			}
			else if (i <= max)
			{
				string stringDirect2 = LocalizationManager.GetStringDirect("VALANCE_ENCHANT_OPTIONSLOT_PROBABILITY");
				AddOptionSlot(stringDirect2, ValanceEnchantOptionSlot.ValanceEnchantSlotState.Preview);
			}
			else
			{
				AddOptionSlot("", ValanceEnchantOptionSlot.ValanceEnchantSlotState.None);
			}
		}
		HideConfirmButton();
		Reposition();
	}

	public void SetPanelState(EnchantPanelState state, bool changeAnimation = true)
	{
		if (changeAnimation)
		{
			anim.SetTrigger("play");
		}
		anim.SetInteger("state", (int)state);
	}

	private void AddOptionSlot(string str, ValanceEnchantOptionSlot.ValanceEnchantSlotState state)
	{
		ValanceEnchantOptionSlot valanceEnchantOptionSlot = AddNewSlot();
		if (valanceEnchantOptionSlot != null)
		{
			valanceEnchantOptionSlot.Init(str, state);
		}
	}

	private void AddOptionSlot(int index, int step)
	{
		ValanceEnchantOptionSlot valanceEnchantOptionSlot = AddNewSlot();
		if (valanceEnchantOptionSlot != null)
		{
			InitEnchantOptionDesc(valanceEnchantOptionSlot, index, step);
		}
	}

	private void InitEnchantOptionDesc(ValanceEnchantOptionSlot item, int optionIndex, int optionStep)
	{
		if (optionIndex == 0)
		{
			item.Init("", ValanceEnchantOptionSlot.ValanceEnchantSlotState.None);
			return;
		}
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			VespaDebug.LogError("Option data not found : " + optionIndex);
			return;
		}
		OperationType optionType = data.OptionType;
		string enumString = GameRoot.LocalizeProxy.GetEnumString(optionType);
		string statDisplayValue = StatData.GetStatDisplayValue(optionType, data.GetOptionValue(0, optionStep));
		item.Init(enumString, statDisplayValue, ValanceEnchantOptionSlot.ValanceEnchantSlotState.Active);
	}

	private ValanceEnchantOptionSlot AddNewSlot()
	{
		if (template == null)
		{
			return null;
		}
		GameObject gameObject = NGUITools.AddChild(grid.gameObject, template.gameObject);
		if (gameObject == null)
		{
			return null;
		}
		ValanceEnchantOptionSlot component = gameObject.GetComponent<ValanceEnchantOptionSlot>();
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(component.transform);
		component.gameObject.SetActive(value: true);
		slotItems.Add(component);
		return component;
	}

	public void Reposition()
	{
		if (grid != null)
		{
			grid.Reposition();
		}
	}

	public void Clear()
	{
		foreach (ValanceEnchantOptionSlot slotItem in slotItems)
		{
			UIHelper.EnableGameObject(slotItem, enable: false);
			VespaObject.DestroyGo(slotItem);
		}
		slotItems.Clear();
	}
}
