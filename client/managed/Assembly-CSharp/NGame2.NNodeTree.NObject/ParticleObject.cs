using NShader;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NRenderGroupModifier;
using UnityEngine;

namespace NGame2.NNodeTree.NObject;

public class ParticleObject : ObjectBase
{
	private class SetColorVisitor : IGenericVisitor<RenderProxy>, IVisitor
	{
		public Color ColorMix;

		public float ExtraAlpha = 1f;

		void IGenericVisitor<RenderProxy>.Visit(RenderProxy proxy)
		{
			Material[] materials = proxy.materials;
			if (__Extension_Method_ICollection__.IsNullOrEmpty(materials))
			{
				return;
			}
			foreach (Material material in materials)
			{
				if (!(material == null))
				{
					material.SetColor(Define.COLORMIX_ID, ColorMix);
					material.SetFloat(Define.EXTRAALPHA_ID, ExtraAlpha);
				}
			}
		}
	}

	private class PrefabInstance : ILinkData
	{
		public GameObject Go;

		public ParticleProxy Proxy = new ParticleProxy();

		void ILinkData.OnDestroy()
		{
			Proxy.Close();
		}
	}

	private PrefabInstance _instance;

	private bool _isDistanceEmit;

	private LocalTimer _localTimer;

	private bool _play;

	private int _skipUpdateCount;

	private ExtraAlpha _extraAlpha = new ExtraAlpha();

	private TimerEventColorMix _timerEventColorMix = new TimerEventColorMix();

	private SetColorVisitor _setColorVisitor = new SetColorVisitor();

	public bool IsEmptyParticle
	{
		get
		{
			if (_instance == null)
			{
				return true;
			}
			return _instance.Proxy.IsEmpty();
		}
	}

	public bool Play
	{
		get
		{
			return _play;
		}
		set
		{
			_play = value;
		}
	}

	public override void ChainUpdate()
	{
		if (_skipUpdateCount > 0)
		{
			_skipUpdateCount--;
		}
		else if (_instance != null)
		{
			if (_play && !_instance.Proxy.IsPlaying)
			{
				_instance.Proxy.Play();
			}
			else if (!_play && _instance.Proxy.IsPlaying)
			{
				_instance.Proxy.Stop();
			}
			_instance.Proxy.PlaybackSpeed = (float)_localTimer.playbackSpeed * 0.001f;
			_extraAlpha.Update(_localTimer);
			_setColorVisitor.ExtraAlpha = _extraAlpha.Final;
			_setColorVisitor.ColorMix = _timerEventColorMix.UpdateAndCalc(_localTimer);
			_instance.Proxy.Visit(_setColorVisitor);
		}
	}

	public override void Visit(IVisitor visitor)
	{
	}

	public void Open(PrefabGoAuto prefabAuto, bool isDistanceEmit, LocalTimer localTimer)
	{
		_isDistanceEmit = isDistanceEmit;
		_localTimer = localTimer;
		_skipUpdateCount = 1;
		GameObject gameObject = prefabAuto.Factory.Create(base.transform);
		LinkFactoryComponent linkFactoryComponent = SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.Get(gameObject.transform);
		if (linkFactoryComponent == null)
		{
			VespaDebug.LogError("FactoryLink is null");
			SprkGraphicsOptions.ObserveParticles(this);
			return;
		}
		_instance = linkFactoryComponent.GetLinkData<PrefabInstance>();
		if (_instance == null)
		{
			_instance = new PrefabInstance();
			_instance.Go = gameObject;
			_instance.Proxy.Open(gameObject, null);
			linkFactoryComponent.SetLinkData(_instance);
		}
		SprkGraphicsOptions.ObserveParticles(this);
	}

	public void FinalizeParticle()
	{
		SprkGraphicsOptions.ReleaseParticles(this);
		_play = false;
		if (_instance != null)
		{
			_instance.Proxy.Stop();
			_instance.Proxy.Clear();
			FactoryBase.ReleaseGameObject(_instance.Go);
			_instance = null;
		}
	}

	public void Burst(int count)
	{
		if (_instance != null)
		{
			_instance.Proxy.Emit(count);
		}
	}

	public override Transform FindTransform(string nodeName)
	{
		return base.transform;
	}

	protected override void OnCreateDerived()
	{
	}

	protected override void OnReleaseDerived()
	{
		SprkGraphicsOptions.ReleaseParticles(this);
	}
}
