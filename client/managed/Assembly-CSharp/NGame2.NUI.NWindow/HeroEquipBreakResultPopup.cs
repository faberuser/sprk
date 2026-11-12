using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHelper;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class HeroEquipBreakResultPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroEquipBreakResultPopup";

	public UIGrid IconParent;

	public UIScrollGrid ItemIconGrid;

	private List<ItemIconComponent> _iconComponentList = new List<ItemIconComponent>();

	protected ItemAnimationContainer _itemAnimationContainer = new ItemAnimationContainer(open3dModelPopup: true);

	public void Open(Dictionary<int, int> itemCountDictionary)
	{
		Open(delegate
		{
			Init(itemCountDictionary);
		}, null);
	}

	public void Init(Dictionary<int, int> itemCountDictionary)
	{
		foreach (KeyValuePair<int, int> item in itemCountDictionary)
		{
			int key = item.Key;
			int value = item.Value;
			long count = value;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(key);
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemIconGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(data);
			itemIconComponent.SetItemCount(count);
			_iconComponentList.Add(itemIconComponent);
			_itemAnimationContainer.Add(itemIconComponent.CreateGetAnimation());
		}
		UIHelper.RepositionGrid(ItemIconGrid);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_itemAnimationContainer.ChainUpdate();
	}

	public override void Reset()
	{
		base.Reset();
		_itemAnimationContainer.Reset();
		FactoryBase.ReleaseAndClearList(_iconComponentList);
	}
}
