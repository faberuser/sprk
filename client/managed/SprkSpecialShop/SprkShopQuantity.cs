using System;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkShopQuantity : MonoBehaviour
{
	public NewPayShopProductInfo Info;

	public bool Enabled;

	public int Count = 1;

	public int MaxCount;

	private PayShopProductInfoPopup popup;

	private GameObject row;

	private UIInput input;

	private UIButton minimum;

	private UIButton minus;

	private UIButton plus;

	private UIButton maximum;

	private UISprite buyBackground;

	private bool refreshing;

	private float nextCheck;

	private UILabel Label(Transform parent, string name, string text, int width, int height)
	{
		UILabel uILabel = NGUITools.AddWidget<UILabel>(parent.gameObject);
		uILabel.name = name;
		uILabel.bitmapFont = popup.Label_Title.bitmapFont;
		uILabel.trueTypeFont = popup.Label_Title.trueTypeFont;
		uILabel.fontSize = 24;
		uILabel.color = popup.Label_Title.color;
		uILabel.depth = buyBackground.depth + 3;
		uILabel.width = width;
		uILabel.height = height;
		uILabel.overflowMethod = UILabel.Overflow.ShrinkContent;
		uILabel.alignment = NGUIText.Alignment.Center;
		uILabel.text = text;
		return uILabel;
	}

	private UISprite Background(string name, float x, int width)
	{
		UISprite uISprite = NGUITools.AddWidget<UISprite>(row);
		uISprite.name = name;
		uISprite.atlas = buyBackground.atlas;
		uISprite.spriteName = buyBackground.spriteName;
		uISprite.type = UIBasicSprite.Type.Simple;
		uISprite.color = buyBackground.color;
		uISprite.depth = buyBackground.depth + 2;
		uISprite.width = width;
		uISprite.height = 48;
		uISprite.transform.localPosition = new Vector3(x, 0f, 0f);
		NGUITools.AddWidgetCollider(uISprite.gameObject);
		return uISprite;
	}

	private UIButton Button(string name, string text, float x, EventDelegate.Callback callback)
	{
		UISprite uISprite = Background(name, x, 76);
		UIButton uIButton = uISprite.gameObject.AddComponent<UIButton>();
		uIButton.tweenTarget = uISprite.gameObject;
		EventDelegate.Add(uIButton.onClick, callback);
		Label(uISprite.transform, name + "Label", text, 72, 46);
		return uIButton;
	}

	private void Build()
	{
		buyBackground = popup.Button_Buy.GetComponent<UISprite>();
		if (buyBackground == null)
		{
			buyBackground = popup.Button_Buy.GetComponentInChildren<UISprite>();
		}
		if (buyBackground == null)
		{
			throw new InvalidOperationException("Special shop buy button has no background.");
		}
		row = new GameObject("SprkQuantitySelector");
		row.layer = popup.Button_Buy.gameObject.layer;
		row.transform.SetParent(popup.Button_Buy.transform.parent, worldPositionStays: false);
		row.transform.localPosition = popup.Button_Buy.transform.localPosition + new Vector3(0f, (float)buyBackground.height * 0.5f + 45f, 0f);
		minimum = Button("Minimum", "Min", -215f, delegate
		{
			Set(1);
		});
		minus = Button("Decrease", "-", -115f, delegate
		{
			Set(Count - 1);
		});
		plus = Button("Increase", "+", 115f, delegate
		{
			Set(Count + 1);
		});
		maximum = Button("Maximum", "Max", 215f, delegate
		{
			Refresh();
			Set(MaxCount);
		});
		UISprite uISprite = Background("QuantityInput", 0f, 120);
		UILabel label = Label(uISprite.transform, "Quantity", "1", 110, 44);
		input = uISprite.gameObject.AddComponent<UIInput>();
		input.label = label;
		input.validation = UIInput.Validation.Integer;
		input.characterLimit = 3;
		input.selectAllTextOnFocus = true;
		EventDelegate.Add(input.onChange, delegate
		{
			if (!refreshing && int.TryParse(input.value, out var result))
			{
				Set(result);
			}
		});
		EventDelegate.Add(input.onSubmit, delegate
		{
			Set(Count);
		});
		UILabel uILabel = Label(row.transform, "QuantityHeading", "Quantity", 160, 24);
		uILabel.color = new Color(0.36f, 0.3f, 0.23f, 1f);
		uILabel.transform.localPosition = new Vector3(0f, 38f, 0f);
	}

	public void Open(PayShopProductInfoPopup window, NewPayShopProductInfo info)
	{
		popup = window;
		Info = info;
		Count = 1;
		Enabled = SprkSpecialShop.CanSelectQuantity(info);
		if (row == null && Enabled)
		{
			Build();
		}
		if (row != null)
		{
			row.SetActive(Enabled);
		}
		if (Enabled)
		{
			Refresh();
		}
	}

	public void Refresh()
	{
		UISprite[] componentsInChildren = row.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite obj in componentsInChildren)
		{
			obj.atlas = buyBackground.atlas;
			obj.spriteName = buyBackground.spriteName;
		}
		UILabel[] componentsInChildren2 = row.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel obj2 in componentsInChildren2)
		{
			obj2.bitmapFont = popup.Label_Title.bitmapFont;
			obj2.trueTypeFont = popup.Label_Title.trueTypeFont;
		}
		long balance = SprkSpecialShop.Balance(GlobalAutoBehaviour<AccountManager>.instance.UserManager, Info.PriceType);
		int num = 999;
		if (Info.Count != null)
		{
			int[] count = Info.Count;
			foreach (int num2 in count)
			{
				if (num2 > 0)
				{
					num = Math.Min(num, int.MaxValue / num2);
				}
			}
		}
		MaxCount = SprkSpecialShop.Maximum(balance, Info.Price, SprkSpecialShop.Purchased(Info), Info.PurchasableCount, num);
		Set(Count);
	}

	private void Set(int value)
	{
		Count = Math.Max(1, Math.Min(value, Math.Max(1, MaxCount)));
		refreshing = true;
		input.value = Count.ToString();
		refreshing = false;
		minus.isEnabled = MaxCount > 0 && Count > 1;
		plus.isEnabled = Count < MaxCount;
		minimum.isEnabled = minus.isEnabled;
		maximum.isEnabled = plus.isEnabled;
		popup.PriceComponent.Init();
		bool flag = PayShopHelper.IsBuyable(Info);
		if (flag)
		{
			popup.PriceComponent.SetGamePrice(Info.PriceType, (int)(Info.Price * Count));
		}
		else
		{
			popup.PriceComponent.NotBuyableOpen();
		}
		popup.Button_Buy.isEnabled = flag && MaxCount > 0;
	}

	private void Update()
	{
		if (Enabled && !(row == null) && !(Time.realtimeSinceStartup < nextCheck))
		{
			nextCheck = Time.realtimeSinceStartup + 0.5f;
			Refresh();
		}
	}
}
