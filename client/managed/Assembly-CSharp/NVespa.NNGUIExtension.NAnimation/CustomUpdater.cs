using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class CustomUpdater : BaseUpdater<CustomData>
{
	private List<ICustomAnimationAction> _customList = new List<ICustomAnimationAction>();

	private ICustomAnimationAction _custom;

	public override bool Setup(CustomData data, GameObject[] objs, float value, bool isReverse)
	{
		if (!base.Setup(data, objs, value, isReverse))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(objs); i++)
		{
			GameObject gameObject = objs[i];
			if (!(gameObject == null))
			{
				_custom = gameObject.GetComponent<ICustomAnimationAction>();
				if (_custom != null)
				{
					_custom.Setup();
					_customList.Add(_custom);
				}
			}
		}
		return true;
	}

	public override void OnUpdate(float delta)
	{
		if (CanUpdateAnimation(delta))
		{
			if (IsEnd())
			{
				SetEnd();
			}
			else
			{
				Update(delta);
			}
		}
	}

	private void Update(float delta)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_customList); i++)
		{
			_custom = _customList[i];
			if (_custom != null && !_custom.IsEnd())
			{
				_custom.Update(delta);
			}
		}
	}

	public override bool IsEnd()
	{
		bool flag = base.IsEnd();
		bool flag2 = true;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_customList); i++)
		{
			_custom = _customList[i];
			if (_custom != null && !_custom.IsEnd())
			{
				flag2 = false;
				break;
			}
		}
		return flag || flag2;
	}

	public override void SetEnd()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_customList); i++)
		{
			_custom = _customList[i];
			if (_custom != null)
			{
				_custom.SetEnd();
			}
		}
	}

	public override void Release()
	{
		SetEnd();
		base.Release();
		_custom = null;
		_customList.Clear();
	}
}
