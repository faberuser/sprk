using System.Collections.Generic;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class TacticComponent : MonoBehaviour
{
	private static Dictionary<string, IPrefabAuto> PrefabAutos = new Dictionary<string, IPrefabAuto>();

	public static PrefabAuto<T> GetPrefabAuto<T>(string componentName) where T : Component
	{
		if (PrefabAutos.ContainsKey(componentName))
		{
			return PrefabAutos[componentName] as PrefabAuto<T>;
		}
		PrefabAuto<T> prefabAuto = new PrefabAuto<T>("Prefab2/Component/Battle/Tactic/" + componentName);
		PrefabAutos.Add(componentName, prefabAuto);
		return prefabAuto;
	}

	public virtual void Open(CampaignTacticData tacticData)
	{
	}
}
