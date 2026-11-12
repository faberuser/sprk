using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class ModelInstance
{
	public class RenderInfo
	{
		public Vector3 OriginalPosition = Vector3.zero;

		public Quaternion OriginalRotation = Quaternion.identity;

		public Vector3 OriginalScale = Vector3.one;

		public Color OriginalColor = Color.white;

		public bool Enabled
		{
			get
			{
				if (!(Proxy.renderer == null))
				{
					return Proxy.renderer.enabled;
				}
				return false;
			}
			set
			{
				if (Proxy.renderer != null)
				{
					Proxy.renderer.enabled = value;
				}
			}
		}

		public Transform Tm { get; private set; }

		public RenderExtraData Extra { get; private set; }

		public RenderProxy Proxy { get; private set; }

		public RenderInfo(Transform tm, Renderer component, RenderExtraData extra)
		{
			Tm = tm;
			Proxy = RenderProxy.CreateProxy(component);
			Extra = extra;
			OriginalPosition = Tm.localPosition;
			OriginalRotation = Tm.localRotation;
			OriginalScale = Tm.localScale;
			if (Proxy.sharedMaterial != null && Proxy.sharedMaterial.HasProperty("_Color"))
			{
				OriginalColor = Proxy.sharedMaterial.color;
			}
		}

		public void Close()
		{
			if (Proxy != null)
			{
				Proxy.ReleaseProxy();
			}
		}
	}

	private List<RenderInfo> RenderInfoList = new List<RenderInfo>();

	public IModelData Data { get; private set; }

	public BattleFieldMapInstance Owner { get; private set; }

	public GameObject InstanceGo { get; private set; }

	public GameObject ModelGo { get; private set; }

	public ModelInstance(BattleFieldMapInstance owner)
	{
		Owner = owner;
	}

	public bool Open(IBattleFieldMapOpenData openData)
	{
		Data = openData.Model;
		ModelGo = openData.InstantiateModel();
		if (ModelGo == null)
		{
			return false;
		}
		Camera cameraByLayer = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(openData.Stock.Layer);
		InstanceGo = SceneAutoBehaviour<GameObjectFactory>.instance.Create();
		InstanceGo.layer = ModelGo.layer;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(InstanceGo.transform, null);
		Vespa_Unity_Transform_Extension_Method.ParentRelative(ModelGo.transform, InstanceGo.transform);
		SetupRenderer();
		AdjustLayerTransform(openData.Stock, cameraByLayer);
		return true;
	}

	public void Close()
	{
		for (int i = 0; i < RenderInfoList.Count; i++)
		{
			RenderInfoList[i].Enabled = true;
			RenderInfoList[i].Close();
		}
		RenderInfoList.Clear();
		if (ModelGo != null)
		{
			ModelGo.SetActive(value: false);
			ModelGo.transform.parent = null;
			VespaObject.DestroyGo(ModelGo);
			ModelGo = null;
		}
		if (InstanceGo != null)
		{
			GameObjectFactory.ReleaseGameObject(InstanceGo);
			InstanceGo = null;
		}
	}

	public Transform GetNodeByPath(string nodePath)
	{
		if (ModelGo == null)
		{
			return null;
		}
		return ModelGo.transform.Find(nodePath);
	}

	public void ChainUpdate(Camera cam)
	{
		AdjustLayerTransform(Owner.OpenData.Stock, cam);
	}

	public void Visit(IVisitor visitor)
	{
		for (int i = 0; i < RenderInfoList.Count; i++)
		{
			visitor.TryVisit(RenderInfoList[i].Proxy);
		}
	}

	private void SetupRenderer()
	{
		List<RenderExtraData> renderExtraList = Data.GetRenderExtraList();
		for (int i = 0; i < renderExtraList.Count; i++)
		{
			RenderExtraData renderExtraData = renderExtraList[i];
			Transform transform = ModelGo.transform.Find(renderExtraData.RendererPath);
			if (transform == null)
			{
				continue;
			}
			Renderer component = transform.GetComponent<Renderer>();
			if (component == null)
			{
				continue;
			}
			RenderInfo renderInfo = new RenderInfo(transform, component, renderExtraData);
			RenderInfoList.Add(renderInfo);
			if (!DefineBattleFieldMap2.NoAlphaMaterial.Value)
			{
				continue;
			}
			Material[] materials = renderInfo.Proxy.materials;
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(materials); j++)
			{
				Material material = materials[j];
				if (!(material == null))
				{
					material.shader = Shader.Find("Vespa/Etc/BattleFieldMapNoAlpha");
				}
			}
		}
	}

	private void AdjustLayerTransform(BattleFieldMapStock stock, Camera cam)
	{
		if (cam == null)
		{
			return;
		}
		for (int i = 0; i < RenderInfoList.Count; i++)
		{
			RenderInfo renderInfo = RenderInfoList[i];
			Vector3 originalPosition = renderInfo.OriginalPosition;
			Quaternion localRotation = renderInfo.OriginalRotation;
			Vector3 localScale = renderInfo.OriginalScale * renderInfo.Extra.GetFinalScale();
			Vector3 vector = renderInfo.OriginalPosition - cam.transform.position;
			vector.z = Mathf.Abs(vector.z) + 1f;
			vector.x = 0f;
			vector.Normalize();
			if (renderInfo.Extra.IsBillboard)
			{
				localRotation = Quaternion.LookRotation(vector) * renderInfo.OriginalRotation;
			}
			if (renderInfo.Extra.IsBackground)
			{
				originalPosition += vector * renderInfo.Extra.BackgroundDepth;
			}
			if (renderInfo.Extra.IsLoop && renderInfo.Extra.LoopLength > 0.1f)
			{
				float num = renderInfo.Extra.LoopLength * renderInfo.Extra.GetFinalScale();
				int num2 = Mathf.RoundToInt(cam.transform.position.x / num);
				originalPosition += Vector3.right * (num * (float)num2);
			}
			renderInfo.Tm.localPosition = originalPosition;
			renderInfo.Tm.localRotation = localRotation;
			renderInfo.Tm.localScale = localScale;
		}
	}

	public void SetParent(Transform parentTm, BattleFieldMapStock stock, Camera cam)
	{
		if (stock.Layer != -1 && InstanceGo.layer != stock.Layer)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(InstanceGo, stock.Layer);
		}
		Vespa_Unity_Transform_Extension_Method.ParentRelative(InstanceGo.transform, parentTm);
		AdjustLayerTransform(stock, cam);
	}
}
