using System.Collections;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.ExtendMenu;

public class ExtendMenu : UIScrollItemBehaviour, ExtendMenuComponent.IListener
{
	public new interface IListener
	{
		void Click(int mainIndex, ExtendMenuComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/ExtendMenu/ExtendMenu";

	public static PrefabAuto<ExtendMenu> PrefabAuto = new PrefabAuto<ExtendMenu>("Prefab2/Component/ExtendMenu/ExtendMenu");

	private ExtendMenuRoot _root;

	private UITable _table;

	private IListener _iListener;

	private bool _isExtend;

	private bool _isForce;

	public ExtendMenuComponent MainExtendMenuComponent { get; private set; }

	public string LabelText { get; set; }

	public int Index { get; private set; }

	public void Open(ExtendMenuRoot root)
	{
		Init(root);
	}

	private void Init(ExtendMenuRoot root)
	{
		_root = root;
		_iListener = _root;
		_table = GetComponent<UITable>();
		_isExtend = true;
		MainExtendMenuComponent = null;
		Index = _root.ExtendMenuCount;
		InitIsForce();
		InitChild();
		SetLabel();
	}

	private void InitIsForce()
	{
		if (_root == null)
		{
			_isForce = false;
		}
		else
		{
			_isForce = Index == _root.GetBaseExtendMenuIndex();
		}
	}

	private void InitChild()
	{
		Transform transform = null;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			transform = base.transform.GetChild(i);
			if (!(transform == null))
			{
				InitExtendMenu(transform);
				InitExtendMenuComponent(transform, i);
			}
		}
		EnableChild(!_isExtend);
	}

	private void InitExtendMenu(Transform transform)
	{
		ExtendMenu component = transform.GetComponent<ExtendMenu>();
		if (!(component == null))
		{
			if (_root != null)
			{
				ExtendMenuRoot root = _root;
				int extendMenuCount = root.ExtendMenuCount + 1;
				root.ExtendMenuCount = extendMenuCount;
			}
			component.Open(_root);
		}
	}

	private void InitExtendMenuComponent(Transform transform, int index)
	{
		ExtendMenuComponent component = transform.GetComponent<ExtendMenuComponent>();
		if (!(component == null))
		{
			if (index == 0)
			{
				MainExtendMenuComponent = component;
			}
			component.Open(index);
		}
	}

	public void AddChild(Transform childTransform)
	{
		if (!(base.transform == null) && !(childTransform == null))
		{
			childTransform.parent = base.transform;
			RepositionTable();
		}
	}

	public void RepositionTable()
	{
		if (!(_table == null))
		{
			_table.Reposition();
			_table.repositionNow = true;
			ParetRepostionTable();
		}
	}

	private void ParetRepostionTable()
	{
		Transform parent = base.transform.parent;
		if (!(parent == null))
		{
			ExtendMenu component = parent.GetComponent<ExtendMenu>();
			if (!(component == null))
			{
				component.RepositionTable();
			}
		}
	}

	private void SetIsExtend(bool isEnable)
	{
		_isExtend = _isForce || isEnable;
	}

	public void EnableChild(bool isEnable)
	{
		if (_root == null)
		{
			return;
		}
		SetIsExtend(isEnable);
		Transform transform = null;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			transform = base.transform.GetChild(i);
			if (!(transform == null))
			{
				EnableExtendMenu(transform);
				EnableExtendMenuComponent(transform);
			}
		}
		EnableExtendIndicator();
		RepositionTable();
	}

	private IEnumerator SelectSubMenu()
	{
		yield return new WaitForSeconds(0.05f);
		ExtendMenuComponent firstComponent = null;
		bool isSelect = false;
		int index = 0;
		while (index < base.transform.childCount)
		{
			Transform child = base.transform.GetChild(index);
			if (!(child == null))
			{
				ExtendMenuComponent component = child.GetComponent<ExtendMenuComponent>();
				if (component == null)
				{
					yield return null;
				}
				if (component.Index != _root.GetBaseExtendMenuIndex())
				{
					if (component.IsSelect)
					{
						isSelect = true;
						break;
					}
					if (component.Index == 1)
					{
						firstComponent = component;
					}
				}
			}
			int num = index + 1;
			index = num;
		}
		if (firstComponent != null && !isSelect && _isExtend)
		{
			firstComponent.SelectMenu(isSelect: true);
			firstComponent.OnClick();
			yield return null;
		}
	}

	private void EnableExtendMenu(Transform transform)
	{
		ExtendMenu component = transform.GetComponent<ExtendMenu>();
		if (!(component == null) && component.Index != _root.GetBaseExtendMenuIndex())
		{
			UIHelper.EnableGameObject(transform, _isExtend);
		}
	}

	private void EnableExtendMenuComponent(Transform transform)
	{
		ExtendMenuComponent component = transform.GetComponent<ExtendMenuComponent>();
		if (!(component == null) && component.Index != _root.GetBaseExtendMenuIndex())
		{
			UIHelper.EnableGameObject(transform, _isExtend);
		}
	}

	private void EnableExtendIndicator()
	{
		if (!(MainExtendMenuComponent == null) && Index != 0 && base.transform.childCount > 1)
		{
			if (_isExtend)
			{
				MainExtendMenuComponent.EnableExtendIndicator();
			}
			else
			{
				MainExtendMenuComponent.EnableNormalIndicator();
			}
		}
	}

	public void SetLabel()
	{
		if (!(MainExtendMenuComponent == null))
		{
			MainExtendMenuComponent.LabelText = LabelText;
			MainExtendMenuComponent.SetLabel();
		}
	}

	public void SetEnablNew(bool isEnable)
	{
		if (!(MainExtendMenuComponent == null))
		{
			MainExtendMenuComponent.SetEnableNew(isEnable);
		}
	}

	public void SetEnableEvent(bool isEnable)
	{
		if (!(MainExtendMenuComponent == null))
		{
			MainExtendMenuComponent.SetEnableEvent(isEnable);
		}
	}

	public void SetEnableBooster(bool isEnable)
	{
		if (!(MainExtendMenuComponent == null))
		{
			MainExtendMenuComponent.SetEnableBooster(isEnable);
		}
	}

	void ExtendMenuComponent.IListener.Click(ExtendMenuComponent component)
	{
		if (!(MainExtendMenuComponent == null) && !(component == null))
		{
			if (component.Index == MainExtendMenuComponent.Index)
			{
				EnableChild(!_isExtend);
				VespaCoroutine.Start(SelectSubMenu());
			}
			RepositionTable();
			if ((base.transform.childCount <= 1 || component.Index != 0) && _iListener != null)
			{
				_iListener.Click(Index, component);
			}
		}
	}
}
