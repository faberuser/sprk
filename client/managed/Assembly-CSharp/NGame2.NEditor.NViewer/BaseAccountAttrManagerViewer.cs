using NShared;

namespace NGame2.NEditor.NViewer;

public class BaseAccountAttrManagerViewer<TKey, TValue> : BaseAbstarctViewer2<TValue> where TValue : IAccountAttrKey<TKey>
{
	protected AccountAttrManager<TKey, TValue> _manager;

	public override void Set(object managerObj)
	{
		_manager = managerObj as AccountAttrManager<TKey, TValue>;
	}

	protected override void Refresh()
	{
		if (_manager != null)
		{
			Initilize();
			InitCachedList();
			SetCachedList(_manager.GetValues());
		}
	}
}
