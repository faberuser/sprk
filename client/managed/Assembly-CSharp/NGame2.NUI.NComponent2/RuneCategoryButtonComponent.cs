using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class RuneCategoryButtonComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnSelectRuneCategory(ItemType type);
	}

	public static string PrefabPath = "Resources/Prefab2/Component/Hero/RuneCategoryButtonComponent";

	public static PrefabAuto<RuneCategoryButtonComponent> PrefabAuto = new PrefabAuto<RuneCategoryButtonComponent>(PrefabPath);

	public UILabel Label_Category;

	private IListener _listener;

	private ItemType _itemType;

	public void Open(IListener listener, ItemType type)
	{
		_listener = listener;
		_itemType = type;
		switch (type)
		{
		case ItemType.Rune:
			UIHelper.SetLabelText(Label_Category, LocalizationManager.GetStringDirect("RUNE_PAGE_SORT_NORMAL"));
			break;
		case ItemType.PunishmentRune:
			UIHelper.SetLabelText(Label_Category, LocalizationManager.GetStringDirect("RUNE_PAGE_SORT_PUNISHMENT"));
			break;
		}
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnSelectRuneCategory(_itemType);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
