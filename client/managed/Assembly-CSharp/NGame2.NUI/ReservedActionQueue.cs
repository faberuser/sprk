using System.Collections.Generic;

namespace NGame2.NUI;

public class ReservedActionQueue
{
	private readonly List<IReservedAction> _reservedPopupList = new List<IReservedAction>();

	private IReservedAction _currentAction;

	public void Add(IReservedAction action)
	{
		_reservedPopupList.Add(action);
	}

	public bool IsEmpty()
	{
		if (_reservedPopupList.Count > 0)
		{
			return false;
		}
		if (_currentAction != null && !_currentAction.Ended())
		{
			return false;
		}
		return true;
	}

	public int Count()
	{
		return _reservedPopupList.Count;
	}

	public void Clear()
	{
		_reservedPopupList.Clear();
		_currentAction = null;
	}

	public void Update()
	{
		if (_reservedPopupList.Count != 0)
		{
			if (_currentAction == null)
			{
				_currentAction = _reservedPopupList[0];
			}
			if (_currentAction.Ended())
			{
				IReservedAction currentAction = _reservedPopupList[0];
				_reservedPopupList.RemoveAt(0);
				_currentAction = currentAction;
				_currentAction.Execute();
			}
		}
	}
}
