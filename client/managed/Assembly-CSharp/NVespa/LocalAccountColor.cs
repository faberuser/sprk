using UnityEngine;

namespace NVespa;

public class LocalAccountColor
{
	private long _accountId = long.MinValue;

	private string _key;

	private Color _default;

	private LocalStorageColor _value;

	public Color Value
	{
		get
		{
			Init();
			return _value.Value;
		}
		set
		{
			Init();
			_value.Value = value;
		}
	}

	public LocalAccountColor(Color i_default, string i_key)
	{
		_key = i_key;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageColor(_default, _accountId + _key);
		}
	}
}
