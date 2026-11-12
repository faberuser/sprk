using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

public class HandlerVespa : SceneAutoBehaviour<HandlerVespa>, InitOnCreate
{
	public int GlobalTrailRendererCount;

	private IncreaseDic<Material, List<PCTrail>> _matToTrailList;

	private List<Mesh> _combineMeshList;

	private int _skipUnityUpdate;

	void InitOnCreate.InitializeSingleton()
	{
		_matToTrailList = new IncreaseDic<Material, List<PCTrail>>(32);
		_combineMeshList = new List<Mesh>(32);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_combineMeshList); i++)
		{
			VespaObject.DestroyWithAssets(_combineMeshList[i]);
		}
		_combineMeshList.Clear();
	}

	public void ReserveMaterial(Material trailMaterial)
	{
		if (!_matToTrailList.ContainsKey(trailMaterial))
		{
			_matToTrailList.Add(trailMaterial, new List<PCTrail>(32));
		}
	}

	public void AddTrail(Material trailMaterial, PCTrail trail)
	{
		List<PCTrail> list = _matToTrailList.GETVALUE(trailMaterial, null);
		if (list == null)
		{
			list = new List<PCTrail>();
			_matToTrailList.Add(trailMaterial, list);
		}
		list.Add(trail);
	}

	public void ChainUpdate()
	{
		_skipUnityUpdate = 2;
		RenderTrailList();
	}

	private void LateUpdate()
	{
		if (_skipUnityUpdate > 0)
		{
			_skipUnityUpdate--;
		}
		else
		{
			RenderTrailList();
		}
	}

	private void RenderTrailList()
	{
		int num = 0;
		for (int i = 0; i < _matToTrailList.Count; i++)
		{
			KeyValuePair<Material, List<PCTrail>> keyValuePairByIndex = _matToTrailList.GetKeyValuePairByIndex(i);
			Material key = keyValuePairByIndex.Key;
			List<PCTrail> value = keyValuePairByIndex.Value;
			int num2 = 0;
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(value); j++)
			{
				if (value[j].Mesh != null)
				{
					num2++;
				}
			}
			if (num2 == 0)
			{
				value.Clear();
				continue;
			}
			CombineInstance[] array = new CombineInstance[num2];
			int num3 = 0;
			for (int k = 0; k < __Extension_Method_ICollection__.COUNT(value); k++)
			{
				if (!(value[k].Mesh == null))
				{
					if (num3 >= __Extension_Method_ICollection__.COUNT(array))
					{
						break;
					}
					array[num3] = new CombineInstance
					{
						mesh = value[k].Mesh,
						subMeshIndex = 0,
						transform = Matrix4x4.identity
					};
					num3++;
				}
			}
			Mesh mesh;
			if (num < _combineMeshList.Count)
			{
				mesh = _combineMeshList[num];
			}
			else
			{
				mesh = new Mesh();
				_combineMeshList.Add(mesh);
			}
			mesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: false);
			DrawMesh(mesh, key);
			value.Clear();
			num++;
		}
		int num4 = _combineMeshList.Count - num;
		for (int l = 0; l < num4; l++)
		{
			Mesh obj = _combineMeshList[_combineMeshList.Count - 1];
			_combineMeshList.RemoveAt(_combineMeshList.Count - 1);
			VespaObject.DestroyWithAssets(obj);
		}
	}

	private void DrawMesh(Mesh trailMesh, Material trailMaterial)
	{
		Graphics.DrawMesh(trailMesh, Matrix4x4.identity, trailMaterial, base.gameObject.layer);
	}
}
