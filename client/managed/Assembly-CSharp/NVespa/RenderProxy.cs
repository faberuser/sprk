using System;
using NShader;
using NVespa.NRenderProxy;
using UnityEngine;
using UnityEngine.Rendering;

namespace NVespa;

[Serializable]
public class RenderProxy
{
	public class ReplaceMaterialData
	{
		public Material From;

		public Material To;
	}

	public class MaterialKeywordVisitor : IGenericVisitor<RenderProxy>, IVisitor
	{
		public bool IsEnable;

		public string Keyword;

		void IGenericVisitor<RenderProxy>.Visit(RenderProxy renderProxy)
		{
			if (!String_Extension_Method.IsNullOrEmpty(Keyword))
			{
				if (IsEnable)
				{
					renderProxy.EnableMaterialKeyword(Keyword);
				}
				else
				{
					renderProxy.DisableMaterialKeyword(Keyword);
				}
			}
		}
	}

	private static MaterialPropertyBlock EmptyBlock = new MaterialPropertyBlock();

	private Material[] _sharedMaterials;

	private Material[] _sharedMaterialsOriginal;

	[SerializeField]
	private Renderer _renderer;

	private IMaterialFactory _materialFactory;

	private Material[] _materials;

	public bool IncludeAlphaTest { get; private set; }

	public bool HasCloneMaterial => _materials != null;

	public Renderer renderer => _renderer;

	public Material material
	{
		get
		{
			if (_materials != null)
			{
				return _materials[0];
			}
			CloneMaterials();
			if (_materials == null)
			{
				return null;
			}
			return _materials[0];
		}
	}

	public Material sharedMaterial
	{
		get
		{
			if (_renderer == null)
			{
				return null;
			}
			return _renderer.sharedMaterial;
		}
	}

	public Material[] materials
	{
		get
		{
			CloneMaterials();
			return _materials;
		}
	}

	public Material[] SharedMaterials => _sharedMaterials;

	public static RenderProxy CreateProxy(Renderer renderer, IMaterialFactory mtrlFactory = null)
	{
		return CreateProxy(renderer, null, mtrlFactory);
	}

	public static RenderProxy CreateProxy(Renderer renderer, ReplaceMaterialData replaceMtrl, IMaterialFactory mtrlFactory = null)
	{
		if (renderer == null)
		{
			return null;
		}
		return new RenderProxy(renderer, replaceMtrl, mtrlFactory);
	}

	internal RenderProxy(Renderer renderer, ReplaceMaterialData replaceMtrl, IMaterialFactory mtrlFactory)
	{
		_renderer = renderer;
		if (_renderer != null)
		{
			_renderer.shadowCastingMode = ShadowCastingMode.Off;
			_renderer.receiveShadows = false;
			_renderer.useLightProbes = false;
			_renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		IMaterialFactory materialFactory;
		if (mtrlFactory != null)
		{
			materialFactory = mtrlFactory;
		}
		else
		{
			IMaterialFactory materialFactory2 = DefaultMaterialFactory.Default;
			materialFactory = materialFactory2;
		}
		_materialFactory = materialFactory;
		_sharedMaterialsOriginal = _renderer.sharedMaterials;
		_sharedMaterials = _sharedMaterialsOriginal;
		if (replaceMtrl != null)
		{
			_sharedMaterials = new Material[__Extension_Method_ICollection__.COUNT(_sharedMaterialsOriginal)];
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_sharedMaterialsOriginal); i++)
			{
				if (_sharedMaterialsOriginal[i] == replaceMtrl.From)
				{
					_sharedMaterials[i] = replaceMtrl.To;
				}
				else
				{
					_sharedMaterials[i] = _sharedMaterialsOriginal[i];
				}
			}
			_renderer.materials = _sharedMaterials;
		}
		if (!(_renderer != null))
		{
			return;
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_sharedMaterials); j++)
		{
			Material material = _sharedMaterials[j];
			if (!(material == null) && material.HasProperty(Define.BLENDMODEDST_ID) && material.GetInt(Define.BLENDMODEDST_ID) == 0)
			{
				IncludeAlphaTest = true;
			}
		}
	}

	public void EnableMaterialKeyword(string keyword)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(materials); i++)
		{
			Material material = materials[i];
			if (!(material == null))
			{
				material.EnableKeyword(keyword);
			}
		}
	}

	public void DisableMaterialKeyword(string keyword)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(materials); i++)
		{
			Material material = materials[i];
			if (!(material == null))
			{
				material.DisableKeyword(keyword);
			}
		}
	}

	private void CloneMaterials()
	{
		if (_materials == null && !(_renderer == null) && !__Extension_Method_ICollection__.IsNullOrEmpty(_sharedMaterials))
		{
			_materials = _materialFactory.CloneMaterials(_sharedMaterials);
			_renderer.materials = _materials;
		}
	}

	public void ReleaseProxy()
	{
		if (!(_renderer == null))
		{
			_renderer.SetPropertyBlock(EmptyBlock);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(_materials))
			{
				_renderer.materials = _sharedMaterialsOriginal;
				_materialFactory.ReleaseMaterials(_materials, _sharedMaterials);
				_materials = null;
			}
		}
	}
}
