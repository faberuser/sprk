using NVespa.NPrefab2;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public interface IGrid
{
	T AddGridItem<T>(PrefabAuto<T> prefab) where T : Component;

	void ClearGridItem<T>() where T : Component;

	void RemoveGridItem<T>(T comp) where T : Component;

	void RepositionGrid();

	void ScrollTo(Component comp, float amount, float strength);
}
