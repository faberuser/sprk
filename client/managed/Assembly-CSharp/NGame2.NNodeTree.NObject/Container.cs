using System;
using System.Collections.Generic;
using NGame2.NNodeTree.NObject.NMotionTrail;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

namespace NGame2.NNodeTree.NObject;

public class Container : MonoBehaviour, IInspectorGuiDrawer
{
	private const int ReserveCount = 3;

	private static Container _instance;

	private long _lastObjectUid = 9000000000L;

	private IncreaseDic<long, ObjectBase> _objectDic = new IncreaseDic<long, ObjectBase>(100);

	private Dictionary<Type, Queue<ObjectBase>> _unusedDic = new Dictionary<Type, Queue<ObjectBase>>(100);

	private List<ParticleObject> _particleEndingList = new List<ParticleObject>();

	private List<TrailObject> _trailEndingList = new List<TrailObject>();

	private List<MotionTrailObject> _motionTrailEndingList = new List<MotionTrailObject>();

	public static bool IsValidInstance => _instance != null;

	public static Container instance
	{
		get
		{
			CreateInstance();
			return _instance;
		}
	}

	public static void CreateInstance()
	{
		if (!(_instance != null))
		{
			GameObject obj = new GameObject("NodeTreeObjectContainer")
			{
				layer = Game3DRoot.instance.GameCamera.MainCamera.gameObject.layer
			};
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(obj.transform, Game3DRoot.instance.ContentsGroup.transform);
			_instance = obj.AddComponent<Container>();
			_instance.OnCreateSingleton();
		}
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	internal void OnCreateSingleton()
	{
		for (int i = 0; i < 3; i++)
		{
			EnqueueUnused(CreateNew<PivotInvisible>());
			EnqueueUnused(CreateNew<TrailObject>());
			EnqueueUnused(CreateNew<MotionTrailObject>());
			EnqueueUnused(CreateNew<ParticleObject>());
			EnqueueUnused(CreateNew<ModelObject>());
		}
	}

	public void ChainUpdate()
	{
		for (int i = 0; i < _particleEndingList.Count; i++)
		{
			ParticleObject particleObject = _particleEndingList[i];
			particleObject.ChainUpdate();
			if (particleObject.IsEmptyParticle)
			{
				particleObject.FinalizeParticle();
				EnqueueUnused(particleObject);
				_particleEndingList.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < _trailEndingList.Count; j++)
		{
			TrailObject trailObject = _trailEndingList[j];
			trailObject.ChainUpdate();
			if (trailObject.IsEmpty)
			{
				trailObject.FinalizeTrail();
				EnqueueUnused(trailObject);
				_trailEndingList.RemoveAt(j);
				j--;
			}
		}
		for (int k = 0; k < _motionTrailEndingList.Count; k++)
		{
			MotionTrailObject motionTrailObject = _motionTrailEndingList[k];
			motionTrailObject.ChainUpdate();
			if (motionTrailObject.IsEmpty)
			{
				motionTrailObject.FinalizeMotionTrail();
				EnqueueUnused(motionTrailObject);
				_motionTrailEndingList.RemoveAt(k);
				k--;
			}
		}
		for (int l = 0; l < _objectDic.Count; l++)
		{
			_objectDic.GetValueByIndex(l).ChainUpdate();
		}
		SceneAutoBehaviour<HandlerVespa>.instance.ChainUpdate();
	}

	public ObjectBase GetObjectBase(long uid)
	{
		return _objectDic.GETVALUE(uid, null);
	}

	public ObjectBase CreatePivotInvisible(Transform parentTm, RuntimeInstance runtime)
	{
		PivotInvisible pivotInvisible = DequeueUnused<PivotInvisible>();
		if (pivotInvisible == null)
		{
			pivotInvisible = PivotInvisible.PrefabAuto.Factory.Create(null);
		}
		InitializeObjectBase(pivotInvisible, "Pivot", parentTm, runtime);
		return pivotInvisible;
	}

	public TrailObject CreateTrail(PrefabGoAuto prefabAuto, Transform parentTm, RuntimeInstance runtime)
	{
		if (prefabAuto.Factory == null)
		{
			return null;
		}
		TrailObject trailObject = DequeueUnused<TrailObject>();
		if (trailObject == null)
		{
			trailObject = CreateNew<TrailObject>();
		}
		InitializeObjectBase(trailObject, "Trail", parentTm, runtime);
		trailObject.Open(prefabAuto, runtime.LocalTimer);
		return trailObject;
	}

	public MotionTrailObject CreateMotionTrail(CreateData create, Transform parentTm, RuntimeInstance runtime)
	{
		MotionTrailObject motionTrailObject = DequeueUnused<MotionTrailObject>();
		if (motionTrailObject == null)
		{
			motionTrailObject = CreateNew<MotionTrailObject>();
		}
		InitializeObjectBase(motionTrailObject, "MotionTrail", parentTm, runtime);
		if (!motionTrailObject.Open(create, parentTm, runtime.LocalTimer, runtime.Layer))
		{
			EnqueueUnused(motionTrailObject);
			return null;
		}
		return motionTrailObject;
	}

	public ParticleObject CreateParticle(PrefabGoAuto prefabAuto, bool isDistanceEmit, Transform parentTm, RuntimeInstance runtime)
	{
		if (prefabAuto.Factory == null)
		{
			return null;
		}
		ParticleObject particleObject = DequeueUnused<ParticleObject>();
		if (particleObject == null)
		{
			particleObject = CreateNew<ParticleObject>();
		}
		InitializeObjectBase(particleObject, "Particle", parentTm, runtime);
		particleObject.Open(prefabAuto, isDistanceEmit, runtime.LocalTimer);
		return particleObject;
	}

	public ModelObject CreateModel(PrefabGoAuto prefabAuto, Transform parentTm, RuntimeInstance runtime)
	{
		if (prefabAuto.Factory == null)
		{
			return null;
		}
		ModelObject modelObject = DequeueUnused<ModelObject>();
		if (modelObject == null)
		{
			modelObject = CreateNew<ModelObject>();
		}
		InitializeObjectBase(modelObject, "Model", parentTm, runtime);
		modelObject.Open(prefabAuto, runtime.LocalTimer);
		return modelObject;
	}

	private void InitializeObjectBase(ObjectBase objectBase, string namePrefix, Transform parentTm, RuntimeInstance runtime)
	{
		objectBase.Uid = _lastObjectUid++;
		objectBase.LockCount = 1L;
		_objectDic.Add(objectBase.Uid, objectBase);
		if (parentTm == null)
		{
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(objectBase.transform, base.transform);
		}
		else
		{
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(objectBase.transform, parentTm);
		}
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(objectBase.gameObject, runtime.Layer);
		objectBase.OnCreate(runtime.Observer);
	}

	public ObjectBase CaptureObject(ObjectBase objectBase)
	{
		if (objectBase == null)
		{
			return null;
		}
		objectBase.LockCount++;
		return objectBase;
	}

	public bool ReleaseObjectUid(long uid)
	{
		if (uid == 0L)
		{
			return false;
		}
		ObjectBase vALUE = _objectDic.GETVALUE(uid, null);
		if (vALUE == null)
		{
			return false;
		}
		ReleaseObject(vALUE);
		return true;
	}

	public void ReleaseObject(ObjectBase objectBase)
	{
		if (objectBase == null)
		{
			return;
		}
		objectBase.LockCount--;
		if (objectBase.LockCount <= 0)
		{
			objectBase.OnRelease();
			if (!_objectDic.RemoveSlow(objectBase.Uid))
			{
				ReleaseException(objectBase);
			}
			else if (objectBase is PivotInvisible)
			{
				EnqueueUnused(objectBase);
			}
			else if (objectBase is ParticleObject)
			{
				PendingReleaseParticle(objectBase as ParticleObject);
			}
			else if (objectBase is TrailObject)
			{
				PendingReleaseTrail(objectBase as TrailObject);
			}
			else if (objectBase is MotionTrailObject)
			{
				PendingReleaseMotionTrail(objectBase as MotionTrailObject);
			}
			else if (objectBase is ModelObject)
			{
				(objectBase as ModelObject).Close();
				EnqueueUnused(objectBase);
			}
		}
	}

	private void ReleaseException(ObjectBase objectBase)
	{
		objectBase.name = $"Invalid Release: {objectBase.Uid}";
		objectBase.transform.parent = base.transform;
		objectBase.gameObject.SetActive(value: false);
	}

	private T DequeueUnused<T>() where T : ObjectBase
	{
		Queue<ObjectBase> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_unusedDic, typeof(T), null);
		if (vALUE == null)
		{
			return null;
		}
		while (vALUE.Count > 0)
		{
			ObjectBase objectBase = vALUE.Dequeue();
			T val = objectBase as T;
			if (val == null)
			{
				VespaObject.DestroyGo(objectBase);
				continue;
			}
			val.gameObject.SetActive(value: true);
			return val;
		}
		return null;
	}

	private T CreateNew<T>() where T : ObjectBase
	{
		return new GameObject().AddComponent<T>();
	}

	private void EnqueueUnused(ObjectBase objectBase)
	{
		Type type = objectBase.GetType();
		Queue<ObjectBase> queue = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_unusedDic, type, null);
		if (queue == null)
		{
			queue = new Queue<ObjectBase>();
			_unusedDic.Add(type, queue);
		}
		objectBase.transform.parent = base.transform;
		objectBase.gameObject.SetActive(value: false);
		queue.Enqueue(objectBase);
	}

	private void PendingReleaseParticle(ParticleObject particleObject)
	{
		particleObject.Play = false;
		particleObject.transform.parent = base.transform;
		_particleEndingList.Add(particleObject);
	}

	private void PendingReleaseTrail(TrailObject trailObject)
	{
		trailObject.Emit = false;
		trailObject.transform.parent = base.transform;
		_trailEndingList.Add(trailObject);
	}

	private void PendingReleaseMotionTrail(MotionTrailObject motionTrailObject)
	{
		motionTrailObject.Emit = false;
		motionTrailObject.transform.parent = base.transform;
		_motionTrailEndingList.Add(motionTrailObject);
	}
}
