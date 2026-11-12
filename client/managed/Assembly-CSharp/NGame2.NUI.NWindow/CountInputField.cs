using UnityEngine;

namespace NGame2.NUI.NWindow;

public class CountInputField : MonoBehaviour
{
	public interface IListener
	{
		void ChangeCount(bool isMax);
	}

	public class CountInputFieldInfo
	{
		public int InitCount { get; set; }

		public long MaxCount { get; set; }

		public long InitPrice { get; set; }
	}

	public UIInput InputField;

	public UIButton PlusButton;

	public UIButton MinusButton;

	public UIButton MaxButton;

	public RequirePrice RequirePrice;

	private IListener _iListener;

	private CountInputFieldInfo _countInputFieldInfo;

	private int _increaseValue = 1;

	private bool _isClickButton;

	public long CurrentCount { get; private set; }

	public long PrevCount { get; private set; }

	public void Open(IListener iListener, CountInputFieldInfo info, int increaseValue = 1)
	{
		if (info != null)
		{
			Init(iListener, info, increaseValue);
		}
	}

	private void Init(IListener iListener, CountInputFieldInfo info, int increaseValue)
	{
		_iListener = iListener;
		_countInputFieldInfo = info;
		_increaseValue = increaseValue;
		int initCount = info.InitCount;
		CurrentCount = initCount;
		InputField.value = CurrentCount.ToString();
		ValidateNumber();
	}

	private void SetPlusButtonState()
	{
		bool enable = _countInputFieldInfo.MaxCount > CurrentCount;
		EnablePlusButton(enable);
	}

	private void SetMinusButtonState()
	{
		int initCount = _countInputFieldInfo.InitCount;
		long currentCount = CurrentCount;
		int num = initCount;
		bool enable = num < currentCount;
		UIHelper.EnableButton(MinusButton, enable);
	}

	private void SetMaxButtonState()
	{
		bool isEnabled = PlusButton.isEnabled;
		EnableMaxButton(isEnabled);
	}

	private void SetCurrentCountInputField()
	{
		if (InputField != null)
		{
			InputField.value = CurrentCount.ToString();
		}
	}

	private void SetButtonStatus()
	{
		SetPlusButtonState();
		SetMinusButtonState();
		SetMaxButtonState();
	}

	private void EnablePlusButton(bool enable)
	{
		UIHelper.EnableButton(PlusButton, enable);
	}

	private void EnableMaxButton(bool enable)
	{
		UIHelper.EnableButton(MaxButton, enable);
	}

	public void SetStateByCurrentCount(int count)
	{
		CurrentCount = count;
		SetCurrentCountInputField();
		SetPrice(checked(_countInputFieldInfo.InitPrice * CurrentCount));
		SetButtonStatus();
	}

	public void SetPrice(long price)
	{
		if (RequirePrice != null)
		{
			RequirePrice.Open(price, 0L);
		}
	}

	public void SetLabelColor(Color color)
	{
		if (RequirePrice != null)
		{
			RequirePrice.SetLabelColor(color);
		}
	}

	private long GetCalcCurrentCount(long count)
	{
		int initCount = _countInputFieldInfo.InitCount;
		long maxCount = _countInputFieldInfo.MaxCount;
		int min = initCount;
		int num = checked(Mathf.Clamp((int)count, min, (int)maxCount));
		return num;
	}

	private void OnChangeCount(bool isMax = false)
	{
		if (_iListener != null)
		{
			_iListener.ChangeCount(isMax);
		}
	}

	public int GetNumber()
	{
		int result = 0;
		string value = InputField.value;
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

	private void SetNumber(long Number)
	{
		PrevCount = CurrentCount;
		CurrentCount = GetCalcCurrentCount(Number);
		SetCurrentCountInputField();
		SetPrice(checked(_countInputFieldInfo.InitPrice * CurrentCount));
		SetButtonStatus();
	}

	public void OnSubmit()
	{
		ValidateNumber();
		OnChangeCount(CurrentCount == _countInputFieldInfo.MaxCount);
	}

	public void OnNumberChanged()
	{
		if (String_Extension_Method.IsNullOrEmpty(InputField.value))
		{
			int num = 0;
			CurrentCount = num;
		}
		else if (_isClickButton)
		{
			_isClickButton = false;
		}
		else
		{
			ValidateNumber();
			OnChangeCount(CurrentCount == _countInputFieldInfo.MaxCount);
		}
	}

	public void OnClickMax()
	{
		_isClickButton = true;
		SetNumber(_countInputFieldInfo.MaxCount);
		OnChangeCount(isMax: true);
	}

	public void OnClickPlus()
	{
		_isClickButton = true;
		int number = GetNumber();
		int num = 1;
		int num2 = number;
		checked
		{
			SetNumber(unchecked((long)num2) + unchecked((long)num));
			OnChangeCount();
		}
	}

	public void OnClickMinus()
	{
		_isClickButton = true;
		int number = GetNumber();
		int num = 1;
		int num2 = number;
		checked
		{
			SetNumber(unchecked((long)num2) - unchecked((long)num));
			OnChangeCount();
		}
	}
}
