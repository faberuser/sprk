using System.Collections.Generic;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa;

public class GlobalBillboardManager : SingletonUpdater<GlobalBillboardManager>, ILateUpdater
{
	public enum EBillboardType
	{
		None,
		Billboard,
		VerticalBillboard
	}

	public class EntryData
	{
		public EBillboardType BillboardType;

		public Transform Tm;

		public Quaternion LocalRot;

		public void Close()
		{
			Tm = null;
		}

		public void UpdateBillboard(Camera camera)
		{
			if (!(Tm == null) && Tm.gameObject.activeInHierarchy)
			{
				if (BillboardType == EBillboardType.Billboard)
				{
					Tm.rotation = camera.transform.rotation * LocalRot;
				}
				else if (BillboardType == EBillboardType.VerticalBillboard)
				{
					Tm.rotation = Math.CalcVerticalBillboard(camera) * LocalRot;
				}
			}
		}
	}

	private class BillboardLayerData
	{
		public int Layer;

		public Camera LayerCamera;

		public List<EntryData> List = new List<EntryData>();

		public bool VerifyCamera()
		{
			if (LayerCamera != null && LayerCamera.gameObject.layer != Layer)
			{
				LayerCamera = null;
			}
			if (LayerCamera == null)
			{
				LayerCamera = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(Layer);
				if (LayerCamera == null)
				{
					return false;
				}
			}
			return true;
		}
	}

	private IncreaseDic<int, BillboardLayerData> _dic = new IncreaseDic<int, BillboardLayerData>();

	public EntryData AddBiilboard(EBillboardType type, Transform tm)
	{
		if (tm == null)
		{
			return null;
		}
		return AddBiilboard(type, tm, tm.gameObject.layer);
	}

	public EntryData AddBiilboard(EBillboardType type, Transform tm, int layer)
	{
		if (tm == null)
		{
			return null;
		}
		BillboardLayerData billboardLayerData = _dic.GETVALUE(layer, null);
		if (billboardLayerData == null)
		{
			billboardLayerData = new BillboardLayerData();
			billboardLayerData.Layer = layer;
			_dic.Add(layer, billboardLayerData);
		}
		EntryData entryData = new EntryData();
		entryData.BillboardType = type;
		entryData.Tm = tm;
		entryData.LocalRot = tm.localRotation;
		billboardLayerData.List.Add(entryData);
		return entryData;
	}

	public override void ChainUpdate()
	{
	}

	public void ChainLateUpdate()
	{
		for (int i = 0; i < _dic.Count; i++)
		{
			BillboardLayerData valueByIndex = _dic.GetValueByIndex(i);
			if (!valueByIndex.VerifyCamera())
			{
				_dic.RemoveByIndex(i);
				i--;
				continue;
			}
			for (int j = 0; j < valueByIndex.List.Count; j++)
			{
				EntryData entryData = valueByIndex.List[j];
				if (entryData.Tm == null)
				{
					valueByIndex.List.RemoveAt(j);
					j--;
				}
				else
				{
					entryData.UpdateBillboard(valueByIndex.LayerCamera);
				}
			}
		}
	}
}
