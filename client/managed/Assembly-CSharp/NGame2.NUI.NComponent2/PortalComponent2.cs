using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NodeFunctionType;
using NGame2.NUI.NWindow.NodeFunctionType.Condition;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalComponent2 : LinkFactoryComponent, UICamera.IOnDragNoArgListener
{
	public interface Listener
	{
		void SelectPortal(PortalComponent2 Selected);

		void SelectButton(PortalComponent2 Selected);

		void SelectEnableContentsShortCut(PortalComponent2 Selected);

		void SelectDisableContentsShortCut(string disableText);

		void StartDrag();

		void OnDrag();

		void EndDrag();

		bool CanOpenShorCutBtn();
	}

	public const string PrefabPath = "Prefab2/Component/Portal/PortalComponent2";

	public static PrefabAuto<PortalComponent2> PrefabAuto = new PrefabAuto<PortalComponent2>("Prefab2/Component/Portal/PortalComponent2");

	public const string PrefabLargePath = "Prefab2/Component/Portal/PortalComponentLarge";

	public static PrefabAuto<PortalComponent2> PrefabAutoLarge = new PrefabAuto<PortalComponent2>("Prefab2/Component/Portal/PortalComponentLarge");

	public const string PrefabButtonPath = "Prefab2/Component/Portal/PortalComponentButton";

	public static PrefabAuto<PortalComponent2> PrefabAutoButton = new PrefabAuto<PortalComponent2>("Prefab2/Component/Portal/PortalComponentButton");

	public const string PrefabDungeonNameTagPath = "Prefab2/Component/Portal/PortalComponent3";

	public static PrefabAuto<PortalComponent2> PrefabAutoDungeonNameTag = new PrefabAuto<PortalComponent2>("Prefab2/Component/Portal/PortalComponent3");

	public UITexture Texture_Portal;

	public UITexture Ani_Dummy_Texture;

	public UILabel Label_PortalName;

	public UIWidget ComponentWidget;

	public UIButton Button_Portal;

	public UISprite Sprite_EventFlag;

	public UISprite Sprite_ChapterBG;

	public UILabel Label_ChapterName;

	public UILabel Label_Count;

	public UISprite Sprite_Disable;

	public UISprite Sprite_UpperDungeonIcon;

	public UISprite Sprite_ConquestDungeonIcon;

	public UISprite Sprite_ShopRestockNew;

	public UILabel Label_Disable;

	public UIWidget Widget_NewMark;

	public UIWidget WidgetEffect;

	public UILabel Label_DevIndex;

	[Header("[Gauge Group]")]
	public UITexture Texture_Gauge;

	public UILabel Label_GaugePercent;

	public UISlider GaugeSlider;

	private Listener _listener;

	protected bool _isLock;

	private bool _playOpenEffect;

	private float _updateTime;

	public int ChapterIndex { get; private set; }

	public int DungeonIndex { get; private set; }

	public PortalData PortalData { get; private set; }

	public void Open(Listener listener, int chapterIndex, int dungeonIndex, string labelKey, string textureKey, int mapWidth, int mapHeight, int listCount = -1, CurrencyType gaugeCurrencyType = CurrencyType.None, string gaugeKey = null)
	{
		if (listCount != -1)
		{
			_playOpenEffect = PortalSelect3.GetOpenPortalEffectEnable(listCount);
		}
		_listener = listener;
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		_isLock = false;
		PortalData = GameRoot.Instance.PortalDataContainer.GetData(chapterIndex, dungeonIndex);
		Vector3 localPosition = base.gameObject.transform.localPosition;
		localPosition.x = PortalData.PositionX - mapWidth / 2;
		localPosition.y = -PortalData.PositionY + mapHeight / 2;
		localPosition.z = 0f;
		base.gameObject.transform.localPosition = localPosition;
		if (String_Extension_Method.IsNullOrEmpty(textureKey))
		{
			Texture_Portal.mainTexture = null;
		}
		else
		{
			AssetTexture assetTexture = new AssetTexture();
			assetTexture.Open(textureKey);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Portal, assetTexture, UIHelper.EAssetTextureAssign.None, delegate
			{
				Texture_Portal.ResizeCollider();
			});
			if (Ani_Dummy_Texture != null)
			{
				UIHelper.ResetPlaneAssetTextureAsync(this, Ani_Dummy_Texture, assetTexture, UIHelper.EAssetTextureAssign.None, delegate
				{
					Ani_Dummy_Texture.ResizeCollider();
				});
				UIHelper.EnableGameObject(Ani_Dummy_Texture, enable: false);
			}
		}
		if (gaugeCurrencyType == CurrencyType.ShakmehMiddleBossPoint)
		{
			NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
			if (instance != null)
			{
				AssetTexture assetTexture2 = new AssetTexture();
				assetTexture2.Open(gaugeKey);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Gauge, assetTexture2);
				if (instance.IsOpenShakmehDungeon)
				{
					long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
					int maxValue = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint).MaxValue;
					string text = $"{(float)userStat / (float)maxValue * 100f:0}%";
					UIHelper.SetLabelText(Label_GaugePercent, text);
					UIHelper.EnableGameObject(Label_GaugePercent, enable: true);
					GaugeSlider.value = (float)userStat / (float)maxValue;
				}
				else
				{
					UIHelper.EnableGameObject(Label_GaugePercent, enable: false);
					GaugeSlider.value = 0f;
				}
			}
		}
		else
		{
			Texture_Gauge.mainTexture = null;
			UIHelper.EnableGameObject(Label_GaugePercent, enable: false);
		}
		UIHelper.SetLabelText(Label_PortalName, LocalizationManager.GetStringDirect(labelKey));
		string text2 = ((ChapterIndex == 11) ? "X" : ChapterIndex.ToString());
		UIHelper.SetLabelText(Label_ChapterName, LocalizationManager.GetStringDirect("CHAPTER_DUNGEON_NUMBER", text2));
		UIHelper.EnableGameObject(Label_ChapterName, DungeonIndex == 1);
		UIHelper.EnableGameObject(Sprite_ChapterBG, DungeonIndex == 1);
		UIHelper.EnableGameObject(Widget_NewMark, enable: false);
		CheckEnable();
	}

	public void Open(Listener listener, int chapterIndex, int dungeonIndex, string labelKey, string textureKey)
	{
		_listener = listener;
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		_isLock = false;
		PortalData = GameRoot.Instance.PortalDataContainer.GetData(chapterIndex, dungeonIndex);
		if (String_Extension_Method.IsNullOrEmpty(textureKey))
		{
			Texture_Portal.mainTexture = null;
		}
		else
		{
			AssetTexture assetTexture = new AssetTexture();
			assetTexture.Open(textureKey);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Portal, assetTexture, UIHelper.EAssetTextureAssign.MakePixelPerfect, delegate
			{
				Texture_Portal.ResizeCollider();
			});
		}
		UIHelper.EnableGameObject(Widget_NewMark, enable: false);
		UIHelper.SetLabelText(Label_PortalName, LocalizationManager.GetStringDirect(labelKey));
		UIHelper.EnableGameObject(Sprite_EventFlag, enable: false);
		List<PortalPropData> propDataOfPortalPopup = GameRoot.Instance.PortalPropDataContainer.GetPropDataOfPortalPopup(chapterIndex, dungeonIndex);
		if (propDataOfPortalPopup != null)
		{
			bool enable = false;
			bool enable2 = false;
			foreach (PortalPropData item in propDataOfPortalPopup)
			{
				if (CheckPropDataCondition(item))
				{
					enable = true;
					break;
				}
			}
			foreach (PortalPropData item2 in propDataOfPortalPopup)
			{
				if (CheckShopRestockCondition(item2))
				{
					enable2 = true;
					break;
				}
			}
			UIHelper.EnableGameObject(Sprite_EventFlag, enable);
			UIHelper.EnableGameObject(Sprite_ShopRestockNew, enable2);
		}
		CheckEnable_NameTag();
		if (PortalData.ParentNode != null && PortalData.ParentNode.Length == 2 && PortalData.ParentNode[0] == 100)
		{
			string text = ((ChapterIndex == 11) ? "X" : ChapterIndex.ToString());
			UIHelper.SetLabelText(Label_ChapterName, text);
			UIHelper.EnableGameObject(Sprite_UpperDungeonIcon, enable: false);
			UIHelper.EnableGameObject(Sprite_ConquestDungeonIcon, enable: false);
			if (PortalData.ParentNode[1] == 2)
			{
				UIHelper.EnableGameObject(Sprite_UpperDungeonIcon, enable: true);
				int remainCount = 0;
				int maxCount = 0;
				GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.GetChapterCount(ChapterIndex, out remainCount, out maxCount);
				UIHelper.SetLabelText(Label_Count, $"{remainCount} / {maxCount}");
				int num = ChapterIndex - 100;
				string text2 = ((num == 11) ? "X" : num.ToString());
				UIHelper.SetLabelText(Label_ChapterName, LocalizationManager.GetStringDirect("CHAPTER_DUNGEON_NUMBER", text2));
			}
			else if (PortalData.ParentNode[1] == 3)
			{
				int maxCount2 = 0;
				int remainCount2 = 0;
				UIHelper.EnableGameObject(Sprite_ConquestDungeonIcon, enable: true);
				GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetChapterCount(StaminaType.ConquestKey, ChapterIndex, out remainCount2, out maxCount2);
				UIHelper.SetLabelText(Label_Count, $"{remainCount2} / {maxCount2}");
				UIHelper.SetLabelText(Label_ChapterName, LocalizationManager.GetStringDirect("CHAPTER_DUNGEON_NUMBER", text));
			}
		}
	}

	public void LockClick(bool isLock)
	{
		_isLock = isLock;
	}

	public virtual void OpenButton(Listener listener, int chapterIndex, int dungeonIndex, string labelKey, string textureKey)
	{
		_listener = listener;
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		UIHelper.EnableGameObject(Widget_NewMark, enable: false);
		UIHelper.EnableGameObject(WidgetEffect, enable: false);
		PortalData = GameRoot.Instance.PortalDataContainer.GetData(chapterIndex, dungeonIndex);
		if (!String_Extension_Method.IsNullOrEmpty(textureKey))
		{
			AssetTexture assetTexture = new AssetTexture();
			assetTexture.Open(textureKey);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Portal, assetTexture);
		}
		CheckEnable_ShortCutButton();
		UIHelper.SetLabelText(Label_PortalName, LocalizationManager.GetStringDirect(labelKey));
	}

	public void Update()
	{
		if (Time.time > _updateTime)
		{
			UpdateCheckNewMarkContentsShortCut();
			_updateTime = Time.time + 1f;
		}
	}

	private void UpdateCheckNewMarkContentsShortCut()
	{
		if (PortalData != null && PortalData.ContentsShortCutButton)
		{
			CheckNewMarkContentsShortCut();
		}
	}

	private void CheckNewMarkContentsShortCut()
	{
		if (!PortalData.HasChild)
		{
			return;
		}
		IList<PortalData> childDatas = GameRoot.Instance.PortalDataContainer.GetChildDatas(PortalData);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(childDatas))
		{
			return;
		}
		bool flag = false;
		foreach (PortalData item in childDatas)
		{
			AdventureNoticeData data = GameRoot.Instance.AdventureNoticeDataContainer.GetData(item.AdventureNoticeCode);
			if (data != null && !__Extension_Method_ICollection__.IsNullOrEmpty(data.OpenCondition) && NGame2.NUI.Common.CheckContentsOpenCondition(data.OpenCondition) && NGame2.NUI.Common.CheckContentsOpenCondition(item.Condition) && NGame2.NUI.Common.IsClearTutorial(data) && !NGame2.NUI.Common.CheckFirstOpen(data))
			{
				flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
			}
		}
		UIHelper.EnableGameObject(Widget_NewMark, flag);
	}

	public void CheckEnable()
	{
		bool flag = true;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(ChapterIndex);
		if (data != null)
		{
			flag = ((data.Type != ChapterType.City && __Extension_Method_ICollection__.IsNullOrEmpty(PortalData.Condition)) ? (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(ChapterIndex) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(ChapterIndex, DungeonIndex) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanOpenChapterPortal(ChapterIndex)) : CheckPotalDataCondtion());
		}
		if (_playOpenEffect)
		{
			flag = false;
		}
		OffComponent(!flag);
	}

	public bool CheckPotalDataCondtion()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(PortalData.Condition))
		{
			return false;
		}
		string valueString = ArgParser.GetValueString(PortalData.Condition, 0);
		ICondition condition = SingletonAuto<SwitchingContainer>.instance.GetCondition(valueString);
		if (condition == null)
		{
			return false;
		}
		return !condition.EnterConditionCheck(PortalData.Condition);
	}

	public void CheckEnable_NameTag()
	{
		bool flag = true;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(ChapterIndex);
		if (data != null)
		{
			flag = ((data.Type == ChapterType.City) ? (Helper.GetFunctionState(ChapterIndex, DungeonIndex, containsDisableText: true).State == Helper.FunctionStateData.EFunctionState.Enabled) : (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(ChapterIndex) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(ChapterIndex, DungeonIndex)));
		}
		OffComponent(!flag);
	}

	public void CheckEnable_ShortCutButton()
	{
		if (_listener != null && PortalData != null && !PortalData.HasChild)
		{
			bool flag = _listener.CanOpenShorCutBtn();
			UIHelper.SetWidgetGrayScale(ComponentWidget, !flag);
			UIHelper.EnableButton(Button_Portal, flag);
		}
	}

	protected void OffComponent(bool isOff, string text = null)
	{
		UIHelper.SetWidgetGrayScale(ComponentWidget, isOff);
		UIHelper.EnableGameObject(Sprite_Disable, isOff);
		UIHelper.EnableGameObject(Label_Disable, isOff);
		if (Label_Disable != null && text != null)
		{
			text = LocalizationManager.GetStringDirect(text);
			UIHelper.SetLabelText(Label_Disable, text);
		}
	}

	public bool CheckPropDataCondition(PortalPropData propData)
	{
		if (propData.ConditionType == PortalPropConditionType.BonusEvent)
		{
			EventBonusType valueEnum = ArgParser.GetValueEnum(propData.ConditionArgs, 0, EventBonusType.None);
			string valueString = ArgParser.GetValueString(propData.ConditionArgs, 1);
			if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum, valueString) != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool Selectable()
	{
		return !ComponentWidget.isGrayscale;
	}

	public bool hasChild()
	{
		return PortalData.HasChild;
	}

	public void RefreshComponentGrayScale()
	{
		UIHelper.EnableGameObject(ComponentWidget, enable: false);
		UIHelper.SetWidgetGrayScale(ComponentWidget, grayScale: false);
		UIHelper.EnableGameObject(ComponentWidget, enable: true);
		UIHelper.EnableGameObject(ComponentWidget, enable: false);
		UIHelper.EnableGameObject(ComponentWidget, enable: true);
	}

	private bool CheckShopRestockCondition(PortalPropData propData)
	{
		if (propData.ConditionType == PortalPropConditionType.ShopRestockEffect)
		{
			int valueInt = ArgParser.GetValueInt(propData.ConditionArgs, 1);
			EventBonusType valueEnum = ArgParser.GetValueEnum(propData.ConditionArgs, 2, EventBonusType.None);
			string valueString = ArgParser.GetValueString(propData.ConditionArgs, 3);
			DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum, valueString);
			if (valueEnum != EventBonusType.None && effectiveEvent == null)
			{
				return false;
			}
			return CheckShopRestockEffect(valueInt);
		}
		return false;
	}

	private bool CheckShopRestockEffect(int shopIndex)
	{
		bool result = false;
		ShopData data = GameRoot.Instance.ShopDataContainer.GetData(shopIndex);
		if (data != null)
		{
			ShopRestockTimeInfo shopRestockTimeInfo = GlobalAutoBehaviour<AccountManager>.instance.ShopRestockTimeManager.Get(data.Index);
			if (shopRestockTimeInfo != null)
			{
				DateTime o_dateTime = DateTime.MinValue;
				if (Vespa_String_Extension_Method2.TryParseNetworkTime2(shopRestockTimeInfo.RestockTime, out o_dateTime))
				{
					DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
					int num = ((data.RestockSecond > 0) ? data.RestockSecond : 3600);
					result = currentServerUTCTime > o_dateTime.AddSeconds(num);
				}
			}
			else
			{
				result = true;
			}
		}
		return result;
	}

	public void OnClickComponent()
	{
		if (!_isLock && _listener != null)
		{
			_listener.SelectPortal(this);
		}
	}

	public void OnClickButton()
	{
		if (!_isLock && _listener != null)
		{
			_listener.SelectButton(this);
		}
	}

	public void StartDrag()
	{
		if (_listener != null && !PortalData.hasParent())
		{
			_listener.StartDrag();
		}
	}

	public void OnDrag()
	{
		if (_listener != null && !PortalData.hasParent())
		{
			_listener.OnDrag();
		}
	}

	public void EndDrag()
	{
		if (_listener != null && !PortalData.hasParent())
		{
			_listener.EndDrag();
		}
	}
}
