using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.NMonsterPortraitSub;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPortrait;

[UIEvent(AllowDuringPatchDownloading = true)]
public sealed class MonsterComponent : UIScrollItemBehaviour, IInspectorGuiDrawer, InitOnCreate
{
	public new interface IListener
	{
		void OnClickViewInfo(HeroInfo heroInfo);
	}

	public struct StockData
	{
		public bool ShowQuestTag;

		public bool ShowName;

		public bool IsHidden;
	}

	[Serializable]
	public class GradeWidget
	{
		public UIWidget Widget;

		[SerializeField]
		private string _gradeName = string.Empty;

		public MonsterGradeType Grade
		{
			get
			{
				return EnumLookup<MonsterGradeType>.GetByName(_gradeName).EnumValue;
			}
			set
			{
				_gradeName = EnumLookup<MonsterGradeType>.GetByEnum(value).Name;
			}
		}
	}

	public enum ESubComponent
	{
		ClassTag
	}

	[Serializable]
	public class RootWidgetInfo
	{
		public ESubComponent SubComponent;

		public UIWidget RootWidget;
	}

	public const string PrefabPath = "Prefab2/Component/Portrait/Monster";

	public const string BossPrefabPath = "Prefab2/Component/Portrait/MonsterBoss";

	public static PrefabAuto<MonsterComponent> PrefabAuto = new PrefabAuto<MonsterComponent>("Prefab2/Component/Portrait/Monster");

	public static PrefabAuto<MonsterComponent> BossPrefabAuto = new PrefabAuto<MonsterComponent>("Prefab2/Component/Portrait/MonsterBoss");

	public RootWidgetInfo[] RootWidgetInfos;

	public UIWidget ThisWidget;

	public UIBundleLoader PortraitBundleTex;

	public UIWidget PortraitHidden;

	public UIWidget QuestTagWidget;

	public UIWidget BossTagWidget;

	public UILabel LevelLabel;

	public UILabel NamePrefixLabel;

	public UILabel NameLabel;

	public UIWidget HPRoot;

	public UISprite HPProgressSprite;

	public UILabel HPValueLabel;

	[HideInInspector]
	public List<GradeWidget> GradeWidgetList = new List<GradeWidget>();

	public SubComponentContainer<MonsterComponent> SubCompContainer = new SubComponentContainer<MonsterComponent>();

	private IListener _listener;

	private HeroTooltipTextInfo _tooltipTextInfo;

	private bool _disableClick;

	public MonsterHeroInfo Info { get; private set; }

	public StockData Stock { get; private set; }

	public CreatureData TableData { get; private set; }

	public MonsterTierData TierData { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		SubCompContainer.ReleaseSubComponent();
	}

