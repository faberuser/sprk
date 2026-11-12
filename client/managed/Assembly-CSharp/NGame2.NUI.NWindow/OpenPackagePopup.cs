using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class OpenPackagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/OpenPackagePopup";

	public UISimpleGrid PackageItemGrid;

	public UILabel Label_ItemName;

	public UILabel Label_Confirm;

	public UILabel Label_Confirm2;

	public UIWidget WidgetYesOnly;

	public UIWidget WidgetYesWithdrawal;

	private Action _onYes;

	private Action _onNo;

	private Action _onClickWithdrawal;

	private ItemIconComponent _PackageItemComponent;

	public static void Open(ItemData itemData, int itemCount, Action onYes, Action onNo = null, Action onClickWithdrawal = null)
	{
		WindowPrefab<OpenPackagePopup> windowPrefab = new WindowPrefab<OpenPackagePopup>("Resources/Prefab2/Window/MessagePopup/OpenPackagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(itemData, itemCount, onYes, onNo, onClickWithdrawal);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(ItemData itemData, int itemCount, Action onYes, Action onNo, Action onClickWithdrawal)
	{
		base.Open();
		Setup(itemData, itemCount);
		_onYes = onYes;
		_onNo = onNo;
		_onClickWithdrawal = onClickWithdrawal;
		UIHelper.EnableGameObject(WidgetYesOnly, onClickWithdrawal == null);
		UIHelper.EnableGameObject(WidgetYesWithdrawal, onClickWithdrawal != null);
	}

	private void Setup(ItemData itemData, int itemCount)
	{
		FactoryBase.SafeReleaseComponent(ref _PackageItemComponent);
		_PackageItemComponent = UIHelper.AddGridItem(PackageItemGrid, ItemIconComponent.PrefabAuto);
		_PackageItemComponent.Open(itemData);
		_PackageItemComponent.SetItemCount(itemCount);
		UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(itemData));
		string text = ((itemCount > 1) ? LocalizationManager.GetString(Inventory.OPEN_PACKAGE_COUNT, itemCount) : LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_OPEN));
		UIHelper.SetLabelText(Label_Confirm, text);
		UIHelper.SetLabelText(Label_Confirm2, text);
	}

	public void OnClickYes()
	{
		Close();
		Vespa_System_Action_Extension_Method.fire(_onYes);
	}

	public void OnClickNo()
	{
		Close();
		Vespa_System_Action_Extension_Method.fire(_onNo);
	}

	public void OnClickWithdrawal()
	{
		Vespa_System_Action_Extension_Method.fire(_onClickWithdrawal);
	}
}
