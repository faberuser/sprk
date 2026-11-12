using System.Collections.Generic;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree.NObject;

public class ModelObject : ObjectBase
{
	public class PrefabInstance : ILinkData
	{
		public GameObject Go;

		public List<RenderProxy> RenderList = new List<RenderProxy>();

		public void SetRenderers(Renderer[] renderers)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(renderers); i++)
			{
				RenderProxy renderProxy = RenderProxy.CreateProxy(renderers[i]);
				if (renderProxy != null)
				{
					RenderList.Add(renderProxy);
				}
			}
		}

		void ILinkData.OnDestroy()
		{
			for (int i = 0; i < RenderList.Count; i++)
			{
				RenderList[i].ReleaseProxy();
			}
			RenderList.Clear();
		}
	}

	private PrefabInstance _instance;

	private ILocalTimer _localTimer;

	private RenderGroupModifier2 _renderGroupModifier;

	public List<RenderProxy> RenderList => _instance.RenderList;

	public void Open(PrefabGoAuto prefabAuto, ILocalTimer localTimer)
	{
		_renderGroupModifier = SmallPooling<RenderGroupModifier2>.CreateInstance();
		_renderGroupModifier.Open(useSoloTimeColor: true);
		_localTimer = localTimer;
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
			_instance.SetRenderers(gameObject.GetComponentsInChildren<Renderer>());
			linkFactoryComponent.SetLinkData(_instance);
		}
	}

	public void Close()
	{
		if (_instance != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_instance.RenderList); i++)
			{
				_instance.RenderList[i].ReleaseProxy();
			}
			FactoryBase.ReleaseGameObject(_instance.Go);
			_instance = null;
		}
		if (_renderGroupModifier != null)
		{
			SmallPooling<RenderGroupModifier2>.ReleaseInstance(_renderGroupModifier);
			_renderGroupModifier = null;
		}
	}

	public override void ChainUpdate()
	{
		if (_instance != null)
		{
			_renderGroupModifier.ChainUpdate(_localTimer);
			if (_renderGroupModifier.BeginFlush())
			{
				_renderGroupModifier.VisitList(_instance.RenderList);
				_renderGroupModifier.EndFlush();
			}
		}
	}

	public override void Visit(IVisitor visitor)
	{
		if (_instance != null)
		{
			visitor.VisitList(_instance.RenderList);
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
