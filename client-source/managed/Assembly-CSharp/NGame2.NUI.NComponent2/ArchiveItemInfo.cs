using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI3.NEmbed;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveItemInfo : MonoBehaviour
{
	public enum EToggle
	{
		None,
		Option,
		Story
	}

	public UILabel LabelName;

	public UISimpleGrid IconGrid;

	public UILabel LabelMainStatType;

	public UILabel LabelMainStatValue;

	public UILabel LabelCreatureLimit;

	public UILabel LabelOptionDesc;

	public UITexture UniqueHeroTexture;

	public UIToggle ToggleOption;

	public UIToggle ToggleStory;

	public UIWidget MainStatRoot;

	public UIWidget ClassRoot;

	public UIWidget StarRoot;

	public UIButton Button3D;

	public UISimpleGrid ClassTagGrid;

	public UIWidget CreatureLimitRoot;

	public UIScrollView ScrollViewDesc;

	public UILabel LabelButtonName;

	public UIGame3DWidget EffectWidget;

	public UIWidget RuneSlotInfoRoot;

	public EquipRuneSlotInfoComponent EquipRuneSlotInfoComponent;

	[Header("--[ 장비 Star(각성) 표기 ]")]
	public Embed_EquipStarBox_Comp EquipStarBox;

	private ItemData _itemData;

	private ItemIconComponent _itemIconComponent;

	private List<ClassTagIconComponent> _classTagIconComponentList = new List<ClassTagIconComponent>();

	private int _star;

	private int _referencedLevel;

	private EToggle _toggle = EToggle.Option;

	private ArchiveSubType _subType;

	private IAsyncLoad<IEffect> _effectAsync;

	private bool isTechnoMagic;

	protected ItemAnimationContainer _itemAnimationContainer = new ItemAnimationContainer(open3dModelPopup: true);

	public void Init(ArchiveSubType subType, ItemData itemData)
	{
		_subType = subType;
		_itemData = itemData;
		string text = itemData.Name;
		UIHelper.SetLabelText(LabelName, text);
		_itemIconComponent = UIHelper.AddGridItem(IconGrid, ItemIconComponent.PrefabAuto);
		_itemIconComponent.Open(itemData);
		_referencedLevel = GameRoot.Instance.ConstantDataContainer.GetValueInt("ArchiveItemReferencedLevel", 1);
		EquipItemData subData = itemData.GetSubData<EquipItemData>();
		isTechnoMagic = subData != null && subData.WorkPlace == EquipItemWorkPlace.Valance;
		SetupInfo(subData, isInit: true);
		SetupUI();
	}

	public void Reset()
	{
		_itemAnimationContainer.Reset();
		if (_itemIconComponent != null)
		{
			FactoryBase.ReleaseComponent(_itemIconComponent);
			_itemIconComponent = null;
		}
		ClearClassItemIconComponentList();
		_star = 0;
		ScrollViewDesc.ResetPosition();
	}

	public void ChainUpdate()
	{
		if (_effectAsync != null)
		{
			_effectAsync.Value.Update();
		}
	}

	private void SetupUI()
	{
		bool enable = true;
		bool enable2 = true;
		bool enable3 = true;
		bool enable4 = true;
		string key = "EQUIP_ENCHANT.FIXED_OPTION";
		if (_subType == ArchiveSubType.Artifact)
		{
			enable = false;
		}
		else if (_subType == ArchiveSubType.EquipItem)
		{
			key = "HERO_INFO_SETEFFECT";
			enable4 = false;
		}
		else if (_subType == ArchiveSubType.NormalItem || _subType == ArchiveSubType.Rune)
		{
			enable = false;
			enable2 = false;
			enable3 = false;
			enable4 = false;
			key = "HERO_INFO";
		}
		UIHelper.EnableGameObject(MainStatRoot, enable);
		UIHelper.EnableGameObject(ClassRoot, enable2);
		UIHelper.EnableGameObject(StarRoot, enable3);
		UIHelper.EnableGameObject(Button3D, enable4);
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.SetLabelText(LabelButtonName, stringDirect);
	}

	private void ClearClassItemIconComponentList()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_classTagIconComponentList))
		{
			FactoryBase.ReleaseAndClearList(_classTagIconComponentList);
		}
	}

	private void SetupClassTagIcon(EquipItemData equipItemData)
	{
		if (ClassTagGrid == null)
		{
			return;
		}
		CreatureTagType[] tagType = equipItemData.TagType;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tagType))
		{
			return;
		}
		ClearClassItemIconComponentList();
		if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(equipItemData.EquipCode))
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			_classTagIconComponentList.Add(classTagIconComponent);
			classTagIconComponent.OpenAllClassTag(null);
			UIHelper.RepositionGrid(ClassTagGrid);
			return;
		}
		for (int num = tagType.Length - 1; num >= 0; num--)
		{
			CreatureTagType tagType2 = tagType[num];
			ClassTagIconComponent classTagIconComponent2 = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			if (!(classTagIconComponent2 == null))
			{
				_classTagIconComponentList.Add(classTagIconComponent2);
				classTagIconComponent2.Open(tagType2, null);
			}
		}
		UIHelper.RepositionGrid(ClassTagGrid);
	}

	private void SetupInfo(EquipItemData equipItemData, bool isInit = false)
	{
		SetupMainStat(equipItemData);
		if (_subType == ArchiveSubType.UniqueWeapon || _subType == ArchiveSubType.UniqueTreasure)
		{
			if (GameRoot.Instance.EquipItemDetailDataContainer.GetData(equipItemData.DetailIndex).EquipItemDetailType == EquipItemDetailType.UNIQUE_CLASSWEAPON)
			{
				UIHelper.EnableGameObject(CreatureLimitRoot, enable: false);
				UIHelper.EnableGameObject(ClassTagGrid, enable: true);
				SetupClassTagIcon(equipItemData);
			}
			else
			{
				UIHelper.EnableGameObject(CreatureLimitRoot, enable: true);
				UIHelper.EnableGameObject(ClassTagGrid, enable: false);
				SetupCreatureInfo(equipItemData);
			}
		}
		else if (_subType == ArchiveSubType.Artifact || _subType == ArchiveSubType.EquipItem)
		{
			UIHelper.EnableGameObject(CreatureLimitRoot, enable: false);
			UIHelper.EnableGameObject(ClassTagGrid, enable: true);
			SetupClassTagIcon(equipItemData);
		}
		_toggle = EToggle.Option;
		ToggleOption.Set(state: true);
		SetupOptionDesc();
		if (isInit)
		{
			SetStar(0);
		}
	}

	private void SetupMainStat(EquipItemData equipItemData)
	{
		if (equipItemData != null)
		{
			int level = _referencedLevel;
			if (_subType == ArchiveSubType.EquipItem)
			{
				level = GameRoot.Instance.EquipItemDetailDataContainer.GetEquipItemMaxLevel(equipItemData.DetailIndex, 1);
			}
			long statValue = 0L;
			if (equipItemData.GetMainStat(out var statType, out statValue, _star, equipItemData.DetailIndex, level))
			{
				UIHelper.SetLabelText(LabelMainStatType, statType);
				UIHelper.SetLabelText(LabelMainStatValue, statValue);
			}
		}
	}

	private void SetupCreatureInfo(EquipItemData equipItemData)
	{
		if (equipItemData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(equipItemData.CreatureIndex))
		{
			int num = equipItemData.CreatureIndex[0];
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(num);
			UIHelper.SetLabelText(LabelCreatureLimit, data.Name);
			AssetTexture heroIcon = UIHelper.GetHeroIcon2(num, 3);
			UIHelper.ResetPlaneAssetTextureAsync(this, UniqueHeroTexture, heroIcon);
		}
	}

	private void SetupOptionDesc()
	{
		EquipItemData subData = _itemData.GetSubData<EquipItemData>();
		UIHelper.EnableGameObject(RuneSlotInfoRoot, _itemData.Type == ItemType.Rune || _itemData.Type == ItemType.PunishmentRune);
		if (subData == null)
		{
			if (_itemData.Type == ItemType.Potion)
			{
				string text = new PotionItemDataBuilder(_itemData, appendLine: false).Build();
				UIHelper.SetLabelText(LabelOptionDesc, text);
			}
			else if (_itemData.Type == ItemType.Flask)
			{
				ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(_itemData.Index);
				if (data != null)
				{
					string text2 = _itemData.Description2.Replace("{1}", data.GetSubData<FlaskItemData>().FlaskExp.ToString());
					UIHelper.SetLabelText(LabelOptionDesc, text2);
				}
				else
				{
					UIHelper.SetLabelText(LabelOptionDesc, _itemData.Description2);
				}
			}
			else if (_itemData.Type == ItemType.Rune)
			{
				if (!(_itemData is ItemData<RuneItemData> itemData))
				{
					return;
				}
				string empty = string.Empty;
				string empty2 = string.Empty;
				Dictionary<Enum, long> statTypeAndValue = itemData.SubData.GetStatTypeAndValue();
				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
				foreach (KeyValuePair<Enum, long> item in statTypeAndValue)
				{
					empty = LocalizationManager.GetString(item.Key);
					empty2 = StatData.GetStatDisplayValue(item.Key, item.Value);
					stringBuilder.Append(empty).Append(" : ").Append(empty2)
						.Append("\r\n");
				}
				if (EquipRuneSlotInfoComponent != null)
				{
					RuneSlotType[] runeSlotTypes = RuneHelper.GetRuneSlotTypes(_itemData);
					EquipRuneSlotInfoComponent.DirectRefresh(runeSlotTypes.ToList());
				}
				UIHelper.SetLabelText(LabelOptionDesc, stringBuilder.ToString());
			}
			else if (_itemData.Type == ItemType.Package)
			{
				UIHelper.SetLabelText(LabelOptionDesc, string.Empty);
			}
			else
			{
				UIHelper.SetLabelText(LabelOptionDesc, _itemData.Description2);
			}
			return;
		}
		if (_itemData.Type == ItemType.PunishmentRune)
		{
			PunishmentRuneData data2 = GameRoot.Instance.PunishmentRuneDataContainer.GetData(_itemData.Index);
			System.Text.StringBuilder stringBuilder2 = new System.Text.StringBuilder();
			if (data2 != null)
			{
				PunishmentRuneOptionData data3 = GameRoot.Instance.PunishmentRuneOptionDataContainer.GetData(data2.MainOptionIndex);
				if (data3 != null)
				{
					string text3 = LocalizationManager.GetString(data3.StatTypes[0]);
					string statDisplayValue = StatData.GetStatDisplayValue(data3.StatTypes[0], data3.OptionValueMin0, data3.OptionValueMax0);
					stringBuilder2.AppendLine(text3 + " : " + statDisplayValue);
					stringBuilder2.AppendLine();
					if (!__Extension_Method_ICollection__.IsNullOrEmpty(data2.GuideSkillCode))
					{
						stringBuilder2.AppendLine(LocalizationManager.GetStringDirect("RUNE_OPTION_CLASS_1"));
						stringBuilder2.AppendLine(LocalizationManager.GetStringDirect(data2.GuideSkillCode[0]));
						stringBuilder2.AppendLine();
						stringBuilder2.AppendLine(LocalizationManager.GetStringDirect("RUNE_OPTION_CLASS_2"));
						stringBuilder2.AppendLine(LocalizationManager.GetStringDirect(data2.GuideSkillCode[1]));
						stringBuilder2.AppendLine();
						stringBuilder2.AppendLine(LocalizationManager.GetStringDirect("RUNE_OPTION_CLASS_3"));
						stringBuilder2.AppendLine(LocalizationManager.GetStringDirect(data2.GuideSkillCode[2]));
					}
				}
			}
			if (EquipRuneSlotInfoComponent != null)
			{
				RuneSlotType[] runeSlotTypes2 = RuneHelper.GetRuneSlotTypes(_itemData);
				EquipRuneSlotInfoComponent.DirectRefresh(runeSlotTypes2.ToList());
			}
			UIHelper.SetLabelText(LabelOptionDesc, stringBuilder2.ToString());
			return;
		}
		int fixedOptionIndex = subData.GetFixedOptionIndex(1);
		if (fixedOptionIndex != 0)
		{
			EquipOptionData data4 = GameRoot.Instance.EquipOptionDataContainer.GetData(fixedOptionIndex);
			if (data4 == null)
			{
				VespaDebug.LogError("Option data not found : " + fixedOptionIndex);
				return;
			}
			OperationType optionType = data4.OptionType;
			int optionValue = data4.GetOptionValue(_star, 0);
			string optionDisplay = StatData.GetOptionDisplay(optionType, optionValue);
			UIHelper.SetLabelText(LabelOptionDesc, optionDisplay);
			return;
		}
		UIHelper.SetLabelText(LabelOptionDesc, "");
		EquipSetBonusData data5 = GameRoot.Instance.EquipSetBonusDataContainer.GetData(subData.SetIndex);
		if (data5 == null)
		{
			return;
		}
		data5.GetOptionIndexList(0, out var _, out var closedOptionIndexList);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(closedOptionIndexList))
		{
			return;
		}
		int num = 0;
		System.Text.StringBuilder stringBuilder3 = new System.Text.StringBuilder();
		foreach (int item2 in closedOptionIndexList)
		{
			int reqSetEquipCount = data5.GetReqSetEquipCount(++num);
			EquipOptionData data6 = GameRoot.Instance.EquipOptionDataContainer.GetData(item2);
			if (data6 == null)
			{
				VespaDebug.LogError("Option data not found : " + fixedOptionIndex);
				return;
			}
			OperationType optionType2 = data6.OptionType;
			int optionValue2 = data6.GetOptionValue(0, 0);
			string setOptionDisplay = StatData.GetSetOptionDisplay(optionType2, optionValue2, opened: false, reqSetEquipCount, appendColor: false);
			stringBuilder3.Append(setOptionDisplay).Append("\r\n");
		}
		UIHelper.SetLabelText(LabelOptionDesc, stringBuilder3.ToString());
	}

	public void OnToggle()
	{
		EToggle eToggle = EToggle.None;
		if (ToggleOption.value)
		{
			eToggle = EToggle.Option;
		}
		else if (ToggleStory.value)
		{
			eToggle = EToggle.Story;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			switch (_toggle)
			{
			case EToggle.Option:
				SetupOptionDesc();
				break;
			case EToggle.Story:
			{
				string empty = string.Empty;
				UIHelper.SetLabelText(text: (!String_Extension_Method.IsNullOrEmpty(_itemData.ItemStory)) ? LocalizationManager.GetStringDirect(_itemData.ItemStory) : LocalizationManager.GetStringDirect("ARCHIVE_ITEM_NO_STORY"), label: LabelOptionDesc);
				break;
			}
			}
			ScrollViewDesc.ResetPosition();
		}
	}

	public void OnClickButton3D()
	{
		Item3DBook window = SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.GetInstance<Item3DBook>();
		window.Open(delegate
		{
			window.Init(_itemData);
		});
	}

	public void OnClickNext()
	{
		ChangeStar(bPositive: true);
	}

	public void OnClickPrev()
	{
		ChangeStar(bPositive: false);
	}

	private void SetStar(int star)
	{
		_star = star;
		if (EquipStarBox != null)
		{
			EquipStarBox.InitUi(_star, isTechnoMagic);
		}
	}

	private void ChangeStar(bool bPositive)
	{
		EquipItemData subData = _itemData.GetSubData<EquipItemData>();
		if (subData != null)
		{
			_star += (bPositive ? 1 : (-1));
			if (bPositive)
			{
				int maxStar_Available = Item_Equip_MaxStar.GetMaxStar_Available(_itemData);
				_star = Mathf.Min(_star, maxStar_Available);
			}
			else
			{
				_star = Mathf.Max(0, _star);
			}
			SetStar(_star);
			SetupInfo(subData);
			ScrollViewDesc.ResetPosition();
		}
	}
}
