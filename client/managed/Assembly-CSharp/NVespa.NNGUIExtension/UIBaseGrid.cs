using NVespa.NPrefab2;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIWidget))]
public abstract class UIBaseGrid : MonoBehaviour, IGrid
{
	T IGrid.AddGridItem<T>(PrefabAuto<T> prefab)
	{
		return AddGridItem(prefab);
	}

	void IGrid.ClearGridItem<T>()
	{
		ClearGridItem<T>();
	}

	void IGrid.RemoveGridItem<T>(T comp)
	{
		RemoveGridItem(comp);
	}

	void IGrid.RepositionGrid()
	{
		RepositionGrid();
	}

	void IGrid.ScrollTo(Component comp, float amount, float strength)
	{
		ScrollTo(comp, amount, strength);
	}

	public abstract T AddGridItem<T>(PrefabAuto<T> prefab) where T : Component;

	public abstract void ClearGridItem<T>() where T : Component;

	public abstract void RemoveGridItem<T>(T comp) where T : Component;

	public abstract void RepositionGrid();

	public abstract void ScrollTo(Component comp, float amount, float strength);
}
