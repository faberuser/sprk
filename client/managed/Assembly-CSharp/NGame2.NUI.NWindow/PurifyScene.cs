using System;
using System.Collections;
using NGame2.N3D;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NRender3D;
using NGame2.NUtil.NCostume;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NSound;
using NShared;
using NVespa;
using NVespa.NGui;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PurifyScene
{
	public enum EPurifyState
	{
		Begin,
		Half,
		Full
	}

	public const string Action_Purify1 = "Purify1";

	public const string Action_PurifyReaction1 = "PurifyReaction1";

	public const string Action_Purify2 = "Purify2";

	public const string Action_PurifyReaction2 = "PurifyReaction2";

	public static readonly string[] PurifyActions = new string[4] { "Purify1", "PurifyReaction1", "Purify2", "PurifyReaction2" };

	public EPurifyState PurifyState;

	protected ISound _heroSound;

	private UITexture _uiTex;

	private CachedHeroInfo _heroInfo;

	private float _purifyRatio;

	private bool _isAwaken;

	private Coroutine _openCoroutine;

	private Coroutine _trailCoroutine;

	private PurifyCamera _purifyCamera = new PurifyCamera();

	private HeroPreview _heroPreview;

	private Vector3 _darknessPos;

	private float _purifyScale = 1f;

	private Vector3 _cameraOffset;

	private IAsyncLoad<IEffect> _darknessEffectAsync;

	private IAsyncLoad<IEffect> _groundEffectAsync;

	private IAsyncLoad<IEffect> _trailEffectAsync;

	private GameObject _trailGo;

	public PurifyCamera GetPurifyCamera()
	{
		return _purifyCamera;
	}

	public void Open(UITexture uiTex, CachedHeroInfo heroInfo, float purifyRatio, bool isAwaken)
	{
		if (!(_uiTex == uiTex))
		{
			Close();
			_purifyScale = 1f;
			_cameraOffset = Vector3.zero;
			_uiTex = uiTex;
			_heroInfo = heroInfo;
			_purifyRatio = purifyRatio;
			_isAwaken = isAwaken;
			_openCoroutine = VespaCoroutine.StartScene(coOpen());
		}
	}

	public void Close()
	{
		if (_trailCoroutine != null)
		{
			VespaCoroutine.StopScene(_trailCoroutine);
			_trailCoroutine = null;
		}
		if (_openCoroutine != null)
		{
			VespaCoroutine.StopScene(_openCoroutine);
			_openCoroutine = null;
		}
		if (_darknessEffectAsync != null)
		{
			EffectContainer.ReleaseEffect(_darknessEffectAsync, detachParent: true, immediatelyRelease: true);
			_darknessEffectAsync = null;
		}
		if (_groundEffectAsync != null)
		{
			EffectContainer.ReleaseEffect(_groundEffectAsync, detachParent: true, immediatelyRelease: true);
			_groundEffectAsync = null;
		}
		if (_trailEffectAsync != null)
		{
			EffectContainer.ReleaseEffect(_trailEffectAsync, detachParent: true, immediatelyRelease: true);
			_trailEffectAsync = null;
		}
		if (_heroPreview != null)
		{
			HeroPreview.ReleaseInstance(_heroPreview);
			_heroPreview = null;
		}
		if (_uiTex != null)
		{
			_uiTex.mainTexture = null;
			_uiTex = null;
		}
		_heroInfo = null;
		ClearSound();
	}

	public void CreatePurifyTrail(System.Action onComplete)
	{
		if (_trailCoroutine != null)
		{
			VespaCoroutine.StopScene(_trailCoroutine);
			_trailCoroutine = null;
		}
		if (_trailEffectAsync != null)
		{
			EffectContainer.ReleaseEffect(_trailEffectAsync, detachParent: true);
			_trailEffectAsync = null;
		}
		_trailCoroutine = VespaCoroutine.StartScene(coCreatePurifyTrail(onComplete));
	}

	private IEnumerator coCreatePurifyTrail(System.Action onComplete)
	{
		Vector3 beginPos = _heroPreview.MainCamera.transform.position + _heroPreview.MainCamera.transform.forward * GameSetting.data.Ui.PurifyHero_TrailBeginForward + _heroPreview.MainCamera.transform.up * GameSetting.data.Ui.PurifyHero_TrailBeginUp;
		if (_trailGo == null)
		{
			_trailGo = new GameObject("TrailGo");
		}
		_trailGo.transform.position = beginPos;
		if (_isAwaken)
		{
			_trailEffectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Purify.Point", _heroPreview.RenderFacade.gameObject.layer);
		}
		else
		{
			_trailEffectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Transcendence.Point", _heroPreview.RenderFacade.gameObject.layer);
		}
		while (!_trailEffectAsync.ISDONE())
		{
			yield return null;
		}
		_trailEffectAsync.Value.SetParent(_trailGo.transform);
		float elapsedTime = -0.1f;
		float purifyHero_TrailTimeLength;
		do
		{
			yield return null;
			purifyHero_TrailTimeLength = GameSetting.data.Ui.PurifyHero_TrailTimeLength;
			elapsedTime += GlobalTimer.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / purifyHero_TrailTimeLength);
			float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.Ui.PurifyHero_TrailUpCurve, num, 0f) * GameSetting.data.Ui.PurifyHero_TrailUpCurveLength;
			float num3 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.Ui.PurifyHero_TrailRightCurve, num, 0f) * GameSetting.data.Ui.PurifyHero_TrailRightCurveLength;
			Vector3 position = Vector3.Lerp(beginPos, _darknessPos, num) + _heroPreview.MainCamera.transform.right * num3 * _purifyScale + _heroPreview.MainCamera.transform.up * num2 * _purifyScale;
			_trailGo.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 0.5f, num) * _purifyScale;
			_trailGo.transform.position = position;
			_trailEffectAsync.Value.Update();
		}
		while (!(elapsedTime >= purifyHero_TrailTimeLength));
		yield return null;
		EffectContainer.ReleaseEffect(_trailEffectAsync, detachParent: true);
		IAsyncLoad<IEffect> asyncLoad = ((!_isAwaken) ? SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Transcendence.Glow", _heroPreview.RenderFacade.gameObject.layer) : SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Purify.Glow", _heroPreview.RenderFacade.gameObject.layer));
		asyncLoad.Value.SetDummyParent(_darknessPos, _heroPreview.MainCamera.transform.rotation, Vector3.one * _purifyScale * 0.5f);
		SingletonUpdater<AssetUpdateManager>.instance.Add(asyncLoad, detachParentOnRelease: true, isGlobal: false, null);
		_trailEffectAsync = null;
		_trailCoroutine = null;
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void UpdatePurifyRatio(float ratio)
	{
		_purifyRatio = ratio;
		if (_darknessEffectAsync != null && !(ratio < 0.25f))
		{
			if (ratio < 0.5f)
			{
				_darknessEffectAsync.Value.PlayPhase("25");
			}
			else if (ratio < 0.75f)
			{
				_darknessEffectAsync.Value.PlayPhase("50");
			}
			else
			{
				_darknessEffectAsync.Value.PlayPhase("75");
			}
		}
	}

	public void PurifyAction(bool isReaction, bool blendAction = true)
	{
		if (_heroPreview != null && _heroPreview.RenderFacade != null)
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, GetPurifyAction(isReaction));
			stockData.MotionEnd = ((!isReaction) ? DefineCreature.EMotionEnd.Loop : DefineCreature.EMotionEnd.End);
			_heroPreview.RenderFacade.Action.Motion(stockData, blendAction, null);
			bool enable = !TryPlaySound(stockData.AnimationKey.PlayName);
			_heroPreview.RenderFacade.EnableAnimationSound(enable);
		}
	}

	private bool TryPlaySound(string motionName)
	{
		ClearSound();
		SoundStock stock = default(SoundStock);
		if (_heroInfo == null)
		{
			return false;
		}
		string codeName = _heroInfo.CreatureData.CodeName;
		string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(_heroInfo);
		string soundCode = SoundHelper.GetSoundCode(motionName, codeName, costumeVoicePostfix);
		stock.Code = soundCode;
		_heroSound = SingletonAuto<SoundContainer>.instance.CreateSoundExact(stock);
		if (_heroSound == null)
		{
			return false;
		}
		SingletonAuto<SoundContainer>.instance.PlayOnce(_heroSound, 10f);
		return true;
	}

	protected void ClearSound()
	{
		if (_heroSound != null)
		{
			SoundContainer.ReleaseSound(_heroSound);
			_heroSound = null;
		}
	}

	public void ChainUpdate()
	{
		if (_heroPreview != null && _heroPreview.RenderFacade != null && _heroPreview.RenderFacade.Action.IsActionEnd())
		{
			PurifyAction(isReaction: false);
		}
		if (_darknessEffectAsync != null)
		{
			_darknessEffectAsync.Value.Update();
		}
		if (_groundEffectAsync != null)
		{
			_groundEffectAsync.Value.Update();
		}
	}

	private string GetPurifyAction(bool isReaction)
	{
		if (PurifyState == EPurifyState.Begin)
		{
			if (isReaction)
			{
				return "PurifyReaction1";
			}
			return "Purify1";
		}
		if (isReaction)
		{
			return "PurifyReaction2";
		}
		return "Purify2";
	}

	private IEnumerator coOpen()
	{
		using (new DisposableAction(delegate
		{
			_openCoroutine = null;
		}))
		{
			_uiTex.mainTexture = DrawTools.BlackTex;
			int num = HeroCostume.CheckReplaceBasicCostume(_heroInfo.CostumeIndex, _heroInfo.HeroIndex, _heroInfo.Star);
			RenderFacadeBuilder renderFacadeBuilder;
			if (num != -1)
			{
				CachedHeroInfo cachedHeroInfo = JsonMarshaler.Clone(_heroInfo);
				cachedHeroInfo.CostumeIndex = num;
				renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, cachedHeroInfo);
			}
			else
			{
				renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, _heroInfo);
			}
			renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
			for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(PurifyActions); num2++)
			{
				AnimationKeyData animationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, PurifyActions[num2]);
				renderFacadeBuilder.AddPreload(animationKey);
			}
			HeroPreview.InfoData info = default(HeroPreview.InfoData);
			SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out info.RenderTargetWidth, out info.RenderTargetHeight);
			info.BackgroundName = null;
			info.RenderFacadeBuilder = renderFacadeBuilder;
			_heroPreview = HeroPreview.CreateInstance(info);
			if (_heroPreview == null)
			{
				yield break;
			}
			PurifyAction(isReaction: false);
			ICreatureLoad creatureAsync = _heroPreview.RenderFacade.AssetCreatureAsync;
			while (!creatureAsync.ISDONE())
			{
				yield return null;
			}
			if (!creatureAsync.ISVALID())
			{
				yield break;
			}
			ClientCreatureData clientCreatureData = DataHelper.GetClientCreatureDataByIndex(_heroInfo.HeroIndex, _heroInfo.CostumeIndex);
			TransformData transformData = new TransformData();
			if (clientCreatureData != null)
			{
				_darknessPos = NVespa.Math.Vec3FromArray(clientCreatureData.PurifyDarknessOffset);
				_purifyScale = clientCreatureData.PurifyCameraScale;
				if (_purifyScale < 0.1f)
				{
					_purifyScale = 1f;
				}
				_cameraOffset = NVespa.Math.Vec3FromArray(clientCreatureData.PurifyCameraOffset);
			}
			else
			{
				_darknessPos = new Vector3(0f, 1.5f, 0.6f);
				_purifyScale = 1f;
				_cameraOffset = Vector3.zero;
			}
			transformData.Position = _darknessPos;
			if (_isAwaken)
			{
				_darknessEffectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Purify.Darkness", _heroPreview.RenderFacade.gameObject.layer, transformData);
				_groundEffectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Purify.Background", _heroPreview.RenderFacade.gameObject.layer);
			}
			else
			{
				_darknessEffectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Transcendence.Darkness", _heroPreview.RenderFacade.gameObject.layer, transformData);
				_groundEffectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Transcendence.Background", _heroPreview.RenderFacade.gameObject.layer);
			}
			_darknessEffectAsync.Value.SetParent(_heroPreview.RenderFacade.transform);
			_groundEffectAsync.Value.SetParent(_heroPreview.RenderFacade.transform);
			_darknessPos = _heroPreview.RenderFacade.transform.localToWorldMatrix.MultiplyPoint(_darknessPos);
			while (!_darknessEffectAsync.ISDONE() || !_groundEffectAsync.ISDONE())
			{
				yield return null;
			}
			Vector3 purifyHero_CameraTarget = GameSetting.data.Ui.PurifyHero_CameraTarget;
			Vector3 purifyHero_CameraPos = GameSetting.data.Ui.PurifyHero_CameraPos;
			if (clientCreatureData != null)
			{
				purifyHero_CameraTarget *= _purifyScale;
				purifyHero_CameraPos *= _purifyScale;
				purifyHero_CameraTarget += _cameraOffset;
				purifyHero_CameraPos += _cameraOffset;
			}
			_purifyCamera.Open(purifyHero_CameraTarget, (purifyHero_CameraTarget - purifyHero_CameraPos).normalized, Vector3.Distance(purifyHero_CameraPos, purifyHero_CameraTarget));
			_heroPreview.PreviewCamera = _purifyCamera;
			_uiTex.mainTexture = _heroPreview.RenderTarget;
			UpdatePurifyRatio(_purifyRatio);
		}
	}
}
