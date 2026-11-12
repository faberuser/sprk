using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa;

public class ShakeMaker
{
	public class ShakeHolder : SmallPooling<ShakeHolder>, ISmallPoolingItem
	{
		public Vector3 Value;

		public bool IsDone;

		void ISmallPoolingItem.OnCreateNew()
		{
		}

		void ISmallPoolingItem.OnDestroy()
		{
		}

		void ISmallPoolingItem.OnCreate()
		{
			Value = Vector3.zero;
			IsDone = false;
		}

		void ISmallPoolingItem.OnRelease()
		{
		}
	}

	public interface IShakeSimple
	{
		bool IsDone { get; }

		void Open(Vector3 force, float repeatTime, float lifeTime, AnimationCurve powerByLifeTime, ILocalTimer localTimer, ShakeHolder holder);

		void Update();

		void ReleaseSelf();
	}

	public class ShakeSimpleDefault : SmallPooling<ShakeSimpleDefault>, ISmallPoolingItem, IShakeSimple
	{
		private Vector3 _force;

		private float _repeatTime;

		private float _lifeTime;

		private AnimationCurve _powerByLifeTime;

		private ILocalTimer _localTimer;

		private float _elapsedTime;

		private ShakeHolder _holder;

		bool IShakeSimple.IsDone => _lifeTime <= _elapsedTime;

		void ISmallPoolingItem.OnCreateNew()
		{
		}

		void ISmallPoolingItem.OnDestroy()
		{
		}

		void ISmallPoolingItem.OnCreate()
		{
		}

		void ISmallPoolingItem.OnRelease()
		{
			_powerByLifeTime = null;
			_localTimer = null;
			if (_holder != null)
			{
				_holder.IsDone = true;
				SmallPooling<ShakeHolder>.ReleaseInstance(_holder);
				_holder = null;
			}
		}

		void IShakeSimple.Open(Vector3 force, float repeatTime, float lifeTime, AnimationCurve powerByLifeTime, ILocalTimer localTimer, ShakeHolder holder)
		{
			_force = force;
			_repeatTime = repeatTime;
			_lifeTime = lifeTime;
			_powerByLifeTime = powerByLifeTime;
			_localTimer = localTimer;
			_elapsedTime = 0f;
			_holder = holder;
		}

		void IShakeSimple.Update()
		{
			_elapsedTime += (float)_localTimer.deltaTick * 0.001f;
			if (!(_elapsedTime >= _lifeTime))
			{
				AnimationCurve i_this = ((_powerByLifeTime != null) ? _powerByLifeTime : GameSharedSetting.data.Value.ShakeMaker_PowerByLifeTime);
				float num = _elapsedTime / _lifeTime;
				float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(i_this, num, 1f - num);
				float num3 = Mathf.Repeat(_elapsedTime, _repeatTime) / _repeatTime;
				Vector3 value = _force * Mathf.Sin(num3 * MathF.PI * 2f) * num2;
				if (_holder != null)
				{
					_holder.Value = value;
				}
			}
		}

		void IShakeSimple.ReleaseSelf()
		{
			SmallPooling<ShakeSimpleDefault>.ReleaseInstance(this);
		}
	}

	public class ShakeSimpleRandom : SmallPooling<ShakeSimpleRandom>, ISmallPoolingItem, IShakeSimple
	{
		private static readonly float[][] RandomMultiply = new float[6][]
		{
			new float[3] { 0.5f, 1f, 1.5f },
			new float[3] { 0.5f, 1.5f, 1f },
			new float[3] { 1f, 0.5f, 1.5f },
			new float[3] { 1f, 1.5f, 0.5f },
			new float[3] { 1.5f, 0.5f, 1f },
			new float[3] { 1.5f, 1f, 0.5f }
		};

		private Vector3 _force;

		private Vector3 _repeatTime;

		private float _lifeTime;

		private AnimationCurve _powerByLifeTime;

		private ILocalTimer _localTimer;

		private float _elapsedTime;

		private ShakeHolder _holder;

		bool IShakeSimple.IsDone => _lifeTime <= _elapsedTime;

		void ISmallPoolingItem.OnCreateNew()
		{
		}

		void ISmallPoolingItem.OnDestroy()
		{
		}

		void ISmallPoolingItem.OnCreate()
		{
		}

		void ISmallPoolingItem.OnRelease()
		{
			_powerByLifeTime = null;
			_localTimer = null;
			if (_holder != null)
			{
				_holder.IsDone = true;
				SmallPooling<ShakeHolder>.ReleaseInstance(_holder);
				_holder = null;
			}
		}

		void IShakeSimple.Open(Vector3 force, float repeatTime, float lifeTime, AnimationCurve powerByLifeTime, ILocalTimer localTimer, ShakeHolder holder)
		{
			float[] array = RandomMultiply[UnityEngine.Random.Range(0, RandomMultiply.Length)];
			_force.x = ((UnityEngine.Random.value < 0.5f) ? force.x : (0f - force.x));
			_force.y = ((UnityEngine.Random.value < 0.5f) ? force.y : (0f - force.y));
			_force.z = ((UnityEngine.Random.value < 0.5f) ? force.z : (0f - force.z));
			_repeatTime.x = repeatTime * array[0];
			_repeatTime.y = repeatTime * array[1];
			_repeatTime.z = repeatTime * array[2];
			_lifeTime = lifeTime;
			_powerByLifeTime = powerByLifeTime;
			_localTimer = localTimer;
			_elapsedTime = 0f;
			_holder = holder;
		}

