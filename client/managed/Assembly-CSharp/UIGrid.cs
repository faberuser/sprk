using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NNGUIExtension;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Grid")]
public class UIGrid : UIWidgetContainer
{
	public interface IGridSort
	{
		void Sort(List<Transform> list);
	}

	public delegate void OnReposition();

	public enum Arrangement
	{
		Horizontal,
		Vertical,
		CellSnap
	}

	public enum Sorting
	{
		None,
		Alphabetic,
		Horizontal,
		Vertical,
		Custom,
		GridSortInterface
	}

	public Arrangement arrangement;

	public Sorting sorting;

	public UIWidget.Pivot pivot;

	public int maxPerLine;

	public float cellWidth = 200f;

	public float cellHeight = 200f;

	public bool animateSmoothly;

	public bool hideInactive;

	public bool keepWithinPanel;

	public OnReposition onReposition;

	public Comparison<Transform> onCustomSort;

	public IGridSort gridSort;

	public bool UseGridAnimation;

	[HideInInspector]
	[SerializeField]
	private bool sorted;

	protected bool mReposition;

	protected UIPanel mPanel;

	protected bool mInitDone;

	private List<Transform> _cachedChildList = new List<Transform>();

	private List<SpringPosition> _springPositionList = new List<SpringPosition>();

	private int _repositionRequestFrame = -1;

	private static List<Transform> _tempListInReposition;

	public UIGridAnimation GridAnimation => SingletonAuto<LinkedTransform<UIGridAnimation>>.instance.Get(base.transform);

	public bool repositionNow
	{
		set
		{
			if (value)
			{
				mReposition = true;
				base.enabled = true;
			}
		}
	}

