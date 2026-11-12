using System;

namespace NVespa.NPrefab2;

public struct PrefabKey
{
	public bool IsGlobal;

	public bool IsPooling;

	public bool TouchMaterial;

	public EPrefabType PrefabType;

	public Type Type;

	public string Path;
}
