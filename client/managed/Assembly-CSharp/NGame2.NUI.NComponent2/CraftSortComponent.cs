using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class CraftSortComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(CraftSortData sortData, bool onHead);
	}

	public const string PrefabPath = "Prefab2/Component/Inventory/CraftSortComponent";

	public static PrefabAuto<CraftSortComponent> PrefabAuto = new PrefabAuto<CraftSortComponent>("Prefab2/Component/Inventory/CraftSortComponent");

	public UISpriteSelector BackGroundSelector;

	public UILabel ComponentLabel;

	public UIWidget StarWidget;

	public UILabel StarLabel;

	private Listener _listener;

	private bool _onHead;

	public CraftSortData SortData { get; private set; }

	public void Open(Listener listener, CraftSortData sortData, bool onHead = false)
	{
		SortData = sortData;
		_listener = listener;
		_onHead = onHead;
		Init();
	}

	private void Init()
	{
		UIHelper.EnableGameObject(ComponentLabel, enable: true);
		UIHelper.EnableGameObject(StarWidget, enable: false);
		BackGroundSelector.SelectInt(SortData.BackGroundIndex);
		switch (SortData.Category)
		{
		case CraftSortCategory.Tier:
			if (SortData.Value == 0)
			{
				UIHelper.SetLabelText(ComponentLabel, LocalizationManager.GetStringDirect(SortData.StringKey));
			}
			else
			{
				UIHelper.SetLabelText(ComponentLabel, SortData.Value + LocalizationManager.GetStringDirect("PVP_TIER"));
			}
			break;
		case CraftSortCategory.EquipPartType:
			UIHelper.SetLabelText(ComponentLabel, LocalizationManager.GetStringDirect(SortData.StringKey));
			break;
		}
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(SortData, _onHead);
		}
	}

	public int GetValue()
	{
		return SortData.Value;
	}
}