	public void Open(MonsterHeroInfo info, StockData stock, IListener listener = null, bool bDisableClick = false, bool isShowHP = false)
	{
		Info = info;
		Stock = stock;
		_listener = listener;
		_tooltipTextInfo = null;
		_disableClick = bDisableClick;
		if (Info != null)
		{
			TableData = GameRoot.Instance.CreatureDataContainer.GetData(info.HeroInfo.HeroIndex);
			TierData = GameRoot.Instance.MonsterTierDataContainer.GetData(Info.Tier);
			UIHelper.SetLabelText(LevelLabel, info.HeroInfo.Level.ToString());
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GradeWidgetList); i++)
			{
				GradeWidget gradeWidget = GradeWidgetList[i];
				UIHelper.EnableGameObject(gradeWidget.Widget, gradeWidget.Grade == info.Grade);
			}
			if (stock.IsHidden)
			{
				UIHelper.EnableGameObject(PortraitBundleTex, enable: false);
				UIHelper.EnableGameObject(PortraitHidden, enable: true);
			}
			else
			{
				UIHelper.EnableGameObject(PortraitBundleTex, enable: true);
				UIHelper.EnableGameObject(PortraitHidden, enable: false);
				string heroPortraitPath = UIHelper.GetHeroPortraitPath(TableData.CodeName);
				UIHelper.SetBundleTexture(PortraitBundleTex, heroPortraitPath, UIHelper.EAssetTextureAssign.ClipBottom);
			}
			UIHelper.EnableGameObject(QuestTagWidget, Stock.ShowQuestTag);
			UIHelper.EnableGameObject(BossTagWidget, Info.IsBoss);
			UIHelper.EnableGameObject(HPRoot, isShowHP);
			SetupName();
			SetupClassTag();
		}
	}

	private void SetupName()
	{
		if (NameLabel == null)
		{
			return;
		}
		if (!Stock.ShowName)
		{
			UIHelper.EnableGameObject(NamePrefixLabel, enable: false);
			UIHelper.EnableGameObject(NameLabel, enable: false);
			return;
		}
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(TableData, Info.VisualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		UIHelper.EnableGameObject(NameLabel, enable: true);
		UIHelper.SetLabelText(NameLabel, creatureName);
		UIHelper.EnableGameObject(NamePrefixLabel, !String_Extension_Method.IsNullOrEmpty(creatureNamePrefix));
		if (!String_Extension_Method.IsNullOrEmpty(creatureNamePrefix))
		{
			UIHelper.SetLabelText(NamePrefixLabel, creatureNamePrefix);
		}
	}

	private void SetupClassTag()
	{
		if (TableData != null)
		{
			ClassTagSubComponent subComponent = GetSubComponent<ClassTagSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetTag(TableData.TagType);
			}
		}
	}

	public void OnShowTooltip()
	{
		HeroIconTooltip.Open(ThisWidget, GetTooltipTextInfo());
	}

	public void OnHideTooltip()
	{
		HeroIconTooltip.Hide(ThisWidget);
	}

	public void OnClickViewInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickViewInfo(Info);
		}
	}

	public void OnClickComponent()
	{
		if (!_disableClick)
		{
			MonsterInfoPopup.Open(Info, Stock, TableData, TierData);
		}
	}

	private HeroTooltipTextInfo GetTooltipTextInfo()
	{
		if (_tooltipTextInfo != null)
		{
			return _tooltipTextInfo;
		}
		_tooltipTextInfo = new HeroTooltipTextInfo();
		if (Info == null || TableData == null)
		{
			return _tooltipTextInfo;
		}
		if (Stock.IsHidden)
		{
			_tooltipTextInfo.Name.Assign(new EnumBuilder().Key(WorldMapEvent.WORLDMAP_EVENT_DUNGEN_HIDDEN_NAME));
			_tooltipTextInfo.Desc.Assign(new EnumBuilder().Key(WorldMapEvent.WORLDMAP_EVENT_DUNGEN_HIDDEN_DESC));
		}
		else
		{
			_tooltipTextInfo.Name.Assign(new CreatureNameBuilder(TableData, Info.VisualInfo, Info.HeroInfo.Level, appendLine: false));
			_tooltipTextInfo.Desc.Assign(new CreatureDescBuilder(TableData, Info.VisualInfo, appendLine: false));
			_tooltipTextInfo.CreatureTagType = TableData.TagType;
			if (TableData.AttrType != null)
			{
				_tooltipTextInfo.ListAttrTypes.AddRange(TableData.AttrType);
			}
		}
		return _tooltipTextInfo;
	}

	public T GetSubComponent<T>() where T : BaseSubComponent<MonsterComponent>
	{
		T subComponent = SubCompContainer.GetSubComponent<T>();
		if (subComponent != null)
		{
			return subComponent;
		}
		subComponent = SubCompContainer.CreateSubComponent<T>(base.gameObject.transform);
		subComponent.Open(this);
		return subComponent;
	}

	public bool ContainSubComponent<T>() where T : BaseSubComponent<MonsterComponent>
	{
		return SubCompContainer.GetSubComponent<T>() != null;
	}

	public bool ActiveSubComponent<T>() where T : BaseSubComponent<MonsterComponent>
	{
		return false;
	}

	public GameObject GetRootGO(Enum e)
	{
		if (e.GetType() != typeof(ESubComponent))
		{
			return null;
		}
		ESubComponent eSubComponent = (ESubComponent)(object)e;
		for (int i = 0; i < RootWidgetInfos.Length; i++)
		{
			if (RootWidgetInfos[i].SubComponent == eSubComponent && RootWidgetInfos[i].RootWidget != null)
			{
				return RootWidgetInfos[i].RootWidget.gameObject;
			}
		}
		return ThisWidget.gameObject;
	}

	public void SetHP(long hp, long max)
	{
		UIHelper.SetProgressValue(HPProgressSprite, HPValueLabel, hp, max, showMaxValue: false);
		bool isGrayscale = hp == 0;
		ThisWidget.isGrayscale = isGrayscale;
	}
}
