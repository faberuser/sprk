using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenUseMultipleHeroSelect : BaseWindow
{
	public interface Listener
	{
		void OnGetReward(InvenUseMultipleHeroSelect caller, int leftHeroIndex, int rightHeroIndex);

		void OnOpenHeroInfoPopup(InvenUseMultipleHeroSelect caller, CreatureData firstShowHero, List<CreatureData> heroGroupList, SkillAttrType recommandedSkillAttrType = SkillAttrType.None, HeroUsageType heroUsageType = HeroUsageType.None, bool showOpenTotalList = false);
	}

	public const string PrefabPath = "Prefab2/Window/Inventory/MultipleHeroSelect/InvenUseMultipleHeroSelect";

	public const string RECOMMANDED_COMBINATION = "HERO_SELECT_ITEM_BEGINNER_MAIN_COMMENT_POSITIVE";

	public const string NONRECOMMANDED_COMBINATION = "HERO_SELECT_ITEM_BEGINNER_MAIN_COMMENT_NEGATIVE";

	public const string SAFTYCHECK_NONRECOMMANDED_MESSAGE = "HERO_SELECT_ITEM_BEGINNER_MAIN_FINALSAFETY";

	public const string SAFTYCHECK_TITLE_WARNING = "COMMON_WARNING";

	public readonly Color32 nonRecommandedColor = new Color32(byte.MaxValue, 54, 0, byte.MaxValue);

	public readonly Color32 recommandedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	protected Listener _iListener;

	private bool hasAllHeroGroup1 = true;

	private bool hasAllHeroGroup2 = true;

	private int currentSelectSlotIndex;

	private List<CreatureData> _heroList1;

	private List<CreatureData> _heroList2;

	private bool isRestoreStep;

	[Header("Root")]
	public UIWidget RootWidget;

	[Header("Basic Window Button")]
	public UIButton Button_Exit;

	public UIButton Button_Guide;

	[Header("Background Theme")]
	public UITexture Texture_BG;

	[Header("Hero Select 1")]
	public Transform Slot1_Root;

	public GameObject GameObject_EmptySlot1_Root;

	public UIWidget Widget_AllOwnReward1_Root;

	public GameObject GameObject_HeroPortrait1_Root;

	public HeroPortraitComponent HeroPortraitComp1;

	[Header("Hero Select 2")]
	public Transform Slot2_Root;

	public GameObject GameObject_EmptySlot2_Root;

	public UIWidget Widget_AllOwnReward2_Root;

	public GameObject GameObject_HeroPortrait2_Root;

	public HeroPortraitComponent HeroPortraitComp2;

	[Header("Combination Guide Comment")]
	public UILabel Label_CombinationCondition;

	[Header("Confirm Button")]
	public UIButton Button_SelectConfirm;

	public UILabel Label_SelectConfirm;

	public ItemData ItemData { get; protected set; }

	public MultipleHeroSelectItemData MultipleHeroSelectItemData { get; private set; }

	public int LastSelectedSlot => currentSelectSlotIndex;

	public CreatureData SelectedHero1
	{
		get
		{
			if (HeroPortraitComp1 == null || HeroPortraitComp1.CreatureData == null)
			{
				return null;
			}
			return HeroPortraitComp1.CreatureData;
		}
	}

	public CreatureData SelectedHero2
	{
		get
		{
			if (HeroPortraitComp2 == null || HeroPortraitComp2.CreatureData == null)
			{
				return null;
			}
			return HeroPortraitComp2.CreatureData;
		}
	}

	private int HeroLevel { get; set; }

	private int HeroStar { get; set; }

	private int HeroTrans { get; set; }

	public void Open(Listener listener, int selectedItemIndex)
	{
		Open(delegate
		{
			Init(listener, selectedItemIndex);
		}, null);
	}

	public override void Close()
	{
		ItemData = null;
		MultipleHeroSelectItemData = null;
		_heroList1.Clear();
		_heroList2.Clear();
		base.Close();
	}

	public void Init(Listener listener, int selectItemIndex, CreatureData selectedHeroData1 = null, CreatureData selectedHeroData2 = null, CreatureData lastSelectedHeroData = null, int selectedSlot = 0)
	{
		_iListener = listener;
		isRestoreStep = selectedSlot != 0;
		if (!InitItemData(selectItemIndex))
		{
			VespaDebug.LogError("Init fail.\nCheck MultipleSelectItem, ItemTable.\nIndex: " + selectItemIndex);
			Close();
			return;
		}
		InitUIComponent();
		if (isRestoreStep)
		{
			RestoreInit(selectedHeroData1, selectedHeroData2, lastSelectedHeroData, selectedSlot);
		}
		else if (!new LocalStorageBool(i_default: false, typeof(HeroSelectGuidePopup).Name + MultipleHeroSelectItemData.ItemCode).Value)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.GetInstance<HeroSelectGuidePopup>().Open(MultipleHeroSelectItemData.ItemCode);
		}
	}

	private bool InitItemData(int selectItemIndex)
	{
		ItemData = GameRoot.Instance.ItemDataContainer.GetData(selectItemIndex);
		if (ItemData == null)
		{
			VespaDebug.LogError("Can't find item data.(Item index = " + selectItemIndex);
			return false;
		}
		MultipleHeroSelectItemData = GameRoot.Instance.MultipleHeroSelectItemDataContainer.GetData(ItemData.Code);
		if (MultipleHeroSelectItemData == null)
		{
			VespaDebug.LogError("Can't find MultipleSelectItem data.(Item index = " + ItemData.Code);
			return false;
		}
		HeroLevel = MultipleHeroSelectItemData.HeroLevel;
		HeroStar = MultipleHeroSelectItemData.HeroStar;
		HeroTrans = MultipleHeroSelectItemData.HeroTranscend;
		_heroList1 = GetListOrderbyOwnOnly(MultipleHeroSelectItemData.HeroIndices1);
		_heroList2 = GetListOrderbyOwnOnly(MultipleHeroSelectItemData.HeroIndices2);
		return true;
	}

	private void InitUIComponent()
	{
		currentSelectSlotIndex = 0;
		hasAllHeroGroup1 = true;
		hasAllHeroGroup2 = true;
		int[] heroIndices = MultipleHeroSelectItemData.HeroIndices1;
		foreach (int key in heroIndices)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key) == null)
			{
				hasAllHeroGroup1 = false;
				break;
			}
		}
		heroIndices = MultipleHeroSelectItemData.HeroIndices2;
		foreach (int key2 in heroIndices)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key2) == null)
			{
				hasAllHeroGroup2 = false;
				break;
			}
		}
		UIHelper.EnableGameObject(GameObject_EmptySlot1_Root, !hasAllHeroGroup1);
		UIHelper.EnableGameObject(GameObject_EmptySlot2_Root, !hasAllHeroGroup2);
		UIHelper.EnableGameObject(Widget_AllOwnReward1_Root.gameObject, hasAllHeroGroup1);
		UIHelper.EnableGameObject(Widget_AllOwnReward2_Root.gameObject, hasAllHeroGroup2);
		if (hasAllHeroGroup1)
		{
			UIHelper.CreateRewardMainItemIconComponent(MultipleHeroSelectItemData.AllOwnedRewardIndex1, Widget_AllOwnReward1_Root.transform);
		}
		if (hasAllHeroGroup2)
		{
			UIHelper.CreateRewardMainItemIconComponent(MultipleHeroSelectItemData.AllOwnedRewardIndex2, Widget_AllOwnReward2_Root.transform);
		}
		HeroPortraitComp1.ResetPortrait(null, null);
		HeroPortraitComp2.ResetPortrait(null, null);
		UIHelper.EnableGameObject(GameObject_HeroPortrait1_Root, enable: false);
		UIHelper.EnableGameObject(GameObject_HeroPortrait2_Root, enable: false);
		UpdateCombinationUI();
		UpdateConfirmButton();
	}

	public void OnClickExit()
	{
		VespaCoroutine.Start(CoClose());
	}

	private IEnumerator CoClose()
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		float timeOut = 0f;
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		Close();
		while (IsOpen() && timeOut < 5f)
		{
			timeOut += Time.deltaTime;
			yield return null;
		}
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
	}

	public void OnClickGuideButton()
	{
		SceneAutoBehaviour<InventoryManagement>.instance.GetInstance<HeroSelectGuidePopup>().Open(MultipleHeroSelectItemData.ItemCode);
	}

	public void OnClickGetReward()
	{
		if (MultipleHeroSelectItemData == null)
		{
			VespaDebug.LogError("multipleHeroSelectItemData is null");
			Close();
			return;
		}
		if (_iListener == null)
		{
			VespaDebug.LogError("Listener is null");
			Close();
			return;
		}
		int heroIndex1 = 0;
		int heroIndex2 = 0;
		Action action = delegate
		{
			_iListener.OnGetReward(this, heroIndex1, heroIndex2);
		};
		if (SelectedHero1 != null)
		{
			heroIndex1 = SelectedHero1.Index;
		}
		if (SelectedHero2 != null)
		{
			heroIndex2 = SelectedHero2.Index;
		}
		if (heroIndex1 <= 0 || heroIndex2 <= 0)
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else if (SelectedHero1.HeroAttrType != SelectedHero2.HeroAttrType)
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
			string stringDirect = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_MAIN_FINALSAFETY");
			MessagePopup.Open(text, stringDirect, action, null);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
	}

	public void OnClickSelectHeroSlot1()
	{
		if (!hasAllHeroGroup1)
		{
			currentSelectSlotIndex = 1;
			SelectHeroSlot();
		}
	}

	public void OnClickSelectHeroSlot2()
	{
		if (!hasAllHeroGroup2)
		{
			currentSelectSlotIndex = 2;
			SelectHeroSlot();
		}
	}

	public void SelectHeroSlot()
	{
		CreatureData firstShowHero = null;
		bool showOpenTotalList = true;
		bool flag = false;
		SkillAttrType skillAttrType = SkillAttrType.None;
		HeroUsageType heroUsageType = HeroUsageType.None;
		HeroPortraitComponent heroPortraitComponent;
		HeroPortraitComponent heroPortraitComponent2;
		List<CreatureData> list;
		switch (currentSelectSlotIndex)
		{
		default:
			return;
		case 1:
			heroPortraitComponent = HeroPortraitComp1;
			heroPortraitComponent2 = HeroPortraitComp2;
			_ = MultipleHeroSelectItemData.HeroIndices1;
			flag = hasAllHeroGroup2;
			list = _heroList1;
			heroUsageType = HeroUsageType.Dealer;
			break;
		case 2:
			heroPortraitComponent = HeroPortraitComp2;
			heroPortraitComponent2 = HeroPortraitComp1;
			_ = MultipleHeroSelectItemData.HeroIndices2;
			flag = hasAllHeroGroup1;
			list = _heroList2;
			heroUsageType = HeroUsageType.Supporter;
			break;
		}
		if (heroPortraitComponent.CreatureData != null)
		{
			showOpenTotalList = false;
			firstShowHero = heroPortraitComponent.CreatureData;
		}
		if (heroPortraitComponent2.CreatureData == null || flag)
		{
			_iListener.OnOpenHeroInfoPopup(this, firstShowHero, list, SkillAttrType.None, heroUsageType, showOpenTotalList);
			return;
		}
		skillAttrType = heroPortraitComponent2.CreatureData.HeroAttrType;
		List<CreatureData> listOrderByHeroAttrTypeAndOwn = GetListOrderByHeroAttrTypeAndOwn(list, skillAttrType);
		_iListener.OnOpenHeroInfoPopup(this, firstShowHero, listOrderByHeroAttrTypeAndOwn, skillAttrType, heroUsageType, showOpenTotalList);
	}

	protected void UpdateConfirmButton()
	{
		bool flag = false;
		bool num = HeroPortraitComp1.CreatureData != null || hasAllHeroGroup1;
		bool flag2 = HeroPortraitComp2.CreatureData != null || hasAllHeroGroup2;
		flag = num && flag2;
		UIHelper.EnableButton(Button_SelectConfirm, flag);
	}

	protected void UpdateCombinationUI()
	{
		bool num = HeroPortraitComp1.CreatureData != null;
		bool flag = HeroPortraitComp2.CreatureData != null;
		bool flag2 = num && flag;
		UIHelper.EnableGameObject(Label_CombinationCondition, flag2);
		if (flag2)
		{
			string stringDirect;
			Color32 color;
			bool show;
			if (HeroPortraitComp1.CreatureData.HeroAttrType == HeroPortraitComp2.CreatureData.HeroAttrType)
			{
				stringDirect = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_MAIN_COMMENT_POSITIVE");
				color = recommandedColor;
				show = true;
			}
			else
			{
				stringDirect = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_MAIN_COMMENT_NEGATIVE");
				color = nonRecommandedColor;
				show = false;
			}
			HeroPortraitComp1.ShowRecommandIcon(show);
			HeroPortraitComp2.ShowRecommandIcon(show);
			UIHelper.SetLabelText(Label_CombinationCondition, stringDirect);
			UIHelper.SetLabelColor(Label_CombinationCondition, color);
		}
	}

	public void SetHeroPortrait(CreatureData heroData)
	{
		HeroPortraitComponent heroPortraitComponent;
		GameObject gameObject;
		GameObject gameObject2;
		if (currentSelectSlotIndex == 1)
		{
			heroPortraitComponent = HeroPortraitComp1;
			gameObject = GameObject_EmptySlot1_Root;
			gameObject2 = GameObject_HeroPortrait1_Root;
		}
		else
		{
			heroPortraitComponent = HeroPortraitComp2;
			gameObject = GameObject_EmptySlot2_Root;
			gameObject2 = GameObject_HeroPortrait2_Root;
		}
		CachedHeroInfo cachedHeroInfo = new CachedHeroInfo(heroData);
		cachedHeroInfo.Level = (sbyte)HeroLevel;
		cachedHeroInfo.Star = (sbyte)HeroStar;
		cachedHeroInfo.Transcended = (sbyte)HeroTrans;
		bool level = HeroLevel > 1;
		heroPortraitComponent.ResetPortrait(cachedHeroInfo, heroData);
		heroPortraitComponent.ShowName(heroData.Name);
		heroPortraitComponent.SetLevel(level);
		UIHelper.EnableGameObject(gameObject2, enable: true);
		UIHelper.EnableGameObject(gameObject, enable: false);
		UpdateConfirmButton();
		UpdateCombinationUI();
	}

	private void RestoreInit(CreatureData selectedHeroData1 = null, CreatureData selectedHeroData2 = null, CreatureData lastSelectedHeroData = null, int selectedSlot = 0)
	{
		if (selectedSlot != 0)
		{
			if (selectedHeroData1 != null)
			{
				currentSelectSlotIndex = 1;
				SetHeroPortrait(selectedHeroData1);
			}
			if (selectedHeroData2 != null)
			{
				currentSelectSlotIndex = 2;
				SetHeroPortrait(selectedHeroData2);
			}
			RestoreOpenHeroInfoPopup(lastSelectedHeroData, selectedSlot);
		}
	}

	private void RestoreOpenHeroInfoPopup(CreatureData lastSelectedHero, int lastSlotIndex)
	{
		SkillAttrType skillAttrType = SkillAttrType.None;
		HeroUsageType heroUsageType = HeroUsageType.None;
		currentSelectSlotIndex = lastSlotIndex;
		List<CreatureData> list;
		switch (lastSlotIndex)
		{
		default:
			return;
		case 1:
			heroUsageType = HeroUsageType.Dealer;
			list = _heroList1;
			if (HeroPortraitComp2.CreatureData != null && !hasAllHeroGroup2)
			{
				skillAttrType = HeroPortraitComp2.CreatureData.HeroAttrType;
			}
			break;
		case 2:
			heroUsageType = HeroUsageType.Supporter;
			list = _heroList2;
			if (HeroPortraitComp1.CreatureData != null && !hasAllHeroGroup1)
			{
				skillAttrType = HeroPortraitComp1.CreatureData.HeroAttrType;
			}
			break;
		}
		if (skillAttrType != SkillAttrType.None)
		{
			list = GetListOrderByHeroAttrTypeAndOwn(list, skillAttrType);
		}
		_iListener.OnOpenHeroInfoPopup(this, lastSelectedHero, list, skillAttrType, heroUsageType);
	}

	private List<CreatureData> GetListOrderbyOwnOnly(int[] heroIndices)
	{
		if (heroIndices == null)
		{
			VespaDebug.LogError("heroIndices is null");
			return null;
		}
		List<CreatureData> list = new List<CreatureData>(__Extension_Method_ICollection__.COUNT(heroIndices));
		foreach (int item in heroIndices.OrderBy((int heroIndex) => GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex) != null))
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(item);
			if (data != null)
			{
				list.Add(data);
			}
		}
		return list;
	}

	private List<CreatureData> GetListOrderByHeroAttrTypeAndOwn(List<CreatureData> originalHeroList, SkillAttrType recommandedType)
	{
		if (originalHeroList == null)
		{
			VespaDebug.LogError("heroIndices is null");
			return null;
		}
		return (from hero in originalHeroList
			orderby GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(hero.Index) != null, hero.HeroAttrType != recommandedType
			select hero).ToList();
	}
}