	public int GetGridChildCount()
	{
		int num = 0;
		Transform transform = base.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!hideInactive || ((bool)child && NGUITools.GetActive(child.gameObject)))
			{
				num++;
			}
		}
		return num;
	}

	public List<Transform> GetCachedChildList()
	{
		_cachedChildList.Clear();
		Transform transform = base.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!hideInactive || ((bool)child && NGUITools.GetActive(child.gameObject)))
			{
				_cachedChildList.Add(child);
			}
		}
		if (sorting != Sorting.None && arrangement != Arrangement.CellSnap)
		{
			if (sorting == Sorting.Alphabetic)
			{
				_cachedChildList.Sort(SortByName);
			}
			else if (sorting == Sorting.Horizontal)
			{
				_cachedChildList.Sort(SortHorizontal);
			}
			else if (sorting == Sorting.Vertical)
			{
				_cachedChildList.Sort(SortVertical);
			}
			else if (sorting == Sorting.GridSortInterface)
			{
				gridSort.SORT(_cachedChildList);
			}
			else if (onCustomSort != null)
			{
				_cachedChildList.Sort(onCustomSort);
			}
		}
		return _cachedChildList;
	}

	public List<Transform> GetChildList()
	{
		List<Transform> list = new List<Transform>();
		GetChildList(list);
		return list;
	}

	public void GetChildList(List<Transform> list)
	{
		Transform transform = base.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!hideInactive || ((bool)child && NGUITools.GetActive(child.gameObject)))
			{
				list.Add(child);
			}
		}
		if (sorting != Sorting.None && arrangement != Arrangement.CellSnap)
		{
			if (sorting == Sorting.Alphabetic)
			{
				list.Sort(SortByName);
			}
			else if (sorting == Sorting.Horizontal)
			{
				list.Sort(SortHorizontal);
			}
			else if (sorting == Sorting.Vertical)
			{
				list.Sort(SortVertical);
			}
			else if (sorting == Sorting.GridSortInterface)
			{
				gridSort.SORT(list);
			}
			else if (onCustomSort != null)
			{
				list.Sort(onCustomSort);
			}
			else
			{
				Sort(list);
			}
		}
	}

	public List<Transform> GetCustomSortedFirstChildren()
	{
		if (onCustomSort == null)
		{
			return null;
		}
		List<Transform> list = new List<Transform>();
		Transform transform = base.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!hideInactive || ((bool)child && NGUITools.GetActive(child.gameObject)))
			{
				list.Add(child);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		Transform transform2 = list[0];
		List<Transform> list2 = new List<Transform>();
		list2.Add(transform2);
		for (int j = 1; j < list.Count; j++)
		{
			Transform transform3 = list[j];
			int num = onCustomSort(transform2, transform3);
			if (num >= 0)
			{
				if (num > 0)
				{
					transform2 = transform3;
					list2.Clear();
				}
				list2.Add(transform3);
			}
		}
		return list2;
	}

	public UIPanel GetPanel()
	{
		if (Application.isPlaying && !mInitDone && NGUITools.GetActive(base.gameObject))
		{
			Init();
		}
		return mPanel;
	}

	public Transform GetChild(int index)
	{
		List<Transform> childList = GetChildList();
		if (index >= childList.Count)
		{
			return null;
		}
		return childList[index];
	}

	public int GetIndex(Transform trans)
	{
		return GetChildList().IndexOf(trans);
	}

	public void AddChild(Transform trans)
	{
		AddChild(trans, sort: true);
	}

	public void AddChild(Transform trans, bool sort)
	{
		if (trans != null)
		{
			trans.parent = base.transform;
			ResetPosition(GetChildList());
		}
	}

	public void RemoveChild(Transform child, bool destroy = false)
	{
		if (!(child == null) && !(child.parent != base.transform))
		{
			if (destroy)
			{
				UnityEngine.Object.Destroy(child.gameObject);
			}
			else
			{
				child.parent = null;
			}
			ResetPosition(GetChildList());
			Reposition();
		}
	}

	public void RemoveChild(int index, bool destroy = false)
	{
		List<Transform> childList = GetChildList();
		if (index >= 0 && index < childList.Count)
		{
			Transform child = childList[index];
			RemoveChild(child, destroy);
		}
	}

	protected virtual void Init()
	{
		mInitDone = true;
		mPanel = NGUITools.FindInParents<UIPanel>(base.gameObject);
	}

	public void RequestRepositionNextFrame()
	{
		_repositionRequestFrame = Time.frameCount;
		base.enabled = true;
	}

	protected virtual void Start()
	{
		if (!mInitDone)
		{
			Init();
		}
		bool flag = animateSmoothly;
		animateSmoothly = false;
		if (!UseGridAnimation)
		{
			Reposition();
		}
		animateSmoothly = flag;
		base.enabled = false;
		RequestRepositionNextFrame();
	}

	protected virtual void Update()
	{
		if (!UseGridAnimation)
		{
			Reposition();
		}
		base.enabled = false;
	}

	private void LateUpdate()
	{
		if (_repositionRequestFrame >= 0 && Time.frameCount > _repositionRequestFrame)
		{
			Reposition();
			_repositionRequestFrame = -1;
			base.enabled = false;
		}
	}

	private void OnValidate()
	{
		if (!Application.isPlaying && NGUITools.GetActive(this))
		{
			Reposition();
		}
	}

	public static int SortByName(Transform a, Transform b)
	{
		return string.Compare(a.name, b.name);
	}

	public static int SortHorizontal(Transform a, Transform b)
	{
		return a.localPosition.x.CompareTo(b.localPosition.x);
	}

	public static int SortVertical(Transform a, Transform b)
	{
		return b.localPosition.y.CompareTo(a.localPosition.y);
	}

	protected virtual void Sort(List<Transform> list)
	{
	}

	public virtual List<Tuple<Transform, Vector3, Vector3>> CalcReposition()
	{
		if (Application.isPlaying && !mInitDone && NGUITools.GetActive(base.gameObject))
		{
			Init();
		}
		List<Transform> cachedChildList = GetCachedChildList();
		List<Tuple<Transform, Vector3, Vector3>> list = new List<Tuple<Transform, Vector3, Vector3>>();
		for (int i = 0; i < cachedChildList.Count; i++)
		{
			list.Add(Tuple.Create(cachedChildList[i], cachedChildList[i].transform.localPosition, cachedChildList[i].transform.localPosition));
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		_ = base.transform;
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			Tuple<Transform, Vector3, Vector3> tuple = list[j];
			Vector3 item = tuple.Item2;
			float z = item.z;
			if (arrangement == Arrangement.CellSnap)
			{
				if (cellWidth > 0f)
				{
					item.x = Mathf.Round(item.x / cellWidth) * cellWidth;
				}
				if (cellHeight > 0f)
				{
					item.y = Mathf.Round(item.y / cellHeight) * cellHeight;
				}
			}
			else
			{
				item = ((arrangement == Arrangement.Horizontal) ? new Vector3(cellWidth * (float)num, (0f - cellHeight) * (float)num2, z) : new Vector3(cellWidth * (float)num2, (0f - cellHeight) * (float)num, z));
			}
			tuple.Item3 = item;
			num3 = Mathf.Max(num3, num);
			num4 = Mathf.Max(num4, num2);
			if (++num >= maxPerLine && maxPerLine > 0)
			{
				num = 0;
				num2++;
			}
		}
		if (pivot != UIWidget.Pivot.TopLeft)
		{
			Vector2 pivotOffset = NGUIMath.GetPivotOffset(pivot);
			float num5;
			float num6;
			if (arrangement == Arrangement.Horizontal)
			{
				num5 = Mathf.Lerp(0f, (float)num3 * cellWidth, pivotOffset.x);
				num6 = Mathf.Lerp((float)(-num4) * cellHeight, 0f, pivotOffset.y);
			}
			else
			{
				num5 = Mathf.Lerp(0f, (float)num4 * cellWidth, pivotOffset.x);
				num6 = Mathf.Lerp((float)(-num3) * cellHeight, 0f, pivotOffset.y);
			}
			for (int k = 0; k < list.Count; k++)
			{
				Tuple<Transform, Vector3, Vector3> tuple2 = list[k];
				Vector3 item2 = tuple2.Item3;
				item2.x -= num5;
				item2.y -= num6;
				tuple2.Item3 = item2;
			}
		}
		return list;
	}

	[ContextMenu("Execute")]
	public virtual void Reposition()
	{
		if (Application.isPlaying && !mInitDone && NGUITools.GetActive(base.gameObject))
		{
			Init();
		}
		if (sorted)
		{
			sorted = false;
			if (sorting == Sorting.None)
			{
				sorting = Sorting.Alphabetic;
			}
			NGUITools.SetDirty(this);
		}
		List<Transform> list;
		if (_tempListInReposition == null)
		{
			list = new List<Transform>();
		}
		else
		{
			list = _tempListInReposition;
			_tempListInReposition = null;
		}
		GetChildList(list);
		ResetPosition(list);
		list.Clear();
		_tempListInReposition = list;
		if (keepWithinPanel)
		{
			ConstrainWithinPanel();
		}
		if (onReposition != null)
		{
			onReposition();
		}
	}

	public void ConstrainWithinPanel(bool considerInactive = false)
	{
		if (mPanel != null)
		{
			mPanel.ConstrainTargetToBounds(base.transform, immediate: true, considerInactive);
			UIScrollView component = mPanel.GetComponent<UIScrollView>();
			if (component != null)
			{
				component.UpdateScrollbars(recalculateBounds: true);
			}
		}
	}

	public void EnableCustomSortedFirstChildren(bool enable)
	{
		List<Transform> customSortedFirstChildren = GetCustomSortedFirstChildren();
		if (customSortedFirstChildren != null && customSortedFirstChildren.Count > 0)
		{
			for (int i = 0; i < customSortedFirstChildren.Count; i++)
			{
				customSortedFirstChildren[i].gameObject.SetActive(enable);
			}
		}
	}

	public void DisableChildIfNotVisible(bool skipDragged = true)
	{
		UIPanel panel = GetPanel();
		if (hideInactive || panel == null)
		{
			return;
		}
		Vector3 center = new Vector3(panel.finalClipRegion.x, panel.finalClipRegion.y);
		Vector3 size = new Vector3(panel.finalClipRegion.z, panel.finalClipRegion.w);
		Bounds bounds = new Bounds(center, size);
		bounds.extents += new Vector3(cellWidth, cellHeight);
		List<Transform> list = new List<Transform>();
		if (skipDragged)
		{
			List<GameObject> draggedGo = UICamera.GetDraggedGo();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(draggedGo); i++)
			{
				Transform parent = draggedGo[i].transform;
				while (!(parent.parent == null))
				{
					if (parent.parent == base.transform)
					{
						list.Add(parent);
						break;
					}
					parent = parent.parent;
				}
			}
		}
		Transform transform = base.transform;
		for (int j = 0; j < transform.childCount; j++)
		{
			Transform child = transform.GetChild(j);
			if (child == null)
			{
				continue;
			}
			Vector3 point = panel.cachedTransform.worldToLocalMatrix.MultiplyPoint3x4(child.transform.position);
			point.z = 0f;
			bool flag = bounds.Contains(point);
			if (flag != child.gameObject.activeSelf)
			{
				if (list.Contains(child))
				{
					child.gameObject.SetActive(value: true);
				}
				else
				{
					child.gameObject.SetActive(flag);
				}
			}
		}
	}

	[ContextMenu("PlayForwardAnimation")]
	public void PlayForwardAnimation()
	{
		List<Transform> childList = GetChildList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(childList))
		{
			UIGridAnimation gridAnimation = GridAnimation;
			if (gridAnimation != null)
			{
				gridAnimation.PlayForward(childList, arrangement, maxPerLine);
			}
		}
	}

	[ContextMenu("PlayBackwardAnimation")]
	public void PlayBackwardAnimation()
	{
		List<Transform> childList = GetChildList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(childList))
		{
			UIGridAnimation gridAnimation = GridAnimation;
			if (gridAnimation != null)
			{
				gridAnimation.PlayBackward(childList, arrangement, maxPerLine);
			}
		}
	}

	protected virtual void ResetPosition(List<Transform> list)
	{
		mReposition = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		Transform transform = base.transform;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			Transform transform2 = list[i];
			Vector3 vector = transform2.localPosition;
			float z = vector.z;
			if (arrangement == Arrangement.CellSnap)
			{
				if (cellWidth > 0f)
				{
					vector.x = Mathf.Round(vector.x / cellWidth) * cellWidth;
				}
				if (cellHeight > 0f)
				{
					vector.y = Mathf.Round(vector.y / cellHeight) * cellHeight;
				}
			}
			else
			{
				vector = ((arrangement == Arrangement.Horizontal) ? new Vector3(cellWidth * (float)num, (0f - cellHeight) * (float)num2, z) : new Vector3(cellWidth * (float)num2, (0f - cellHeight) * (float)num, z));
			}
			if (animateSmoothly && Application.isPlaying)
			{
				SpringPosition springPosition = SpringPosition.Begin(transform2.gameObject, vector, 15f);
				springPosition.updateScrollView = true;
				springPosition.ignoreTimeScale = true;
			}
			else
			{
				transform2.localPosition = vector;
			}
			num3 = Mathf.Max(num3, num);
			num4 = Mathf.Max(num4, num2);
			if (++num >= maxPerLine && maxPerLine > 0)
			{
				num = 0;
				num2++;
			}
		}
		if (pivot != UIWidget.Pivot.TopLeft)
		{
			Vector2 pivotOffset = NGUIMath.GetPivotOffset(pivot);
			float num5;
			float num6;
			if (arrangement == Arrangement.Horizontal)
			{
				num5 = Mathf.Lerp(0f, (float)num3 * cellWidth, pivotOffset.x);
				num6 = Mathf.Lerp((float)(-num4) * cellHeight, 0f, pivotOffset.y);
			}
			else
			{
				num5 = Mathf.Lerp(0f, (float)num4 * cellWidth, pivotOffset.x);
				num6 = Mathf.Lerp((float)(-num3) * cellHeight, 0f, pivotOffset.y);
			}
			for (int j = 0; j < transform.childCount; j++)
			{
				Transform child = transform.GetChild(j);
				SpringPosition springPosition2 = null;
				if (animateSmoothly)
				{
					springPosition2 = child.GetComponent<SpringPosition>();
				}
				if (springPosition2 != null)
				{
					springPosition2.target.x -= num5;
					springPosition2.target.y -= num6;
					continue;
				}
				Vector3 localPosition = child.localPosition;
				localPosition.x -= num5;
				localPosition.y -= num6;
				child.localPosition = localPosition;
			}
		}
		if (mPanel != null && mPanel.widgetsAreStatic)
		{
			BroadcastMessage("ForceUpdateTransform", SendMessageOptions.DontRequireReceiver);
		}
	}
}
