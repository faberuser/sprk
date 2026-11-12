using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class MapEdgeComponent : LinkFactoryComponent
{
	public static PrefabAuto<MapEdgeComponent> PrefabAuto = new PrefabAuto<MapEdgeComponent>("Prefab2/WorldMap/EdgeComponent");

	public UIWidget ComponentWidget;

	public void Open(Vector3 localTrans)
	{
		base.gameObject.transform.localPosition = localTrans;
	}
}
