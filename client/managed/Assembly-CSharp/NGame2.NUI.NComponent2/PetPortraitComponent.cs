using System;
using NGame2.NUI.NComponent2.NPetPortraitSub;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PetPortraitComponent : UIScrollItemBehaviour, InitOnCreate, IInspectorGuiDrawer
{
	[Serializable]
	public class PrefabSelector
	{
		public enum EPrefabType
		{
			Small,
			Large
		}

		public EPrefabType PrefabType;

		public PrefabAuto<PetPortraitComponent> PrefabAuto
		{
			get
			{
				if (PrefabType == EPrefabType.Small)
				{
					return SmallPrefabAuto;
				}
				if (PrefabType == EPrefabType.Large)
				{
					return LargePrefabAuto;
				}
				return SmallPrefabAuto;
			}
		}

		public PrefabSelector(EPrefabType prefabType)
		{
			PrefabType = prefabType;
		}
	}

	public enum ESubComponent
	{
		SleepMark,
		MainPet,
		NewMark,
		ExclamationMark,
		PetStar,
		Frame,
		Grade,
		Select,
		Owned,
		Name,
		DisableStatus
	}

	public enum EPortraitMode
	{
		Icon,
		Long
	}

	public new interface IListener
	{
		void OnClick(PetPortraitComponent comp);
	}

	[Serializable]
	public class RootWidgetInfo
	{
		public ESubComponent SubComponent;

		public UIWidget RootWidget;
	}

	public static string SmallPrefabPath = "Prefab2/Component/Portrait/PetPortraitSmall";

	public static string LargePrefabPath = "Prefab2/Component/Portrait/PetPortraitLarge";

	public static PrefabAuto<PetPortraitComponent> SmallPrefabAuto = new PrefabAuto<PetPortraitComponent>(SmallPrefabPath);

	public static PrefabAuto<PetPortraitComponent> LargePrefabAuto = new PrefabAuto<PetPortraitComponent>(LargePrefabPath);

	public UIWidget WidgetThis;

	public UIBundleLoader PetBundleTex;

	public UILabel PetIndexLabel;

	public RootWidgetInfo[] RootWidgetInfos;

	private IListener _listener;

	private bool _selected;

	public SubComponentContainer<PetPortraitComponent> SubCompContainer = new SubComponentContainer<PetPortraitComponent>();

	public PetInfo PetInfo { get; set; }

	public CreatureData CreatureData { get; private set; }

	public EPortraitMode PortraitMode { get; private set; }

	public int VisualStar { get; private set; }

	public bool IsSelected => _selected;

	public int PetIndex
	{
		get
		{
			if (PetInfo == null)
			{
				return 0;
			}
			return PetInfo.PetIndex;
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		SetGrayScale(grayScale: false);
		Close();
	}

	public T GetSubComponent<T>() where T : BaseSubComponent<PetPortraitComponent>
	{
		T subComponent = SubCompContainer.GetSubComponent<T>();
		if (subComponent != null)
		{
			return subComponent;
		}
		subComponent = SubCompContainer.CreateSubComponent<T>(base.gameObject.transform);
		if (subComponent == null)
		{
			return null;
		}
		subComponent.Open(this);
		return subComponent;
	}

	public bool ContainSubComponent<T>() where T : BaseSubComponent<PetPortraitComponent>
	{
		return SubCompContainer.GetSubComponent<T>() != null;
	}

	public bool ActiveSubComponent<T>() where T : BaseSubComponent<PetPortraitComponent>
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
		return WidgetThis.gameObject;
	}

	public void Open(PetInfo petInfo, IListener listener, EPortraitMode portraitMode = EPortraitMode.Icon)
	{
		PetInfo = petInfo;
		_listener = listener;
		PortraitMode = portraitMode;
		SetupInfo();
	}

	public void Open(CreatureData creatureData, IListener listener, EPortraitMode portraitMode = EPortraitMode.Icon, int visualStar = 0)
	{
		CreatureData = creatureData;
		_listener = listener;
		PortraitMode = portraitMode;
		SetupInfo();
	}

	public void OpenByIndex(int petIndex, IListener listener, EPortraitMode portraitMode = EPortraitMode.Icon, int visualStar = 0)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(petIndex);
		if (data == null)
		{
			VespaDebug.LogError($"CreatureData is null : petIndex {petIndex}");
			return;
		}
		CreatureData = data;
		_listener = listener;
		PortraitMode = portraitMode;
		SetupInfo();
	}

	public void ResetPortrait(PetInfo petInfo, CreatureData creatureData)
	{
		PetInfo = petInfo;
		CreatureData = creatureData;
		UIHelper.EnableGameObject(PetBundleTex, enable: false);
		SetupInfo();
	}

	public void OpenAndRefresh(PetInfo petInfo, IListener listener, EPortraitMode portraitMode = EPortraitMode.Icon)
	{
		PetInfo = petInfo;
		_listener = listener;
		PortraitMode = portraitMode;
		CreatureData = null;
		SetupInfo();
	}

	private void SetupInfo()
	{
		if (CreatureData == null && PetInfo != null)
		{
			CreatureData = PetInfo.CreatureData;
		}
		UIHelper.EnableGameObject(PetIndexLabel, enable: false);
		SetVisualStar();
		SetPetImage();
		SetPetFrame();
		if (PetInfo != null)
		{
			SetStarMark(show: true);
			return;
		}
		SetMainPet(show: false);
		SetExclamationMark(show: false);
		SetNew(show: false);
		SetSleep(show: false);
		SetStarMark(show: false);
	}

	private void SetVisualStar()
	{
		SetVisualStar(CreatureData);
		SetVisualStar(PetInfo);
	}

	private void SetVisualStar(CreatureData creatureData)
	{
		VisualStar = creatureData?.StartHeroStar ?? 0;
	}

	private void SetVisualStar(PetInfo petInfo)
	{
		VisualStar = petInfo?.Star ?? 0;
	}

	public void Close()
	{
		CreatureData = null;
		VisualStar = 0;
		_selected = false;
		SetGrayScale(grayScale: false);
		SubCompContainer.ReleaseSubComponent();
	}

	public void SetPetImage()
	{
		if (CreatureData == null)
		{
			UIHelper.EnableGameObject(PetBundleTex, enable: false);
			return;
		}
		string empty = string.Empty;
		UIHelper.EAssetTextureAssign assetTexAssign = UIHelper.EAssetTextureAssign.None;
		if (PortraitMode == EPortraitMode.Long)
		{
			empty = UIHelper.GetPetPortraitPath(CreatureData.Index);
			assetTexAssign = UIHelper.EAssetTextureAssign.ClipCenter;
		}
		else
		{
			empty = UIHelper.GetPetIconPath(CreatureData.Index);
		}
		UIHelper.SetBundleTexture(PetBundleTex, empty, assetTexAssign);
		UIHelper.EnableGameObject(PetBundleTex, enable: true);
	}

	public void SetPetFrame()
	{
		if (CreatureData == null)
		{
			return;
		}
		if (PortraitMode == EPortraitMode.Long)
		{
			PetFrameSubComponent subComponent = GetSubComponent<PetFrameSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetFrame(EPortraitMode.Long);
			}
		}
		else
		{
			SetPetGradeFrame();
		}
	}

	public void SetPetGradeFrame()
	{
		if (CreatureData != null)
		{
			PetGradeSubComponent subComponent = GetSubComponent<PetGradeSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetGrade(CreatureData.Grade);
			}
		}
	}

	public void SetMainPet(bool show)
	{
		MainPetMarkSubComponent subComponent = GetSubComponent<MainPetMarkSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(show);
		}
	}

	public void SetExclamationMark(bool show)
	{
		ExclamationMarkSubComponent subComponent = GetSubComponent<ExclamationMarkSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(show);
		}
	}

	public void SetNew(bool show)
	{
		if (show || ContainSubComponent<NewMarkSubComponent>())
		{
			NewMarkSubComponent subComponent = GetSubComponent<NewMarkSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetShow(show);
			}
		}
	}

	public void SetSleep(bool show)
	{
		SleepMarkSubComponent subComponent = GetSubComponent<SleepMarkSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(show);
		}
	}

	public void SetStarMark(bool show)
	{
		StarSubComponent subComponent = GetSubComponent<StarSubComponent>();
		if (subComponent != null)
		{
			subComponent.Show(show);
		}
	}

	public void SetGrayScale(bool grayScale)
	{
		UIHelper.SetWidgetGrayScale(WidgetThis, grayScale);
	}

	public void SetSelection(bool select, SelectSubComponent.ESelectType selectType = SelectSubComponent.ESelectType.Select_Checked)
	{
		_selected = select;
		SelectSubComponent subComponent = GetSubComponent<SelectSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(select);
			if (select)
			{
				subComponent.SetSelect(selectType);
			}
		}
	}

	public void SetOwned(bool owned)
	{
		OwnedSubComponent subComponent = GetSubComponent<OwnedSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(owned);
		}
	}

	public void ShowPetName(bool showShadow = true)
	{
		PetNameComponent subComponent = GetSubComponent<PetNameComponent>();
		if (subComponent != null)
		{
			subComponent.SetShowName(ShowName: true, showShadow);
		}
	}

	public void ShowPetDiableStatus(bool disable)
	{
		DisableStatusComponent subComponent = GetSubComponent<DisableStatusComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(disable);
		}
	}

	public void OnClickPortrait()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
