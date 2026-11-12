using System;
using System.Collections.Generic;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class BuyConfirmPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/BuyConfirmPopup";

	public UILabel MessageLabel;

	public UISprite Sprite_Gold;

	public UISprite Sprite_Gem;

	public UISprite Sprite_PvpCoin;

	public UISprite Sprite_GuildPoint;

	public UISprite Sprite_Custom;

	public UILabel Label_Title;

	public UILabel Label_Confirm;

	public UILabel Label_BuyCount;

	private Action _onYes;

	public static void Open(string title, string message, string confirm, ShopCostType costType, int Value, Action onYes)
	{
		WindowPrefab<BuyConfirmPopup> windowPrefab = new WindowPrefab<BuyConfirmPopup>("Resources/Prefab2/Window/MessagePopup/BuyConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, confirm, costType, Value, onYes);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string title, string message, string confirm, ShopCostType costType, int Value, Action onYes)
	{
		base.Open();
		UIHelper.SetLabelText(Label_Title, title);
		UIHelper.SetLabelText(MessageLabel, message);
		UIHelper.SetLabelText(Label_Confirm, confirm);
		_onYes = onYes;
		UIHelper.EnableGameObject(Sprite_Gold, costType == ShopCostType.Gold);
		UIHelper.EnableGameObject(Sprite_Gem, costType == ShopCostType.Gem);
		UIHelper.EnableGameObject(Sprite_PvpCoin, costType == ShopCostType.PvpCoin);
		UIHelper.EnableGameObject(Sprite_GuildPoint, costType == ShopCostType.GuildPoint);
		if ((uint)(costType - 10) <= 1u && Sprite_Custom != null)
		{
			List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.EventDungeon);
			int result = -1;
			if (allEffectiveEvent != null)
			{
				for (int i = 0; i < allEffectiveEvent.Count; i++)
				{
					DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
					if (!String_Extension_Method.IsNullOrEmpty(dropBonusEvent.Condition))
					{
						int.TryParse(dropBonusEvent.Condition, out result);
						break;
					}
				}
			}
			if (result != -1)
			{
				CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(result);
				if (data != null)
				{
					string userStatTypeIconSprite = data.GetUserStatTypeIconSprite(costType.ToString());
					if (!String_Extension_Method.IsNullOrEmpty(userStatTypeIconSprite) && Sprite_Custom != null)
					{
						Sprite_Custom.GetComponent<UISprite>().spriteName = userStatTypeIconSprite;
					}
				}
			}
			UIHelper.EnableGameObject(Sprite_Custom, enable: true);
		}
		UIHelper.SetLabelText(Label_BuyCount, Vespa_Int_Extension_Method.ToCommaString(Value));
	}

	public void OnClickYes()
	{
		if (_onYes != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onYes);
			_onYes = null;
		}
		base.Close();
	}

	public void OnClickNo()
	{
		base.Close();
	}
}
