using System.Collections.Generic;

namespace NGame2.NNodeTree.NRuntime;

public class ItemContainer
{
	private struct KeyItemPair
	{
		public IKey Key;

		public ItemBase Item;

		public KeyItemPair(IKey key, ItemBase item)
		{
			Key = key;
			Item = item;
		}
	}

	private VespaList<KeyItemPair> _itemList = new VespaList<KeyItemPair>(128);

	private Dictionary<IKeyShared, ItemBase> _sharedItemDic;

	public RuntimeInstance Runtime { get; private set; }

	public ItemContainer(RuntimeInstance runtime)
	{
		Runtime = runtime;
	}

	public void ResetAnyway()
	{
		_itemList.Clear();
		if (_sharedItemDic != null)
		{
			_sharedItemDic.Clear();
			_sharedItemDic = null;
		}
	}

	public T CreateItem<T>(IKey key, IVariable variable) where T : ItemBase, new()
	{
		T val = new T();
		val.OpenFromRuntime(Runtime, variable);
		_itemList.Add(new KeyItemPair(key, val));
		if (key is IKeyShared key2)
		{
			if (_sharedItemDic == null)
			{
				_sharedItemDic = new Dictionary<IKeyShared, ItemBase>();
			}
			_sharedItemDic.Add(key2, val);
		}
		return val;
	}

	public T GetSharedItem<T>(IKeyShared key) where T : ItemBase
	{
		if (_sharedItemDic == null)
		{
			return null;
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_sharedItemDic, key, null) as T;
	}

	public void Clear()
	{
		for (int i = 0; i < _itemList.Count; i++)
		{
			KeyItemPair keyItemPair = _itemList[i];
			if (keyItemPair.Key is IKeyFinalize keyFinalize)
			{
				keyFinalize.Finalize(keyItemPair.Item);
			}
			if (keyItemPair.Item != null)
			{
				keyItemPair.Item.CloseFromRuntime();
			}
		}
		ResetAnyway();
	}
}
