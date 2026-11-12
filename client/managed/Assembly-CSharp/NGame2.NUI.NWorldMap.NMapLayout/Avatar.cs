using System;
using System.Collections;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NBattleView.NUIOn3D;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NController;
using NGame2.NUI.NRender3D.NCinema;
using NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NSound;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class Avatar : WorldMapHud.IValue
{
	public class AvatarInfo
	{
		public AvatarController AvatarController;

		public MapComponent MapComponent;

		public RenderFacadeBuilder RenderFacadeBuilder;

		public IAiAvatarListener IAiAvatarListener;

		public Vector2 Distance;

		public AvatarInfo()
		{
			IAiAvatarListener = null;
			Distance = Vector2.zero;
		}
	}

	public Vector3 RearrangeOffset = Vector3.zero;

	protected WorldMapHud _hud;

	private CreatureStock.ELoadLevel renderLoadLevel = CreatureStock.ELoadLevel.Minimum;

	protected AvatarInfo _avatarInfo;

	protected RenderFacade _renderFacade;

	protected GameObject _heroGo;

	protected Vector3 _lastRearrangeOffset;

	private string _heroCode;

	private int _costumeIndex;

	private ISound _heroSoundInstance;

	private IAsyncLoad<IEffect> _headEffect;

	private float _extraAlpha = 1f;

	private DailyGiftBoxController _dailyGiftBoxController;

	private WorldMapPairEffect _pairEffect;

	public Vector3 CurrentPosition { get; private set; }

	public Vector2 CurrentDirection { get; private set; }

	public bool IsShow => _extraAlpha != 0f;

	public float ExtraAlpha => _extraAlpha;

	int WorldMapHud.IValue.CreatureUid => 0;

	float WorldMapHud.IValue.HudOffset => _renderFacade.ShapeBounds.size.y + 0.1f;

	public WorldMapHud HUD => _hud;

	public RenderFacade RenderFacade => _renderFacade;

	public AiInteractionComponent AiInteractionComponent { get; private set; }

	public InteractionComponent TouchInteractionComponent { get; protected set; }

	public void SetRenderFacadeLoadLevel(CreatureStock.ELoadLevel renderLoadLevel)
	{
		this.renderLoadLevel = renderLoadLevel;
	}

	public void Open(AvatarInfo avatarInfo)
	{
		Game3DRoot.CreateInstance();
		_avatarInfo = avatarInfo;
		CurrentDirection = Vector3.right;
		CreateHeroGameObject();
		if (_avatarInfo != null)
		{
			CreateRenderFacade(_avatarInfo.RenderFacadeBuilder);
		}
	}

	private void CreateHeroGameObject()
	{
		if (_avatarInfo != null && _avatarInfo.MapComponent != null)
		{
			_heroGo = new GameObject("WorldMap_Hero");
			_heroGo.layer = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D).Layer;
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(_heroGo.transform, _avatarInfo.MapComponent.Layout.AvatarRoot);
			SetScale(100);
		}
	}

	public void MoveToDungeonIndex(int dungeonIndex, bool userInput)
	{
		if (_avatarInfo != null && _avatarInfo.AvatarController != null)
		{
			_avatarInfo.AvatarController.BeginMoveTo(dungeonIndex, userInput);
		}
	}

	public bool IsMoving()
	{
		if (_avatarInfo == null || _avatarInfo.AvatarController == null)
		{
			return false;
		}
		return _avatarInfo.AvatarController.IsAvatarMoving();
	}

	public bool IsMoving(out int destinationIndex)
	{
		if (_avatarInfo == null || _avatarInfo.AvatarController == null)
		{
			destinationIndex = 0;
			return false;
		}
		return _avatarInfo.AvatarController.IsAvatarMoving(out destinationIndex);
	}

	public virtual int GetLastDungeonIndex()
	{
		if (_avatarInfo == null || _avatarInfo.AvatarController == null)
		{
			return 0;
		}
		return _avatarInfo.AvatarController.LastDungeonIndex;
	}

	public virtual int GetDungeonIndex()
	{
		if (_avatarInfo == null || _avatarInfo.AvatarController == null)
		{
			return 0;
		}
		return _avatarInfo.AvatarController.GetDungeonIndex();
	}

	public virtual bool CanBeRearranged()
	{
		return true;
	}

	public virtual void Close()
	{
		if (_avatarInfo != null)
		{
			if ((bool)_heroGo)
			{
				VespaObject.DestroyGo(_heroGo);
				_heroGo = null;
			}
			if ((bool)_renderFacade)
			{
				RenderFacadeBuilder.ReleaseRenderFacade(_renderFacade);
				_renderFacade = null;
			}
			ClearSound();
			RemoveHeadEffect();
			_avatarInfo.MapComponent = null;
			if (_hud != null)
			{
				WorldMapHud.ReleaseInstance(_hud);
				_hud = null;
			}
			RemovePairEffect();
			FactoryBase.ReleaseComponent(TouchInteractionComponent);
		}
	}

	public void OnLoadMap(int chapterIndex, int dungeonIndex)
	{
		if (_avatarInfo != null && _avatarInfo.MapComponent != null)
		{
			_heroGo.transform.localScale = Vector3.one * _avatarInfo.MapComponent.ScaleData.PlayerScale;
			SetChapterAvatarScale(chapterIndex);
			SetPositionByDungeonIndex(dungeonIndex);
		}
	}

	private void SetChapterAvatarScale(int chapterIndex)
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null && data.AvatarScale > 0)
		{
			SetScale(data.AvatarScale);
		}
	}

	public void SetForcedDirection(Vector2 dir)
	{
		if (_avatarInfo != null && _avatarInfo.AvatarController != null)
		{
			_avatarInfo.AvatarController.SetNextDungeonPosition(CurrentPosition + Vespa_Unity_Vector2_Extension_Method.ToVec3(dir));
		}
	}

	public void CreateRenderFacade(RenderFacadeBuilder renderFacadeBuilder)
	{
		if (_avatarInfo != null && _avatarInfo.MapComponent != null)
		{
			VectorXZ direction = VectorXZ.zero;
			bool isBattleAction = true;
			if (_renderFacade != null)
			{
				direction = _renderFacade.Action.GetDirection();
				isBattleAction = _renderFacade.Action.IsBattleMode;
				RenderFacadeBuilder.ReleaseRenderFacade(_renderFacade);
				_renderFacade = null;
			}
			renderFacadeBuilder.ParentTm = _heroGo.transform;
			renderFacadeBuilder.LoadLevel = renderLoadLevel;
			_renderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
			if (!(_renderFacade == null))
			{
				_renderFacade.Action.SetBattleMode(isBattleAction, blendMotion: false);
				_renderFacade.Action.SetDirection(direction, blendMotion: false);
				_renderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(_avatarInfo.MapComponent.Layout.Render3dAngle, depthShift: true), null);
				_heroCode = renderFacadeBuilder.GetStock().Code;
				_costumeIndex = renderFacadeBuilder.GetStock().CostumeIndex;
				SetInteractionLocalPosition();
			}
		}
	}

	public void SetEntryMenuOffset(bool isEnter)
	{
		EnableAllAiAvatar(!isEnter);
		ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(_heroCode, _costumeIndex);
		if (data != null)
		{
			Vector3 to = Vector3.zero;
			float to2 = 1f;
			if (isEnter && __Extension_Method_ICollection__.COUNT(data.EntryMenuOffset) >= 3)
			{
				to = new Vector3(data.EntryMenuOffset[0], data.EntryMenuOffset[1], data.EntryMenuOffset[2]);
				to2 = data.EntryMenuScale;
			}
			if (_dailyGiftBoxController != null)
			{
				_dailyGiftBoxController.ExceptionEnable(!isEnter);
			}
			float timeLength = Vespa_Float_Extension_Method.NoneZero(GameSetting.data.WorldMap.EnterNodeMenu.TimeForCameraBlend) * 0.8f;
			AnimationCurve curveForZoom = GameSetting.data.WorldMap.EnterNodeMenu.CurveForZoom;
			Vector3 runtimeOffset = _renderFacade.GetRuntimeOffset("EntryMenu");
			float runtimeScale = _renderFacade.GetRuntimeScale("EntryMenu");
			_renderFacade.ExtraOffsetController.SetOffset("EntryMenu", runtimeOffset, to, timeLength, curveForZoom);
			_renderFacade.ExtraScaleController.SetScale("EntryMenu", runtimeScale, to2, timeLength, curveForZoom);
		}
	}

	private void EnableAllAiAvatar(bool enable)
	{
		if (_avatarInfo != null && _avatarInfo.IAiAvatarListener != null)
		{
			_avatarInfo.IAiAvatarListener.EnableAllAiAvatar(enable);
		}
	}

	public void SetActionBattleMode(bool isBattle)
	{
		_renderFacade.Action.SetBattleMode(isBattle);
	}

	public void PlayNamedAction(string actionName)
	{
		if (!(_renderFacade == null) && !String_Extension_Method.IsNullOrEmpty(actionName))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stockData.AnimationKey = AnimationKeyData.CreateNamedAction(actionName);
			stockData.MotionEnd = DefineCreature.EMotionEnd.End;
			SetMotion(stockData);
		}
	}

	public void SetMotion(NGame2.NRender.NRenderFacade.AnimationData.StockData motionStock, bool unUseBlenMotion = false)
	{
		_renderFacade.Action.Motion(motionStock, null, null, unUseBlenMotion);
		bool enable = !TryPlaySound(motionStock.AnimationKey.PlayName);
		_renderFacade.EnableAnimationSound(enable);
	}

	private bool TryPlaySound(string motionCode)
	{
		ClearSound();
		if (RenderFacade == null || RenderFacade.AssetCreatureAsync == null)
		{
			return false;
		}
		CreatureStock stock = RenderFacade.AssetCreatureAsync.Stock;
		string code = stock.Code;
		string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(stock.CostumeIndex);
		SoundStock stock2 = new SoundStock
		{
			Code = $"{motionCode}.{code}{costumeVoicePostfix}"
		};
		if (!SoundHelper.IsExistSoundData(stock2.Code))
		{
			stock2.Code = $"{motionCode}.{code}";
		}
		_heroSoundInstance = SingletonAuto<SoundContainer>.instance.CreateSoundExact(stock2);
		if (_heroSoundInstance == null)
		{
			return false;
		}
		SingletonAuto<SoundContainer>.instance.PlayOnce(_heroSoundInstance, 10f);
		return true;
	}

	protected void ClearSound()
	{
		if (_heroSoundInstance != null)
		{
			SoundContainer.ReleaseSound(_heroSoundInstance);
			_heroSoundInstance = null;
		}
	}

	public void SetAnimation(bool isMoving)
	{
		if (isMoving)
		{
			if (_renderFacade.AnimationData.CurrentState != NGame2.NRender.NRenderFacade.AnimationData.EState.Run)
			{
				NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
				stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Run;
				stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Run);
				stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
				_renderFacade.AnimationData.Play(stockData, null);
			}
		}
		else if (_renderFacade.AnimationData.CurrentState != NGame2.NRender.NRenderFacade.AnimationData.EState.Action && _renderFacade.AnimationData.CurrentState != NGame2.NRender.NRenderFacade.AnimationData.EState.Idle)
		{
			_renderFacade.Action.Idle();
		}
	}

	public void SetPosition(Vector3 pos)
	{
		CurrentPosition = pos;
		UpdatePosition();
	}

	public void SetPositionByDungeonIndex(int dungeonIndex)
	{
		if (_avatarInfo != null && _avatarInfo.MapComponent != null)
		{
			Vector3 nodePositionByDungeonIndex = _avatarInfo.MapComponent.NodeGraph.GetNodePositionByDungeonIndex(dungeonIndex);
			if (nodePositionByDungeonIndex == Vector3.zero)
			{
				nodePositionByDungeonIndex = _avatarInfo.MapComponent.NodeGraph.GetNodePositionByDungeonIndex(1);
				NGame2.NAccount.UserCampaignManager.DungeonIndex = 1;
			}
			SetPosition(nodePositionByDungeonIndex);
		}
	}

	public void SetDirection(float angle)
	{
		CurrentDirection = NVespa.Math.Rotate(angle);
	}

	public virtual void SetScale(int scalePercentage)
	{
		if (_avatarInfo != null && _avatarInfo.MapComponent != null)
		{
			_heroGo.transform.localScale = Vector3.one * _avatarInfo.MapComponent.ScaleData.PlayerScale * scalePercentage / 100f;
		}
	}

	public Vector3 GetCurrentWorldPosition()
	{
		return _heroGo.transform.position;
	}

	public Vector3 GetLocalPosition()
	{
		return CurrentPosition;
	}

	public Vector3 GetFinalLocalPosition()
	{
		return _heroGo.transform.localPosition;
	}

	public void SetExtraAlpha(float alpha)
	{
		_extraAlpha = alpha;
		if (_renderFacade != null && _renderFacade.AssetCreature != null)
		{
			_renderFacade.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(alpha);
		}
		if (_headEffect != null)
		{
			_headEffect.Value.GetRenderGroupModifier().SetExtraAlpha(alpha);
		}
	}

	public void SetHeadEffect(string EffectCode)
	{
		if (_avatarInfo != null && _avatarInfo.MapComponent != null && !(_renderFacade == null))
		{
			if (_headEffect != null)
			{
				RemoveHeadEffect();
			}
			_headEffect = SingletonAuto<EffectContainer>.instance.CreateAsync(EffectCode, _heroGo.layer);
			_headEffect.Value.SetParent(_renderFacade.HeightTm);
			_headEffect.Value.SetShaderTransform(new ShaderTransformData(_avatarInfo.MapComponent.Layout.Render3dAngle, depthShift: true), _renderFacade.transform);
			if (!Mathf.Approximately(_extraAlpha, 1f))
			{
				_headEffect.Value.GetRenderGroupModifier().SetExtraAlpha(_extraAlpha);
			}
		}
	}

	public void RemoveHeadEffect()
	{
		if (_headEffect != null)
		{
			EffectContainer.ReleaseEffect(_headEffect, detachParent: true, immediatelyRelease: true);
			_headEffect = null;
		}
	}

	public virtual void ChainUpdate()
	{
		UpdatePosition();
		if ((bool)_renderFacade)
		{
			_renderFacade.ChainUpdate();
		}
		if (_headEffect != null)
		{
			_headEffect.Value.Update();
		}
		if (_hud != null)
		{
			_hud.ChainUpdate();
		}
	}

	private void UpdatePosition()
	{
		if (_avatarInfo != null && _avatarInfo.MapComponent != null && _avatarInfo.AvatarController != null)
		{
			Vector3 vector = Vector3.zero;
			if (CanBeRearranged())
			{
				vector = RearrangeOffset;
			}
			float rearrangeLerpFactor = _avatarInfo.MapComponent.Layout.AvatarOption.RearrangeLerpFactor;
			_lastRearrangeOffset = Vector3.Lerp(_lastRearrangeOffset, vector, Time.deltaTime * rearrangeLerpFactor);
			Vector3 vector2 = CurrentPosition + _lastRearrangeOffset;
			vector2.x += _avatarInfo.Distance.x;
			vector2.y += _avatarInfo.Distance.y;
			Vector3 localPosition = _heroGo.transform.localPosition;
			if (!Mathf.Approximately(Vector3.Distance(vector2, localPosition), 0f))
			{
				_heroGo.transform.localPosition = vector2;
			}
			VectorXZ direction = _renderFacade.Action.GetDirection();
			VectorXZ vectorXZ = Vespa_Unity_Vector2_Extension_Method.ToVecXZ(CurrentDirection);
			float basicRotate = _avatarInfo.MapComponent.Layout.AvatarOption.BasicRotate;
			if (!IsMoving() && vector.magnitude > Mathf.Epsilon)
			{
				vectorXZ = -Vespa_Unity_Vector2_Extension_Method.ToVecXZ(Vespa_Unity_Vector3_Extension_Method.XY(vector).normalized);
				basicRotate = _avatarInfo.MapComponent.Layout.AvatarOption.RearrangeRotate;
			}
			else
			{
				basicRotate *= VectorXZ.Angle(direction, vectorXZ) * (MathF.PI / 180f);
			}
			if (VectorXZ.Angle(direction, -vectorXZ) < 0.1f)
			{
				vectorXZ = NVespa.Math.Rotate(vectorXZ, 0.2f);
			}
			VectorXZ vectorXZ2 = Vespa_Unity_Vector3_Extension_Method.VecXZ(Vector3.RotateTowards(direction.ToVec3(), vectorXZ.ToVec3(), Time.deltaTime * basicRotate, 0f));
			if (!NVespa.Math.Approximately(direction, vectorXZ2))
			{
				_renderFacade.Action.SetDirection(vectorXZ2, blendMotion: false);
			}
		}
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(_heroGo, show);
		if (_hud != null)
		{
			UIHelper.EnableGameObject(_hud.gameObject, show);
		}
	}

	public void CreateHud()
	{
		if (_hud != null)
		{
			WorldMapHud.ReleaseInstance(_hud);
			_hud = null;
		}
		_hud = WorldMapHud.CreateInstance(_heroGo.transform);
		if (_hud == null)
		{
			VespaDebug.LogError("hud is null");
			return;
		}
		_hud.Open(this);
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(_hud.gameObject, layerInfo.Layer);
	}

	public void SetHudName(string hudName)
	{
		if (_hud == null)
		{
			CreateHud();
		}
		_hud.SetName(hudName);
	}

	public void CreateDailyGiftBox()
	{
		_dailyGiftBoxController = new DailyGiftBoxController();
		if (_dailyGiftBoxController != null)
		{
			CreateInteractionComponent(_dailyGiftBoxController);
			if (!(AiInteractionComponent == null))
			{
				_dailyGiftBoxController.Open(AiInteractionComponent, this);
				AiInteractionComponent.SetInterationType(AiInteractionComponent.InteractionType.Lobby);
				AiInteractionComponent.ShowName(isShow: false);
			}
		}
	}

	private void SetInteractionLocalPosition()
	{
		if (AiInteractionComponent != null)
		{
			AiInteractionComponent.SetLocalPosition(this);
		}
	}

	protected void CreateInteractionComponent(AiInteractionComponent.Listener listener)
	{
		if (AiInteractionComponent != null)
		{
			AiInteractionComponent.ReleaseInstance(AiInteractionComponent);
			AiInteractionComponent = null;
		}
		if (!(_heroGo == null))
		{
			AiInteractionComponent = AiInteractionComponent.CreateInstance(_heroGo.transform);
			if (AiInteractionComponent == null)
			{
				VespaDebug.LogError("interActionComponent is null");
				return;
			}
			AiInteractionComponent.Open(this, listener);
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D);
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(AiInteractionComponent.gameObject, layerInfo.Layer);
		}
	}

	private void CreatePairEffect()
	{
		RemovePairEffect();
		_pairEffect = WorldMapPairEffect.CreateInstance(_heroGo.transform);
		if (_pairEffect == null)
		{
			VespaDebug.LogError("Pair Effecct is null");
			return;
		}
		_pairEffect.Open(_renderFacade.ShapeBounds.size.y + 0.7f);
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(_pairEffect.gameObject, layerInfo.Layer);
	}

	public void RemovePairEffect()
	{
		if (!(_pairEffect == null))
		{
			WorldMapPairEffect.ReleaseInstance(_pairEffect);
			_pairEffect = null;
		}
	}

	public void PlayPairEffect(PairEffectType type, System.Action completeAction)
	{
		if (_pairEffect == null)
		{
			CreatePairEffect();
		}
		_pairEffect.PlayPairEffect(type, completeAction);
	}

	public void Show()
	{
		VespaCoroutine.Start(coShowHide());
	}

	public void Hide()
	{
		VespaCoroutine.Start(coShowHide(isShow: false));
	}

	private IEnumerator coShowHide(bool isShow = true)
	{
		float elapsedTime = 0f;
		float blendTimeLength = Vespa_Float_Extension_Method.NoneZero(GameSetting.data.WorldMap.EnterNodeMenu.TimeForCameraBlend) * 0.8f;
		do
		{
			yield return null;
			elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / blendTimeLength);
			float extraAlpha = (isShow ? num : (1f - num));
			SetExtraAlpha(extraAlpha);
		}
		while (!(elapsedTime >= blendTimeLength));
	}
}
