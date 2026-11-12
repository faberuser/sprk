using NVespa.NPrefab2;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UIScrollItemBehaviour : LinkFactoryComponent, IGridItem, IReuseItem, IReuseItemListenerSetter, LinkFactoryComponent.IReleaseEvent, IInspectorGuiDrawer
{
	public interface IListener
	{
		void ChangedItemIndex(Component comp, int index);
	}

	[HideInInspector]
	public UIDragScrollView DragScrollView;

	private IListener _reuseListener;

	public void AddReleaseEvent()
	{
		AddReleaseEvent(this);
	}

	public void SetupReuseItemListener(IListener listener)
	{
		_reuseListener = listener;
		if (_reuseListener != null)
		{
			AddReleaseEvent();
		}
	}

	public void SetReuseItemIndex(int index)
	{
		if (_reuseListener != null)
		{
			_reuseListener.ChangedItemIndex(this, index);
		}
	}

	public virtual void OnRelease()
	{
		if (_reuseListener != null)
		{
			_reuseListener = null;
		}
	}

	UIDragScrollView IGridItem.GetDragScrollView()
	{
		return DragScrollView;
	}
}
