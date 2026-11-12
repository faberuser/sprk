using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RuneBreakCountMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/RuneBreakCountMessagePopup";

	public UISimpleGrid ItemIconGrid;

	public UIInput NumberInput;

	public UIButton Button_Inc;

	public UIButton Button_Dec;

	private int _min;

	private long _max;

	private Action<int> _onYes;

	private Action _onNo;

	private long _currentCount;

	private ItemIconComponent _ItemIconComponent;

	public static RuneBreakCountMessagePopup Open(ItemData itemData, int min, long max, int current, Action<int> onYes, Action onNo = null)
	{
		WindowPrefab<RuneBreakCountMessagePopup> windowPrefab = new WindowPrefab<RuneBreakCountMessagePopup>("Resources/Prefab2/Window/MessagePopup/RuneBreakCountMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(itemData, min, max, current, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected void OpenInternal(ItemData itemData, int min, long max, int current, Action<int> onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		_min = min;
		_max = max;
		_currentCount = current;
		SetItemIcon(itemData);
		SetNumber(current);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _ItemIconComponent);
		_ItemIconComponent = null;
	}

	private void SetItemIcon(ItemData itemData)
	{
		_ItemIconComponent = UIHelper.AddGridItem(ItemIconGrid, ItemIconComponent.PrefabAuto);
		_ItemIconComponent.Open(itemData);
		_ItemIconComponent.SetItemCount(_max);
	}

	public int GetNumber()
	{
		int result = 0;
		string value = NumberInput.value;
		if (!string.IsNullOrEmpty(value))
		{
			int.TryParse(value, out result);
		}
		return result;
	}

	private void ValidateNumber()
	{
		int number = GetNumber();
		long number2 = number;
		SetNumber(number2);
	}

	private void SetNumber(long number)
	{
		int min = _min;
		long max = _max;
		int min2 = min;
		int num = checked(Mathf.Clamp((int)number, min2, (int)max));
		_currentCount = num;
		NumberInput.value = _currentCount.ToString();
		UpdateCountButtonState();
	}

	private void UpdateCountButtonState()
	{
		bool enable = _currentCount < _max;
		long currentCount = _currentCount;
		int min = _min;
		bool enable2 = currentCount > min;
		UIHelper.EnableButton(Button_Inc, enable);
		UIHelper.EnableButton(Button_Dec, enable2);
	}

	public void OnClickYes()
	{
		int number = GetNumber();
		Vespa_System_Action_Extension_Method.fire(_onYes, number);
		_onYes = null;
		Close();
	}

	public void OnClickNo()
	{
		Vespa_System_Action_Extension_Method.fire(_onNo);
		_onNo = null;
		Close();
	}

	public void OnClickPlus()
	{
		int number = GetNumber();
		int num = 1;
		int num2 = number;
		checked
		{
			SetNumber(unchecked((long)num2) + unchecked((long)num));
		}
	}

	public void OnClickMinus()
	{
		int number = GetNumber();
		int num = 1;
		int num2 = number;
		checked
		{
			SetNumber(unchecked((long)num2) - unchecked((long)num));
		}
	}

	public void OnClickMax()
	{
		SetNumber(_max);
	}

	public void OnNumberChanged()
	{
	}

	public void OnNumberSubmit()
	{
		ValidateNumber();
	}

	public override void OnSystemBackButton()
	{
		OnClickNo();
	}
}
