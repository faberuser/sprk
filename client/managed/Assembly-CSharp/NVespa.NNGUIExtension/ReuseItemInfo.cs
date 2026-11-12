using NVespa.NPrefab2;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public class ReuseItemInfo
{
	private int _index = -1;

	private Transform _transform;

	private IReuseItem _reuseItem;

	private ReuseCalculator _calculator;

	public bool IsValid()
	{
		if (_transform == null)
		{
			return false;
		}
		if (_calculator == null || !_calculator.IsValid())
		{
			return false;
		}
		return true;
	}

	public virtual void Setup(Transform trans, ReuseCalculator calculator)
	{
		if (trans == null || calculator == null)
		{
			Close();
			return;
		}
		_transform = trans;
		_calculator = calculator;
		LinkFactoryComponent linkFactoryComponent = SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.Get(trans);
		if (linkFactoryComponent != null)
		{
			_reuseItem = linkFactoryComponent as IReuseItem;
		}
		else
		{
			_reuseItem = null;
		}
	}

	public int GetItemIndex()
	{
		return _index;
	}

	public void SetIndex(int index)
	{
		if (IsValid() && _index != index)
		{
			_index = index;
			ChangePosition();
			if (_reuseItem != null)
			{
				_reuseItem.SetReuseItemIndex(index);
			}
		}
	}

	private void ChangePosition()
	{
		_transform.localPosition = _calculator.GetPositionByIndex(_index);
	}

	private void Close()
	{
		_index = -1;
		_reuseItem = null;
		_calculator = null;
	}
}
