using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NUI;
using UnityEngine;

namespace NNguiExtension;

public class UIComboBox : MonoBehaviour, UIComboBoxItem.Listener
{
	[Serializable]
	public class ShowOrHideObjects
	{
		public EShowCondtion Condition;

		public GameObject Go;
	}

	public enum EShowCondtion
	{
		SHOWTRUE,
		SHOWFALSE
	}

	public UISprite SelectedSprite;

	public UILabel SelectedLabel;

	public UIPopupRoot RootWindow;

	public UIGrid Grid;

	public Action<int> OnChanged;

	public UIComboBoxItem Template;

	public UISprite GridBg;

	public ShowOrHideObjects[] ConditionGameObjects;

	private bool needAdjust = true;

	private UIComboBoxItem CurrentSelectItem;

	public const int UNSELECTED_KEY = -1;

	public int paddingX = 16;

	public int paddingY = 32;

	private bool _disableClick;

	private List<UIComboBoxItem> Items = new List<UIComboBoxItem>();

	public int CurrentKey
	{
		get
		{
			if (CurrentSelectItem == null)
			{
				return -1;
			}
			return CurrentSelectItem.Key;
		}
	}

	public int GetItemCount()
	{
		return __Extension_Method_ICollection__.COUNT(Items);
	}

	private void Start()
	{
		if (GridBg != null)
		{
			SetGridBg();
		}
		if (RootWindow != null)
		{
			AddCallBack("OnShowPopupRoot", isShow: true);
			AddCallBack("OnHidePopupRoot", isShow: false);
		}
	}

	private void SetGridBg()
	{
		Grid.onReposition = delegate
		{
			AdjustBg();
		};
	}

	private void AddCallBack(string methodName, bool isShow)
	{
		if (!(RootWindow == null))
		{
			EventDelegate value = new EventDelegate(this, methodName);
			IList list = (isShow ? RootWindow.onShow : RootWindow.onHide);
			if (list != null && !list.Contains(value))
			{
				list.Add(value);
			}
		}
	}

	public virtual void OnClick()
	{
		if (!_disableClick)
		{
			if (RootWindow.IsShow)
			{
				RootWindow.HidePopup();
			}
			else
			{
				RootWindow.ShowPopup();
			}
		}
	}

