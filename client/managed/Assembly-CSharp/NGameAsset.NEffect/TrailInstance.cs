using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NEffect;

public class TrailInstance
{
	public enum EPlayState
	{
		Ready,
		Play,
		Stop,
		End
	}

	private IAssetFactory<GameObject> _assetFactory;

	private GameObject _trailGo;

	private NVespa.TrailProxy _proxy = new NVespa.TrailProxy();

	private EPlayState _playState;

	private bool _effectModelAttached;

	private Queue<TrailSequenceData> SequenceQueue;

	public ITrailData Data { get; private set; }

	public EffectInstance Owner { get; private set; }

	public GameObject InstanceGo { get; private set; }

	public float ElapsedTime { get; private set; }

	public bool IsEnd => _playState == EPlayState.End;

	public TrailInstance(EffectInstance owner)
	{
		Owner = owner;
	}

	public bool Open(ITrailData data, IAssetFactory<GameObject> assetFactory, int layer, ITransformData extraTransform)
	{
		Data = data;
		_assetFactory = assetFactory;
		_trailGo = _assetFactory.Create();
		if (_trailGo == null)
		{
			return false;
		}
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(_trailGo, layer);
		if (!_proxy.Open(_trailGo))
		{
			_proxy.Close();
			VespaObject.DestroyGo(_trailGo);
			_trailGo = null;
			return false;
		}
		CommonData commonData = Data.GetCommonData();
		Transform transformForEffectModelAttach = commonData.GetTransformForEffectModelAttach(Owner);
		if (transformForEffectModelAttach != null)
		{
			_effectModelAttached = true;
		}
		InstanceGo = SceneAutoBehaviour<GameObjectFactory>.instance.Create();
		InstanceGo.layer = _trailGo.layer;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(InstanceGo.transform, transformForEffectModelAttach);
		commonData.TransformSet.AssignTransform(InstanceGo.transform, _effectModelAttached ? null : extraTransform);
		Vespa_Unity_Transform_Extension_Method.ParentRelative(_trailGo.transform, InstanceGo.transform);
		ElapsedTime = 0f;
		_playState = EPlayState.Ready;
		UpdateVisible();
		if (data.GetSequenceCount() > 0)
		{
			SequenceQueue = new Queue<TrailSequenceData>();
			for (int i = 0; i < data.GetSequenceCount(); i++)
			{
				TrailSequenceData sequenceData = data.GetSequenceData(i);
				if (sequenceData != null)
				{
					SequenceQueue.Enqueue(sequenceData);
				}
			}
		}
		return true;
	}

	public void Close(bool detachParent)
	{
		_proxy.Stop();
		_playState = EPlayState.End;
		if (detachParent && InstanceGo != null)
		{
			InstanceGo.transform.parent = AssetUpdateManager.DummyRootTm;
		}
	}

	public void PlayPhase(string phaseName)
	{
		if (_playState == EPlayState.End)
		{
			return;
		}
		foreach (TrailSequenceData item in SequenceQueue)
		{
			if (item.PhaseName == phaseName)
			{
				ElapsedTime = item.SequenceTime;
				break;
			}
		}
	}

	public bool IsEmpty()
	{
		if (_proxy == null)
		{
			return true;
		}
		return _proxy.IsEmpty();
	}

	public void Release()
	{
		if (_proxy != null)
		{
			_proxy.RenderEnabled = true;
			_proxy.Clear();
			_proxy.Close();
			_proxy = null;
		}
		if (_trailGo != null)
		{
			_assetFactory.Release(_trailGo);
			_trailGo = null;
		}
		if (InstanceGo != null)
		{
			GameObjectFactory.ReleaseGameObject(InstanceGo);
			InstanceGo = null;
		}
	}

	public void UpdateVisible()
	{
		if (_proxy != null)
		{
			_proxy.RenderEnabled = Owner.IsVisible;
		}
	}

	public void Visit(IVisitor visitor)
	{
		_proxy.Visit(visitor);
	}

	public void SetParent(ICreature parent)
	{
		if (!(InstanceGo == null) && !_effectModelAttached)
		{
			Transform attachTransform = Data.GetCommonData().GetAttachTransform(parent);
			if (!(attachTransform == null))
			{
				Vespa_Unity_Transform_Extension_Method.ParentRelative(InstanceGo.transform, attachTransform);
			}
		}
	}

	public void SetParent(Transform parentTm)
	{
		if (!(InstanceGo == null) && !_effectModelAttached)
		{
			Transform attachTransform = Data.GetCommonData().GetAttachTransform(parentTm);
			if (!(attachTransform == null))
			{
				Vespa_Unity_Transform_Extension_Method.ParentRelative(InstanceGo.transform, attachTransform);
			}
		}
	}

	public void Update(ILocalTimer timer, float deltaTime)
	{
		ElapsedTime += deltaTime;
		if (!(ElapsedTime < Data.GetCommonData().BeginTime) && _playState != EPlayState.End)
		{
			if (ElapsedTime > Data.GetCommonData().EndTime)
			{
				_proxy.Stop();
				_playState = EPlayState.End;
			}
			else
			{
				UpdateSequence();
				_proxy.ChainUpdate(timer, deltaTime);
			}
		}
	}

	private void UpdateSequence()
	{
		if (SequenceQueue == null || SequenceQueue.Count == 0)
		{
			if (_playState == EPlayState.Ready)
			{
				_playState = EPlayState.Play;
				_proxy.Play();
			}
			return;
		}
		if (_playState == EPlayState.Ready)
		{
			_playState = EPlayState.Stop;
		}
		TrailSequenceData trailSequenceData = null;
		while (SequenceQueue.Count > 0)
		{
			TrailSequenceData trailSequenceData2 = SequenceQueue.Peek();
			if (trailSequenceData2.SequenceTime > ElapsedTime)
			{
				break;
			}
			trailSequenceData = trailSequenceData2;
			SequenceQueue.Dequeue();
		}
		if (trailSequenceData != null)
		{
			if (_playState == EPlayState.Stop && trailSequenceData.IsPlay)
			{
				_playState = EPlayState.Play;
				_proxy.Play();
			}
			else if (_playState == EPlayState.Play && !trailSequenceData.IsPlay)
			{
				_playState = EPlayState.Stop;
				_proxy.Stop();
			}
		}
	}
}
