using System.Collections.Generic;

namespace NGame2.NNodeTree.NRuntime;

public class ExecuteContainer
{
	public class ExecuteData
	{
		public IExecute Execute;

		public ItemBase Item;

		public bool IsWeak;

		public ExecuteData(IExecute execute, ItemBase item, bool isWeak)
		{
			Execute = execute;
			Item = item;
			IsWeak = isWeak;
		}
	}

	private List<ExecuteData> _executeList = new List<ExecuteData>(128);

	private List<ExecuteData> _pendingList = new List<ExecuteData>(128);

	public RuntimeInstance Runtime { get; private set; }

	public ExecuteContainer(RuntimeInstance runtime)
	{
		Runtime = runtime;
	}

	public void ResetAnyway()
	{
		_executeList.Clear();
		_pendingList.Clear();
	}

	public void Pending(IExecute i_execute, ItemBase i_executeItem)
	{
		if (!Runtime.IsStoped)
		{
			_pendingList.Add(new ExecuteData(i_execute, i_executeItem, isWeak: false));
		}
	}

	public void PendingWeak(IExecute i_execute, ItemBase i_executeItem)
	{
		if (!Runtime.IsStoped)
		{
			_pendingList.Add(new ExecuteData(i_execute, i_executeItem, isWeak: true));
		}
	}

	public void Update(RuntimeInstance runtime)
	{
		_executeList.AddRange(_pendingList);
		_pendingList.Clear();
		for (int i = 0; i < _executeList.Count; i++)
		{
			ExecuteData executeData = _executeList[i];
			executeData.Execute.UpdateExecute(executeData.Item);
			if (executeData.Execute.IsExecuteEnd(executeData.Item))
			{
				_executeList.RemoveAt(i);
				i--;
			}
		}
	}

	public void RequestStop(RuntimeInstance runtime, ERequestStop requestStop)
	{
		_executeList.AddRange(_pendingList);
		_pendingList.Clear();
		for (int i = 0; i < _executeList.Count; i++)
		{
			ExecuteData executeData = _executeList[i];
			executeData.Execute.RequestStopExecute(executeData.Item, requestStop);
		}
	}

	public bool IsEnd()
	{
		if (_pendingList.Count > 0)
		{
			return false;
		}
		for (int i = 0; i < _executeList.Count; i++)
		{
			if (!_executeList[i].IsWeak)
			{
				return false;
			}
		}
		return true;
	}
}
