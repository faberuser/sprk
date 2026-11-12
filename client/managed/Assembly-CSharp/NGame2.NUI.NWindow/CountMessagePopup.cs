using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class CountMessagePopup : BaseWindow
{
	public enum PriceType
	{
		Gold,
		Gem
	}

	[Serializable]
	public class PriceIcon
	{
		public ShopCostType Type;

		public Transform Icon;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/CountMessagePopup";

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIInput NumberInput;

	public UILabel ActionLabel;

	public UIButton MinusButton;

	public UIButton PlusButton;

	public Transform PriceRoot;

	public PriceIcon[] PriceIcons;

	public UILabel PriceLabel;

	private int _min;

	private long _max;

	private Action<int> _onYes;

	private Action _onNo;

	private long _currentCount;

	private int _pricePerItem;

	public static CountMessagePopup Open(string title, string message, string action, int min, long max, int current, Action<int> onYes, Action onNo = null)
	{
		WindowPrefab<CountMessagePopup> windowPrefab = new WindowPrefab<CountMessagePopup>("Resources/Prefab2/Window/MessagePopup/CountMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, action, min, max, current, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected void OpenInternal(string title, string message, string action, int min, long max, int current, Action<int> onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		_min = min;
		_max = max;
		_currentCount = current;
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		UIHelper.SetLabelText(ActionLabel, action);
		UIHelper.EnableGameObject(PriceRoot, enable: false);
		if ((bool)NumberInput)
		{
			NumberInput.selectAllTextOnFocus = true;
		}
		SetNumber(current);
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

	public void SetSinglePrice(ShopCostType costType, int price)
	{
		_pricePerItem = price;
		UIHelper.EnableGameObject(PriceRoot, enable: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PriceIcons); i++)
		{
			UIHelper.EnableGameObject(PriceIcons[i].Icon, PriceIcons[i].Type == costType);
		}
		UpdatePrice();
	}

	private void UpdatePrice()
	{
		UIHelper.SetLabelText(PriceLabel, Vespa_Long_Extension_Method.ToCommaString(checked(_pricePerItem * _currentCount)));
		UIButton minusButton = MinusButton;
		long currentCount = _currentCount;
		int min = _min;
		UIHelper.EnableButton(minusButton, currentCount > min);
		UIHelper.EnableButton(PlusButton, _currentCount < _max);
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
		UpdatePrice();
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
