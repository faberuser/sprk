using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NEffect;

public class ParticleInstance
{
	public enum EPlayState
	{
		Ready,
		Play,
		Stop,
		End
	}

	private IAssetFactory<GameObject> _assetFactory;

	private GameObject _particleGo;

	private GlobalBillboardManager.EntryData _billboardEntry;

	private ParticleProxy _proxy = new ParticleProxy();

	private EPlayState _playState;

	private int _skipFrame;

	private bool _effectModelAttached;

	private Queue<ParticleSequenceData> SequenceQueue;

	public IParticleData Data { get; private set; }

	public EffectInstance Owner { get; private set; }

	public GameObject InstanceGo { get; private set; }

	public float ElapsedTime { get; private set; }

	public bool IsEnd => _playState == EPlayState.End;

	public ParticleInstance(EffectInstance owner)
	{
		Owner = owner;
	}

	public bool Open(IParticleData data, IAssetFactory<GameObject> assetFactory, int layer, ITransformData extraTransform)
	{
		Data = data;
		_assetFactory = assetFactory;
		_particleGo = _assetFactory.Create();
		if (_particleGo == null)
		{
			return SprkGraphicsOptions.ObserveAssetParticles(opened: false, this);
		}
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(_particleGo, layer);
		if (!_proxy.Open(_particleGo, data.GetMaterialFactory()))
		{
			_proxy.Close();
			VespaObject.DestroyGo(_particleGo);
			_particleGo = null;
			return SprkGraphicsOptions.ObserveAssetParticles(opened: false, this);
		}
		CommonData commonData = Data.GetCommonData();
		Transform transformForEffectModelAttach = commonData.GetTransformForEffectModelAttach(Owner);
		if (transformForEffectModelAttach != null)
		{
			_effectModelAttached = true;
		}
		InstanceGo = SceneAutoBehaviour<GameObjectFactory>.instance.Create();
		InstanceGo.layer = _particleGo.layer;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(InstanceGo.transform, transformForEffectModelAttach);
		commonData.TransformSet.AssignTransform(InstanceGo.transform, _effectModelAttached ? null : extraTransform);
		if (commonData.BillboardType != CommonData.EBillboardType.None)
		{
			GlobalBillboardManager.EBillboardType globalBillboardType = commonData.GetGlobalBillboardType();
			_billboardEntry = SingletonUpdater<GlobalBillboardManager>.instance.AddBiilboard(globalBillboardType, InstanceGo.transform);
		}
		Vespa_Unity_Transform_Extension_Method.ParentRelative(_particleGo.transform, InstanceGo.transform);
		ElapsedTime = 0f;
		_playState = EPlayState.Ready;
		_skipFrame = 1;
		UpdateVisible();
		if (data.GetSequenceCount() > 0)
		{
			SequenceQueue = new Queue<ParticleSequenceData>();
			for (int i = 0; i < data.GetSequenceCount(); i++)
			{
				ParticleSequenceData sequenceData = data.GetSequenceData(i);
				if (sequenceData != null)
				{
					SequenceQueue.Enqueue(sequenceData);
				}
			}
		}
		return SprkGraphicsOptions.ObserveAssetParticles(opened: true, this);
	}

	public void Close(bool detachParent)
	{
		SprkGraphicsOptions.ReleaseAssetParticles(this);
		_proxy.Stop();
		_playState = EPlayState.End;
		if (detachParent && InstanceGo != null)
		{
			InstanceGo.transform.parent = AssetUpdateManager.DummyRootTm;
		}
	}

	public void PlayPhase(string phaseName)
	{
		if (SequenceQueue == null || _playState == EPlayState.End)
		{
			return;
		}
		foreach (ParticleSequenceData item in SequenceQueue)
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
		return _proxy.IsEmpty();
	}

	public void Release()
	{
		SprkGraphicsOptions.ReleaseAssetParticles(this);
		if (_proxy != null)
		{
			_proxy.RenderEnabled = true;
			_proxy.Clear();
			_proxy.Close();
			_proxy = null;
		}
		if (_billboardEntry != null)
		{
			_billboardEntry.Close();
			_billboardEntry = null;
		}
		if (_particleGo != null)
		{
			_assetFactory.Release(_particleGo);
			_particleGo = null;
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

	public void Update(float deltaTime, float playbackSpeed)
	{
		ElapsedTime += deltaTime;
		_proxy.AdjustParentTransform();
		if (_skipFrame > 0)
		{
			_skipFrame--;
			return;
		}
		_proxy.PlaybackSpeed = playbackSpeed;
		if (ElapsedTime < Data.GetCommonData().BeginTime || _playState == EPlayState.End)
		{
			return;
		}
		if (ElapsedTime > Data.GetCommonData().EndTime)
		{
			_proxy.Stop();
			_playState = EPlayState.End;
			return;
		}
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
		ParticleSequenceData particleSequenceData = null;
		while (SequenceQueue.Count > 0)
		{
			ParticleSequenceData particleSequenceData2 = SequenceQueue.Peek();
			if (particleSequenceData2.SequenceTime > ElapsedTime)
			{
				break;
			}
			particleSequenceData = particleSequenceData2;
			SequenceQueue.Dequeue();
		}
		if (particleSequenceData != null)
		{
			if (_playState == EPlayState.Stop && particleSequenceData.IsPlay)
			{
				_playState = EPlayState.Play;
				_proxy.Play();
			}
			else if (_playState == EPlayState.Play && !particleSequenceData.IsPlay)
			{
				_playState = EPlayState.Stop;
				_proxy.Stop();
			}
		}
	}
}
