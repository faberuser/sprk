using System;
using NGame2.NRender;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager.NPet;
using NGameAsset;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetAwakenResultWindow : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetAwakenResultWindow";

	public UIBundleLoader BackgroundLoader;

	public Transform RenderPivot;

	public UICreatureWidget CreatureWidget;

	public Transform ResultRoot;

	public UILabel BeforeStar;

	public UILabel AfterStar;

	public UIScrollGrid StatGrid;

	public UISimpleGrid ResultPetGrid;

	private const string PET_AWAKEN_MOTION = "Eat";

	private PetInfo PetInfo;

	private PetPortraitComponent _petPortraitComponent;

	private StatListComponent _statListComponent;

	private RenderFacade RenderFacade
	{
		get
		{
			if (CreatureWidget == null)
			{
				return null;
			}
			return CreatureWidget.RenderFacade;
		}
	}

	public void Open(PetInfo petInfo)
	{
		if (petInfo == null)
		{
			Close();
			return;
		}
		Open(delegate
		{
			PetInfo = petInfo;
			Init();
		}, delegate
		{
			UIHelper.EnableGameObject(ResultRoot, enable: true);
		});
	}

	public void Init()
	{
		SetPetInfo();
		SetPetModel();
		SetPetStat();
	}

	private void SetPetInfo()
	{
		if (PetInfo.CreatureData != null)
		{
			int num = ((PetInfo.Star - 1 >= 0) ? (PetInfo.Star - 1) : 0);
			UIHelper.SetLabelText(BeforeStar, num.ToString());
			int star = PetInfo.Star;
			UIHelper.SetLabelText(AfterStar, star.ToString());
			ClearPetPortraitComp();
			_petPortraitComponent = UIHelper.AddGridItem(ResultPetGrid, PetPortraitComponent.SmallPrefabAuto);
			_petPortraitComponent.Open(PetInfo, null);
		}
	}

	private void SetPetModel()
	{
		if (CreatureWidget != null)
		{
			CreatureWidget.OpenPet(PetInfo, CreatureStock.ELoadLevel.IncludeFacial);
		}
		if (!(RenderFacade != null))
		{
			return;
		}
		RenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
		ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(PetInfo.CreatureData.CodeName);
		if (data != null)
		{
			if (__Extension_Method_ICollection__.COUNT(data.PetListOffset) >= 2)
			{
				Vector3 localPosition = new Vector3(data.PetListOffset[0], data.PetListOffset[1]);
				if (localPosition.magnitude > 1f)
				{
					RenderPivot.localPosition = localPosition;
				}
			}
			if (data.PetListScale > 1f)
			{
				RenderPivot.localScale = Vector3.one * data.PetListScale;
			}
		}
		CreatureWidget.SetEmotiAction("Eat");
	}

	private void SetPetStat()
	{
		if (StatGrid == null || StatGrid.ClippingPanel == null)
		{
			return;
		}
		CreatureData creatureData = PetInfo.CreatureData;
		if (creatureData != null)
		{
			ArrayFor(creatureData.PetOptionIndices, delegate(int optionIndex)
			{
				SetStatComponent(optionIndex, PetInfo.Star);
			});
			ArrayFor(creatureData.PetBonusIndices, delegate(int bonusOptionIndex)
			{
				SetBonusStatComponent(bonusOptionIndex, PetInfo.Star);
			});
			if (StatGrid.ScrollView != null)
			{
				StatGrid.ScrollView.ResetPosition();
			}
		}
	}

	private void ArrayFor<T>(T[] array, Action<T> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			T i_argA = array[i];
			Vespa_System_Action_Extension_Method.fire(action, i_argA);
		}
	}

	private void SetStatComponent(int optionIndex, int star)
	{
		if (_statListComponent == null)
		{
			_statListComponent = StatListComponent.PetStatCreate(StatGrid.ClippingPanel, 5);
		}
		_statListComponent.AddPetStat(optionIndex, star);
	}

	private void SetBonusStatComponent(int optionIndex, int star)
	{
		if (_statListComponent == null)
		{
			_statListComponent = StatListComponent.PetStatCreate(StatGrid.ClippingPanel, 5);
		}
		_statListComponent.AddPetBonusStat(optionIndex, star);
	}

	public override void Reset()
	{
		base.Reset();
		ClearCreatureWidget();
		ClearPetPortraitComp();
		ClearStatComponent();
	}

	private void ClearCreatureWidget()
	{
		if (CreatureWidget != null)
		{
			CreatureWidget.Close();
		}
	}

	private void ClearPetPortraitComp()
	{
		FactoryBase.SafeReleaseComponent(ref _petPortraitComponent);
	}

	public void ClearStatComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
	}

	public override void Close()
	{
		ClearCreatureWidget();
		base.Close();
		if (SceneAutoBehaviour<PetManagement>.instance.IsOpened<PetList>())
		{
			SceneAutoBehaviour<PetManagement>.instance.GetInstance<PetList>().EnableCreatureWidget(enabled: true);
		}
		UIHelper.EnableGameObject(ResultRoot, enable: false);
	}

	public void OnClickSuccess()
	{
		Close();
	}
}
