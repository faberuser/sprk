using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI;

public class BaseSubComponent<TOwner> : LinkFactoryComponent where TOwner : Component
{
	public UIWidget Root;

	public TOwner Owner { get; private set; }

	public virtual void Open(TOwner owner)
	{
		Owner = owner;
	}

	public virtual void Refresh()
	{
	}

	public void SetRootAnchor(GameObject rootGo)
	{
		Root.SetAnchor(rootGo, 0, 0, 0, 0);
		Root.ForceUpdateAnchors(includeChildren: true);
	}

	public virtual void SetShow(bool show)
	{
		UIHelper.EnableGameObject(Root.gameObject, show);
	}
}