		void IShakeSimple.Update()
		{
			_elapsedTime += (float)_localTimer.deltaTick * 0.001f;
			if (!(_elapsedTime >= _lifeTime))
			{
				AnimationCurve i_this = ((_powerByLifeTime != null) ? _powerByLifeTime : GameSharedSetting.data.Value.ShakeMaker_PowerByLifeTime);
				float num = _elapsedTime / _lifeTime;
				float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(i_this, num, 1f - num);
				Vector3 vector = default(Vector3);
				vector.x = Mathf.Repeat(_elapsedTime, _repeatTime.x) / _repeatTime.x;
				vector.y = Mathf.Repeat(_elapsedTime, _repeatTime.y) / _repeatTime.y;
				vector.z = Mathf.Repeat(_elapsedTime, _repeatTime.z) / _repeatTime.z;
				Vector3 value = default(Vector3);
				value.x = _force.x * Mathf.Sin(vector.x * MathF.PI * 2f) * num2;
				value.y = _force.y * Mathf.Sin(vector.y * MathF.PI * 2f) * num2;
				value.z = _force.z * Mathf.Sin(vector.z * MathF.PI * 2f) * num2;
				if (_holder != null)
				{
					_holder.Value = value;
				}
			}
		}

		void IShakeSimple.ReleaseSelf()
		{
			SmallPooling<ShakeSimpleRandom>.ReleaseInstance(this);
		}
	}

	private List<ShakeHolder> _holderList = new List<ShakeHolder>();

	private IncreaseDic<long, IShakeSimple> _uniqueSimpleDic = new IncreaseDic<long, IShakeSimple>();

	private List<IShakeSimple> _simpleList = new List<IShakeSimple>();

	public void Clear()
	{
		for (int i = 0; i < _uniqueSimpleDic.Count; i++)
		{
			_uniqueSimpleDic.GetValueByIndex(i).ReleaseSelf();
		}
		_uniqueSimpleDic.Clear();
		for (int j = 0; j < _simpleList.Count; j++)
		{
			_simpleList[j].ReleaseSelf();
		}
		_simpleList.Clear();
		for (int k = 0; k < _holderList.Count; k++)
		{
			SmallPooling<ShakeHolder>.ReleaseInstance(_holderList[k]);
		}
		_holderList.Clear();
	}

	public void ApplyShake(ref Vector3 shakeTarget, float shakeWeight)
	{
		for (int i = 0; i < _uniqueSimpleDic.Count; i++)
		{
			IShakeSimple valueByIndex = _uniqueSimpleDic.GetValueByIndex(i);
			valueByIndex.Update();
			if (valueByIndex.IsDone)
			{
				_uniqueSimpleDic.RemoveByIndex(i);
				i--;
				valueByIndex.ReleaseSelf();
				valueByIndex = null;
			}
		}
		for (int j = 0; j < _simpleList.Count; j++)
		{
			IShakeSimple shakeSimple = _simpleList[j];
			shakeSimple.Update();
			if (shakeSimple.IsDone)
			{
				_simpleList.RemoveAt(j);
				j--;
				shakeSimple.ReleaseSelf();
				shakeSimple = null;
			}
		}
		float num = 0f;
		Vector3 vector = Vector3.zero;
		for (int k = 0; k < _holderList.Count; k++)
		{
			ShakeHolder shakeHolder = _holderList[k];
			if (shakeHolder.IsDone)
			{
				SmallPooling<ShakeHolder>.ReleaseInstance(shakeHolder);
				_holderList.RemoveAt(k);
				k--;
				continue;
			}
			float magnitude = shakeHolder.Value.magnitude;
			if (num < magnitude)
			{
				num = magnitude;
			}
			vector += shakeHolder.Value;
		}
		float magnitude2 = vector.magnitude;
		if (num < magnitude2 && num > Mathf.Epsilon)
		{
			float num2 = 2f - num / magnitude2;
			vector = vector.normalized * num * num2;
		}
		shakeTarget += vector * shakeWeight;
	}

	public void AddForce(long uniqueNo, Vector3 force, bool isRandomForce, float repeatTime, float lifeTime, AnimationCurve powerByLifeTime, ILocalTimer localTimer)
	{
		if (uniqueNo == 0L || !_uniqueSimpleDic.ContainsKey(uniqueNo))
		{
			IShakeSimple shakeSimple = (isRandomForce ? ((IShakeSimple)SmallPooling<ShakeSimpleRandom>.CreateInstance()) : ((IShakeSimple)SmallPooling<ShakeSimpleDefault>.CreateInstance()));
			shakeSimple.Open(force, Mathf.Max(repeatTime, 0.0001f), Mathf.Max(lifeTime, 0.0001f), powerByLifeTime, localTimer, Vespa_SmallPooling_ExtensionMethod.CaptureSmallInstance(CreateHolder()));
			if (uniqueNo != 0L)
			{
				_uniqueSimpleDic.Add(uniqueNo, shakeSimple);
			}
			else
			{
				_simpleList.Add(shakeSimple);
			}
		}
	}

	public ShakeHolder CreateHolder()
	{
		ShakeHolder shakeHolder = SmallPooling<ShakeHolder>.CreateInstance();
		_holderList.Add(shakeHolder);
		return shakeHolder;
	}
}
