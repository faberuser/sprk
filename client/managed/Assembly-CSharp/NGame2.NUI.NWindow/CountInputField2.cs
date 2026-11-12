using UnityEngine;

namespace NGame2.NUI.NWindow;

public class CountInputField2 : MonoBehaviour
{
	public interface IListener
	{
		void ChangeCount();
	}

	public class CountInputFieldInfo
	{
		public int MinCount { get; set; }

		public long MaxCount { get; set; }

		public long InitCount { get; set; }
	}

	public UIInput InputField;

	public UIButton PlusButton;

	public UIButton MinusButton;

	public UIButton MinButton;

	public UIButton MaxButton;

	private IListener _iListener;

	private CountInputFieldInfo _countInputFieldInfo;

	private int _increaseValue = 1;

	private bool _isClickButton;

	public long CurrentCount { get; private set; }

	public long PrevCount { get; private set; }

	public void Open(IListener iListener, CountInputFieldInfo info)
	{
		if (info != null)
		{
			Init(iListener, info);
		}
	}

	private void Init(IListener iListener, CountInputFieldInfo info)
	{
		_iListener = iListener;
		_countInputFieldInfo = info;
		CurrentCount = info.InitCount;
		InputField.value = CurrentCount.ToString();
		ValidateNumber();
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
		SetMinButtonState();
		SetMaxButtonState();
	}

	private void SetPlusButtonState()
	{
		bool enable = _countInputFieldInfo.MaxCount > CurrentCount;
		EnablePlusButton(enable);
	}

	private void SetMinusButtonState()
	{
		int minCount = _countInputFieldInfo.MinCount;
		long currentCount = CurrentCount;
		int num = minCount;
		bool enable = num < currentCount;
		EnableMinusButton(enable);
	}

	private void SetMinButtonState()
	{
		bool isEnabled = MinusButton.isEnabled;
		EnableMinButton(isEnabled);
	}

	private void SetMaxButtonState()
	{
		bool isEnabled = PlusButton.isEnabled;
		EnableMaxButton(isEnabled);
	}

	private void EnablePlusButton(bool enable)
	{
		UIHelper.EnableButton(PlusButton, enable);
	}

	private void EnableMinusButton(bool enable)
	{
		UIHelper.EnableButton(MinusButton, enable);
	}

	private void EnableMaxButton(bool enable)
	{
		UIHelper.EnableButton(MaxButton, enable);
	}

	private void EnableMinButton(bool enable)
	{
		UIHelper.EnableButton(MinButton, enable);
	}

	private long GetCalcCurrentCount(long count)
	{
		int minCount = _countInputFieldInfo.MinCount;
		long maxCount = _countInputFieldInfo.MaxCount;
		int min = minCount;
		int num = checked(Mathf.Clamp((int)count, min, (int)maxCount));
		return num;
	}

	private void OnChangeCount()
	{
		if (_iListener != null)
		{
			_iListener.ChangeCount();
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
		SetButtonStatus();
		OnChangeCount();
	}

	public void OnSubmit()
	{
		ValidateNumber();
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
		}
	}

	public void OnClickMax()
	{
		_isClickButton = true;
		SetNumber(_countInputFieldInfo.MaxCount);
	}

	public void OnClickMin()
	{
		_isClickButton = true;
		int minCount = _countInputFieldInfo.MinCount;
		SetNumber(minCount);
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
		}
	}
}
