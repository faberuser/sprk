using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UIWidgetResizer : MonoBehaviour
{
	public UIWidget RootWidget;

	public Transform[] CustomChildren;

	public bool useDefaultChildren = true;

	public bool UseCustomChildren;

	public bool UseResizeWidth = true;

	public bool UseResizeHeight = true;

	public bool OnlyExpand;

	public int Padding = 5;

	private bool _initialized;

	private Vector3 _initialLocalPos;

	private Vector2 _initialSize;

	public void InitRoot()
	{
		RootWidget = GetComponent<UIWidget>();
		if (!(RootWidget == null))
		{
			_initialLocalPos = new Vector3(RootWidget.transform.localPosition.x, RootWidget.transform.localPosition.y, RootWidget.transform.localPosition.z);
			_initialSize = new Vector2(RootWidget.localSize.x, RootWidget.localSize.y);
			_initialized = true;
		}
	}

	public void Reset()
	{
		if (!(RootWidget == null))
		{
			RootWidget.transform.localPosition = _initialLocalPos;
			RootWidget.SetDimensions((int)_initialSize.x, (int)_initialSize.y);
			RootWidget.Invalidate(includeChildren: false);
			RootWidget.ForceUpdateAnchors(includeChildren: true);
		}
	}

	public void Resize()
	{
		if (!_initialized || RootWidget == null)
		{
			InitRoot();
		}
		Reset();
		Bounds rootBounds = default(Bounds);
		if (useDefaultChildren)
		{
			TraversalChildrenRecursivly(RootWidget.transform, ref rootBounds, isRoot: true);
		}
		if (UseCustomChildren)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CustomChildren); i++)
			{
				Transform transform = CustomChildren[i];
				if (!(transform == null))
				{
					UpdateRootBounds(transform, ref rootBounds);
				}
			}
		}
		if (UseResizeWidth)
		{
			RootWidget.width = CalcNewSize((int)rootBounds.size.x, (int)_initialSize.x);
		}
		if (UseResizeHeight)
		{
			RootWidget.height = CalcNewSize((int)rootBounds.size.y, (int)_initialSize.y);
		}
		RootWidget.ForceUpdateAnchors(includeChildren: true);
		RootWidget.Invalidate(includeChildren: true);
	}

	private void OnStart()
	{
		InitRoot();
	}

	private void TraversalChildrenRecursivly(Transform current, ref Bounds rootBounds, bool isRoot = false)
	{
		int i = 0;
		for (int childCount = current.childCount; i < childCount; i++)
		{
			Transform child = current.GetChild(i);
			if (!(child == null))
			{
				TraversalChildrenRecursivly(child, ref rootBounds);
				if (!isRoot)
				{
					UpdateRootBounds(current, ref rootBounds);
				}
			}
		}
	}

	private void UpdateRootBounds(Transform child, ref Bounds rootBound)
	{
		if (!(child == null))
		{
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(RootWidget.transform, child, considerInactive: false);
			rootBound.Encapsulate(bounds);
		}
	}

	private int CalcNewSize(int newSize, int size)
	{
		int num = newSize + Padding * 2;
		if (OnlyExpand)
		{
			num = Mathf.Max(size, num);
		}
		return num;
	}
}
