using System.Collections.Generic;
using NGame2.NCutScene.NModifier;
using NGame2.NUtil;
using NGame2.NUtil.NSoundFx;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class AnimationData
{
	public enum EState
	{
		None,
		Idle,
		Run,
		Die,
		Damage,
		Stun,
		Action,
		Pause
	}

	public enum EMotionEnd
	{
		End,
		Loop,
		Infinite
	}

	public struct Modifier
	{
		public AnimationKeyData AnimationKey;

		public bool UseCustomTimeValue;

		public StockData.ETimeType TimeType;

		public float TimeValue;

		public int Stack;
	}

	public class StockData
	{
		public enum ETimeType
		{
			Length,
			Multiply
		}

		public EState State;

		public AnimationKeyData AnimationKey;

		public DefineCreature.EMotionEnd MotionEnd;

		public ETimeType TimeType;

		public float TimeValue;

		public float BlendTime = -1f;

		public AnimationCurve BlendCurve;

		public bool IsPlaySound = true;

		public float TimeLength
		{
			set
			{
				TimeType = ETimeType.Length;
				TimeValue = value;
			}
		}

		public float TimeMultiply
		{
			set
			{
				TimeType = ETimeType.Multiply;
				TimeValue = value;
			}
		}
	}

	public class RuntimeData
	{
		public float BlendTime;

		public ICreatureLoad CreatureAsync;

		public StockData Stock;

		public AnimationStock GameAssetStock;

		public float BlendElapsedTime;

		private IAnimationData _animationData;

		private bool _isEnd;

		private bool _enableSoundFx = true;

		private int _soundFxPlayCount;

		private int _soundFxPlayCount2;

		private List<FrameSoundData> _soundFxList2;

		public float TimeLength
		{
			get
			{
				if (Stock.TimeType == StockData.ETimeType.Length)
				{
					if (Stock.TimeValue > Mathf.Epsilon)
					{
						return Stock.TimeValue;
					}
					if (AnimationData != null)
					{
						return AnimationData.TimeLength;
					}
					return 1f;
				}
				if (Stock.TimeType == StockData.ETimeType.Multiply)
				{
					return ((AnimationData != null) ? AnimationData.TimeLength : 1f) * Vespa_Float_Extension_Method.NoneZero(Stock.TimeValue);
				}
				return 1f;
			}
		}

		public bool IsEnd
		{
			get
			{
				if (Stock.State == EState.Pause)
				{
					return false;
				}
				if (Stock.MotionEnd != DefineCreature.EMotionEnd.End)
				{
					return false;
				}
				return _isEnd;
			}
		}

		public bool IsMotionEnd => _isEnd;

		public bool EnableSoundFx
		{
			get
			{
				return _enableSoundFx;
			}
			set
			{
				_enableSoundFx = value;
			}
		}

		public IAnimationData AnimationData
		{
			get
			{
				if (_animationData != null)
				{
					return _animationData;
				}
				if (CreatureAsync == null || CreatureAsync.Value == null)
				{
					return null;
				}
				_animationData = CreatureAsync.Value.GetAnimationData(Stock.AnimationKey);
				return _animationData;
			}
		}

		public float Frame
		{
			get
			{
				return GameAssetStock.Frame;
			}
			set
			{
				GameAssetStock.Frame = value;
			}
		}

		public float Weight
		{
			get
			{
				return GameAssetStock.Weight;
			}
			set
			{
				GameAssetStock.Weight = value;
			}
		}

		public int Uid { get; private set; }

		private List<EventSoundFx> _soundFxList
		{
			get
			{
				if (!Stock.IsPlaySound)
				{
					return null;
				}
				return AnimationData?.GetSoundFxList();
			}
		}

		private RuntimeData()
		{
		}

		internal RuntimeData(int uid, StockData stock, ICreatureLoad creatureAsync, FacialAnimationData facialAnimation)
		{
			Uid = uid;
			Stock = stock;
			CreatureAsync = creatureAsync;
			GameAssetStock = default(AnimationStock);
			GameAssetStock.Key = stock.AnimationKey;
			GameAssetStock.FacialAnimation = facialAnimation;
			if (Stock.IsPlaySound)
			{
				_soundFxPlayCount2 = 0;
				_soundFxList2 = SingletonAuto<SoundFxManager>.instance.GetAnimationSoundFx(CreatureAsync.Stock.Code, stock.AnimationKey);
			}
		}

		internal void Framemove(float deltaTime, ILocalTimer localTimer, RenderFacade facade)
		{
			if (_isEnd)
			{
				return;
			}
			float frame = Frame;
			Frame += deltaTime / Vespa_Float_Extension_Method.NoneZero(TimeLength);
			if (!Mathf.Approximately(frame, Frame))
			{
				UpdateSoundFx(localTimer, facade);
				UpdateSoundFx2(localTimer, facade);
			}
			if (Frame >= 1f)
			{
				if (Stock.MotionEnd == DefineCreature.EMotionEnd.Loop)
				{
					Frame = Mathf.Repeat(Frame, 1f);
					_soundFxPlayCount = 0;
				}
				else
				{
					Frame = 1f;
					_isEnd = true;
				}
			}
		}

		private void UpdateSoundFx(ILocalTimer localTimer, RenderFacade facade)
		{
			List<EventSoundFx> soundFxList = _soundFxList;
			if (soundFxList == null)
			{
				return;
			}
			for (int i = _soundFxPlayCount; i < soundFxList.Count && !(soundFxList[i].Frame > Frame); i++)
			{
				_soundFxPlayCount++;
				if (!EnableSoundFx)
				{
					break;
				}
				SoundStock stock = new SoundStock
				{
					Code = soundFxList[i].SoundCode,
					LocalTimer = facade.LocalTimer
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 99f);
			}
		}

		private void UpdateSoundFx2(ILocalTimer localTimer, RenderFacade facade)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_soundFxList2))
			{
				return;
			}
			for (int i = _soundFxPlayCount2; i < _soundFxList2.Count && !(_soundFxList2[i].Frame > Frame); i++)
			{
				_soundFxPlayCount2++;
				if (!EnableSoundFx)
				{
					break;
				}
				SoundStock stock = new SoundStock
				{
					Code = _soundFxList2[i].SoundCode,
					LocalTimer = facade.LocalTimer
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 99f);
			}
		}

		public void UpdateFacialAnimationTick(float deltaTick)
		{
			if (GameAssetStock.FacialAnimation != null && GameAssetStock.FacialAnimation.BlendData != null)
			{
				GameAssetStock.FacialAnimation.BlendData.UpdateTick(deltaTick);
			}
			if (GameAssetStock.PrevFacialAnimation != null && GameAssetStock.PrevFacialAnimation.BlendData != null)
			{
				if (GameAssetStock.PrevFacialAnimation.BlendData.Weight != 0f)
				{
					GameAssetStock.PrevFacialAnimation.BlendData.UpdateTick(0f - deltaTick);
				}
				else
				{
					GameAssetStock.PrevFacialAnimation = null;
				}
			}
		}
	}

	private class AnimationTimer
	{
		public bool PauseCurrent;

		private ILocalTimer _baseTimer;

		private ILocalTimer _timer;

		public float DeltaTime
		{
			get
			{
				if (PauseCurrent)
				{
					return 0f;
				}
				if (_timer != null)
				{
					return (float)_timer.deltaTick * 0.001f;
				}
				if (_baseTimer != null)
				{
					return (float)_baseTimer.deltaTick * 0.001f;
				}
				return MainTimer.deltaTime;
			}
		}

		public ILocalTimer Timer
		{
			get
			{
				if (_timer != null)
				{
					return _timer;
				}
				if (_baseTimer != null)
				{
					return _baseTimer;
				}
				return MainTimer.DefaultLocal;
			}
		}

		public void SetBaseTimer(ILocalTimer baseTimer)
		{
			_baseTimer = baseTimer;
		}

		public void SetTimer(ILocalTimer i_timer)
		{
			_timer = i_timer;
			PauseCurrent = false;
		}
	}

	private class DirectionData
	{
		public bool IsDirty;

		private float _viewAngle;

		private float _realAngle;

		public VectorXZ RealDirection => Math.RotateXZ(_realAngle);

		public float ViewAngle
		{
			get
			{
				return _viewAngle;
			}
			private set
			{
				if (!Mathf.Approximately(_viewAngle, value))
				{
					_viewAngle = value;
					IsDirty = true;
				}
			}
		}

		public float RealAngle
		{
			get
			{
				return _realAngle;
			}
			private set
			{
				if (!Mathf.Approximately(_realAngle, value))
				{
					_realAngle = value;
					IsDirty = true;
				}
			}
		}

		public void SetAllDirection(VectorXZ dir)
		{
			float realAngle = (ViewAngle = Math.Angle(dir));
			RealAngle = realAngle;
		}

		public void SetRealDirection(VectorXZ dir)
		{
			float realAngle = Math.Angle(dir);
			RealAngle = realAngle;
		}

		public void Framemove(float blendTime, float deltaTime)
		{
			if (!Mathf.Approximately(_viewAngle, _realAngle))
			{
				float maxDelta = 180f * Mathf.Clamp01(deltaTime / Vespa_Float_Extension_Method.NoneZero(blendTime));
				ViewAngle = Mathf.MoveTowardsAngle(ViewAngle, RealAngle, maxDelta);
			}
		}
	}

	public const float DirectionBlendSpeed = 20f;

	public List<RuntimeData> SubRuntimeList = new List<RuntimeData>();

	private RenderFacade _facade;

	private AnimationTimer _timer = new AnimationTimer();

	private DirectionData _direction = new DirectionData();

	private AnimationKeyData _prevAnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.BattleIdle);

	private int _nextRuntimeUid = 1;

	public EState CurrentState
	{
		get
		{
			if (Runtime == null)
			{
				return EState.None;
			}
			return Runtime.Stock.State;
		}
	}

	public RuntimeData Runtime { get; private set; }

	public VectorXZ RealDirection => _direction.RealDirection;

	public AnimationData(RenderFacade facade)
	{
		_facade = facade;
		_direction.SetAllDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(Vector3.forward));
	}

	public void SetBaseTimer(ILocalTimer baseTimer)
	{
		_timer.SetBaseTimer(baseTimer);
	}

	public void SetSFXEnable(bool enable)
	{
		if (Runtime != null)
		{
			Runtime.EnableSoundFx = enable;
		}
	}

	public void SetRuntimeTimer(ILocalTimer timer)
	{
		_timer.SetTimer(timer);
	}

	public void ChainUpdate()
	{
		if (Runtime == null)
		{
			return;
		}
		FramemoveDirection();
		if (!_timer.PauseCurrent)
		{
			Framemove();
			float blendTime = Runtime.BlendTime;
			float weightDelta = _timer.DeltaTime / Vespa_Float_Extension_Method.NoneZero(blendTime);
			if (Runtime.Stock != null && Runtime.Stock.BlendCurve != null && !Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(Runtime.Stock.BlendCurve))
			{
				Runtime.BlendElapsedTime += _timer.DeltaTime;
				float num = Mathf.Clamp01(Runtime.BlendElapsedTime / Vespa_Float_Extension_Method.NoneZero(blendTime));
				float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Runtime.Stock.BlendCurve, num, num);
				weightDelta = Mathf.Max(0f, num2 - Runtime.Weight);
			}
			Runtime.UpdateFacialAnimationTick(_timer.DeltaTime);
			Runtime.Weight = CalcRuntimeWeight(weightDelta);
			if (_facade.AssetCreature != null)
			{
				_facade.AssetCreature.SetAnimation(Runtime.GameAssetStock);
			}
		}
	}

	public void Pause()
	{
		_timer.PauseCurrent = true;
		if (Runtime != null)
		{
			Runtime.Stock.State = EState.Pause;
		}
	}

	public void Resume(EState newState)
	{
		_timer.PauseCurrent = false;
		if (Runtime != null)
		{
			Runtime.Stock.State = newState;
		}
	}

	public void SetDirection(VectorXZ dir)
	{
		_direction.SetAllDirection(dir);
		_facade.DirectionReal.localRotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z), Vector3.up);
	}

	public void BlendDirection(VectorXZ dir)
	{
		_direction.SetRealDirection(dir);
		_facade.DirectionReal.localRotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z), Vector3.up);
	}

	public void Play(StockData stock, float frame, ILocalTimer timer, FacialAnimationData facialAnimation = null)
	{
		if (_facade.AssetCreatureAsync != null)
		{
			RuntimeData runtimeData = new RuntimeData(_nextRuntimeUid++, stock, _facade.AssetCreatureAsync, facialAnimation);
			runtimeData.Frame = frame;
			runtimeData.Weight = 1f;
			Play(runtimeData, timer);
		}
	}

	public void Play(StockData stock, bool blendMotion, ILocalTimer timer, FacialAnimationData facialAnimation = null)
	{
		if (_facade.AssetCreatureAsync != null)
		{
			RuntimeData runtimeData = new RuntimeData(_nextRuntimeUid++, stock, _facade.AssetCreatureAsync, facialAnimation);
			runtimeData.Weight = (blendMotion ? 0f : 1f);
			Play(runtimeData, timer);
		}
	}

	public void Play(StockData stock, ILocalTimer timer, FacialAnimationData facialAnimation = null)
	{
		if (_facade.AssetCreatureAsync != null)
		{
			RuntimeData runtimeData = new RuntimeData(_nextRuntimeUid++, stock, _facade.AssetCreatureAsync, facialAnimation);
			runtimeData.Weight = 0f;
			Play(runtimeData, timer);
		}
	}

	private void Play(RuntimeData runtime, ILocalTimer timer)
	{
		if (Runtime != null)
		{
			Runtime.GameAssetStock.IsMain = false;
			ClearPrevFacialAnimation();
			SubRuntimeList.Add(Runtime);
			_prevAnimationKey = Runtime.Stock.AnimationKey;
		}
		Runtime = runtime;
		Runtime.GameAssetStock.IsMain = true;
		if (Runtime.Stock.BlendTime >= 0f)
		{
			Runtime.BlendTime = runtime.Stock.BlendTime;
		}
		else
		{
			Runtime.BlendTime = _facade.AssetCreature.GetAnimationBlend(_prevAnimationKey, Runtime.Stock.AnimationKey);
		}
		_timer.SetTimer(timer);
		Runtime.Weight = CalcRuntimeWeight(Mathf.Clamp01(Runtime.Weight));
		_facade.AssetCreature.SetAnimation(Runtime.GameAssetStock);
	}

	private void FramemoveDirection()
	{
		_direction.Framemove(Runtime.BlendTime, _timer.DeltaTime);
		if (_direction.IsDirty)
		{
			float num = 0f - _direction.ViewAngle + 90f;
			Quaternion localRotation = Quaternion.AngleAxis(num, Vector3.up);
			_facade.DirectionView.localRotation = localRotation;
			float angle = ((!_facade.IsFlip) ? num : (0f - num));
			_facade.AssetCreature.SetRotation(Quaternion.AngleAxis(angle, Vector3.up), reset: false);
			_direction.IsDirty = false;
		}
	}

	private void Framemove()
	{
		Runtime.Framemove(_timer.DeltaTime, _timer.Timer, _facade);
	}

	private float CalcRuntimeWeight(float weightDelta)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(SubRuntimeList))
		{
			return 1f;
		}
		float num = weightDelta;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SubRuntimeList); i++)
		{
			float num2 = num / (float)(__Extension_Method_ICollection__.COUNT(SubRuntimeList) - i);
			RuntimeData runtimeData = SubRuntimeList[i];
			if (runtimeData.GameAssetStock.FacialAnimation != null)
			{
				runtimeData.GameAssetStock.FacialAnimation.BlendData = null;
			}
			if (runtimeData.Weight <= num2)
			{
				num -= runtimeData.Weight;
				runtimeData.Weight = 0f;
			}
			else
			{
				num -= num2;
				runtimeData.Weight -= num2;
			}
		}
		float num3 = 0f;
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(SubRuntimeList); j++)
		{
			RuntimeData runtimeData2 = SubRuntimeList[j];
			if (runtimeData2.Weight < Mathf.Epsilon)
			{
				ReleaseRuntime(runtimeData2);
				SubRuntimeList.RemoveAt(j);
				j--;
				continue;
			}
			num3 += runtimeData2.Weight;
			if (_facade != null && _facade.AssetCreature != null)
			{
				_facade.AssetCreature.SetAnimation(runtimeData2.GameAssetStock);
			}
		}
		_ = 1.01f;
		return Mathf.Clamp01(1f - num3);
	}

	private void ReleaseRuntime(RuntimeData runtime)
	{
		if (_facade != null && _facade.AssetCreature != null)
		{
			_facade.AssetCreature.SetAnimation(runtime.GameAssetStock);
		}
	}

	public void ChangeFacialAnimation(FacialAnimationData facialAnimation)
	{
		if (Runtime.GameAssetStock.FacialAnimation != null)
		{
			_facade.AssetCreature.ClearFacialAnimation(Runtime.GameAssetStock.FacialAnimation);
		}
		Runtime.GameAssetStock.FacialAnimation = facialAnimation;
	}

	private void ClearPrevFacialAnimation()
	{
		if (Runtime.GameAssetStock.PrevFacialAnimation != null)
		{
			_facade.AssetCreature.ClearFacialAnimation(Runtime.GameAssetStock.PrevFacialAnimation);
			Runtime.GameAssetStock.PrevFacialAnimation = null;
		}
	}

	public void ChangeFacialAnimation(FacialAnimationModifier facialModifier)
	{
		ClearPrevFacialAnimation();
		FacialAnimationData facialAnimationData = new FacialAnimationData();
		facialAnimationData.StaticShapeList.AddRange(facialModifier.FacialNameArray);
		if (facialModifier.BlendAction)
		{
			if (Runtime.GameAssetStock.FacialAnimation != null)
			{
				Runtime.GameAssetStock.FacialAnimation.BlendData = new FacialAnimationData.FacialBlendData
				{
					BlendTime = 0.2f,
					BlendElapsedTime = 0.2f
				};
				Runtime.GameAssetStock.PrevFacialAnimation = Runtime.GameAssetStock.FacialAnimation;
			}
			facialAnimationData.BlendData = new FacialAnimationData.FacialBlendData
			{
				BlendTime = facialModifier.BlendTime,
				BlendCurve = facialModifier.BlendCurve
			};
		}
		else if (Runtime.GameAssetStock.FacialAnimation != null)
		{
			_facade.AssetCreature.ClearFacialAnimation(Runtime.GameAssetStock.FacialAnimation);
		}
		Runtime.GameAssetStock.FacialAnimation = facialAnimationData;
	}
}
