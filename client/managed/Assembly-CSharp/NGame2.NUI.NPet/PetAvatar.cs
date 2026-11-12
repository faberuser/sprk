using System;
using System.Collections;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NNodeTree;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NComponent2.NPet;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NRender3D.NCinema;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NPet;

public class PetAvatar : NGame2.NUI.NWorldMap.NMapLayout.Avatar, IPet, IPetAction, IPetInteraction
{
	public class PetAvatarInfo
	{
		public RenderTargetCinema Cinema { get; set; }

		public PetInfo PetInfo { get; set; }

		public AvatarInfo AvatarInfo { get; set; }

		public Vector3 Position { get; set; }

		public System.Action TouchAction { get; set; }

		public bool AddCheckHIt { get; set; }

		public IListener IListener { get; set; }

		public PetAvatarInfo()
		{
			Cinema = null;
			Position = Vector3.zero;
			TouchAction = null;
		}
	}

	public interface IListener
	{
		void StartInteration(PetAvatar petAvatar);

		void EndInteration(bool isNext);

		void Changed(int petIndex, EPetActionType ePetActionType);
	}

	private readonly float RunSpeed = 1f;

	private readonly float WalkSpeed = 0.5f;

	private readonly VectorXZ ForwardDirection = VectorXZ.zero;

	private readonly string SpecialAnimationKey = "Unique";

	private PetAvatarInfo _petAvatarInfo;

	private PetInteractionComponent _petInteractionComponent;

	private Vector3? _moveTargetPosition;

	private EPetActionType _ePetActionType;

	private bool _isFocus;

	private PetTierInteractiveData _petTierInteractiveData;

	private CheckHitController _checkHitController;

	private bool CheckArriveToTarget
	{
		get
		{
			if (_renderFacade == null)
			{
				return true;
			}
			if (!_moveTargetPosition.HasValue)
			{
				return true;
			}
			return Customizing_Extension_Method.Approximately(_renderFacade.transform.position, _moveTargetPosition.Value);
		}
	}

	private bool CheckRayCastHit
	{
		get
		{
			if (_renderFacade == null)
			{
				return true;
			}
			if (!_moveTargetPosition.HasValue)
			{
				return true;
			}
			Vector3 position = _renderFacade.transform.position;
			float maxDistance = Vector3.Distance(_renderFacade.transform.position, _moveTargetPosition.Value);
			Vector3 direction = _renderFacade.Action.GetDirection().ToVec3();
			RaycastHit hitInfo;
			return Physics.Raycast(position, direction, out hitInfo, maxDistance);
		}
	}

	private CreatureData PetCreatureData => GameRoot.Instance.CreatureDataContainer.GetData(PetIndex);

	private int PetIndex
	{
		get
		{
			if (_petAvatarInfo == null || _petAvatarInfo.PetInfo == null)
			{
				return 0;
			}
			return _petAvatarInfo.PetInfo.PetIndex;
		}
	}

	PetInfo IPet.PetInfo
	{
		get
		{
			if (_petAvatarInfo == null)
			{
				return null;
			}
			return _petAvatarInfo.PetInfo;
		}
	}

	int IPet.PetIndex => PetIndex;

	EPetActionType IPetAction.EPetActionType => _ePetActionType;

	public void Open(PetAvatarInfo petAvatarInfo, MapComponent mapComponent)
	{
		Open(petAvatarInfo.AvatarInfo);
		Init(petAvatarInfo, mapComponent);
	}

	public override void Close()
	{
		base.Close();
		if (_petAvatarInfo != null && _petAvatarInfo.Cinema != null && _petAvatarInfo.PetInfo != null)
		{
			_petAvatarInfo.Cinema.DestroyCreature(_petAvatarInfo.PetInfo.PetIndex);
		}
		FactoryBase.SafeReleaseComponent(ref _petInteractionComponent);
		_petAvatarInfo = null;
		_moveTargetPosition = null;
	}

	private void Init(PetAvatarInfo petAvatarInfo, MapComponent mapComponent)
	{
		_petAvatarInfo = petAvatarInfo;
		_avatarInfo.MapComponent = mapComponent;
		_petTierInteractiveData = _petAvatarInfo.PetInfo.PetTierInteractiveData;
		CreatePetRendererFacade();
		CreateTouchInteraction();
		CreateInteractionComponent();
		AddCheckHitController();
	}

