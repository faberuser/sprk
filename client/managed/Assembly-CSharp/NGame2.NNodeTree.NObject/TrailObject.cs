using NVespa;
using NVespa.NPrefab2;
using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

namespace NGame2.NNodeTree.NObject;

public class TrailObject : ObjectBase
{
	private class PrefabInstance : ILinkData
	{
		public GameObject Go;

		public TrailRenderer_Base[] Trails;

		public void Open()
		{
		}

		public void Close()
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Trails); i++)
			{
				Trails[i].ClearSystem(emitState: false);
			}
		}

		void ILinkData.OnDestroy()
		{
		}
	}

	private PrefabInstance _instance;

	private ILocalTimer _timer;

	private bool _play;

	private int _openFrameCount;

	private float _openTime;

	public bool IsEmpty
	{
		get
		{
			if (_instance == null)
			{
				return true;
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_instance.Trails); i++)
			{
				if (_instance.Trails[i].NumSegments() > 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool Emit
	{
		get
		{
			if (_instance == null)
			{
				return false;
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_instance.Trails); i++)
			{
				if (_instance.Trails[i].Emit)
				{
					return true;
				}
			}
			return false;
		}
		set
		{
			if (_instance != null)
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_instance.Trails); i++)
				{
					_instance.Trails[i].Emit = value;
				}
			}
		}
	}

	public override void ChainUpdate()
	{
		if (Time.frameCount >= _openFrameCount + 2 && _instance != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_instance.Trails); i++)
			{
				_instance.Trails[i].ChainUpdate(_timer, (float)_timer.deltaTick * 0.001f);
			}
			_ = Time.realtimeSinceStartup - _openTime;
			_ = 10000f;
		}
	}

	public override void Visit(IVisitor visitor)
	{
	}

	public void Open(PrefabGoAuto prefabAuto, ILocalTimer localTimer)
	{
		_openTime = Time.realtimeSinceStartup;
		if (localTimer != null)
		{
			_timer = localTimer;
		}
		else
		{
			_timer = MainTimer.DefaultLocal;
		}
		_openFrameCount = Time.frameCount;
		GameObject gameObject = prefabAuto.Factory.Create(base.transform);
		LinkFactoryComponent linkFactoryComponent = SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.Get(gameObject.transform);
		if (linkFactoryComponent == null)
		{
			VespaDebug.LogError("FactoryLink is null");
			return;
		}
		_instance = linkFactoryComponent.GetLinkData<PrefabInstance>();
		if (_instance == null)
		{
			_instance = new PrefabInstance();
			_instance.Go = gameObject;
			_instance.Trails = gameObject.GetComponents<TrailRenderer_Base>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_instance.Trails); i++)
			{
				_instance.Trails[i].Emit = false;
			}
			linkFactoryComponent.SetLinkData(_instance);
		}
		_instance.Open();
	}

	public void FinalizeTrail()
	{
		if (_instance != null)
		{
			_instance.Close();
			FactoryBase.ReleaseGameObject(_instance.Go);
			_instance = null;
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
	}
}
