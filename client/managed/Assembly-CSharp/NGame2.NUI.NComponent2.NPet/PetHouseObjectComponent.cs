using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NPet;
using NGame2.NUI.NRender3D.NCinema;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetHouseObjectComponent : IPetHouseObject, PetHouseObjectNameComponent.IListener
{
	public class PetHouseObjectInfo
	{
		public RenderFacade RenderFacade { get; set; }

		public EPetHouseObjectType EPetHouseObjectType { get; set; }

		public Vector3 NameComponentLocalPositionOffset { get; set; }

		public float NameComponentScale { get; set; }

		public bool IsPlayTouchAnimation { get; set; }

		public IListener IListener { get; set; }

		public string LocalizationKey { get; set; }
	}

	public interface IListener
	{
		void Touch(IPetHouseObject touchIPetHouseObject);
	}

	private readonly string AnimationKeyTouch = "Touch";

	private readonly string AnimationKeyIdle = "Idle";

	private PetHouseObjectInfo _petHouseObjectInfo;

	private InteractionComponent _interactionComponent;

	private PetHouseObjectNameComponent _petHouseObjectNameComponent;

	private EPetHouseObjectState _ePetHouseObjectState;

	private CreatureData PetHouseObjectData => GameRoot.Instance.CreatureDataContainer.GetDataByCode(PetHouseObjectCreatureCode);

	private string PetHouseObjectModelCode
	{
		get
		{
			if (_petHouseObjectInfo == null)
			{
				return string.Empty;
			}
			if (_petHouseObjectInfo.RenderFacade == null)
			{
				return string.Empty;
			}
			if (_petHouseObjectInfo.RenderFacade.AssetCreatureAsync == null)
			{
				return string.Empty;
			}
			return _petHouseObjectInfo.RenderFacade.AssetCreatureAsync.Stock.Code;
		}
	}

	private string PetHouseObjectCreatureCode => PetHouseObjectModelCode;

	EPetHouseObjectState IPetHouseObject.EPetHouseObjectState
	{
		get
		{
			return _ePetHouseObjectState;
		}
		set
		{
			_ePetHouseObjectState = value;
		}
	}

	EPetHouseObjectType IPetHouseObject.EPetHouseObjectType
	{
		get
		{
			if (_petHouseObjectInfo == null)
			{
				return EPetHouseObjectType.None;
			}
			return _petHouseObjectInfo.EPetHouseObjectType;
		}
	}

	RenderFacade IPetHouseObject.RenderFacade
	{
		get
		{
			if (_petHouseObjectInfo == null)
			{
				return null;
			}
			return _petHouseObjectInfo.RenderFacade;
		}
	}

	Vector3 IPetHouseObject.PetHouseObjectPosition
	{
		get
		{
			Vector3 zero = Vector3.zero;
			if (_petHouseObjectInfo == null)
			{
				return zero;
			}
			if (_petHouseObjectInfo.RenderFacade == null)
			{
				return zero;
			}
			return _petHouseObjectInfo.RenderFacade.transform.position;
		}
	}

	bool IPetHouseObject.IsEnable => UIHelper.IsEnabledGameObjet(_petHouseObjectInfo.RenderFacade);

	private Transform RootTransform
	{
		get
		{
			if (_petHouseObjectInfo == null)
			{
				return null;
			}
			if (_petHouseObjectInfo.RenderFacade == null)
			{
				return null;
			}
			if (_petHouseObjectInfo.RenderFacade.Pivot == null)
			{
				return null;
			}
			return _petHouseObjectInfo.RenderFacade.Pivot.FindTransform(EPivotType.Root);
		}
	}

	public void Open(PetHouseObjectInfo info)
	{
		Init(info);
	}

	private void Init(PetHouseObjectInfo info)
	{
		_petHouseObjectInfo = info;
		CreateInteraction();
		CreatePetHouseObjectNameComponent();
	}

	private void Close()
	{
		FactoryBase.SafeReleaseComponent(ref _interactionComponent);
		FactoryBase.SafeReleaseComponent(ref _petHouseObjectNameComponent);
		_petHouseObjectInfo = null;
	}

	private void CreateInteraction()
	{
		if (_petHouseObjectInfo == null || _petHouseObjectInfo.RenderFacade == null || PetHouseObjectData == null)
		{
			return;
		}
		RenderFacadeInteractionProvider renderFacadeInteractionProvider = new RenderFacadeInteractionProvider(PetHouseObjectModelCode, null);
		if (renderFacadeInteractionProvider != null)
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.BottomUI);
			_interactionComponent = InteractionComponent.PrefabAuto.Factory.Create(_petHouseObjectInfo.RenderFacade.transform, layerInfo.Layer);
			if (!(_interactionComponent == null))
			{
				renderFacadeInteractionProvider.SetRenderfacade(_petHouseObjectInfo.RenderFacade);
				_interactionComponent.Set(renderFacadeInteractionProvider);
			}
		}
	}

	private void CreatePetHouseObjectNameComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _petHouseObjectNameComponent);
		if (_petHouseObjectInfo != null && !(_petHouseObjectInfo.RenderFacade == null) && !(_petHouseObjectInfo.RenderFacade.gameObject == null))
		{
			int layer = _petHouseObjectInfo.RenderFacade.gameObject.layer;
			_petHouseObjectNameComponent = PetHouseObjectNameComponent.PrefabAuto.Factory.Create(RootTransform, layer);
			if (!(_petHouseObjectNameComponent == null))
			{
				_petHouseObjectNameComponent.Open(new PetHouseObjectNameComponent.PetHouseObjectNameInfo
				{
					EPetHouseObjectType = _petHouseObjectInfo.EPetHouseObjectType,
					LocalPositionOffset = _petHouseObjectInfo.NameComponentLocalPositionOffset,
					Scale = _petHouseObjectInfo.NameComponentScale,
					IListener = this,
					LocalizationKey = _petHouseObjectInfo.LocalizationKey
				});
			}
		}
	}

	private void Touch()
	{
		if (_ePetHouseObjectState == EPetHouseObjectState.Idle && _petHouseObjectInfo != null && _petHouseObjectInfo.EPetHouseObjectType != EPetHouseObjectType.None && _petHouseObjectInfo.IListener != null)
		{
			StartActionAnimation(AnimationKeyTouch, isLoop: false);
			_petHouseObjectInfo.IListener.Touch(this);
		}
	}

	private void StartActionAnimation(string animationKey, bool isLoop)
	{
		if (_petHouseObjectInfo != null && !(_petHouseObjectInfo.RenderFacade == null) && _petHouseObjectInfo.IsPlayTouchAnimation)
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stock.AnimationKey = AnimationKeyData.CreateEmotiMotion(animationKey);
			stock.MotionEnd = (isLoop ? DefineCreature.EMotionEnd.Loop : DefineCreature.EMotionEnd.End);
			_petHouseObjectInfo.RenderFacade.Action.ApplyModifier(ref stock);
			_petHouseObjectInfo.RenderFacade.AnimationData.Play(stock, blendMotion: true, null);
		}
	}

	private void EndAnimationTouch()
	{
		if (_petHouseObjectInfo != null && !(_petHouseObjectInfo.RenderFacade == null) && _petHouseObjectInfo.RenderFacade.AnimationData != null && _petHouseObjectInfo.IsPlayTouchAnimation && _petHouseObjectInfo.RenderFacade.AnimationData.Runtime.Stock.AnimationKey.PlayName.Equals(AnimationKeyTouch) && _petHouseObjectInfo.RenderFacade.Action.IsActionEnd())
		{
			StartActionAnimation(AnimationKeyIdle, isLoop: true);
		}
	}

	public void EnableNameComponent(bool isEnable)
	{
		if (isEnable)
		{
			_petHouseObjectNameComponent.RefreshNotice();
		}
		UIHelper.EnableGameObject(_petHouseObjectNameComponent, isEnable);
	}

	void IPetHouseObject.Close()
	{
		Close();
	}

	void IPetHouseObject.Update()
	{
		if (_petHouseObjectNameComponent != null)
		{
			_petHouseObjectNameComponent.UpdateRemainTimeLabel();
		}
		EndAnimationTouch();
	}

	bool IPetHouseObject.CheckHit(RaycastHit hit)
	{
		if (_interactionComponent == null)
		{
			return false;
		}
		if (!_interactionComponent.CheckRaycastHit(hit))
		{
			return false;
		}
		if (hit.transform.parent == null)
		{
			return false;
		}
		RenderFacade component = hit.transform.parent.GetComponent<RenderFacade>();
		if (component == null || component.AssetCreatureAsync == null)
		{
			return false;
		}
		return component.AssetCreatureAsync.Stock.Code.Contains(PetHouseObjectModelCode);
	}

	void IPetHouseObject.Touch()
	{
		Touch();
	}

	void IPetHouseObject.Enable(bool enable)
	{
		UIHelper.EnableGameObject(_petHouseObjectInfo.RenderFacade, enable);
	}

	void IPetHouseObject.RefreshNotice()
	{
		if (_petHouseObjectNameComponent != null)
		{
			_petHouseObjectNameComponent.RefreshNotice();
		}
	}

	void PetHouseObjectNameComponent.IListener.Touch()
	{
		Touch();
	}
}
