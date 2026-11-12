using NGameAsset.NUtil;
using NVespa.NRenderProxy;
using UnityEngine;

namespace NVespa;

public class ParticleProxy
{
	private PoolingList<RenderProxy> _renderProxyList;

	private bool _renderEnabled = true;

	private Transform _particleTm;

	private ParticleSystem _particleSystem;

	private ParticleInitialData _particleInitial = new ParticleInitialData();

	private bool _isFlip;

	private float _lastLossyScale;

	public float PlaybackSpeed
	{
		get
		{
			if (!(_particleSystem == null))
			{
				return _particleSystem.main.simulationSpeed;
			}
			return 1f;
		}
		set
		{
			if (_particleSystem != null)
			{
				ParticleSystem.MainModule main = _particleSystem.main;
				main.simulationSpeed = value;
			}
		}
	}

	public bool RenderEnabled
	{
		get
		{
			return _renderEnabled;
		}
		set
		{
			if (_renderEnabled == value)
			{
				return;
			}
			_renderEnabled = value;
			for (int i = 0; i < _renderProxyList.Count; i++)
			{
				RenderProxy renderProxy = _renderProxyList[i];
				if (renderProxy.renderer != null)
				{
					renderProxy.renderer.enabled = _renderEnabled;
				}
			}
		}
	}

	public bool IsPlaying { get; private set; }

	public bool Open(GameObject particleGo, IMaterialFactory mtrlFactory)
	{
		_particleSystem = particleGo.GetComponent<ParticleSystem>();
		if (_particleSystem == null)
		{
			return false;
		}
		ParticleSubEmitterProxy component = particleGo.GetComponent<ParticleSubEmitterProxy>();
		if (component != null)
		{
			component.RestoreSubEmittersInPlaying();
		}
		ParticleSystemRenderer[] componentsInChildren = particleGo.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(componentsInChildren))
		{
			return false;
		}
		_particleTm = particleGo.transform;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			RenderProxy renderProxy = RenderProxy.CreateProxy(componentsInChildren[i], mtrlFactory);
			if (renderProxy != null)
			{
				_renderProxyList.Add(renderProxy);
			}
		}
		IsPlaying = false;
		ParticleSystem.EmissionModule emission = _particleSystem.emission;
		emission.enabled = false;
		_particleSystem.Play(withChildren: true);
		_particleInitial.Init(_particleSystem, _particleTm);
		_isFlip = false;
		_lastLossyScale = Vespa_Unity_Vector3_Extension_Method.MaxComponent(Vespa_Unity_Vector3_Extension_Method.Abs(_particleInitial.TransformLocalScale));
		return true;
	}

	public void Close()
	{
		if (_particleSystem != null)
		{
			_particleInitial.ApplyToParticleSystem(_particleSystem, 1f);
			_particleSystem = null;
		}
		if (_particleTm != null)
		{
			_particleTm.localRotation = _particleInitial.TransformLocalRotation;
			_particleTm.localScale = _particleInitial.TransformLocalScale;
			_particleTm = null;
		}
		for (int i = 0; i < _renderProxyList.Count; i++)
		{
			_renderProxyList[i].ReleaseProxy();
		}
		_renderProxyList.Clear();
	}

	public void Emit(int count)
	{
		if (_particleSystem != null)
		{
			_particleSystem.Emit(count);
		}
	}

	public void Play()
	{
		IsPlaying = true;
		if (_particleSystem != null)
		{
			ParticleSystem.EmissionModule emission = _particleSystem.emission;
			emission.enabled = true;
			_particleSystem.Simulate(0f, withChildren: false, restart: true);
			_particleSystem.Play(withChildren: true);
		}
	}

	public void Stop()
	{
		IsPlaying = false;
		if (_particleSystem != null)
		{
			ParticleSystem.EmissionModule emission = _particleSystem.emission;
			emission.enabled = false;
		}
	}

	public void Clear()
	{
		if (_particleSystem != null)
		{
			_particleSystem.Clear();
		}
	}

	public bool IsEmpty()
	{
		if (_particleSystem == null)
		{
			return true;
		}
		return _particleSystem.particleCount == 0;
	}

	public void AdjustParentTransform()
	{
		if (_particleSystem == null || _particleSystem.main.scalingMode == ParticleSystemScalingMode.Hierarchy)
		{
			return;
		}
		Vector3 vector = Vector3.one;
		if (_particleTm.parent != null)
		{
			vector = _particleTm.parent.lossyScale;
		}
		bool flag = vector.x < 0f && vector.y < 0f && vector.z < 0f;
		if (flag != _isFlip)
		{
			_isFlip = flag;
			if (_isFlip)
			{
				_particleTm.localScale = -_particleInitial.TransformLocalScale;
				_particleTm.localRotation = _particleInitial.TransformLocalRotation * Quaternion.AngleAxis(180f, Vector3.right);
			}
			else
			{
				_particleTm.localScale = _particleInitial.TransformLocalScale;
				_particleTm.localRotation = _particleInitial.TransformLocalRotation;
			}
		}
		float num = Vespa_Unity_Vector3_Extension_Method.MaxComponent(Vespa_Unity_Vector3_Extension_Method.Abs(_particleTm.lossyScale));
		if (!Mathf.Approximately(_lastLossyScale, num))
		{
			_lastLossyScale = num;
			float scale = _lastLossyScale / Vespa_Unity_Vector3_Extension_Method.MaxComponent(_particleInitial.TransformLocalScale);
			_particleInitial.ApplyToParticleSystem(_particleSystem, scale);
		}
	}

	public void Visit(IVisitor visitor)
	{
		visitor.VisitList(_renderProxyList);
	}
}
