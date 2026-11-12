using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NGame3DWidget;

public class RenderQueueVisitor : IGenericVisitor<RenderProxy>, IVisitor
{
	private struct MaterialPair
	{
		public Material material;

		public Material sharedMaterial;
	}

	private List<MaterialPair> _materialPairList = new List<MaterialPair>();

	public int UpdateExtraRenderQueue(int renderQueue, bool isExplicit)
	{
		if (_materialPairList.Count == 0)
		{
			return renderQueue;
		}
		_materialPairList.Sort((MaterialPair lhs, MaterialPair rhs) => lhs.sharedMaterial.renderQueue.CompareTo(rhs.sharedMaterial.renderQueue));
		int renderQueue2 = _materialPairList[0].sharedMaterial.renderQueue;
		for (int num = 0; num < _materialPairList.Count; num++)
		{
			Material sharedMaterial = _materialPairList[num].sharedMaterial;
			Material material = _materialPairList[num].material;
			if (sharedMaterial.renderQueue != renderQueue2 && !isExplicit)
			{
				renderQueue++;
				renderQueue2 = sharedMaterial.renderQueue;
			}
			material.renderQueue = renderQueue;
		}
		_materialPairList.Clear();
		renderQueue++;
		return renderQueue;
	}

	void IGenericVisitor<RenderProxy>.Visit(RenderProxy renderProxy)
	{
		Material[] sharedMaterials = renderProxy.SharedMaterials;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(sharedMaterials))
		{
			return;
		}
		Material[] materials = renderProxy.materials;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(materials) || sharedMaterials.Length != materials.Length)
		{
			return;
		}
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			Material material = materials[i];
			Material material2 = sharedMaterials[i];
			if (!(material == null) && !(material2 == null))
			{
				_materialPairList.Add(new MaterialPair
				{
					material = material,
					sharedMaterial = material2
				});
			}
		}
	}
}
