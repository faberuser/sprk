using UnityEngine;

namespace NVespa;

public class LocalAccountColorArray
{
	private long _accountId = long.MinValue;

	private string _keyPrefix;

	private Color _default;

	private LocalStorageColorArray _value;

	public Color this[long index]
	{
		get
		{
			Init();
			return _value[index];
		}
		set
		{
			Init();
			_value[index] = value;
		}
	}

	public LocalAccountColorArray(Color i_default, string i_keyPrefix)
	{
		_keyPrefix = i_keyPrefix;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageColorArray(_default, _accountId + _keyPrefix);
		}
	}
}
