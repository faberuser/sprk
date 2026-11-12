using System.Collections.Generic;
using NGame2.NUtil;
using NGame2.NUtil.NSoundFx;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class SubCreatureContoller
{
	public class RuntimeData
	{
		public float BlendTime;

		public ICreatureLoad CreatureAsync;

		public AnimationData.StockData Stock;

		public AnimationStock GameAssetStock;

		public float BlendElapsedTime;

		private IAnimationData _animationData;

		private bool _isEnd;

		private int _soundFxPlayCount;

		private int _soundFxPlayCount2;

		private List<FrameSoundData> _soundFxList2;

		public float TimeLength
		{
			get
			{
				if (Stock.TimeType == NGame2.NRender.NRenderFacade.AnimationData.StockData.ETimeType.Length)
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
				if (Stock.TimeType == NGame2.NRender.NRenderFacade.AnimationData.StockData.ETimeType.Multiply)
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
				if (Stock.State == NGame2.NRender.NRenderFacade.AnimationData.EState.Pause)
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

		public IAnimationData AnimationData
		{
			get
			{
				if (_animationData != null)
				{
					return _animationData;
				}
				if (CreatureAsync == null)
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

		internal RuntimeData(int uid, AnimationData.StockData stock, ICreatureLoad creatureAsync)
		{
			Uid = uid;
			Stock = stock;
			CreatureAsync = creatureAsync;
			GameAssetStock = default(AnimationStock);
			GameAssetStock.Key = stock.AnimationKey;
			if (Stock.IsPlaySound)
			{
				_soundFxPlayCount2 = 0;
				_soundFxList2 = SingletonAuto<SoundFxManager>.instance.GetAnimationSoundFx(CreatureAsync.Stock.Code, stock.AnimationKey);
			}
		}

		internal void Framemove(float deltaTime, ILocalTimer localTimer)
		{
			if (_isEnd)
			{
				return;
			}
			float frame = Frame;
			Frame += deltaTime / Vespa_Float_Extension_Method.NoneZero(TimeLength);
			Mathf.Approximately(frame, Frame);
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
			if (soundFxList != null)
			{
				for (int i = _soundFxPlayCount; i < soundFxList.Count && !(soundFxList[i].Frame > Frame); i++)
				{
					SoundStock stock = new SoundStock
					{
						Code = soundFxList[i].SoundCode,
						LocalTimer = facade.LocalTimer
					};
					SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 99f);
					_soundFxPlayCount++;
				}
			}
		}

		private void UpdateSoundFx2(ILocalTimer localTimer, RenderFacade facade)
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(_soundFxList2))
			{
				for (int i = _soundFxPlayCount2; i < _soundFxList2.Count && !(_soundFxList2[i].Frame > Frame); i++)
				{
					SoundStock stock = new SoundStock
					{
						Code = _soundFxList2[i].SoundCode,
						LocalTimer = facade.LocalTimer
					};
					SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 99f);
					_soundFxPlayCount2++;
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

	public const float DirectionBlendSpeed = 20f;

	public List<RuntimeData> SubRuntimeList = new List<RuntimeData>();

	public ICreatureLoad AssetSubCreatureAsync;

	private AnimationTimer _timer = new AnimationTimer();

	private AnimationKeyData _prevAnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.BattleIdle);

	private int _nextRuntimeUid = 1;

	public AnimationData.EState CurrentState
	{
		get
		{
			if (Runtime == null)
			{
				return AnimationData.EState.None;
			}
			return Runtime.Stock.State;
		}
	}

	public RuntimeData Runtime { get; private set; }

	public ICreature AssetSubCreature
	{
		get
		{
			if (AssetSubCreatureAsync == null)
			{
				return null;
			}
			return AssetSubCreatureAsync.Value;
		}
	}

	public SubCreatureContoller(ICreatureLoad subCreautureLoad)
	{
		AssetSubCreatureAsync = subCreautureLoad;
	}

	public void ChainUpdate()
	{
		if (Runtime != null && !_timer.PauseCurrent)
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
			Runtime.Weight = CalcRuntimeWeight(weightDelta);
			if (AssetSubCreature != null)
			{
				AssetSubCreature.SetAnimation(Runtime.GameAssetStock);
			}
		}
	}

	public void Pause()
	{
		_timer.PauseCurrent = true;
		if (Runtime != null)
		{
			Runtime.Stock.State = AnimationData.EState.Pause;
		}
	}

	public void Resume(AnimationData.EState newState)
	{
		_timer.PauseCurrent = false;
		if (Runtime != null)
		{
			Runtime.Stock.State = newState;
		}
	}

	public void Play(AnimationData.StockData stock, float frame, ILocalTimer timer)
	{
		RuntimeData runtimeData = new RuntimeData(_nextRuntimeUid++, stock, AssetSubCreatureAsync);
		runtimeData.Frame = frame;
		runtimeData.Weight = 1f;
		Play(runtimeData, timer);
	}

	public void Play(AnimationData.StockData stock, bool blendMotion, ILocalTimer timer)
	{
		RuntimeData runtimeData = new RuntimeData(_nextRuntimeUid++, stock, AssetSubCreatureAsync);
		runtimeData.Weight = (blendMotion ? 0f : 1f);
		Play(runtimeData, timer);
	}

	public void Play(AnimationData.StockData stock, ILocalTimer timer)
	{
		RuntimeData runtimeData = new RuntimeData(_nextRuntimeUid++, stock, AssetSubCreatureAsync);
		runtimeData.Weight = 0f;
		Play(runtimeData, timer);
	}

	private void Play(RuntimeData runtime, ILocalTimer timer)
	{
		if (Runtime != null)
		{
			Runtime.GameAssetStock.IsMain = false;
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
			Runtime.BlendTime = AssetSubCreature.GetAnimationBlend(_prevAnimationKey, Runtime.Stock.AnimationKey);
		}
		_timer.SetTimer(timer);
		Runtime.Weight = CalcRuntimeWeight(Mathf.Clamp01(Runtime.Weight));
		AssetSubCreature.SetAnimation(Runtime.GameAssetStock);
	}

	private void Framemove()
	{
		Runtime.Framemove(_timer.DeltaTime, _timer.Timer);
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
			}
			else
			{
				num3 += runtimeData2.Weight;
				AssetSubCreature.SetAnimation(runtimeData2.GameAssetStock);
			}
		}
		_ = 1.01f;
		return Mathf.Clamp01(1f - num3);
	}

	private void ReleaseRuntime(RuntimeData runtime)
	{
		AssetSubCreature.SetAnimation(runtime.GameAssetStock);
	}
}
