using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class RenderCache : MonoBehaviour
{
	public List<RenderProxy> RenderProxyList = new List<RenderProxy>();

	private bool _init;

	public void Initialize(RenderProxy.ReplaceMaterialData replaceMtrl = null)
	{
		if (_init)
		{
			return;
		}
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
		{
			RenderProxy renderProxy = RenderProxy.CreateProxy(componentsInChildren[i], replaceMtrl);
			if (renderProxy != null)
			{
				RenderProxyList.Add(renderProxy);
				SkinnedMeshRenderer skinnedMeshRenderer = componentsInChildren[i] as SkinnedMeshRenderer;
				if (!(skinnedMeshRenderer == null))
				{
					skinnedMeshRenderer.updateWhenOffscreen = true;
				}
			}
		}
		_init = true;
	}

	public RenderProxy GetRenderProxyByName(string name)
	{
		for (int i = 0; i < RenderProxyList.Count; i++)
		{
			RenderProxy renderProxy = RenderProxyList[i];
			if (!(renderProxy.renderer == null) && string.Equals(renderProxy.renderer.name, name))
			{
				return renderProxy;
			}
		}
		return null;
	}

	public Material GetMaterialByNames(string rendererName, string mtrlName, bool isShared)
	{
		RenderProxy renderProxyByName = GetRenderProxyByName(rendererName);
		if (renderProxyByName == null)
		{
			return null;
		}
		Material[] array = ((!isShared) ? renderProxyByName.materials : renderProxyByName.SharedMaterials);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			Material material = array[i];
			if (!(material == null) && material.name == mtrlName)
			{
				return material;
			}
		}
		return null;
	}

	public void Visit(IVisitor visitor)
	{
		for (int i = 0; i < RenderProxyList.Count; i++)
		{
			visitor.TryVisit(RenderProxyList[i]);
		}
	}
}
