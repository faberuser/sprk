using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LobbyLeftMenuAligner : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Lobby/LeftMenuAligner";

	public Transform IconRoot;

	public float MenuOffsetY = -100f;

	public float MenuPositionLerpFactor = 10f;

	private readonly List<AlignedMenu> _menuList = new List<AlignedMenu>();

	private readonly List<Transform> _activatedRootList = new List<Transform>();

	private bool _positionDirty;

	public void Register(AlignedMenu menu)
	{
		if (!_menuList.Any((AlignedMenu e) => e.Root == menu.Root))
		{
			_menuList.Add(menu);
			_menuList.Sort((AlignedMenu l, AlignedMenu r) => l.Order.CompareTo(r.Order));
		}
	}

	public void Register(IAlignedMenuContainer container)
	{
		AlignedMenu[] alignedMenues = container.GetAlignedMenues();
		foreach (AlignedMenu menu in alignedMenues)
		{
			Register(menu);
		}
	}

	public void Clear()
	{
		_menuList.Clear();
		_positionDirty = false;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RemoveInvalidMenu();
		UpdateMenuChanged();
		UpdateMenuPosition();
	}

	private void RemoveInvalidMenu()
	{
		if (_menuList.RemoveAll((AlignedMenu e) => e.Root == null) > 0)
		{
			_positionDirty = true;
		}
		if (_activatedRootList.RemoveAll((Transform e) => e == null) > 0)
		{
			_positionDirty = true;
		}
	}

	private void UpdateMenuChanged()
	{
		if (_positionDirty)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_activatedRootList); i++)
		{
			if (!_activatedRootList[i].gameObject.activeInHierarchy)
			{
				_activatedRootList.RemoveAt(i);
				_positionDirty = true;
				return;
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_menuList); j++)
		{
			AlignedMenu alignedMenu = _menuList[j];
			if (alignedMenu.Root.gameObject.activeInHierarchy && !_activatedRootList.Contains(alignedMenu.Root))
			{
				_activatedRootList.Add(alignedMenu.Root);
				_positionDirty = true;
				break;
			}
		}
	}

	private void UpdateMenuPosition()
	{
		if (!_positionDirty)
		{
			return;
		}
		int num = 0;
		bool flag = true;
		Transform iconRoot = IconRoot;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_menuList); i++)
		{
			AlignedMenu alignedMenu = _menuList[i];
			if (!(alignedMenu.Root == null) && alignedMenu.Root.gameObject.activeInHierarchy)
			{
				int num2 = num++;
				Vector3 position = alignedMenu.Root.position;
				Vector3 vector = iconRoot.worldToLocalMatrix.MultiplyPoint(position);
				Vector3 vector2 = new Vector3(0f, (float)num2 * MenuOffsetY, 0f);
				if (Vector3.Magnitude(vector - vector2) > 0.1f)
				{
					Vector3 point = Vector3.Lerp(vector, vector2, Time.deltaTime * MenuPositionLerpFactor);
					Vector3 position2 = iconRoot.localToWorldMatrix.MultiplyPoint(point);
					alignedMenu.Root.position = position2;
					flag = false;
				}
			}
		}
		if (flag)
		{
			_positionDirty = false;
		}
	}
}
