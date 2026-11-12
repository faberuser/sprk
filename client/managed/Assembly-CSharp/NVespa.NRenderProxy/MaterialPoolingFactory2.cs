using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NRenderProxy;

public class MaterialPoolingFactory2 : SingletonAuto<MaterialPoolingFactory2>, IMaterialFactory
{
	private struct PoolingKey
	{
		public int LastMtrlId;

		public int HashCode;

		public int Count;

		public PoolingKey(Material[] sharedMaterials)
		{
			LastMtrlId = 0;
			HashCode = 0;
			Count = __Extension_Method_ICollection__.COUNT(sharedMaterials);
			for (int i = 0; i < Count; i++)
			{
				Material material = sharedMaterials[i];
				if (!(material == null))
				{
					LastMtrlId = material.GetInstanceID();
					HashCode ^= material.GetHashCode();
				}
			}
		}
	}

	private class KeyComparer : IEqualityComparer<PoolingKey>
	{
		bool IEqualityComparer<PoolingKey>.Equals(PoolingKey x, PoolingKey y)
		{
			if (x.LastMtrlId == y.LastMtrlId && x.HashCode == y.HashCode)
			{
				return x.Count == y.Count;
			}
			return false;
		}

		int IEqualityComparer<PoolingKey>.GetHashCode(PoolingKey obj)
		{
			return obj.HashCode;
		}
	}

	private IncreaseDic<PoolingKey, Queue<Material[]>> _unusedDic;

	public MaterialPoolingFactory2()
	{
		_unusedDic = new IncreaseDic<PoolingKey, Queue<Material[]>>(new KeyComparer());
	}

	Material[] IMaterialFactory.CloneMaterials(Material[] sharedMaterials)
	{
		Queue<Material[]> unusedQueue = GetUnusedQueue(sharedMaterials, autoCreate: false);
		if (unusedQueue != null && unusedQueue.Count > 0)
		{
			Material[] array = unusedQueue.Dequeue();
			for (int i = 0; i < array.Length; i++)
			{
				if (!(sharedMaterials[i] == null))
				{
					array[i].CopyPropertiesFromMaterial(sharedMaterials[i]);
				}
			}
			return array;
		}
		return DefaultMaterialFactory.CloneMaterials(sharedMaterials);
	}

	void IMaterialFactory.ReleaseMaterials(Material[] materials, Material[] sharedMaterials)
	{
		Queue<Material[]> unusedQueue = GetUnusedQueue(sharedMaterials, autoCreate: true);
		if (unusedQueue != null)
		{
			unusedQueue.Enqueue(materials);
		}
		else
		{
			DefaultMaterialFactory.ReleaseMaterials(materials);
		}
	}

	public void OnSceneChanged()
	{
		for (int i = 0; i < _unusedDic.Count; i++)
		{
			Queue<Material[]> valueByIndex = _unusedDic.GetValueByIndex(i);
			while (valueByIndex.Count > 0)
			{
				DefaultMaterialFactory.ReleaseMaterials(valueByIndex.Dequeue());
			}
		}
		_unusedDic.Clear();
	}

	private Queue<Material[]> GetUnusedQueue(Material[] sharedMaterials, bool autoCreate)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(sharedMaterials))
		{
			return null;
		}
		PoolingKey key = new PoolingKey(sharedMaterials);
		if (key.LastMtrlId == 0)
		{
			return null;
		}
		if (_unusedDic.TryGetValue(key, out var o_value))
		{
			return o_value;
		}
		if (!autoCreate)
		{
			return null;
		}
		o_value = new Queue<Material[]>();
		_unusedDic.Add(key, o_value);
		return o_value;
	}
}
