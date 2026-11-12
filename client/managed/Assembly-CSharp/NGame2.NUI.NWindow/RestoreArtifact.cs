using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RestoreArtifact : BaseWindow, IUserObserver<ItemInfo>, IUserObserver, WorldMapManagement.IAvatarHider
{
	public interface Listener
	{
		void OnClickRestore(RestoreArtifact caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Artifact/RestoreArtifact";

	public Transform MaterialIconRoot;

	public UILabel CurCountLabel;

	public UILabel ReqCountLabel;

	public UIClipSprite GaugeSprite;

	public UIButton RestoreButton;

	public Transform OcclusionRoot;

	public Transform SpeakRoot;

	public Transform NpcTransform;

	public int NpcIndex;

	public DefineCreature.EMotion NpcCustomMotionType;

	public string NpcCustomMotionName;

	private Listener _listener;

	private ItemIconComponent _materialIconComponent;

	private bool _canRestore;

	private RenderFacade _renderFacade;

	public int ArtifactRestoreIndex
	{
		get
		{
			if (ArtifactRestoreData == null)
			{
				return 0;
			}
			return ArtifactRestoreData.Index;
		}
	}

	public ArtifactRestoreData ArtifactRestoreData { get; private set; }

	public ItemData MaterialItemData { get; private set; }

	public void Open(Listener listener)
	{
		ArtifactRestoreData firstData = GameRoot.Instance.ArtifactRestoreDataContainer.GetFirstData();
		if (firstData != null)
		{
			base.Open();
			_listener = listener;
			_canRestore = false;
			ArtifactRestoreData = firstData;
			Setup();
		}
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_renderFacade != null)
		{
			_renderFacade.ChainUpdate();
		}
	}

	private void Clear()
	{
		FactoryBase.SafeReleaseComponent(ref _materialIconComponent);
		Close3D();
	}

	private void Setup()
	{
		Clear();
		if (ArtifactRestoreData != null)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(ArtifactRestoreData.MaterialItemCode);
			if (dataByCode == null)
			{
				VespaDebug.LogError("Material item data not found : code=" + ArtifactRestoreData.MaterialItemCode);
				return;
			}
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
			int materialItemCount = ArtifactRestoreData.MaterialItemCount;
			float fillAmount = ((materialItemCount > 0) ? Mathf.Clamp01((float)itemCount / (float)materialItemCount) : 0f);
			bool flag = itemCount >= materialItemCount;
			UIHelper.SetLabelText(CurCountLabel, itemCount);
			UIHelper.SetLabelText(ReqCountLabel, materialItemCount);
			UIHelper.SetSpriteFillAmount(GaugeSprite, fillAmount);
			UIHelper.EnableButton(RestoreButton, materialItemCount > 0 && flag);
			_materialIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(MaterialIconRoot);
			_materialIconComponent.Open(dataByCode);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_materialIconComponent.transform);
			UIHelper.EnableGameObject(OcclusionRoot, flag);
			UIHelper.EnableGameObject(SpeakRoot, flag);
			_canRestore = flag;
			MaterialItemData = dataByCode;
		}
	}

	private void InitHeroModel()
	{
		VectorXZ direction = VectorXZ.zero;
		if (_renderFacade != null)
		{
			direction = _renderFacade.Action.GetDirection();
			RenderFacadeBuilder.ReleaseRenderFacade(_renderFacade);
			_renderFacade = null;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(NpcIndex);
		if (data == null)
		{
			return;
		}
		sbyte startHeroStar = data.StartHeroStar;
		CachedHeroInfo referenceHero = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(NpcIndex, startHeroStar);
		_renderFacade = CreateHeroModel(referenceHero, NpcTransform);
		if (!(_renderFacade == null))
		{
			if (!String_Extension_Method.IsNullOrEmpty(NpcCustomMotionName))
			{
				_renderFacade.Action.SetCustomIdle(new AnimationKeyData(NpcCustomMotionType, NpcCustomMotionName));
			}
			else
			{
				_renderFacade.Action.Idle();
			}
			_renderFacade.Action.SetDirection(direction, blendMotion: false);
		}
	}

	private RenderFacade CreateHeroModel(HeroInfo heroInfo, Transform rootTransform)
	{
		return new RenderFacadeBuilder(rootTransform, TeamType.Left, heroInfo)
		{
			LoadLevel = CreatureStock.ELoadLevel.IncludeFacial
		}.Build().DISABLEGROUNDCLIP();
	}

	private void Close3D()
	{
		if ((bool)_renderFacade)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(_renderFacade);
			_renderFacade = null;
		}
	}

	public void OnClickRestore()
	{
		if (_listener != null)
		{
			_listener.OnClickRestore(this);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null && MaterialItemData != null && changedInfo.ItemIndex == MaterialItemData.Index)
		{
			Setup();
		}
	}

	bool WorldMapManagement.IAvatarHider.IsOpen()
	{
		return base.IsOpen();
	}

	WorldMapManagement.AvatarHideFlag WorldMapManagement.IAvatarHider.GetAvatarHideFlag()
	{
		if (_canRestore)
		{
			return WorldMapManagement.AvatarHideFlag.HideNpc;
		}
		return WorldMapManagement.AvatarHideFlag.None;
	}
}
