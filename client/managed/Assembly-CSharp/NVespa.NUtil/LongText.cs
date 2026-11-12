namespace NVespa.NUtil;

public struct LongText
{
	private const string ZeroText = "0";

	private long _value;

	private bool _isCommaString;

	public long Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				UpdateText();
			}
		}
	}

	public string Text { get; private set; }

	public LongText(bool isCommaString)
	{
		_isCommaString = isCommaString;
		_value = 0L;
		Text = null;
		UpdateText();
	}

	public LongText(bool isCommaString, long value)
	{
		_isCommaString = isCommaString;
		_value = value;
		Text = null;
		UpdateText();
	}

	private void UpdateText()
	{
		if (_value == 0L)
		{
			Text = "0";
		}
		else
		{
			Text = (_isCommaString ? Vespa_Long_Extension_Method.ToCommaString(_value) : _value.ToString());
		}
	}
}
