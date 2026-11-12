using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NDrawCall;

public class MaterialFactory : SingletonAuto<MaterialFactory>
{
	private class MaterialPool
	{
		private Queue<Material> _unusedQueue = new Queue<Material>();

		public Shader BaseShader { get; private set; }

		public Material BaseMaterial { get; private set; }

		public int Count => _unusedQueue.Count;

		public MaterialPool(Shader shader)
		{
			BaseShader = shader;
			BaseMaterial = new Material(shader);
			BaseMaterial.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		}

		public Material Create()
		{
			if (_unusedQueue.Count > 0)
			{
				return _unusedQueue.Dequeue();
			}
			return new Material(BaseShader)
			{
				hideFlags = (HideFlags.DontSave | HideFlags.NotEditable)
			};
		}

		public bool Release(Material mtrl)
		{
			if (BaseMaterial == null)
			{
				return false;
			}
			mtrl.CopyPropertiesFromMaterial(BaseMaterial);
			mtrl.mainTexture = null;
			mtrl.shaderKeywords = null;
			_unusedQueue.Enqueue(mtrl);
			return true;
		}

		public void ReleaseAll()
		{
			while (_unusedQueue.Count > 0)
			{
				VespaObject.DestroyObject(_unusedQueue.Dequeue());
			}
			VespaObject.DestroyObject(BaseMaterial);
		}
	}

	private Dictionary<int, MaterialPool> _dic = new Dictionary<int, MaterialPool>();

	public void DrawGui()
	{
		foreach (KeyValuePair<int, MaterialPool> item in _dic)
		{
			MaterialPool value = item.Value;
			GUILayout.Label($"Shader={Vespa_Unity_Object_Extension_Method.NAME(value.BaseShader)}, BaseMaterial={Vespa_Unity_Object_Extension_Method.NAME(value.BaseMaterial)}, Unused={value.Count}");
		}
	}

	public Material CreateMaterial(Shader shader, string name)
	{
		if (shader == null)
		{
			return null;
		}
		int instanceID = shader.GetInstanceID();
		MaterialPool materialPool = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, instanceID, null);
		if (materialPool == null)
		{
			materialPool = new MaterialPool(shader);
			_dic.Add(instanceID, materialPool);
		}
		Material material = materialPool.Create();
		material.name = name;
		return material;
	}

	public static void ReleaseMaterial(Material mtrl)
	{
		if (mtrl == null || SystemStatus.IsApplicationQuit)
		{
			return;
		}
		if (!SingletonAuto<MaterialFactory>.isValidInstance)
		{
			VespaObject.DestroyObject(mtrl);
			return;
		}
		int i_key = ((!(mtrl.shader == null)) ? mtrl.shader.GetInstanceID() : 0);
		MaterialPool vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(SingletonAuto<MaterialFactory>.instance._dic, i_key, null);
		if (vALUE == null)
		{
			VespaObject.DestroyObject(mtrl);
		}
		else if (!vALUE.Release(mtrl))
		{
			VespaObject.DestroyObject(mtrl);
		}
	}

	public void OnSceneChanged()
	{
		foreach (MaterialPool value in _dic.Values)
		{
			value.ReleaseAll();
		}
		_dic.Clear();
	}
}
