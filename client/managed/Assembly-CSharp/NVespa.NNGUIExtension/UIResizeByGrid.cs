using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIWidget))]
public class UIResizeByGrid : MonoBehaviour, IInspectorGuiDrawer
{
	public UIGrid Grid;

	public Vector2 ExtraSize;

	[HideInInspector]
	public bool IsFixWidth;

	[HideInInspector]
	public bool IsFixHeight;

	private int _lastGridItemCount;

	private void Start()
	{
	}

	private void Update()
	{
		if (!(Grid == null) && _lastGridItemCount != Grid.GetGridChildCount())
		{
			_lastGridItemCount = Grid.GetGridChildCount();
			ResizeWidget(_lastGridItemCount);
		}
	}

	public void Refresh()
	{
		if (!(Grid == null))
		{
			_lastGridItemCount = Grid.GetGridChildCount();
			ResizeWidget(_lastGridItemCount);
		}
	}

	private void ResizeWidget(int gridItemCount)
	{
		UIWidget component = GetComponent<UIWidget>();
		if (!(component == null))
		{
			Vector2 vector = CalcSize(gridItemCount);
			component.width = (IsFixWidth ? component.width : Mathf.RoundToInt(vector.x));
			component.height = (IsFixHeight ? component.height : Mathf.RoundToInt(vector.y));
		}
	}

	public Vector2 GetSize()
	{
		Vector2 result = CalcSize(_lastGridItemCount);
		UIWidget component = GetComponent<UIWidget>();
		if (component == null)
		{
			return result;
		}
		result.x = (IsFixWidth ? component.width : Mathf.RoundToInt(result.x));
		result.y = (IsFixHeight ? component.height : Mathf.RoundToInt(result.y));
		return result;
	}

	private Vector2 CalcSize(int gridItemCount)
	{
		if (Grid == null)
		{
			return ExtraSize;
		}
		if (gridItemCount < 1)
		{
			gridItemCount = 1;
		}
		int num = ((Grid.maxPerLine < 1) ? 9999 : Grid.maxPerLine);
		int num2;
		int num3;
		if (Grid.arrangement == UIGrid.Arrangement.Horizontal)
		{
			num2 = Mathf.Min(gridItemCount, num);
			num3 = (gridItemCount - 1) / num + 1;
		}
		else if (Grid.arrangement == UIGrid.Arrangement.Vertical)
		{
			num3 = Mathf.Min(gridItemCount, num);
			num2 = (gridItemCount - 1) / num + 1;
		}
		else
		{
			num2 = 1;
			num3 = 1;
		}
		return new Vector2(Grid.cellWidth * (float)num2, Grid.cellHeight * (float)num3) + ExtraSize;
	}
}
