using NShared;

namespace NGame2.NEditor.NViewer;

public class UserDefaultViewer<TKey, TValue> : BaseAccountAttrManagerViewer<TKey, TValue> where TValue : IAccountAttrKey<TKey>
{
	public UserDefaultViewer()
	{
	}

	public UserDefaultViewer(AccountAttrManager<TKey, TValue> manager)
	{
		_name = typeof(TValue).Name;
		Set(manager);
	}
}