	public virtual void SetShowObjects(bool isShow)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ConditionGameObjects); i++)
		{
			ShowOrHideObjects showOrHideObjects = ConditionGameObjects[i];
			if (showOrHideObjects != null)
			{
				if (showOrHideObjects.Condition == EShowCondtion.SHOWTRUE)
				{
					UIHelper.EnableGameObject(showOrHideObjects.Go, isShow);
				}
				else if (showOrHideObjects.Condition == EShowCondtion.SHOWFALSE)
				{
					UIHelper.EnableGameObject(showOrHideObjects.Go, !isShow);
				}
			}
		}
	}

	public void AddItem(int key, string title)
	{
		AddItem(key, title, string.Empty);
	}

	public void AddItem(int key, string title, string spriteSelectorKey)
	{
		if (!(Template == null))
		{
			GameObject gameObject = NGUITools.AddChild(Grid.gameObject, Template.gameObject);
			if (!(gameObject == null))
			{
				UIComboBoxItem component = gameObject.GetComponent<UIComboBoxItem>();
				component.SetData(key, title, spriteSelectorKey, this);
				component.gameObject.SetActive(value: true);
				Items.Add(component);
				needAdjust = true;
			}
		}
	}

	public void Reposition()
	{
		if (Grid != null)
		{
			Grid.Reposition();
		}
	}

	public void Clear()
	{
		foreach (UIComboBoxItem item in Items)
		{
			VespaObject.DestroyGo(item);
		}
		Items.Clear();
		SetShowObjects(isShow: false);
	}

	void UIComboBoxItem.Listener.OnClickItem(UIComboBoxItem clickItem)
	{
		OnClickItem(clickItem);
	}

	public bool SelectFirstItem(bool isNotify = true)
	{
		if (Items.Count == 0)
		{
			return false;
		}
		UIComboBoxItem clickItem = Items[0];
		OnClickItem(clickItem, isNotify);
		return true;
	}

	public bool SelectItem(int key, bool isNotify = true)
	{
		foreach (UIComboBoxItem item in Items)
		{
			if (item.Key == key)
			{
				OnClickItem(item, isNotify);
				return true;
			}
		}
		return false;
	}

	private void OnClickItem(UIComboBoxItem clickItem, bool isNotify = true)
	{
		CurrentSelectItem = clickItem;
		if (OnChanged != null && isNotify)
		{
			Vespa_System_Action_Extension_Method.fire(OnChanged, clickItem.Key);
		}
		SetLabelFromSelectedItem(clickItem);
		SetSpriteFromSelectedItem(clickItem);
		RootWindow.HidePopup(immediate: true);
	}

	private void SetLabelFromSelectedItem(UIComboBoxItem clickItem)
	{
		if (SelectedLabel != null)
		{
			SelectedLabel.text = clickItem.Title;
		}
	}

	private void SetSpriteFromSelectedItem(UIComboBoxItem clickItem)
	{
		if (SelectedSprite != null && clickItem.SpriteSelector != null)
		{
			UISprite targetSprite = clickItem.SpriteSelector.GetTargetSprite();
			if (targetSprite != null)
			{
				SelectedSprite.spriteName = targetSprite.spriteName;
			}
		}
	}

	private void AdjustBg()
	{
		if (!needAdjust)
		{
			return;
		}
		UIWidget component = Template.GetComponent<UIWidget>();
		if (component == null || __Extension_Method_ICollection__.COUNT(Items) == 0)
		{
			return;
		}
		Transform transform = null;
		Transform transform2 = null;
		Transform transform3 = null;
		Transform transform4 = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Items); i++)
		{
			if (transform == null)
			{
				transform = Items[i].transform;
			}
			if (transform2 == null)
			{
				transform2 = Items[i].transform;
			}
			if (transform3 == null)
			{
				transform3 = Items[i].transform;
			}
			if (transform4 == null)
			{
				transform4 = Items[i].transform;
			}
			if (transform.transform.localPosition.x > Items[i].transform.localPosition.x)
			{
				transform = Items[i].transform;
			}
			if (transform2.transform.localPosition.x < Items[i].transform.localPosition.x)
			{
				transform2 = Items[i].transform;
			}
			if (transform3.transform.localPosition.y < Items[i].transform.localPosition.y)
			{
				transform3 = Items[i].transform;
			}
			if (transform4.transform.localPosition.y > Items[i].transform.localPosition.y)
			{
				transform4 = Items[i].transform;
			}
		}
		int width = component.width;
		int height = component.height;
		float num = transform2.localPosition.x - transform.localPosition.x;
		float num2 = transform3.localPosition.y - transform4.localPosition.y;
		float x = transform.localPosition.x;
		float y = transform4.localPosition.y;
		GridBg.width = (int)num + width + paddingX;
		GridBg.height = (int)num2 + height + paddingY;
		GridBg.transform.localPosition = new Vector3(x + num * 0.5f, y + num2 * 0.5f);
	}

	public void ShowBg()
	{
		if (!(GridBg == null))
		{
			if (__Extension_Method_ICollection__.COUNT(Items) == 0)
			{
				GridBg.gameObject.SetActive(value: false);
			}
			else
			{
				GridBg.gameObject.SetActive(value: true);
			}
		}
	}

	public void HideBg()
	{
		if ((bool)GridBg)
		{
			GridBg.gameObject.SetActive(value: false);
		}
	}

	private void OnShowPopupRoot()
	{
		ShowBg();
		EnableCollider(this, isEnable: false);
		SetShowObjects(isShow: true);
	}

	private void OnHidePopupRoot()
	{
		HideBg();
		EnableCollider(this, isEnable: true);
		SetShowObjects(isShow: false);
	}

	private void EnableCollider(Component obj, bool isEnable)
	{
		if (!(obj == null))
		{
			UIWidget component = obj.GetComponent<UIWidget>();
			if (component != null && component.isGrayscale)
			{
				isEnable = false;
			}
			BoxCollider component2 = obj.GetComponent<BoxCollider>();
			if (component2 != null)
			{
				component2.enabled = isEnable;
			}
		}
	}

	public void SetDisableClick(bool disableClick)
	{
		_disableClick = disableClick;
	}
}
