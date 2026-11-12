using System;
using System.Collections.Generic;
using NGame2.NUI;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public class WorldMapPairEffect : MonoBehaviour
{
	[Serializable]
	public class PairEffectData
	{
		public PairEffectType Type;

		public UIGame3DWidget Widget;
	}

	public const string PrefabPath = "Resources/Prefab2/UIOn3D/WorldMapPairEffect";

	public static PrefabAuto<WorldMapPairEffect> PrefabAuto = new PrefabAuto<WorldMapPairEffect>("Resources/Prefab2/UIOn3D/WorldMapPairEffect");

	public UIPanel Panel;

	public List<PairEffectData> PairEffectDataList = new List<PairEffectData>();

	public static WorldMapPairEffect CreateInstance(Transform parent)
	{
		return PrefabAuto.Factory.Create(parent);
	}

	public static void ReleaseInstance(WorldMapPairEffect instance)
	{
		FactoryBase.ReleaseComponent(instance);
	}

	public void Open(float offset)
	{
		base.transform.localPosition = Vector3.up * offset;
	}

	public void PlayPairEffect(PairEffectType type, Action completeAction)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(PairEffectDataList))
		{
			return;
		}
		foreach (PairEffectData pairEffectData in PairEffectDataList)
		{
			if (pairEffectData != null && !(pairEffectData.Widget == null))
			{
				UIHelper.EnableGameObject(pairEffectData.Widget, pairEffectData.Type == type);
				pairEffectData.Widget.Stop(immediatelyClear: true);
				if (type == pairEffectData.Type && type != PairEffectType.LoopPair)
				{
					pairEffectData.Widget.Play(completeAction);
				}
			}
		}
	}

	public void ChainUpdate()
	{
		if (!(Panel == null))
		{
			Camera cameraByLayer = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(base.gameObject.layer);
			if (!(cameraByLayer == null))
			{
				base.transform.rotation = cameraByLayer.transform.rotation;
			}
		}
	}
}