	private void CreatePetRendererFacade()
	{
		if (_petAvatarInfo == null || _petAvatarInfo.Cinema == null || _petAvatarInfo.PetInfo == null)
		{
			return;
		}
		CreatureData petCreatureData = PetCreatureData;
		if (petCreatureData == null)
		{
			return;
		}
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, petCreatureData.CodeName, _petAvatarInfo.PetInfo.Star);
		if (renderFacadeBuilder != null)
		{
			renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
			_avatarInfo.RenderFacadeBuilder = renderFacadeBuilder;
			RenderTargetCinema cinema = _petAvatarInfo.Cinema;
			int uid = cinema.SpawnCreature(VariableKeyword.Myself, _petAvatarInfo.PetInfo.PetIndex, renderFacadeBuilder);
			_renderFacade = cinema.GETRENDERFACADE(uid);
			if (!(_renderFacade == null))
			{
				_renderFacade.transform.localScale = Vector3.one;
				_renderFacade.transform.position = _petAvatarInfo.Position;
				_renderFacade.Action.SetDirection(ForwardDirection, blendMotion: true);
				_heroGo = _renderFacade.gameObject;
			}
		}
	}

	private void AddCheckHitController()
	{
		if (_petAvatarInfo != null && _petAvatarInfo.AddCheckHIt)
		{
			_checkHitController = _renderFacade.gameObject.AddComponent<CheckHitController>();
			if (!(_checkHitController == null) && !(base.TouchInteractionComponent == null))
			{
				_checkHitController.Init(_renderFacade.transform);
				_checkHitController.SetRadiusSize(base.TouchInteractionComponent.BoxCollider.size);
			}
		}
	}

	private void CreateTouchInteraction()
	{
		if (_avatarInfo == null || _petAvatarInfo == null || _renderFacade == null)
		{
			return;
		}
		RenderFacadeInteractionProvider renderFacadeInteractionProvider = new RenderFacadeInteractionProvider(_avatarInfo.RenderFacadeBuilder.Code, _petAvatarInfo.TouchAction);
		if (renderFacadeInteractionProvider != null)
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.BottomUI);
			base.TouchInteractionComponent = InteractionComponent.PrefabAuto.Factory.Create(_renderFacade.transform, layerInfo.Layer);
			if (!(base.TouchInteractionComponent == null))
			{
				renderFacadeInteractionProvider.SetRenderfacade(_renderFacade);
				base.TouchInteractionComponent.Set(renderFacadeInteractionProvider);
			}
		}
	}

	private void CreateInteractionComponent()
	{
		if (!(_renderFacade == null) && !(_renderFacade.HeightTm == null) && !(_renderFacade.gameObject == null))
		{
			_petInteractionComponent = PetInteractionComponent.PrefabAuto.Factory.Create(_renderFacade.transform, _renderFacade.gameObject.layer);
			if (!(_petInteractionComponent == null))
			{
				_petInteractionComponent.Open(_renderFacade.HeightTm.position.y);
			}
		}
	}

	private void Touch()
	{
		StartInteraction();
	}

	private void Changed()
	{
		if (_petAvatarInfo != null && _petAvatarInfo.PetInfo != null && _petAvatarInfo.IListener != null)
		{
			_petAvatarInfo.IListener.Changed(_petAvatarInfo.PetInfo.PetIndex, _ePetActionType);
		}
	}

	private void DisablePetInterationState()
	{
		if (_petInteractionComponent != null)
		{
			_petInteractionComponent.DisablePetInteractionState();
		}
	}

	private void UpdatePetInteractionState()
	{
		if (_petInteractionComponent == null)
		{
			CreateInteractionComponent();
		}
		else
		{
			if (_petAvatarInfo == null || _petAvatarInfo.PetInfo == null)
			{
				return;
			}
			EPetInteractionState ePetInteractionState = EPetInteractionState.None;
			if (_ePetActionType == EPetActionType.Interaction || _ePetActionType == EPetActionType.Sleep || _ePetActionType == EPetActionType.Slide || _ePetActionType == EPetActionType.GetUp || _ePetActionType == EPetActionType.Happy || _isFocus)
			{
				_petInteractionComponent.UpdatePetInteractionState(ePetInteractionState);
				return;
			}
			if ((float)_petAvatarInfo.PetInfo.FullPoint / Math.Max(_petAvatarInfo.PetInfo.MaxFood, 0.001f) <= 0.5f && GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount > 0 && PetHelper.CanExplorePetStatus(_petAvatarInfo.PetInfo) && !_petAvatarInfo.PetInfo.IsLinePet)
			{
				ePetInteractionState = EPetInteractionState.Feed;
			}
			if (_petTierInteractiveData == null)
			{
				_petTierInteractiveData = _petAvatarInfo.PetInfo.PetTierInteractiveData;
			}
			_petInteractionComponent.UpdatePetInteractionState(ePetInteractionState);
		}
	}

	private IEnumerator CoStartInteraction(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		StartInteraction();
	}

	private void StartInteraction()
	{
		if (!_isFocus && !(_renderFacade == null) && _petAvatarInfo != null && _ePetActionType != EPetActionType.Slide && _ePetActionType != EPetActionType.GetUp && _petAvatarInfo.IListener != null)
		{
			DisablePetInterationState();
			Stop();
			_isFocus = true;
			_petAvatarInfo.IListener.StartInteration(this);
		}
	}

	private void EndInteration(bool isNext)
	{
		if (_petAvatarInfo != null && _petAvatarInfo.IListener != null)
		{
			_isFocus = false;
			_petAvatarInfo.IListener.EndInteration(isNext);
		}
	}

	private void Idle(bool isForwardDirection, bool isBlendMotion)
	{
		if (!(_renderFacade == null))
		{
			_renderFacade.Action.Idle(isBlendMotion);
			if (isForwardDirection)
			{
				_renderFacade.Action.SetDirection(new VectorXZ(0.5f, -0.5f), isBlendMotion);
			}
			_ePetActionType = EPetActionType.Idle;
		}
	}

	private void Idle(float seconds, bool isFocusDirection, bool isBlendMotion)
	{
		VespaCoroutine.Start(CoStartIdleWaitForSeconds(seconds, isFocusDirection, isBlendMotion), null, null);
	}

	private IEnumerator CoStartIdleWaitForSeconds(float seconds, bool isForwardDirection, bool isBlendMotion)
	{
		Idle(isForwardDirection, isBlendMotion);
		yield return new WaitForSeconds(seconds);
		yield return null;
		if (_ePetActionType != EPetActionType.Interaction)
		{
			_ePetActionType = EPetActionType.None;
		}
	}

	private void Walk(Vector3 targetPosition)
	{
		if (!(_renderFacade == null))
		{
			Walk();
			_moveTargetPosition = targetPosition;
			_ePetActionType = EPetActionType.Walk;
		}
	}

	private void Run(Vector3 targetPosition)
	{
		if (!(_renderFacade == null))
		{
			Run();
			_moveTargetPosition = targetPosition;
			_ePetActionType = EPetActionType.Run;
		}
	}

	private void Walk()
	{
		if (!(_renderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stock.AnimationKey = AnimationKeyData.CreateEmotiMotion("Walk");
			stock.MotionEnd = DefineCreature.EMotionEnd.Loop;
			_renderFacade.Action.ApplyModifier(ref stock);
			_renderFacade.AnimationData.Play(stock, blendMotion: true, null);
			_moveTargetPosition = null;
			_ePetActionType = EPetActionType.Interaction;
		}
	}

	private void Run()
	{
		if (!(_renderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Run;
			stock.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Run);
			stock.MotionEnd = DefineCreature.EMotionEnd.Loop;
			_renderFacade.Action.ApplyModifier(ref stock);
			_renderFacade.AnimationData.Play(stock, blendMotion: true, null);
			_moveTargetPosition = null;
			_ePetActionType = EPetActionType.Interaction;
		}
	}

	private void EndMove()
	{
		_moveTargetPosition = null;
		_ePetActionType = EPetActionType.None;
	}

	private void UpdatePosition()
	{
		if (_ePetActionType != EPetActionType.Walk && _ePetActionType != EPetActionType.Run)
		{
			return;
		}
		if (CheckArriveToTarget)
		{
			EndMove();
			return;
		}
		float num = ((_ePetActionType == EPetActionType.Run) ? RunSpeed : WalkSpeed);
		_renderFacade.transform.position = Vector3.MoveTowards(_renderFacade.transform.position, _moveTargetPosition.Value, Time.deltaTime * num);
		UpdateDirection();
		if (CheckRayCastHit)
		{
			EndMove();
		}
	}

	private void UpdateDirection()
	{
		if (!(_renderFacade == null) && _moveTargetPosition.HasValue)
		{
			Vector3 vector = _moveTargetPosition.Value - _renderFacade.transform.position;
			float magnitude = vector.magnitude;
			if (!(magnitude <= 0f))
			{
				Vector3 i_src = vector / magnitude;
				_renderFacade.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(i_src), blendMotion: true);
			}
		}
	}

	private void StartActionAnimation(string animationKey, bool isLoop, bool blendMotion = true)
	{
		if (!(_renderFacade == null))
		{
			if (_renderFacade.AssetCreature is CreatureInstance creatureInstance)
			{
				creatureInstance.CloseAllNonPermanentEffects();
			}
			NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stock.AnimationKey = AnimationKeyData.CreateEmotiMotion(animationKey);
			stock.MotionEnd = (isLoop ? DefineCreature.EMotionEnd.Loop : DefineCreature.EMotionEnd.End);
			_renderFacade.Action.ApplyModifier(ref stock);
			_renderFacade.AnimationData.Play(stock, blendMotion, null);
		}
	}

	private void HappyForInteraction(bool blendMotion)
	{
		if (!(_renderFacade == null))
		{
			StartActionAnimation("Eat", isLoop: false, blendMotion);
			_ePetActionType = EPetActionType.Interaction;
		}
	}

	private void Happy(bool blendMotion)
	{
		if (!(_renderFacade == null))
		{
			_renderFacade.Action.SetDirection(new VectorXZ(0.5f, -0.5f), blendMotion);
			StartActionAnimation("Eat", isLoop: false, blendMotion);
			_ePetActionType = EPetActionType.Happy;
		}
	}

	private void Sleep()
	{
		if (!(_renderFacade == null))
		{
			StartActionAnimation("Sleep", isLoop: true);
			_ePetActionType = EPetActionType.Sleep;
		}
	}

	private void GetUp()
	{
		if (!(_renderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stock.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Getup);
			stock.MotionEnd = DefineCreature.EMotionEnd.End;
			_renderFacade.Action.ApplyModifier(ref stock);
			_renderFacade.AnimationData.Play(stock, blendMotion: true, null);
			_ePetActionType = EPetActionType.GetUp;
			Changed();
		}
	}

	private void Slide(Vector3 startPosition)
	{
		if (!(_renderFacade == null))
		{
			_ePetActionType = EPetActionType.Slide;
			_renderFacade.transform.position = startPosition;
			_renderFacade.Action.SetDirection(VectorXZ.up, blendMotion: true);
			StartActionAnimation("Slide", isLoop: false);
		}
	}

	private void Speical()
	{
		if (!(_renderFacade == null))
		{
			StartActionAnimation(SpecialAnimationKey, isLoop: false);
			_ePetActionType = EPetActionType.Interaction;
		}
	}

	private void Stop()
	{
		if (!(_renderFacade == null))
		{
			StartActionAnimation("Stop", isLoop: false);
			_ePetActionType = EPetActionType.Interaction;
		}
	}

	private void EndSlide()
	{
		if (_ePetActionType == EPetActionType.Slide && _renderFacade.Action.IsActionEnd())
		{
			_renderFacade.transform.position = new Vector3(0f, 0f, 0.7f);
			Happy(blendMotion: false);
		}
	}

	private void EndGetUp()
	{
		if (_ePetActionType == EPetActionType.GetUp && _renderFacade.Action.IsActionEnd())
		{
			if (_isFocus)
			{
				Stop();
			}
			else
			{
				_ePetActionType = EPetActionType.None;
			}
		}
	}

	private void EndHappy()
	{
		if (_ePetActionType == EPetActionType.Happy && _renderFacade.Action.IsActionEnd())
		{
			_ePetActionType = EPetActionType.None;
		}
	}

	private void EndSpecial()
	{
		if (!(_renderFacade == null) && _renderFacade.AnimationData != null && _renderFacade.AnimationData.Runtime != null && _renderFacade.AnimationData.Runtime.Stock.AnimationKey.Name.Equals(SpecialAnimationKey) && _renderFacade.Action.IsActionEnd())
		{
			Changed();
		}
	}

	void IPet.Update(bool isTouch, bool notTouchCamera, bool isSleep)
	{
		if (_renderFacade == null)
		{
			return;
		}
		if (isSleep && _ePetActionType != EPetActionType.Sleep)
		{
			Sleep();
			return;
		}
		if (_ePetActionType == EPetActionType.Sleep && !isSleep)
		{
			GetUp();
			return;
		}
		if (notTouchCamera)
		{
			if (_ePetActionType == EPetActionType.Interaction)
			{
				Idle(isForwardDirection: false, isBlendMotion: true);
				return;
			}
		}
		else if (isTouch)
		{
			Touch();
			return;
		}
		EndSlide();
		EndGetUp();
		EndHappy();
		EndSpecial();
		if (_isFocus)
		{
			if (_ePetActionType == EPetActionType.None)
			{
				_isFocus = false;
			}
			else if (_ePetActionType != EPetActionType.Sleep)
			{
				_renderFacade.Action.SetDirection(ForwardDirection, blendMotion: true);
			}
		}
		else
		{
			UpdatePosition();
		}
	}

	void IPet.Close()
	{
		Close();
	}

	void IPet.Enable(bool isEnable)
	{
		if (_renderFacade == null)
		{
			return;
		}
		if (!isEnable)
		{
			if (_ePetActionType != EPetActionType.Sleep)
			{
				Idle(isForwardDirection: false, isBlendMotion: true);
			}
		}
		else if (_ePetActionType != EPetActionType.Slide && _ePetActionType != EPetActionType.GetUp && _ePetActionType != EPetActionType.Happy && _ePetActionType != EPetActionType.Sleep)
		{
			_ePetActionType = EPetActionType.None;
		}
		if (!isEnable)
		{
			DisablePetInterationState();
		}
		UIHelper.EnableGameObject(_renderFacade.transform, isEnable);
	}

	void IPet.SetPetInfo(PetInfo petInfo)
	{
		if (petInfo != null && _petAvatarInfo != null)
		{
			_petAvatarInfo.PetInfo = petInfo;
		}
	}

	void IPetAction.Idle(float seconds)
	{
		Idle(seconds, isFocusDirection: false, isBlendMotion: true);
	}

	void IPetAction.Walk(Vector3 targetPosition)
	{
		Walk(targetPosition);
	}

	void IPetAction.Run(Vector3 targetPosition)
	{
		Run(targetPosition);
	}

	void IPetAction.Walk()
	{
		Walk();
	}

	void IPetAction.Run()
	{
		Run();
	}

	void IPetAction.HappyForInteraction()
	{
		HappyForInteraction(blendMotion: true);
	}

	void IPetAction.Happy()
	{
		Happy(blendMotion: true);
	}

	void IPetAction.Sleep()
	{
		Sleep();
	}

	void IPetAction.Slide(Vector3 targetPosition)
	{
		Slide(targetPosition);
	}

	void IPetAction.Special()
	{
		Speical();
	}

	void IPetAction.Stop()
	{
		Stop();
	}

	void IPetInteraction.Start(float seconds)
	{
		VespaCoroutine.Start(CoStartInteraction(seconds), null, null);
	}

	void IPetInteraction.End(bool isNext)
	{
		if (_ePetActionType == EPetActionType.Interaction)
		{
			EndInteration(isNext);
			_ePetActionType = EPetActionType.None;
		}
		else if (_ePetActionType == EPetActionType.Sleep)
		{
			EndInteration(isNext);
		}
	}

	void IPetInteraction.Update()
	{
		UpdatePetInteractionState();
	}

	void IPetInteraction.DisableInteractionState()
	{
		DisablePetInterationState();
	}
}
