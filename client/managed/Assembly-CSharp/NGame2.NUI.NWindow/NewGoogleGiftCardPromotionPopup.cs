using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;

namespace NGame2.NUI.NWindow;

public class NewGoogleGiftCardPromotionPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/NewGoogleGiftCardPromotionPopup";

	public UISimpleGrid GridItem;

	public UILabel LabelTitle;

	public UILabel LabelReceiveConfirm;

	public UILabel LabelReceiveConfirmWarn;

	private ProductType _productType;

	private List<ItemIconComponent> _promotionItemComponentList = new List<ItemIconComponent>();

	private List<NewPayShopProductInfo> _promotionItemProductInfoList = new List<NewPayShopProductInfo>();
}
