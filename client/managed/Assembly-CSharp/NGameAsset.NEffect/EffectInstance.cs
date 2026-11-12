using NGameAsset.NCommon;
using NGameAsset.NSound;
using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class EffectInstance : IEffect, ExtraRenderFxController.IOwner
{
	private PoolingList<ModelInstance> _modelList;

	private PoolingList<ParticleInstance> _particleList;

	private PoolingList<TrailInstance> _trailList;

	private PoolingList<SoundFxInstance> _soundFxList;

	private ExtraRenderFxController _extraRenderFxController;

	private RenderGroupModifier2 _renderGroupModifier;

	private GameObject _dummyGo;

	public BaseData Base { get; private set; }

	public bool IsVisible { get; private set; }

	public int Layer { get; private set; }

	public ILocalTimer LocalTimer { get; private set; }

	private float _deltaTime
	{
		get
		{
			if (LocalTimer != null)
			{
				return (float)LocalTimer.deltaTick * 0.001f;
			}
			return MainTimer.deltaTime;
		}
	}

	private float _playbackSpeed
	{
		get
		{
			if (LocalTimer != null)
			{
				return (float)LocalTimer.playbackSpeed * 0.001f;
			}
			return 1f;
		}
	}

	int ExtraRenderFxController.IOwner.Layer => Layer;

	RenderGroupModifier2 ExtraRenderFxController.IOwner.RenderGroupModifier => _renderGroupModifier;

	public bool Open(IEffectOpenData data, int layer, ITransformData extraTransform)
	{
		_renderGroupModifier = SmallPooling<RenderGroupModifier2>.CreateInstance();
		_renderGroupModifier.Open(useSoloTimeColor: true);
		_extraRenderFxController = new ExtraRenderFxController(this);
		Layer = layer;
		Base = data.GetBaseData();
		IsVisible = true;
		for (int i = 0; i < data.GetModelCount(); i++)
		{
			IModelData modelData = data.GetModelData(i);
			IAssetFactory<GameObject> modelFactory = data.GetModelFactory(i);
			if (modelData != null && modelFactory != null)
			{
				ModelInstance modelInstance = new ModelInstance(this);
				if (modelInstance.Open(modelData, modelFactory, layer, extraTransform))
				{
					_modelList.Add(modelInstance);
				}
			}
		}
		for (int j = 0; j < data.GetParticleCount(); j++)
		{
			IParticleData particleData = data.GetParticleData(j);
			IAssetFactory<GameObject> particleFactory = data.GetParticleFactory(j);
			if (particleData != null && particleFactory != null)
			{
				ParticleInstance particleInstance = new ParticleInstance(this);
				if (particleInstance.Open(particleData, particleFactory, layer, extraTransform))
				{
					_particleList.Add(particleInstance);
				}
			}
		}
		for (int k = 0; k < data.GetTrailCount(); k++)
		{
			ITrailData trailData = data.GetTrailData(k);
			IAssetFactory<GameObject> trailFactory = data.GetTrailFactory(k);
			if (trailData != null && trailFactory != null)
			{
				TrailInstance trailInstance = new TrailInstance(this);
				if (trailInstance.Open(trailData, trailFactory, layer, extraTransform))
				{
					_trailList.Add(trailInstance);
				}
			}
		}
		for (int l = 0; l < data.GetSoundFxCount(); l++)
		{
			SoundFxData soundFxData = data.GetSoundFxData(l);
			if (soundFxData != null)
			{
				SoundFxInstance soundFxInstance = new SoundFxInstance(this);
				if (soundFxInstance.Open(soundFxData))
				{
					_soundFxList.Add(soundFxInstance);
				}
			}
		}
		return true;
	}

	public int GetModelInstanceCount()
	{
		return _modelList.Count;
	}

	public ModelInstance GetModelInstance(int index)
	{
		return _modelList.GetValid(index);
	}

	public void Close(bool detachParent)
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			_modelList[i].Close(detachParent);
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			_particleList[j].Close(detachParent);
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			_trailList[k].Close(detachParent);
		}
		for (int l = 0; l < _soundFxList.COUNT(); l++)
		{
			_soundFxList[l].Close();
		}
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			if (!_modelList[i].IsEmpty())
			{
				return false;
			}
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			if (!_particleList[j].IsEmpty())
			{
				return false;
			}
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			if (!_trailList[k].IsEmpty())
			{
				return false;
			}
		}
		for (int l = 0; l < _soundFxList.COUNT(); l++)
		{
			if (!_soundFxList[l].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public void Release()
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			_modelList[i].Release();
		}
		_modelList.Clear();
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			_particleList[j].Release();
		}
		_particleList.Clear();
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			_trailList[k].Release();
		}
		_trailList.Clear();
		for (int l = 0; l < _soundFxList.COUNT(); l++)
		{
			_soundFxList[l].Release();
		}
		_soundFxList.Clear();
		if (_dummyGo != null)
		{
			VespaObject.DestroyGo(_dummyGo);
			_dummyGo = null;
		}
		_extraRenderFxController = null;
		if (_renderGroupModifier != null)
		{
			SmallPooling<RenderGroupModifier2>.ReleaseInstance(_renderGroupModifier);
			_renderGroupModifier = null;
		}
	}

	RenderGroupModifier2 IEffect.GetRenderGroupModifier()
	{
		return _renderGroupModifier;
	}

	void IEffect.PlayPhase(string phaseName)
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			_modelList[i].PlayPhase(phaseName);
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			_particleList[j].PlayPhase(phaseName);
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			_trailList[k].PlayPhase(phaseName);
		}
	}

	void IEffect.SetVisible(bool isVisible)
	{
		IsVisible = isVisible;
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			_modelList[i].UpdateVisible();
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			_particleList[j].UpdateVisible();
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			_trailList[k].UpdateVisible();
		}
	}

	void IEffect.Visit(IVisitor visitor)
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			_modelList[i].Visit(visitor);
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			_particleList[j].Visit(visitor);
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			_trailList[k].Visit(visitor);
		}
	}

	private void UpdateInternal(float deltaTime)
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			_modelList[i].Update(deltaTime);
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			_particleList[j].Update(deltaTime, _playbackSpeed);
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			_trailList[k].Update(LocalTimer, deltaTime);
		}
		for (int l = 0; l < _soundFxList.COUNT(); l++)
		{
			_soundFxList[l].Update(deltaTime);
		}
		_extraRenderFxController.ChainUpdate();
		_renderGroupModifier.ChainUpdate(LocalTimer);
		if (_renderGroupModifier.BeginFlush(forcePropertyBlockDirty: true))
		{
			Vespa_Common_Class_Extension_Method.Interface((IEffect)this).Visit(_renderGroupModifier);
			_renderGroupModifier.EndFlush();
		}
	}

	void IEffect.SetDummyParent(Vector3 position, Quaternion rotation, Vector3 scale, string dummyName = "")
	{
		GameObject gameObject = new GameObject(dummyName);
		gameObject.transform.position = position;
		gameObject.transform.rotation = rotation;
		gameObject.transform.localScale = scale;
		Vespa_Common_Class_Extension_Method.Interface((IEffect)this).SetParent(gameObject.transform);
		if (_dummyGo != null)
		{
			VespaObject.DestroyGo(_dummyGo);
		}
		_dummyGo = gameObject;
	}

	void IEffect.SetParent(ICreature parent)
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			_modelList[i].SetParent(parent);
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			_particleList[j].SetParent(parent);
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			_trailList[k].SetParent(parent);
		}
	}

	void IEffect.SetParent(Transform parentTm)
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			_modelList[i].SetParent(parentTm);
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			_particleList[j].SetParent(parentTm);
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			_trailList[k].SetParent(parentTm);
		}
	}

	void IEffect.SetShaderTransform(ShaderTransformData shaderTransform, Transform pivotTm)
	{
		_extraRenderFxController.Open(shaderTransform, pivotTm);
	}

	void IEffect.SetGrayscale(bool isEnable)
	{
		_extraRenderFxController.SetGrayscale(isEnable);
	}

	void IEffect.SetTimer(ILocalTimer localTimer)
	{
		LocalTimer = localTimer;
	}

	void IEffect.Update()
	{
		UpdateInternal(_deltaTime);
	}

	void IEffect.Update(float deltaTime)
	{
		UpdateInternal(deltaTime);
	}

	bool IEffect.IsEnd()
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			if (!_modelList[i].IsEnd)
			{
				return false;
			}
		}
		for (int j = 0; j < _particleList.COUNT(); j++)
		{
			if (!_particleList[j].IsEnd)
			{
				return false;
			}
		}
		for (int k = 0; k < _trailList.COUNT(); k++)
		{
			if (!_trailList[k].IsEnd)
			{
				return false;
			}
		}
		for (int l = 0; l < _soundFxList.COUNT(); l++)
		{
			if (!_soundFxList[l].IsEnd)
			{
				return false;
			}
		}
		return true;
	}

	Transform IEffect.FindTransform(string name)
	{
		for (int i = 0; i < _modelList.COUNT(); i++)
		{
			ModelInstance modelInstance = _modelList[i];
			if (!(modelInstance.ModelGo == null))
			{
				Transform transform = Vespa_Unity_Transform_Extension_Method.FindChildRecursively(modelInstance.ModelGo.transform, name);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return null;
	}
}
