using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NRender;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPetPortraitSub;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D;
using NGameAsset;
using NGameAsset.NCreature;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetListBook : BaseWindow, PetPortraitComponent.IListener, LobbyTopmostMenu.IMiniGuideButtonObserver
{
	public interface Listener
	{
		IEnumerator OnEnterIncubator();

		IEnumerator OnEnterSoulInven();
	}

	[Serializable]
	public class ArchivePetInfoUI
	{
		public UILabel PetGradeLabel;

		public UISpriteSelector PetGradeColorSelector;

		public UILabel PetNameLabel;

		public UILabel PetTitleLabel;

		private HeroPreview _petPreview;

		public UITexture Pet3DTexture;

		public GameObject OwnPetInfoObject;

		public UILabel PetStoryLabel;

		public UITable OwnPetStatTable;

		public UIScrollView OwnPetStatScrollView;

		public UIWidget OwnPetStatWidget;

		public UIScrollView OwnPetStoryScrollView;

		public GameObject NonePetInfoObject;

		public UITable NonePetStatTable;

		public UIScrollView NonePetStatScrollView;

		public UIWidget NonePetStatWidget;

		public GameObject ButtonsObject;

		public GameObject NotOpenLabelObject;

		private List<LabelFieldComponent> _petStatComponentList = new List<LabelFieldComponent>();

		private CreatureData _selectedPetData;

		private PetListBook _parent;

		public CreatureData CurrentSelectedPetData => _selectedPetData;

		public void Reset()
		{
			FactoryBase.ReleaseAndClearList(_petStatComponentList);
			UIHelper.CloseHero3dPreview(ref _petPreview, Pet3DTexture);
		}

		public void Init(CreatureData petData, PetListBook parent)
		{
			if (petData != null)
			{
				_selectedPetData = petData;
				_parent = parent;
				SetupPetInfo();
			}
		}

		private void SetupPetInfo()
		{
			PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(_selectedPetData.Index);
			bool flag = petInfo != null;
			UIHelper.SetLabelText(PetNameLabel, _selectedPetData.Name);
			UIHelper.SetLabelText(PetTitleLabel, _selectedPetData.Title);
			UIHelper.SetLabelText(PetGradeLabel, _selectedPetData.CreatureGrade);
			_selectedPetData.CreatureGrade.ToString();
			List<UISpriteSelector.SelectData> selectList = PetGradeColorSelector.SelectList;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(selectList); i++)
			{
				UISpriteSelector.SelectData selectData = selectList[i];
				if (selectData.Name.Equals(selectList))
				{
					UIHelper.SetLabelColor(PetGradeLabel, selectData.SpriteColor);
				}
			}
			if (flag)
			{
				SetupOwnPetInfoObject();
				SetupPetStatList(OwnPetStatTable, OwnPetStatScrollView, OwnPetStatWidget, petInfo.Star);
			}
			else
			{
				SetupNonePetInfoObject();
				SetupPetStatList(NonePetStatTable, NonePetStatScrollView, NonePetStatWidget);
			}
			SetPetModel();
		}

		private void SetupOwnPetInfoObject()
		{
			UIHelper.EnableGameObject(OwnPetInfoObject, enable: true);
			UIHelper.EnableGameObject(NonePetInfoObject, enable: false);
			string stringDirect = LocalizationManager.GetStringDirect(_selectedPetData.ArchiveStory);
			UIHelper.SetLabelText(PetStoryLabel, stringDirect);
			OwnPetStoryScrollView.ResetPosition();
		}

		private void SetupNonePetInfoObject()
		{
			bool flag = SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.Pet);
			UIHelper.EnableGameObject(ButtonsObject, flag);
			UIHelper.EnableGameObject(NotOpenLabelObject, !flag);
			UIHelper.EnableGameObject(OwnPetInfoObject, enable: false);
			UIHelper.EnableGameObject(NonePetInfoObject, enable: true);
		}

		private void SetupPetStatList(UITable table, UIScrollView scrollView, UIWidget widget, int petStar = 0)
		{
			FactoryBase.ReleaseAndClearList(_petStatComponentList);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selectedPetData.PetOptionIndices); i++)
			{
				int optionIndex = _selectedPetData.PetOptionIndices[i];
				string optionName = "";
				string optionValue = "";
				if (PetHelper.TryGetPetOptionLabelText(optionIndex, petStar, ref optionName, ref optionValue))
				{
					LabelFieldComponent item = CreateStatLabelFieldComponent(table.transform, optionName, optionValue);
					_petStatComponentList.Add(item);
				}
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_selectedPetData.PetBonusIndices); j++)
			{
				int bonusOptionIndex = _selectedPetData.PetBonusIndices[j];
				string optionName2 = "";
				string optionValue2 = "";
				if (PetHelper.TryGetPetBonusOptionLabelText(bonusOptionIndex, petStar, ref optionName2, ref optionValue2))
				{
					LabelFieldComponent item2 = CreateStatLabelFieldComponent(table.transform, optionName2, optionValue2);
					_petStatComponentList.Add(item2);
				}
			}
			VespaCoroutine.Start(_parent, coRepositionTable(table, scrollView));
		}

		private IEnumerator coRepositionTable(UITable table, UIScrollView scrollView)
		{
			yield return null;
			table.Reposition();
			table.repositionNow = true;
			scrollView.ResetPosition();
			scrollView.UpdatePosition();
		}

		private LabelFieldComponent CreateStatLabelFieldComponent(Transform parent, OperationType type, int value)
		{
			string field = LocalizationManager.GetString(type);
			string statDisplayValue = StatData.GetStatDisplayValue(type, value, showSign: true, isCommaString: true);
			LabelFieldComponent labelFieldComponent = LabelFieldComponent.PrefabAutoPet.Factory.Create(parent);
			labelFieldComponent.Open(field, statDisplayValue);
			return labelFieldComponent;
		}

		private LabelFieldComponent CreateStatLabelFieldComponent(Transform parent, string name, string value)
		{
			LabelFieldComponent labelFieldComponent = LabelFieldComponent.PrefabAutoPet.Factory.Create(parent);
			labelFieldComponent.Open(name, value);
			return labelFieldComponent;
		}

		private void SetPetModel()
		{
			if (!(Pet3DTexture == null))
			{
				RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, _selectedPetData.CodeName, 0);
				renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.Normal;
				renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
				UIHelper.InitArchive3dPreview(ref _petPreview, Pet3DTexture, _selectedPetData.Index, 0, renderFacadeBuilder, "Normal");
				if (_petPreview.RenderFacade != null)
				{
					_petPreview.RenderFacade.Action.SetDirection(new VectorXZ(0f, 1f), blendMotion: false);
				}
			}
		}

		public void RotatePet(float x)
		{
			if (!(_petPreview == null) && !(_petPreview.RenderFacade == null))
			{
				RenderFacade renderFacade = _petPreview.RenderFacade;
				VectorXZ direction = renderFacade.Action.GetDirection();
				direction = NVespa.Math.Rotate(direction, x);
				renderFacade.Action.SetDirection(direction, blendMotion: false);
			}
		}
	}

	[Serializable]
	public class PetGradeCategory
	{
		public UIToggle GradeToggle;

		public CreatureGrade Grade;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetListBook";

	public ArchivePetInfoUI ArchivePetInfo;

	public UIScrollGrid PetListGrid;

	public List<PetGradeCategory> CategoryList;

	public UISprite SpriteGauge;

	public UILabel LabelGauge;

	private List<CreatureData> _petDataList = new List<CreatureData>();

	private List<PetPortraitComponent> _petPortraitComponentList = new List<PetPortraitComponent>();

	private CreatureGrade? _curToggleType;

	private int _openSelectPetIndex;

	private PetPortraitComponent _openSelectPetComponent;

	private List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	private Listener _listener;

	int LobbyTopmostMenu.IMiniGuideButtonObserver.WindowGuideIndex => WindowGuideIndex;

	public override void Reset()
	{
		_petDataList.Clear();
		_curToggleType = null;
		FactoryBase.ReleaseAndClearList(_petPortraitComponentList);
		ArchivePetInfo.Reset();
		base.Reset();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdatePetCamera();
	}

	public void Open(Listener listener, int selectPetIndex = 0)
	{
		_listener = listener;
		base.Open(delegate
		{
			_openSelectPetIndex = selectPetIndex;
			Init();
		});
	}

	private void Init()
	{
		SetupPetList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CategoryList); i++)
		{
			if (CategoryList[i].Grade == CreatureGrade.None)
			{
				CategoryList[i].GradeToggle.Set(state: true);
				break;
			}
		}
	}

	private void SelectPetIcon(PetPortraitComponent comp)
	{
		foreach (PetPortraitComponent petPortraitComponent in _petPortraitComponentList)
		{
			petPortraitComponent.SetSelection(select: false);
		}
		comp.SetSelection(select: true, SelectSubComponent.ESelectType.Select_BorderBook);
	}

	private void SetupPetList()
	{
		CreatureData[] collection = (from v in GameRoot.Instance.CreatureDataContainer.GetAllData()
			where v.Race == CreatureRaceType.Pet
			select v).ToArray();
		_petDataList.AddRange(collection);
		_petDataList.Sort((CreatureData a, CreatureData b) => (a.Grade == b.Grade) ? a.Index.CompareTo(b.Index) : (a.Grade.CompareTo(b.Grade) * -1));
	}

	private void SetupPetPortraitComponent(CreatureGrade grade)
	{
		FactoryBase.ReleaseAndClearList(_petPortraitComponentList);
		foreach (CreatureData petData in _petDataList)
		{
			if (grade == CreatureGrade.None || grade == petData.CreatureGrade)
			{
				PetPortraitComponent petPortraitComponent = UIHelper.AddGridItem(PetListGrid, PetPortraitComponent.SmallPrefabAuto);
				PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(petData.Index);
				bool flag = petInfo != null;
				if (flag)
				{
					petPortraitComponent.Open(petInfo, this);
					petPortraitComponent.SetStarMark(petInfo.Star > 0);
				}
				else
				{
					petPortraitComponent.Open(petData, this);
					petPortraitComponent.SetStarMark(show: false);
				}
				petPortraitComponent.SetGrayScale(!flag);
				if (petData.Index == _openSelectPetIndex)
				{
					_openSelectPetComponent = petPortraitComponent;
				}
				_petPortraitComponentList.Add(petPortraitComponent);
			}
		}
		UIHelper.ResetGridPosition(PetListGrid);
	}

	private void SetupGauge(CreatureGrade grade)
	{
		int num = 0;
		foreach (CreatureData allDatum in GameRoot.Instance.CreatureDataContainer.GetAllData())
		{
			if (allDatum.Race == CreatureRaceType.Pet && (grade == CreatureGrade.None || allDatum.CreatureGrade == grade))
			{
				num++;
			}
		}
		int petArchiveCount = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetPetArchiveCount(grade);
		SetupGauge(num, petArchiveCount);
	}

	private void SetupGauge(int totalCount, int curCount)
	{
		if (curCount >= totalCount)
		{
			string text = $"{100:N2}% ( {curCount} / {totalCount} )";
			UIHelper.SetSpriteFillAmount(SpriteGauge, 1f);
			UIHelper.SetLabelText(LabelGauge, text);
		}
		else
		{
			float num = Mathf.Clamp((float)curCount / (float)totalCount, 0f, 100f);
			string text2 = $"{num * 100f:N2}% ( {curCount} / {totalCount} )";
			UIHelper.SetLabelText(LabelGauge, text2);
			UIHelper.SetSpriteFillAmount(SpriteGauge, num);
		}
	}

	void PetPortraitComponent.IListener.OnClick(PetPortraitComponent comp)
	{
		SelectPetIcon(comp);
		ArchivePetInfo.Init(comp.CreatureData, this);
	}

	public void OnEnterPetUnion()
	{
		PortalHelper.OpenContentsMovePopup(LocalizationManager.GetStringDirect("PET_TIER_UPGRADE_MOVE_WARNING_BTN_TEXT"), delegate
		{
			IllustratedBookList instance = SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.GetInstance<IllustratedBookList>();
			if (instance != null && instance.IsOpen())
			{
				instance.Close();
			}
			Close();
			if (_listener != null)
			{
				VespaCoroutine.Start(_listener.OnEnterSoulInven());
			}
		});
	}

	public void OnEnterPetGacha()
	{
		PortalHelper.OpenContentsMovePopup(LocalizationManager.GetStringDirect("PET_GACHA_MOVE_WARNING_BTN_TEXT"), delegate
		{
			IllustratedBookList instance = SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.GetInstance<IllustratedBookList>();
			if (instance != null && instance.IsOpen())
			{
				instance.Close();
			}
			Close();
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.PetGacha.ToString());
		});
	}

	public void OnEnterPetIncubator()
	{
		PortalHelper.OpenContentsMovePopup(LocalizationManager.GetStringDirect("PET_INCUBATOR_MOVE_WARNING_BTN_TEXT"), delegate
		{
			IllustratedBookList instance = SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.GetInstance<IllustratedBookList>();
			if (instance != null && instance.IsOpen())
			{
				instance.Close();
			}
			Close();
			if (_listener != null)
			{
				VespaCoroutine.Start(_listener.OnEnterIncubator());
			}
		});
	}

	public void OnToggle()
	{
		CreatureGrade creatureGrade = CreatureGrade.None;
		int num = __Extension_Method_ICollection__.COUNT(CategoryList);
		for (int i = 0; i < num; i++)
		{
			if (CategoryList[i].GradeToggle.value)
			{
				creatureGrade = CategoryList[i].Grade;
				break;
			}
		}
		if (_curToggleType.HasValue && _curToggleType == creatureGrade)
		{
			return;
		}
		_curToggleType = creatureGrade;
		SetupPetPortraitComponent(creatureGrade);
		UIHelper.ResetGridPosition(PetListGrid);
		if (_openSelectPetComponent != null)
		{
			_openSelectPetComponent.OnClickPortrait();
			PetListGrid.ScrollTo(_openSelectPetComponent, 270f, 0f);
		}
		else
		{
			PetPortraitComponent petPortraitComponent = _petPortraitComponentList.First();
			if (petPortraitComponent != null)
			{
				petPortraitComponent.OnClickPortrait();
				PetListGrid.ResetScroll();
			}
		}
		_openSelectPetComponent = null;
		_openSelectPetIndex = 0;
		SetupGauge(creatureGrade);
	}

	private void UpdatePetCamera()
	{
		if (_touchList.Count != 0 && _touchList.Count == 1)
		{
			UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
			ArchivePetInfo.RotatePet(mouseOrTouch.delta.x);
		}
	}

	public void OnPress()
	{
		OnDragBegin();
	}

	public void OnRelease()
	{
		OnDragEnd();
	}

	public void OnTouch()
	{
	}

	public void OnDragBegin()
	{
		if (!_touchList.Contains(UICamera.currentTouch))
		{
			_touchList.Add(UICamera.currentTouch);
		}
	}

	public void OnDragEnd()
	{
		_touchList.Remove(UICamera.currentTouch);
	}
}
