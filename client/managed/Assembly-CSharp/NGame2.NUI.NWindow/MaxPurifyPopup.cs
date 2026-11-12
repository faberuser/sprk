using System;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MaxPurifyPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/PurifyHero/MaxPurifyPopup";

	public UILabel Label_PurifyCount;

	public UILabel Label_CurrentPurifyPercent;

	public UILabel Label_AfterPurifyPercent;

	public UILabel Label_OriginalGold;

	public UILabel Label_CurrentGold;

	public GameObject CompeletPurify;

	public GameObject EventObject;

	public UISimpleGrid ItemIconGrid;

	public Action Callback_OK;

	private ItemIconComponent AwakeMaterialItem;

	public static void Open(CachedHeroInfo heroInfo, Action callback)
	{
		WindowPrefab<MaxPurifyPopup> windowPrefab = new WindowPrefab<MaxPurifyPopup>("Resources/Prefab2/Window/PurifyHero/MaxPurifyPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(heroInfo, callback);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(CachedHeroInfo heroInfo, Action callback)
	{
		Callback_OK = callback;
		base.Open();
		CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(heroInfo.Star, heroInfo.Transcended);
		if (nextData != null)
		{
			HeroAwakeData data = GameRoot.Instance.HeroAwakeDataContainer.GetData(heroInfo.HeroIndex);
			ItemData itemData = null;
			itemData = ((!NGame2.NAccount.UserHeroManager.IsMaxStar(heroInfo)) ? GameRoot.Instance.ItemDataContainer.GetDataByCode(data.GetAwakeMaterialItemCode(heroInfo.Star)) : GameRoot.Instance.ItemDataContainer.GetDataByCode(data.GetTranscendMaterialItemCode(heroInfo.Transcended)));
			AwakeMaterialItem = UIHelper.AddGridItem(ItemIconGrid, ItemIconComponent.PrefabAuto);
			AwakeMaterialItem.Open(itemData);
			int maxPurified = nextData.MaxPurified;
			int num = 0;
			long cost = num;
			int purify = heroInfo.Purified;
			int count = 0;
			int num2 = Convert.ToInt32((float)purify / (float)maxPurified * 100f);
			GetMaxPurifyAndCost(nextData, ref count, ref purify, ref cost);
			float num3 = (float)purify / (float)maxPurified;
			UIHelper.SetLabelText(Label_CurrentPurifyPercent, num2 + "%");
			if (num3 >= 1f)
			{
				UIHelper.EnableGameObject(CompeletPurify, enable: true);
				UIHelper.EnableGameObject(Label_AfterPurifyPercent, enable: false);
			}
			else
			{
				UIHelper.EnableGameObject(CompeletPurify, enable: false);
				UIHelper.EnableGameObject(Label_AfterPurifyPercent, enable: true);
				string text = Convert.ToInt32(num3 * 100f) + "%";
				UIHelper.SetLabelText(Label_AfterPurifyPercent, text);
			}
			string text2 = LocalizationManager.GetString(HeroPurify.PURIFY_COUNT, count);
			UIHelper.SetLabelText(Label_PurifyCount, text2);
			DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountPurify, null);
			if (effectiveEvent != null)
			{
				long i_value = cost;
				long num4 = cost;
				int bonusPercentage = effectiveEvent.BonusPercentage;
				long num5 = checked(num4 * bonusPercentage);
				int num6 = 100;
				long num7 = num5 / num6;
				long val = checked(cost - num7);
				int num8 = 0;
				cost = Math.Max(val, num8);
				UIHelper.EnableGameObject(Label_OriginalGold, enable: true);
				UIHelper.SetLabelText(Label_OriginalGold, Vespa_Int_Extension_Method.ToCommaString(i_value));
			}
			else
			{
				UIHelper.EnableGameObject(Label_OriginalGold, enable: false);
			}
			UIHelper.SetLabelText(Label_CurrentGold, Vespa_Int_Extension_Method.ToCommaString(cost));
			UIHelper.EnableGameObject(EventObject, IsEvent());
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref AwakeMaterialItem);
	}

	private void GetMaxPurifyAndCost(CreatureStarData starData, ref int count, ref int purify, ref long cost)
	{
		count = 0;
		int maxPurified = starData.MaxPurified;
		int cost2 = GetCost(starData);
		int num = 0;
		int gold = starData.Gold;
		while (num + cost2 < GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold)
		{
			num += cost2;
			purify += starData.PurifyAmount1;
			long num2 = cost;
			int num3 = gold;
			cost = checked(num2 + num3);
			count++;
			if (purify >= maxPurified)
			{
				purify = maxPurified;
				break;
			}
		}
	}

	private int GetCost(CreatureStarData starData)
	{
		int num = starData.Gold;
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountPurify, null);
		if (effectiveEvent != null)
		{
			int num2 = num * effectiveEvent.BonusPercentage / 100;
			num -= num2;
			num = Mathf.Max(num, 0);
		}
		return num;
	}

	private bool IsEvent()
	{
		return GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountPurify, null) != null;
	}

	public void OnClickMaxPurify()
	{
		if (Callback_OK != null)
		{
			Callback_OK();
		}
		Close();
	}
}
