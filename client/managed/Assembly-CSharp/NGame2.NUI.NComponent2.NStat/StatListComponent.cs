using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDev;
using NGame2.NUI.NHelper.NPet;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NStat;

public class StatListComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	[Serializable]
	public class ColorInfo
	{
		public Color LabelColor = Color.white;

		public bool Gradient;

		public ColorInfo()
		{
		}

		public ColorInfo(Color color)
		{
			LabelColor = color;
		}
	}

	[Serializable]
	public class ColorInfos
	{
		public ColorInfo StatColor;

		public ColorInfo BeforeValueColor;

		public ColorInfo AfterPlusValueColor;

		public ColorInfo AfterMinusValueColor;
	}

	private interface IWidget
	{
		GameObject GameObject { get; }

		int Width { get; set; }
	}

	private class BaseWidget : IWidget
	{
		public GameObject GameObject { get; set; }

		public UIWidget Widget { get; set; }

		public int Width
		{
			get
			{
				return Widget.width;
			}
			set
			{
				Widget.width = value;
			}
		}

		public int Height => Widget.height;
	}

	public class StatWidget<T> : IWidget where T : MonoBehaviour, IStatInfo
	{
		public GameObject GameObject => Component.gameObject;

		public T Component { get; set; }

		public UIWidget Widget { get; set; }

		public int Width
		{
			get
			{
				return Widget.width;
			}
			set
			{
				Widget.width = value;
			}
		}
	}

	public enum EStatChangeTemplate
	{
		Default,
		ForStarUpgrade,
		ForStarUpgradeResult
	}

	public enum EOptionChangeTemplate
	{
		ForStarUpgrade,
		ForStarUpgradeResult
	}

	public const string PrefabPath = "Prefab2/Component/Stat/StatListComponent2";

	public static PrefabAuto<StatListComponent> PrefabAuto = new PrefabAuto<StatListComponent>("Prefab2/Component/Stat/StatListComponent2");

	public const string PrefabPath2 = "Prefab2/Component/Valance/ValanceStatListComponent";

	public static PrefabAuto<StatListComponent> PrefabAuto2 = new PrefabAuto<StatListComponent>("Prefab2/Component/Valance/ValanceStatListComponent");

	public const string PrefabPath3 = "Prefab2/Component/Stat/PetStatListComponent";

	public static PrefabAuto<StatListComponent> PrefabAuto3 = new PrefabAuto<StatListComponent>("Prefab2/Component/Stat/PetStatListComponent");

	public static ColorInfo DefaultSubjectColor = new ColorInfo
	{
		LabelColor = Color.white,
		Gradient = false
	};

	public static ColorInfo DefaultStatColor = new ColorInfo
	{
		LabelColor = new Color32(byte.MaxValue, 102, 0, byte.MaxValue),
		Gradient = false
	};

	public static ColorInfo DefaultValueColor = new ColorInfo
	{
		LabelColor = new Color32(byte.MaxValue, 102, 0, byte.MaxValue),
		Gradient = false
	};

	public static ColorInfo DefaultDescColor = new ColorInfo
	{
		LabelColor = new Color32(226, 140, 83, byte.MaxValue),
		Gradient = false
	};

	public UIWidget RootWidget;

	public Transform TemplateRoot;

	public StatSubjectInfo BiggerSubjectTemplate;

	public StatSubjectInfo SubjectTemplate;

	public SingleStatInfo NormalStatTemplate;

	public SingleStatInfo StringStatTemplate;

	public StatChangeInfo StatChangeTemplate;

	public StatChangeInfo SkillOptionChangeTemplate;

	public SubTreasureComponent SubTreasureComponentTemplete;

	public GameObject SplitterTemplate;

	public GameObject SpacingTemplate;

	public UITable RootTable;

	public ColorInfo BiggerSubjectColor;

	public ColorInfo FixedOptionStatSubjectColor;

	public ColorInfo FixedOptionStatTypeColor;

	public ColorInfo FixedOptionStatValueColor;

	public ColorInfo OptionStatSubjectColor;

	public ColorInfo OptionStatTypeColor;

	public ColorInfo OptionClosedStatTypeColor;

	public ColorInfo OptionStatValueColor;

	public ColorInfo OptionClosedStatValueColor;

	public ColorInfo RenewOptionStatTypeColor;

	public ColorInfo RenewOptionStatValueColor;

	public ColorInfo ClosedRenewOptionStatTypeColor;

	public ColorInfo ClosedRenewOptionStatValueColor;

	public StatSetOption SetOption;

	private List<IWidget> _createWidgetList = new List<IWidget>();

	private bool _repositionTable;

	private int _repositionCount;

	private const int RepositionCount = 2;

	private UIPanel _parentPanel;

	public static StatListComponent Create(UIPanel panel, int margin = 0, bool isValance = false)
	{
		if (panel == null)
		{
			return null;
		}
		Vector4 finalClipRegion = panel.finalClipRegion;
		Vector2 size = new Vector2(finalClipRegion.z, finalClipRegion.w);
		Vector3 position = new Vector3(finalClipRegion.x, finalClipRegion.y + size.y / 2f);
		StatListComponent obj = ((!isValance) ? Create(panel.transform, size, position, margin) : ValanceStatCreate(panel.transform, size, position, margin));
		obj._parentPanel = panel;
		return obj;
	}

	public static StatListComponent Create(UIWidget widget, int margin = 0, bool findParentPanel = true, bool isValance = false)
	{
		if (widget == null)
		{
			return null;
		}
		Vector2 size = new Vector2(widget.width, widget.height);
		Vector3 zero = Vector3.zero;
		StatListComponent statListComponent = ((!isValance) ? Create(widget.transform, size, zero, margin) : ValanceStatCreate(widget.transform, size, zero, margin));
		if (findParentPanel)
		{
			UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(widget.gameObject);
			if (uIPanel != null)
			{
				statListComponent._parentPanel = uIPanel;
			}
		}
		return statListComponent;
	}

	private static StatListComponent Create(Transform transform, Vector2 size, Vector3 position, int margin = 0)
	{
		if (transform == null)
		{
			return null;
		}
		StatListComponent statListComponent = PrefabAuto.Factory.Create(transform);
		if ((bool)statListComponent.RootWidget)
		{
			statListComponent.RootWidget.SetDimensions((int)size.x - margin * 2, 2);
			statListComponent.RootWidget.transform.localPosition = position;
		}
		return statListComponent;
	}

	private static StatListComponent ValanceStatCreate(Transform transform, Vector2 size, Vector3 position, int margin = 0)
	{
		if (transform == null)
		{
			return null;
		}
		StatListComponent statListComponent = PrefabAuto2.Factory.Create(transform);
		if ((bool)statListComponent.RootWidget)
		{
			statListComponent.RootWidget.SetDimensions((int)size.x - margin * 2, 2);
			statListComponent.RootWidget.transform.localPosition = position;
		}
		return statListComponent;
	}

	public static StatListComponent PetStatCreate(UIPanel panel, int margin = 0)
	{
		if (panel == null)
		{
			return null;
		}
		Vector4 finalClipRegion = panel.finalClipRegion;
		Vector2 vector = new Vector2(finalClipRegion.z, finalClipRegion.w);
		Vector3 localPosition = new Vector3(finalClipRegion.x, finalClipRegion.y + vector.y / 2f);
		StatListComponent statListComponent = PrefabAuto3.Factory.Create(panel.transform);
		if ((bool)statListComponent.RootWidget)
		{
			statListComponent.RootWidget.SetDimensions((int)vector.x - margin * 2, 2);
			statListComponent.RootWidget.transform.localPosition = localPosition;
		}
		statListComponent._parentPanel = panel;
		return statListComponent;
	}

	public static void AddBiggerSubject(StatListComponent component, string subject)
	{
		component.AddBigSubject(subject);
	}

	public static void AddExtraOptionStat(StatListComponent component, EquipItemInfo equipItemInfo, bool addSubject = true)
	{
		if (equipItemInfo == null || !equipItemInfo.IsTechnoMagic)
		{
			return;
		}
		if (addSubject)
		{
			component.AddFixedOptionSubject(LocalizationManager.GetStringDirect("EQUIP_OPTION_ADDITIONALSKILL"));
		}
		if (!equipItemInfo.IsIdentified)
		{
			component.AddEmptyOptionStat();
			return;
		}
		List<EquipItemOptionInfo> extraOptionInfoList = equipItemInfo.GetExtraOptionInfoList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(extraOptionInfoList); i++)
		{
			EquipItemOptionInfo equipItemOptionInfo = extraOptionInfoList[i];
			if (equipItemOptionInfo != null)
			{
				component.AddOptionStat(equipItemOptionInfo.OptionIndex, equipItemInfo.Star, equipItemOptionInfo.OptionStep, opened: true, 0, equipItemOptionInfo.IsRenewedOption, isFixedOption: true);
			}
		}
	}

	public static void AddExtraOptionStat(StatListComponent component, EquipItemData equipItemData, EquipItemInfo equipItemInfo, bool addSubject = true)
	{
		if (equipItemInfo != null)
		{
			AddExtraOptionStat(component, equipItemInfo, addSubject);
		}
		else if (equipItemData != null && equipItemData.EquipType == EquipItemEquipType.TechnoMagic)
		{
			if (addSubject)
			{
				component.AddFixedOptionSubject(LocalizationManager.GetStringDirect("EQUIP_OPTION_ADDITIONALSKILL"));
			}
			string typeName = LocalizationManager.GetString(Equip.EQUIP_OPTION_STAT_TEXT);
			component.AddCustomStat(typeName, null, component.OptionClosedStatValueColor);
		}
	}

	public static void AddOptionStat(StatListComponent component, EquipItemData equipItemData, EquipItemInfo equipItemInfo, bool addSubject = true)
	{
		if (equipItemData == null)
		{
			return;
		}
		if (equipItemInfo != null && equipItemInfo.IsTechnoMagic && !equipItemInfo.IsIdentified)
		{
			if (addSubject)
			{
				component.AddFixedOptionSubject(LocalizationManager.GetString(EquipEnchant.RANDOM_OPTION));
			}
			component.AddEmptyOptionStat();
			component.AddEmptyOptionStat();
		}
		else
		{
			AddFixedOptionStat(component, equipItemData, equipItemInfo, addSubject);
			AddRandomOptionStat(component, equipItemData, equipItemInfo, addSubject);
		}
	}

	public static void AddFixedOptionStat(StatListComponent component, EquipItemData equipItemData, EquipItemInfo equipItemInfo, bool addSubject = true)
	{
		int star = equipItemInfo?.Star ?? 0;
		AddFixedOptionStat(component, equipItemData, star, addSubject);
	}

	public static void AddFixedOptionStat(StatListComponent component, EquipItemData equipItemData, int star, bool addSubject = true)
	{
		int num = 1;
		while (true)
		{
			int fixedOptionIndex = equipItemData.GetFixedOptionIndex(num);
			if (fixedOptionIndex != 0)
			{
				if (num == 1 && addSubject)
				{
					component.AddFixedOptionSubject(LocalizationManager.GetString(EquipEnchant.FIXED_OPTION));
				}
				component.AddFixedOptionStat(fixedOptionIndex, star);
				num++;
				continue;
			}
			break;
		}
	}

	public static void AddRandomOptionStat(StatListComponent component, EquipItemData equipItemData, EquipItemInfo equipItemInfo, bool addSubject = true)
	{
		if (equipItemInfo == null)
		{
			return;
		}
		foreach (EquipItemOptionInfo optionInfo in equipItemInfo.GetOptionInfoList())
		{
			if (optionInfo.SlotIndex == 1 && addSubject)
			{
				component.AddOptionSubject(LocalizationManager.GetString(EquipEnchant.RANDOM_OPTION));
			}
			int optionOpenLevel = equipItemInfo.GetOptionOpenLevel(optionInfo.SlotIndex);
			component.AddOptionStat(optionInfo, equipItemInfo.Star, optionOpenLevel, isFixedOption: false);
		}
	}

	public static void AddItemIconAndRandomOptionStat(StatListComponent component, EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			component.AddSubTreasureComponentTemplete(equipItemInfo);
		}
	}

	public static void AddEnchantOptionStat(StatListComponent component, EquipItemInfo equipItemInfo, bool addSubject = true)
	{
		if (equipItemInfo != null && equipItemInfo.EnchantOptionExists())
		{
			if (addSubject)
			{
				component.AddEnchantOptionSubject(LocalizationManager.GetString(EquipEnchant.ENCHANT_OPTION));
			}
			component.AddEnchantOptionStat(equipItemInfo.EnchantOptionIndex1, equipItemInfo.EnchantOptionStep1);
			component.AddEnchantOptionStat(equipItemInfo.EnchantOptionIndex2, equipItemInfo.EnchantOptionStep2);
			component.AddEnchantOptionStat(equipItemInfo.EnchantOptionIndex3, equipItemInfo.EnchantOptionStep3);
		}
	}

	private static void GetClosedSetOption(EquipItemData equipItemData, out List<EquipOptionData> closedOptionDataList)
	{
		closedOptionDataList = null;
		GameRoot.Instance.EquipSetBonusDataContainer.GetData(equipItemData.SetIndex).GetOptionIndexList(0, out var _, out var closedOptionIndexList);
		if (closedOptionIndexList == null)
		{
			return;
		}
		for (int i = 0; i < closedOptionIndexList.Count; i++)
		{
			int key = closedOptionIndexList[i];
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(key);
			if (data != null)
			{
				closedOptionDataList = closedOptionDataList ?? new List<EquipOptionData>();
				closedOptionDataList.Add(data);
			}
		}
	}

	private static void ToggleSetOptionIcon(StatListComponent component, CachedHeroInfo heroInfo, EquipSetBonusData selectedSetData)
	{
		if (heroInfo == null)
		{
			return;
		}
		foreach (EquipItemInfo equipItemInfo in heroInfo.GetEquipItemInfoList())
		{
			EquipSetBonusData equipSetBonusData = equipItemInfo.GetEquipSetBonusData();
			if (equipSetBonusData == null || selectedSetData.SetIndex != equipSetBonusData.SetIndex)
			{
				continue;
			}
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
			if (data != null)
			{
				EquipItemData subData = data.GetSubData<EquipItemData>();
				if (subData != null)
				{
					component.SetOption.Toggle(subData.PartType, isOn: true);
				}
			}
		}
	}

	public static void AddSetOptionStat(StatListComponent component, CachedHeroInfo heroInfo, EquipItemData equipItemData, EquipItemInfo selectedEquipItemInfo, bool addSubject = true)
	{
		if (equipItemData == null)
		{
			return;
		}
		EquipSetBonusData data = GameRoot.Instance.EquipSetBonusDataContainer.GetData(equipItemData.SetIndex);
		if (data == null)
		{
			return;
		}
		List<EquipOptionData> openedOptionDataList = null;
		List<EquipOptionData> closedOptionDataList = null;
		if (selectedEquipItemInfo == null)
		{
			GetClosedSetOption(equipItemData, out closedOptionDataList);
		}
		else
		{
			component.SetOption.Init();
			if (DevParam.GetDevParamBool("CheckSetItemOptionAccount") && GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(selectedEquipItemInfo.SlotIndex) == null)
			{
				GetClosedSetOption(equipItemData, out closedOptionDataList);
				ToggleSetOptionIcon(component, heroInfo, data);
			}
			else if (heroInfo == null)
			{
				GetClosedSetOption(equipItemData, out closedOptionDataList);
			}
			else
			{
				heroInfo.GetSelectedSetOptionList(selectedEquipItemInfo, out openedOptionDataList, out closedOptionDataList);
				ToggleSetOptionIcon(component, heroInfo, data);
			}
		}
		if (addSubject)
		{
			component.AddSetOptionSubject(LocalizationManager.GetString(EquipEnchant.SET_OPTION), heroInfo != null);
		}
		int num = 0;
		if (openedOptionDataList != null)
		{
			foreach (EquipOptionData item in openedOptionDataList)
			{
				int reqSetEquipCount = data.GetReqSetEquipCount(++num);
				component.AddSetOptionStat(item.Index, opened: true, reqSetEquipCount, renew: false);
			}
		}
		if (closedOptionDataList == null)
		{
			return;
		}
		foreach (EquipOptionData item2 in closedOptionDataList)
		{
			int reqSetEquipCount2 = data.GetReqSetEquipCount(++num);
			component.AddSetOptionStat(item2.Index, opened: false, reqSetEquipCount2, renew: false);
		}
	}

	public static void AddDescription(StatListComponent component, string desc)
	{
		if (!(component == null))
		{
			component.AddDescriptionSubject();
			component.AddCustomStat(desc, null, DefaultDescColor);
		}
	}

	public static void AddSpacing(StatListComponent component)
	{
		if (!(component == null))
		{
			component.AddSpacing();
		}
	}

	public static int GetTotalHeight(StatListComponent component)
	{
		if (component == null)
		{
			return 0;
		}
		return component.GetTotalHeight();
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		Reset();
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Reset();
	}

	private void Reset()
	{
		UIHelper.EnableGameObject(TemplateRoot, enable: false);
		foreach (IWidget createWidget in _createWidgetList)
		{
			VespaObject.DestroyGoAndDetachFromParent(createWidget.GameObject);
		}
		if (RootTable != null && RootTable.onReposition != null)
		{
			RootTable.onReposition = null;
		}
		_createWidgetList.Clear();
	}

	private void Update()
	{
		UpdateHeight();
	}

	private void LateUpdate()
	{
		UpdateHeight();
	}

	public void UpdateHeight()
	{
		if (!_repositionTable)
		{
			return;
		}
		_repositionCount--;
		if (_repositionCount <= 0)
		{
			_repositionTable = false;
		}
		RootTable.Reposition();
		if (_parentPanel != null)
		{
			UIScrollView component = _parentPanel.GetComponent<UIScrollView>();
			if ((bool)component)
			{
				component.ResetPosition();
			}
		}
	}

	private void AddCreatedWidget(IWidget widget, bool dirty = true)
	{
		if (widget == null)
		{
			return;
		}
		GameObject gameObject = widget.GameObject;
		if (!(gameObject == null))
		{
			widget.Width = RootWidget.width;
			UIHelper.EnableGameObject(gameObject, enable: true);
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, RootTable.transform);
			_createWidgetList.Add(widget);
			RootTable.Reposition();
			if (dirty)
			{
				_repositionTable = true;
				_repositionCount = 2;
			}
		}
	}

	public static void SetLabelOverflowOption(UILabel label, UILabel.Overflow option)
	{
		if (!(label == null))
		{
			label.overflowMethod = option;
		}
	}

	public static void SetLabelGradient(UILabel label, bool gradient)
	{
		if (!(label == null))
		{
			label.applyGradient = gradient;
		}
	}

	private StatWidget<SingleStatInfo> CreateStat(string typeName, string value, bool isStringStat, ColorInfo statColor, ColorInfo valueColor, SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal, bool shrinkContents = false, bool enableIcon = true)
	{
		statColor = statColor ?? DefaultStatColor;
		valueColor = valueColor ?? DefaultValueColor;
		GameObject gameObject = null;
		gameObject = (isStringStat ? UnityEngine.Object.Instantiate(StringStatTemplate.gameObject) : UnityEngine.Object.Instantiate(NormalStatTemplate.gameObject));
		SingleStatInfo component = gameObject.GetComponent<SingleStatInfo>();
		UIWidget component2 = gameObject.GetComponent<UIWidget>();
		UIHelper.SetLabelText(component.NameLabel, NGUIText.EncodeColor(typeName, statColor.LabelColor));
		UIHelper.SetLabelText(component.ValueLabel, NGUIText.EncodeColor(value, valueColor.LabelColor));
		if (shrinkContents)
		{
			SetLabelOverflowOption(component.NameLabel, UILabel.Overflow.ShrinkContent);
			SetLabelOverflowOption(component.ValueLabel, UILabel.Overflow.ShrinkContent);
		}
		if (String_Extension_Method.IsNullOrEmpty(value))
		{
			component.NameLabel.rightAnchor.absolute = 0;
			component.NameLabel.UpdateAnchors();
		}
		SetLabelGradient(component.NameLabel, statColor.Gradient);
		SetLabelGradient(component.ValueLabel, valueColor.Gradient);
		if (enableIcon)
		{
			component.SelectIcon(iconType);
		}
		else
		{
			component.DisableAllIcon();
		}
		StatWidget<SingleStatInfo> statWidget = new StatWidget<SingleStatInfo>
		{
			Component = component,
			Widget = component2
		};
		AddCreatedWidget(statWidget);
		return statWidget;
	}

	private StatWidget<SingleStatInfo> CreateNormalStat(Enum statType, long totalValue, ColorInfo statColor, ColorInfo valueColor, SingleStatInfo.StatIconType iconType)
	{
		string typeName = LocalizationManager.GetString(statType);
		string statDisplayValue = StatData.GetStatDisplayValue(statType, totalValue);
		return CreateStat(typeName, statDisplayValue, isStringStat: false, statColor, valueColor, iconType);
	}

	private StatWidget<StatChangeInfo> CreateStatChange(string typeName, string beforeValue, string afterValue, EStatChangeTemplate useTemplate, ColorInfo statColor, ColorInfo beforeValueColor, ColorInfo afterValueColor)
	{
		statColor = statColor ?? DefaultStatColor;
		beforeValueColor = beforeValueColor ?? DefaultValueColor;
		afterValueColor = afterValueColor ?? DefaultValueColor;
		StatWidget<StatChangeInfo> statWidget = null;
		GameObject gameObject = UnityEngine.Object.Instantiate(StatChangeTemplate.gameObject);
		if (gameObject != null)
		{
			StatChangeInfo component = gameObject.GetComponent<StatChangeInfo>();
			UIWidget component2 = gameObject.GetComponent<UIWidget>();
			UIHelper.SetLabelText(component.NameLabel, NGUIText.EncodeColor(typeName, statColor.LabelColor));
			UIHelper.SetLabelText(component.BeforeValueLabel, NGUIText.EncodeColor(beforeValue, beforeValueColor.LabelColor));
			UIHelper.SetLabelText(component.AfterValueLabel, NGUIText.EncodeColor(afterValue, afterValueColor.LabelColor));
			SetLabelGradient(component.NameLabel, statColor.Gradient);
			SetLabelGradient(component.BeforeValueLabel, beforeValueColor.Gradient);
			SetLabelGradient(component.AfterValueLabel, afterValueColor.Gradient);
			statWidget = new StatWidget<StatChangeInfo>
			{
				Component = component,
				Widget = component2
			};
			AddCreatedWidget(statWidget);
			return statWidget;
		}
		return null;
	}

	private StatWidget<StatChangeInfo> CreateStatChange(Enum statType, string beforeValue, string afterValue, EStatChangeTemplate useTemplate, ColorInfo statColor, ColorInfo beforeValueColor, ColorInfo afterValueColor)
	{
		string typeName = LocalizationManager.GetString(statType);
		return CreateStatChange(typeName, beforeValue, afterValue, useTemplate, statColor, beforeValueColor, afterValueColor);
	}

	private StatWidget<StatChangeInfo> CreateSkillOptionChange(string beforeValue, string afterValue, ColorInfo beforeValueColor, ColorInfo afterValueColor, EOptionChangeTemplate useTemplate)
	{
		beforeValueColor = beforeValueColor ?? DefaultValueColor;
		afterValueColor = afterValueColor ?? DefaultValueColor;
		StatWidget<StatChangeInfo> statWidget = null;
		GameObject gameObject = UnityEngine.Object.Instantiate(SkillOptionChangeTemplate.gameObject);
		if (gameObject != null)
		{
			StatChangeInfo component = gameObject.GetComponent<StatChangeInfo>();
			UIWidget component2 = gameObject.GetComponent<UIWidget>();
			UIHelper.SetLabelText(component.BeforeValueLabel, NGUIText.EncodeColor(beforeValue, beforeValueColor.LabelColor));
			UIHelper.SetLabelText(component.AfterValueLabel, NGUIText.EncodeColor(afterValue, afterValueColor.LabelColor));
			SetLabelGradient(component.BeforeValueLabel, beforeValueColor.Gradient);
			SetLabelGradient(component.AfterValueLabel, afterValueColor.Gradient);
			statWidget = new StatWidget<StatChangeInfo>
			{
				Component = component,
				Widget = component2
			};
			AddCreatedWidget(statWidget);
			return statWidget;
		}
		return null;
	}

	public void AddNormalStat(Enum statType, long totalValue)
	{
		CreateNormalStat(statType, totalValue, null, null, SingleStatInfo.StatIconType.Normal);
	}

	public StatWidget<SingleStatInfo> AddCustomStat(string typeName, object value, ColorInfo statColor = null, ColorInfo valueColor = null, bool isResizable = false, SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal, bool shirinkContents = false, bool enableIcon = true)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		return CreateStat(typeName, value.ToString(), isStringStat: false, statColor, valueColor, iconType, shirinkContents, enableIcon);
	}

	public StatWidget<SingleStatInfo> AddStringStat(string typeName, ColorInfo statColor = null, ColorInfo valueColor = null, SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal)
	{
		return CreateStat(typeName, string.Empty, isStringStat: true, statColor, valueColor, iconType);
	}

	public void AddChangedStat(Enum statType, long beforeValue, long afterValue, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfo statColor = null, ColorInfo beforeValueColor = null, ColorInfo afterValueColor = null)
	{
		bool showSign = useTemplate == EStatChangeTemplate.Default;
		string statDisplayValue = StatData.GetStatDisplayValue(statType, beforeValue, showSign);
		string statDisplayValue2 = StatData.GetStatDisplayValue(statType, afterValue, showSign);
		CreateStatChange(statType, statDisplayValue, statDisplayValue2, useTemplate, statColor, beforeValueColor, afterValueColor);
	}

	public void AddSubTreasureComponentTemplete(EquipItemInfo equipItemInfo)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
		if (data != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(SubTreasureComponentTemplete.gameObject);
			SubTreasureComponent component = obj.GetComponent<SubTreasureComponent>();
			UIWidget component2 = obj.GetComponent<UIWidget>();
			component.SetItemInfo(data, equipItemInfo);
			StatWidget<SubTreasureComponent> widget = new StatWidget<SubTreasureComponent>
			{
				Component = component,
				Widget = component2
			};
			AddCreatedWidget(widget);
		}
	}

	public void AddBigSubject(string subject, ColorInfo subjectColor = null)
	{
		CreateSubject(subject, isBig: true, subjectColor);
	}

	private void AddSubject(string subject, ColorInfo subjectColor = null, bool activeSetOption = false)
	{
		CreateSubject(subject, isBig: false, subjectColor, activeSetOption);
	}

	private void CreateSubject(string subject, bool isBig, ColorInfo subjectColor = null, bool activeSetOption = false)
	{
		ColorInfo colorInfo = null;
		GameObject gameObject = null;
		if (isBig)
		{
			colorInfo = BiggerSubjectColor;
			gameObject = BiggerSubjectTemplate.gameObject;
		}
		else
		{
			colorInfo = DefaultSubjectColor;
			gameObject = SubjectTemplate.gameObject;
		}
		subjectColor = subjectColor ?? colorInfo;
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		StatSubjectInfo component = gameObject2.GetComponent<StatSubjectInfo>();
		UIWidget component2 = gameObject2.GetComponent<UIWidget>();
		UIHelper.SetLabelText(component.NameLabel, NGUIText.EncodeColor(subject, subjectColor.LabelColor));
		SetLabelGradient(component.NameLabel, subjectColor.Gradient);
		UIHelper.EnableGameObject(component.SetOption, activeSetOption);
		BaseWidget widget = new BaseWidget
		{
			GameObject = gameObject2,
			Widget = component2
		};
		AddCreatedWidget(widget);
	}

	public void AddFixedOptionSubject(string subject)
	{
		AddSubject(subject, FixedOptionStatSubjectColor);
	}

	public void AddOptionSubject(string subject)
	{
		AddSubject(subject, OptionStatSubjectColor);
	}

	public void AddEnchantOptionSubject(string subject)
	{
		AddSubject(subject, OptionStatSubjectColor);
	}

	public void AddDescriptionSubject()
	{
		AddSubject(LocalizationManager.GetStringDirect("COMMON_DETAIL_INFO"), OptionStatSubjectColor);
	}

	public void AddSetOptionSubject(string subject, bool activeSetOption = false)
	{
		AddSubject(subject, OptionStatSubjectColor, activeSetOption);
	}

	public void AddSplitter()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(SplitterTemplate);
		UIWidget component = gameObject.GetComponent<UIWidget>();
		BaseWidget widget = new BaseWidget
		{
			GameObject = gameObject,
			Widget = component
		};
		AddCreatedWidget(widget, dirty: false);
	}

	public void AddSpacing()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(SpacingTemplate);
		UIWidget component = gameObject.GetComponent<UIWidget>();
		BaseWidget widget = new BaseWidget
		{
			GameObject = gameObject,
			Widget = component
		};
		AddCreatedWidget(widget, dirty: false);
	}

	private List<StatType> GetStatTypeOfSameVisibleGroup(int groupIdx, StatType keyType)
	{
		List<StatType> list = new List<StatType>();
		foreach (ClientStatDisplayScoreData allDatum in GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetAllData())
		{
			if (allDatum.EquipItemVisibleGroup == groupIdx && allDatum.StatType != keyType)
			{
				list.Add(allDatum.StatType);
			}
		}
		return list;
	}

	private void RemoveDuplicatedStat(Dictionary<StatType, long> statDic)
	{
		List<StatType> list = new List<StatType>();
		foreach (KeyValuePair<StatType, long> item in statDic)
		{
			if (list.Contains(item.Key))
			{
				continue;
			}
			int equipItemVisibleGroup = GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(item.Key).EquipItemVisibleGroup;
			if (equipItemVisibleGroup == 0)
			{
				continue;
			}
			foreach (StatType item2 in GetStatTypeOfSameVisibleGroup(equipItemVisibleGroup, item.Key))
			{
				if (statDic.ContainsKey(item2) && item.Value == statDic[item2])
				{
					list.Add(item2);
				}
			}
		}
		foreach (StatType item3 in list)
		{
			statDic.Remove(item3);
		}
	}

	public void AddStat(StatData statData)
	{
		if (statData == null)
		{
			return;
		}
		Dictionary<StatType, long> visibleStats = statData.GetVisibleStats(SkillAttrType.None, all: false);
		RemoveDuplicatedStat(visibleStats);
		foreach (KeyValuePair<StatType, long> item in visibleStats)
		{
			StatType key = item.Key;
			long value = item.Value;
			AddNormalStat(key, value);
		}
	}

	private void RemoveDuplicatedStatChange(List<StatType> changeList, StatData beforeStatData, StatData afterStatData)
	{
		List<StatType> list = new List<StatType>();
		for (int i = 1; i < changeList.Count; i++)
		{
			if (GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(changeList[i]).EquipItemVisibleGroup == 0)
			{
				continue;
			}
			for (int j = 0; j < i; j++)
			{
				if (GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(changeList[i]).EquipItemVisibleGroup == GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(changeList[j]).EquipItemVisibleGroup && beforeStatData.GetStat(changeList[i]) == beforeStatData.GetStat(changeList[j]) && afterStatData.GetStat(changeList[i]) == afterStatData.GetStat(changeList[j]))
				{
					list.Add(changeList[i]);
				}
			}
		}
		foreach (StatType item in list)
		{
			changeList.Remove(item);
		}
	}

	public void AddAwakenChange(int beforeStar, int afterStar, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfos colorInfos = null)
	{
		if (colorInfos == null)
		{
			colorInfos = new ColorInfos();
		}
		ColorInfo afterValueColor = ((afterStar > beforeStar) ? colorInfos.AfterPlusValueColor : colorInfos.AfterMinusValueColor);
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_EQUIP_AWAKEN_NOTICE");
		CreateStatChange(stringDirect, beforeStar.ToString(), afterStar.ToString(), useTemplate, colorInfos.StatColor, colorInfos.BeforeValueColor, afterValueColor);
	}

	public void AddFailBonusChange(int beforeFailBonus, int afterFailBonus, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfos colorInfos = null)
	{
		if (colorInfos == null)
		{
			colorInfos = new ColorInfos();
		}
		ColorInfo afterValueColor = colorInfos.BeforeValueColor;
		if (afterFailBonus > beforeFailBonus)
		{
			afterValueColor = colorInfos.AfterPlusValueColor;
		}
		else if (afterFailBonus < beforeFailBonus)
		{
			afterValueColor = colorInfos.AfterMinusValueColor;
		}
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_EQUIP_FAIL_BONUS_NOTICE");
		string beforeValue = $"{beforeFailBonus}%";
		string afterValue = $"{afterFailBonus}%";
		CreateStatChange(stringDirect, beforeValue, afterValue, useTemplate, colorInfos.StatColor, colorInfos.BeforeValueColor, afterValueColor);
	}

	public bool AddStatChangeColorInfos(StatData beforeStatData, StatData afterStatData, bool all = false, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfos colorInfos = null)
	{
		if (colorInfos == null)
		{
			colorInfos = new ColorInfos();
		}
		return AddStatChange(beforeStatData, afterStatData, all, useTemplate, colorInfos.StatColor, colorInfos.BeforeValueColor, colorInfos.AfterPlusValueColor, colorInfos.AfterMinusValueColor);
	}

	public bool AddStatChange(StatData beforeStatData, StatData afterStatData, bool all = false, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfo statColor = null, ColorInfo beforeValueColor = null, ColorInfo afterPlusValueColor = null, ColorInfo afterMinusValueColor = null)
	{
		if (beforeStatData == null || afterStatData == null)
		{
			return false;
		}
		List<StatType> list = new List<StatType>();
		List<StatType> list2 = new List<StatType>();
		List<StatType> list3 = new List<StatType>();
		StatType[] visibleStats = StatData.GetVisibleStats();
		foreach (StatType statType in visibleStats)
		{
			long stat = beforeStatData.GetStat(statType);
			long stat2 = afterStatData.GetStat(statType);
			if (stat != 0L || stat2 != 0L)
			{
				if (stat2 > stat)
				{
					list.Add(statType);
				}
				else if (stat2 < stat)
				{
					list3.Add(statType);
				}
				else
				{
					list2.Add(statType);
				}
			}
		}
		RemoveDuplicatedStatChange(list, beforeStatData, afterStatData);
		RemoveDuplicatedStatChange(list2, beforeStatData, afterStatData);
		RemoveDuplicatedStatChange(list3, beforeStatData, afterStatData);
		bool result = false;
		foreach (StatType item in list)
		{
			long stat3 = beforeStatData.GetStat(item);
			long stat4 = afterStatData.GetStat(item);
			AddChangedStat(item, stat3, stat4, useTemplate, statColor, beforeValueColor, afterPlusValueColor);
			result = true;
		}
		foreach (StatType item2 in list2)
		{
			long stat5 = beforeStatData.GetStat(item2);
			long stat6 = afterStatData.GetStat(item2);
			AddChangedStat(item2, stat5, stat6, useTemplate, statColor, beforeValueColor, beforeValueColor);
			result = true;
		}
		foreach (StatType item3 in list3)
		{
			long stat7 = beforeStatData.GetStat(item3);
			long stat8 = afterStatData.GetStat(item3);
			AddChangedStat(item3, stat7, stat8, useTemplate, statColor, beforeValueColor, afterMinusValueColor);
			result = true;
		}
		return result;
	}

	public void AddOptionChangeColorInfos(EquipItemOptionInfo optionInfo, int beforeStar, int afterStar, bool all = false, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfos colorInfos = null)
	{
		if (colorInfos == null)
		{
			colorInfos = new ColorInfos();
		}
		AddOptionChange(optionInfo, beforeStar, afterStar, all, useTemplate, colorInfos.StatColor, colorInfos.BeforeValueColor, colorInfos.AfterPlusValueColor, colorInfos.AfterMinusValueColor);
	}

	public void AddOptionChange(EquipItemOptionInfo optionInfo, int beforeStar, int afterStar, bool all = false, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfo statColor = null, ColorInfo beforeValueColor = null, ColorInfo afterPlusValueColor = null, ColorInfo afterMinusValueColor = null)
	{
		if (optionInfo == null)
		{
			return;
		}
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
		if (data != null)
		{
			int optionValue = data.GetOptionValue(beforeStar, optionInfo.OptionStep);
			int optionValue2 = data.GetOptionValue(afterStar, optionInfo.OptionStep);
			if (optionValue < optionValue2)
			{
				AddChangedStat(data.OptionType, optionValue, optionValue2, useTemplate, statColor, beforeValueColor, afterPlusValueColor);
			}
			else if (optionValue > optionValue2)
			{
				AddChangedStat(data.OptionType, optionValue, optionValue2, useTemplate, statColor, beforeValueColor, afterMinusValueColor);
			}
		}
	}

	public StatWidget<SingleStatInfo> AddFixedOptionStat(int optionIndex, int star)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			VespaDebug.LogError("Option data not found : " + optionIndex);
			return null;
		}
		OperationType optionType = data.OptionType;
		int optionValue = data.GetOptionValue(star, 0);
		string optionDisplay = StatData.GetOptionDisplay(optionType, optionValue);
		return AddStringStat(optionDisplay, OptionStatTypeColor, OptionStatValueColor);
	}

	public StatWidget<SingleStatInfo> AddEmptyOptionStat()
	{
		ColorInfo optionStatTypeColor = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		string stringDirect = LocalizationManager.GetStringDirect("EQUIP_TECHNOMAGIC_UNIDENTIFIED");
		SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal;
		return AddCustomStat(stringDirect, "", optionStatTypeColor, optionStatValueColor, isResizable: false, iconType);
	}

	public StatWidget<SingleStatInfo> AddOptionStat(EquipItemOptionInfo optionInfo, int star, int openLevel, bool isFixedOption)
	{
		return AddOptionStat(optionInfo.OptionIndex, star, optionInfo.OptionStep, optionInfo.IsOptionOpened, openLevel, optionInfo.IsRenewedOption, isFixedOption);
	}

	public StatWidget<SingleStatInfo> AddOptionStat(int optionIndex, int star, int optionStep, bool opened, int openLevel, bool renew, bool isFixedOption)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			VespaDebug.LogError("Option data not found : " + optionIndex);
			return null;
		}
		ColorInfo statColor = OptionStatTypeColor;
		ColorInfo valueColor = OptionStatValueColor;
		if (renew)
		{
			statColor = (opened ? RenewOptionStatTypeColor : ClosedRenewOptionStatTypeColor);
			valueColor = (opened ? RenewOptionStatValueColor : ClosedRenewOptionStatValueColor);
		}
		OperationType optionType = data.OptionType;
		int optionValue = data.GetOptionValue(star, optionStep);
		string optionDisplay = StatData.GetOptionDisplay(optionType, optionValue, opened, openLevel, !renew, isFixedOption);
		SingleStatInfo.StatIconType iconType = (renew ? SingleStatInfo.StatIconType.Renew : SingleStatInfo.StatIconType.Normal);
		return AddCustomStat(optionDisplay, "", statColor, valueColor, isResizable: false, iconType);
	}

	public StatWidget<SingleStatInfo> AddSetOptionStat(int optionIndex, bool opened, int setCount, bool renew)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			VespaDebug.LogError("Option data not found : " + optionIndex);
			return null;
		}
		ColorInfo optionStatTypeColor = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		OperationType optionType = data.OptionType;
		int optionValue = data.GetOptionValue(0, 0);
		string setOptionDisplay = StatData.GetSetOptionDisplay(optionType, optionValue, opened, setCount, !renew);
		SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal;
		return AddStringStat(setOptionDisplay, optionStatTypeColor, optionStatValueColor, iconType);
	}

	public StatWidget<SingleStatInfo> AddPetStat(int optionIndex, int petStar)
	{
		ColorInfo optionStatTypeColor = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		string optionName = "";
		string optionValue = "";
		if (!PetHelper.TryGetPetOptionLabelText(optionIndex, petStar, ref optionName, ref optionValue))
		{
			return null;
		}
		return CreateStat(optionName, optionValue, isStringStat: false, optionStatTypeColor, optionStatValueColor);
	}

	public StatWidget<SingleStatInfo> AddPetBonusStat(int optionIndex, int petStar)
	{
		ColorInfo optionStatTypeColor = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		string optionName = "";
		string optionValue = "";
		if (!PetHelper.TryGetPetBonusOptionLabelText(optionIndex, petStar, ref optionName, ref optionValue))
		{
			return null;
		}
		return CreateStat(optionName, optionValue, isStringStat: false, optionStatTypeColor, optionStatValueColor);
	}

	public StatWidget<SingleStatInfo> AddPetStat(string name, string text)
	{
		ColorInfo optionStatTypeColor = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal;
		return CreateStat(name, text, isStringStat: false, optionStatTypeColor, optionStatValueColor, iconType);
	}

	public StatWidget<StatChangeInfo> AddPetStatChange(int optionIndex, int beforeStar, int afterStar)
	{
		ColorInfo optionStatTypeColor = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		ColorInfo renewOptionStatValueColor = RenewOptionStatValueColor;
		PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			return null;
		}
		string optionName = "";
		string optionValue = "";
		string optionValue2 = "";
		if (!PetHelper.TryGetPetOptionLabelText(data, beforeStar, ref optionName, ref optionValue) & !PetHelper.TryGetPetOptionValueLabelText(data, afterStar, ref optionValue2))
		{
			return null;
		}
		return CreatePetStatChange(optionName, optionValue, optionValue2, optionStatTypeColor, optionStatValueColor, renewOptionStatValueColor);
	}

	public StatWidget<StatChangeInfo> AddPetBonusStatChange(int optionIndex, int beforeStar, int afterStar)
	{
		ColorInfo optionStatTypeColor = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		ColorInfo renewOptionStatValueColor = RenewOptionStatValueColor;
		PetBonusData data = GameRoot.Instance.PetBonusDataContainer.GetData(optionIndex);
		if (data == null)
		{
			return null;
		}
		string optionName = "";
		string optionValue = "";
		string optionValue2 = "";
		if (!PetHelper.TryGetPetBonusOptionLabelText(data, beforeStar, ref optionName, ref optionValue) & !PetHelper.TryGetPetBonusOptionValueLabelText(data, afterStar, ref optionValue2))
		{
			return null;
		}
		return CreatePetStatChange(optionName, optionValue, optionValue2, optionStatTypeColor, optionStatValueColor, renewOptionStatValueColor);
	}

	public StatWidget<StatChangeInfo> AddPetStatChange(string beforeText, string afterText)
	{
		_ = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		ColorInfo renewOptionStatValueColor = RenewOptionStatValueColor;
		return CreateSkillOptionChange(beforeText, afterText, optionStatValueColor, renewOptionStatValueColor, EOptionChangeTemplate.ForStarUpgrade);
	}

	private StatWidget<StatChangeInfo> CreatePetStatChange(string typeName, string beforeValue, string afterValue, ColorInfo statColor, ColorInfo beforeValueColor, ColorInfo afterValueColor)
	{
		statColor = statColor ?? DefaultStatColor;
		beforeValueColor = beforeValueColor ?? DefaultValueColor;
		afterValueColor = afterValueColor ?? DefaultValueColor;
		StatWidget<StatChangeInfo> statWidget = null;
		GameObject gameObject = UnityEngine.Object.Instantiate(StatChangeTemplate.gameObject);
		if (gameObject != null)
		{
			StatChangeInfo component = gameObject.GetComponent<StatChangeInfo>();
			UIWidget component2 = gameObject.GetComponent<UIWidget>();
			UIHelper.SetLabelText(component.NameLabel, NGUIText.EncodeColor(typeName, statColor.LabelColor));
			UIHelper.SetLabelText(component.BeforeValueLabel, NGUIText.EncodeColor(beforeValue, beforeValueColor.LabelColor));
			UIHelper.SetLabelText(component.AfterValueLabel, NGUIText.EncodeColor(afterValue, afterValueColor.LabelColor));
			SetLabelGradient(component.NameLabel, statColor.Gradient);
			SetLabelGradient(component.BeforeValueLabel, beforeValueColor.Gradient);
			SetLabelGradient(component.AfterValueLabel, afterValueColor.Gradient);
			statWidget = new StatWidget<StatChangeInfo>
			{
				Component = component,
				Widget = component2
			};
			AddCreatedWidget(statWidget);
			return statWidget;
		}
		return null;
	}

	public void AddIntegerStatChange(string typeName, int before, int after, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfos colorInfos = null, bool isPercentage = false, bool isEnableBeforeStat = true, string disableBeforeStatString = "", bool isEnableAfterStat = true, string disableAfterStatString = "")
	{
		if (colorInfos == null)
		{
			colorInfos = new ColorInfos();
		}
		string text = "";
		string text2 = "";
		text = ((!isEnableBeforeStat) ? ((!isPercentage) ? disableBeforeStatString : $"{disableBeforeStatString}%") : ((!isPercentage) ? before.ToString() : $"{before}%"));
		text2 = ((!isEnableAfterStat) ? ((!isPercentage) ? disableAfterStatString : $"{disableAfterStatString}%") : ((!isPercentage) ? after.ToString() : $"{after}%"));
		ColorInfo afterValueColor = colorInfos.StatColor;
		if (before < after)
		{
			afterValueColor = colorInfos.AfterPlusValueColor;
		}
		else if (before > after)
		{
			afterValueColor = colorInfos.AfterMinusValueColor;
		}
		CreateStatChange(typeName, text, text2, useTemplate, colorInfos.StatColor, colorInfos.BeforeValueColor, afterValueColor);
	}

	public void AddFloatStatChange(string typeName, float before, float after, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfos colorInfos = null, bool isPercentage = false, bool isEnableBeforeStat = true, string disableBeforeStatString = "", bool isEnableAfterStat = true, string disableAfterStatString = "")
	{
		if (colorInfos == null)
		{
			colorInfos = new ColorInfos();
		}
		string text = "";
		string text2 = "";
		text = ((!isEnableBeforeStat) ? ((!isPercentage) ? disableBeforeStatString : $"{disableBeforeStatString}%") : ((!isPercentage) ? before.ToString() : $"{before}%"));
		text2 = ((!isEnableAfterStat) ? ((!isPercentage) ? disableAfterStatString : $"{disableAfterStatString}%") : ((!isPercentage) ? after.ToString() : $"{after}%"));
		ColorInfo afterValueColor = colorInfos.StatColor;
		if (before < after)
		{
			afterValueColor = colorInfos.AfterPlusValueColor;
		}
		else if (before > after)
		{
			afterValueColor = colorInfos.AfterMinusValueColor;
		}
		CreateStatChange(typeName, text, text2, useTemplate, colorInfos.StatColor, colorInfos.BeforeValueColor, afterValueColor);
	}

	public void AddCustomStringFormatStatChange(string typeName, string before, string after, Func<int> compareCallback, EStatChangeTemplate useTemplate = EStatChangeTemplate.Default, ColorInfos colorInfos = null)
	{
		if (colorInfos == null)
		{
			colorInfos = new ColorInfos();
		}
		if (compareCallback != null)
		{
			int num = Vespa_System_Action_Extension_Method.fire(compareCallback);
			ColorInfo afterValueColor = colorInfos.StatColor;
			if (num > 0)
			{
				afterValueColor = colorInfos.AfterPlusValueColor;
			}
			else if (num < 0)
			{
				afterValueColor = colorInfos.AfterMinusValueColor;
			}
			CreateStatChange(typeName, before, after, useTemplate, colorInfos.StatColor, colorInfos.BeforeValueColor, afterValueColor);
		}
	}

	public StatWidget<SingleStatInfo> AddEnchantOptionStat(int optionIndex, int optionStep, bool renew = false)
	{
		if (optionIndex == 0)
		{
			return null;
		}
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			VespaDebug.LogError("Option data not found : " + optionIndex);
			return null;
		}
		ColorInfo statColor = OptionStatTypeColor;
		ColorInfo valueColor = OptionStatValueColor;
		if (renew)
		{
			statColor = RenewOptionStatTypeColor;
			valueColor = RenewOptionStatValueColor;
		}
		OperationType optionType = data.OptionType;
		int optionValue = data.GetOptionValue(0, optionStep);
		string enchantOptionDisplay = StatData.GetEnchantOptionDisplay(optionType, optionValue, optionValue, string.Empty, appendColor: false);
		SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal;
		return AddCustomStat(enchantOptionDisplay, "", statColor, valueColor, isResizable: false, iconType);
	}

	public StatWidget<SingleStatInfo> AddEmptyEnchantOptionStat()
	{
		ColorInfo optionStatTypeColor = OptionStatTypeColor;
		ColorInfo optionStatValueColor = OptionStatValueColor;
		string typeName = LocalizationManager.GetString(EquipEnchant.EMPTY_ENCHANT_OPTION);
		SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal;
		return AddCustomStat(typeName, "", optionStatTypeColor, optionStatValueColor, isResizable: false, iconType);
	}

	public bool AddOptionChangeSkill(int optionIndex, int beforeStar, int afterStar, EOptionChangeTemplate useTemplate, ColorInfos colorInfos)
	{
		if (colorInfos == null)
		{
			colorInfos = new ColorInfos();
		}
		return AddOptionChangeSkill(optionIndex, beforeStar, afterStar, useTemplate, colorInfos.BeforeValueColor, colorInfos.AfterPlusValueColor, colorInfos.AfterMinusValueColor);
	}

	public bool AddOptionChangeSkill(int optionIndex, int beforeStar, int afterStar, EOptionChangeTemplate useTemplate, ColorInfo beforeValueColor = null, ColorInfo afterPlusValueColor = null, ColorInfo afterMinusValueColor = null)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			VespaDebug.LogError("Option data not found : " + optionIndex);
			return false;
		}
		if (data.OptionType != OperationType.UseSkill)
		{
			return false;
		}
		int optionValueMax = data.GetOptionValueMax(beforeStar);
		int optionValueMax2 = data.GetOptionValueMax(afterStar);
		string optionDisplay = StatData.GetOptionDisplay(data.OptionType, optionValueMax, opened: true, 0, appendColor: false);
		string optionDisplay2 = StatData.GetOptionDisplay(data.OptionType, optionValueMax2, opened: true, 0, appendColor: false);
		if (optionValueMax < optionValueMax2)
		{
			CreateSkillOptionChange(optionDisplay, optionDisplay2, beforeValueColor, afterPlusValueColor, useTemplate);
		}
		else if (optionValueMax > optionValueMax2)
		{
			CreateSkillOptionChange(optionDisplay, optionDisplay2, beforeValueColor, afterMinusValueColor, useTemplate);
		}
		return true;
	}

	public int GetTotalHeight()
	{
		int num = 0;
		IWidget last = Vespa_Generic_IList_Extension_Method.GetLast(_createWidgetList);
		if (last == null)
		{
			return num;
		}
		GameObject gameObject = last.GameObject;
		if (gameObject == null)
		{
			return num;
		}
		UIWidget uIWidget = SingletonAuto<LinkedTransform<UIWidget>>.instance.Get(gameObject.transform);
		if (uIWidget != null)
		{
			if (uIWidget.pivot == UIWidget.Pivot.Left || uIWidget.pivot == UIWidget.Pivot.Center || uIWidget.pivot == UIWidget.Pivot.Right)
			{
				num -= uIWidget.height / 2;
			}
			else if (uIWidget.pivot == UIWidget.Pivot.Bottom || uIWidget.pivot == UIWidget.Pivot.BottomLeft || uIWidget.pivot == UIWidget.Pivot.BottomRight)
			{
				num -= uIWidget.height;
			}
			num += (int)(0f - gameObject.transform.localPosition.y);
			if (uIWidget.coverChildBounds)
			{
				num += uIWidget.height;
			}
			else
			{
				UIWidget[] componentsInChildren = uIWidget.GetComponentsInChildren<UIWidget>(includeInactive: false);
				int num2 = 0;
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
				{
					num2 = Mathf.Max(componentsInChildren[i].height, num2);
				}
				num += num2;
			}
		}
		return num;
	}
}
