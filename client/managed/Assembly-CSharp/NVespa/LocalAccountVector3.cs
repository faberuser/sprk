using UnityEngine;

namespace NVespa;

public class LocalAccountVector3
{
	private long _accountId = long.MinValue;

	private string _key;

	private Vector3 _default;

	private LocalStorageVector3 _value;

	public Vector3 Value
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

	public LocalAccountVector3(Vector3 i_default, string i_key)
	{
		_key = i_key;
		_default = i_default;
	}

	private void Init()
	{
		if (_accountId != LocalAccountDefine.AccountId)
		{
			_accountId = LocalAccountDefine.AccountId;
			_value = new LocalStorageVector3(_default, _accountId + _key);
		}
	}
}
