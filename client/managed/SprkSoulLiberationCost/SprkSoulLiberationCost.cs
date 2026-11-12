using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkSoulLiberationCost : MonoBehaviour
{
	private SoulWeaponLiberationWindow window;

	private RequirePrice price;

	private float nextRefresh;

	private bool opened;

	public static void Show(SoulWeaponLiberationWindow window)
	{
		SprkSoulLiberationCost sprkSoulLiberationCost = window.LiberationTransform.GetComponent<SprkSoulLiberationCost>();
		if (sprkSoulLiberationCost == null)
		{
			sprkSoulLiberationCost = window.LiberationTransform.gameObject.AddComponent<SprkSoulLiberationCost>();
		}
		sprkSoulLiberationCost.window = window;
		sprkSoulLiberationCost.Refresh();
	}

	private GameObject Child(string name, float x)
	{
		GameObject obj = new GameObject(name);
		obj.layer = base.gameObject.layer;
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		obj.transform.localPosition = new Vector3(x, -68f, 0f);
		return obj;
	}

	private void Refresh()
	{
		if (!(window == null))
		{
			if (price == null)
			{
				UILabel materialCountLabel = window.SoulWeaponMaterialInfo.MaterialCountLabel;
				UILabel uILabel = Child("SprkLiberationGoldPrice", 22f).AddComponent<UILabel>();
				uILabel.bitmapFont = materialCountLabel.bitmapFont;
				uILabel.trueTypeFont = materialCountLabel.trueTypeFont;
				uILabel.fontSize = 28;
				uILabel.width = 230;
				uILabel.height = 40;
				uILabel.depth = materialCountLabel.depth + 1;
				uILabel.supportEncoding = false;
				uILabel.overflowMethod = UILabel.Overflow.ShrinkContent;
				uILabel.alignment = NGUIText.Alignment.Center;
				UIWidget uIWidget = Child("SprkLiberationGoldIcon", -120f).AddComponent<UIWidget>();
				int width = (uIWidget.height = 36);
				uIWidget.width = width;
				uIWidget.depth = uILabel.depth;
				price = new RequirePrice
				{
					ReqPriceLabel = uILabel,
					IconProxyRootWidget = uIWidget,
					ReqPriceCurrencyType = CurrencyType.Gold,
					UsePriceColor = true
				};
				window.LiberationButton.transform.localPosition += new Vector3(0f, -26f, 0f);
			}
			SoulWeaponUpgradePriceData soulWeaponUpgradePriceData = ((window.EquipItemInfo == null) ? null : window.EquipItemInfo.SoulWeaponInfo)?.GetSoulWeaponUpgradePriceData();
			long gold = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
			if (!opened)
			{
				price.Open(soulWeaponUpgradePriceData?.ReqGold ?? (-1), gold);
				opened = true;
			}
			else
			{
				price.SetPrice(soulWeaponUpgradePriceData?.ReqGold ?? (-1), gold);
			}
			nextRefresh = Time.unscaledTime + 0.25f;
		}
	}

	private void Update()
	{
		if (Time.unscaledTime >= nextRefresh)
		{
			Refresh();
		}
	}

	private void OnDisable()
	{
		if (price != null)
		{
			price.Close();
		}
		opened = false;
	}
}
