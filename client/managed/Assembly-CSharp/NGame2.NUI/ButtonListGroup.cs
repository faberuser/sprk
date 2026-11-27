using System.Collections.Generic;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI;

public class ButtonListGroup : MonoBehaviour, ButtonListComponent.IListener
{
	public interface IListener
	{
		void OnClickButton(string key);
	}

	public class ButtonInfo
	{
		public string Key;

		public string ParentKey;

		public ButtonListComponent Button;

		public Dictionary<string, ButtonListComponent> SubButtonList = new Dictionary<string, ButtonListComponent>();

		public bool IsOpen;

		public bool IsShow;
	}

	public class ButtonMoveInfo
	{
		public float StartY;

		public float EndY;

		public ButtonListComponent Button;
	}

	public static bool IsSkipMove = true;

	public UIScrollView ScrollView;

	public Transform ParentTransform;

	public float padding;

	private Dictionary<string, ButtonInfo> ButtonInfos = new Dictionary<string, ButtonInfo>();

	private string _prevSelectMainKey = "";

	private string _prevSelectSubKey = "";

	private IListener _listener;

	// SPRK: empty widget above the first button, sized to the badge that sticks out of it.
	[System.NonSerialized]
	private UIWidget _sprkTopSpacer;

	public void Init(IListener listener)
	{
		_listener = listener;
	}

	public void AddButton(string key, ButtonListComponent button)
	{
		AddButtonInteranl(key, null, button);
	}

	public void AddButton(string key, string parentKey, ButtonListComponent button)
	{
		AddButtonInteranl(key, parentKey, button);
	}

	public T AddButton<T>(string key, PrefabAuto<T> prefabAuto) where T : ButtonListComponent
	{
		return AddButton(key, null, prefabAuto);
	}

	public T AddButton<T>(string key, string parentKey, PrefabAuto<T> prefabAuto) where T : ButtonListComponent
	{
		T val = prefabAuto.Factory.Create(ParentTransform ?? base.transform);
		AddButtonInteranl(key, parentKey, val);
		return val;
	}

	private void AddButtonInteranl(string key, string parentKey, ButtonListComponent button)
	{
		ButtonInfo value = new ButtonInfo
		{
			Key = key,
			ParentKey = parentKey,
			Button = button
		};
		ButtonInfos.Add(key, value);
		button.Init(this, key);
	}

	public void LinkParent()
	{
		foreach (ButtonInfo value in ButtonInfos.Values)
		{
			if (!String_Extension_Method.IsNullOrEmpty(value.ParentKey) && ButtonInfos.ContainsKey(value.ParentKey))
			{
				ButtonInfos[value.ParentKey].SubButtonList.Add(value.Key, value.Button);
				UIHelper.EnableGameObject(value.Button, enable: false);
			}
		}
	}

	public void OnClickButton(string key)
	{
		ChagneButton(key);
		if (_listener != null)
		{
			_listener.OnClickButton(key);
		}
	}

	public void ChagneButton(string key)
	{
		if (ButtonInfos.ContainsKey(key))
		{
			if (String_Extension_Method.IsNullOrEmpty(ButtonInfos[key].ParentKey))
			{
				OnClickMainButton(key);
			}
			else
			{
				OnClickSubButton(key);
			}
		}
	}

	private void OnClickMainButton(string key)
	{
		ButtonInfo buttonInfo = ButtonInfos[key];
		if (__Extension_Method_ICollection__.COUNT(buttonInfo.SubButtonList) == 0)
		{
			if (_prevSelectMainKey != key)
			{
				bool isOpen = !buttonInfo.IsOpen;
				SetButtonOpen(buttonInfo, isOpen);
			}
			if (!String_Extension_Method.IsNullOrEmpty(_prevSelectSubKey))
			{
				ButtonInfos[_prevSelectSubKey].Button.SetOpen(isOpen: false);
				_prevSelectSubKey = "";
			}
		}
		else
		{
			bool isOpen2 = !buttonInfo.IsOpen;
			if (_prevSelectMainKey == key)
			{
				SetButtonOpen(buttonInfo, isOpen2, isUpdateUI: false);
			}
			else
			{
				SetButtonOpen(buttonInfo, isOpen2);
			}
		}
		if (!String_Extension_Method.IsNullOrEmpty(_prevSelectMainKey) && _prevSelectMainKey != key)
		{
			ButtonInfo buttonInfo2 = ButtonInfos[_prevSelectMainKey];
			SetButtonOpen(buttonInfo2, isOpen: false);
		}
		Reposition();
		_prevSelectMainKey = key;
	}

	private void OnClickSubButton(string key)
	{
		ButtonInfo buttonInfo = ButtonInfos[key];
		if (String_Extension_Method.IsNullOrEmpty(_prevSelectMainKey))
		{
			OnClickMainButton(buttonInfo.ParentKey);
		}
		else if (buttonInfo.ParentKey != _prevSelectMainKey)
		{
			OnClickMainButton(buttonInfo.ParentKey);
		}
		if (!String_Extension_Method.IsNullOrEmpty(_prevSelectSubKey) && _prevSelectSubKey != key)
		{
			ButtonInfos[_prevSelectSubKey].Button.SetOpen(isOpen: false);
		}
		buttonInfo.Button.SetOpen(isOpen: true);
		_prevSelectSubKey = key;
	}

	public void SetButtonOpen(ButtonInfo buttonInfo, bool isOpen, bool isUpdateUI = true)
	{
		OpenSubButtons(buttonInfo, isOpen);
		buttonInfo.IsOpen = isOpen;
		if (isUpdateUI)
		{
			buttonInfo.Button.SetOpen(isOpen);
		}
	}

	private void OpenSubButtons(ButtonInfo buttonInfo, bool isOpen)
	{
		foreach (ButtonListComponent value in buttonInfo.SubButtonList.Values)
		{
			UIHelper.EnableGameObject(value, isOpen);
			if (isOpen)
			{
				float num = CalcHeight(buttonInfo.Button);
				float position = buttonInfo.Button.transform.localPosition.y - num - padding;
				value.SetPosition(position);
			}
		}
	}

	public void ResetPosition()
	{
		if ((bool)ScrollView)
		{
			ScrollView.ResetPosition();
		}
	}

	public void Reposition()
	{
		List<ButtonMoveInfo> list = new List<ButtonMoveInfo>();
		float num = 0f;
		foreach (ButtonInfo value in ButtonInfos.Values)
		{
			ButtonListComponent button = value.Button;
			if (UIHelper.IsEnabledGameObjet(button))
			{
				list.Add(new ButtonMoveInfo
				{
					Button = button,
					StartY = button.transform.localPosition.y,
					EndY = num
				});
				num -= CalcHeight(button) + padding;
			}
		}
		foreach (ButtonMoveInfo item in list)
		{
			item.Button.SetPosition(item.EndY);
		}
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			if (ScrollView != null)
			{
				UpdateTopSpacer((list.Count > 0) ? list[0].Button : null);
				ScrollView.InvalidateBounds();
				if (!String_Extension_Method.IsNullOrEmpty(_prevSelectMainKey))
				{
					ButtonInfo buttonInfo = ButtonInfos[_prevSelectMainKey];
					if (__Extension_Method_ICollection__.COUNT(buttonInfo.SubButtonList) == 0)
					{
						ResetPosition();
					}
					else if (buttonInfo.IsOpen)
					{
						// SPRK: leave room for what sticks out above the button (the shop's
						// FREE/SALE badge), which was clipped when the button sat at the top.
						ScrollTo(buttonInfo.Button, 0f - TopOverflow(buttonInfo.Button), 0f);
					}
					else
					{
						ResetPosition();
					}
				}
			}
		});
	}

	// SPRK: a button's own rect covers its children for layout and scroll bounds, so a
	// badge drawn above the first button (the shop's FREE/SALE tag) lay outside the
	// scrollable area and was clipped at the top. The spacer extends the scroll bounds
	// by that much, so resting and dragged positions keep the badge in view.
	private void UpdateTopSpacer(ButtonListComponent first)
	{
		if (first == null)
		{
			return;
		}
		float overflow = TopOverflow(first);
		if (overflow < 1f)
		{
			if (_sprkTopSpacer != null)
			{
				_sprkTopSpacer.gameObject.SetActive(value: false);
			}
			return;
		}
		if (_sprkTopSpacer == null)
		{
			GameObject gameObject = new GameObject("SprkTopSpacer");
			gameObject.layer = first.gameObject.layer;
			gameObject.transform.SetParent(first.transform.parent, worldPositionStays: false);
			_sprkTopSpacer = gameObject.AddComponent<UIWidget>();
			_sprkTopSpacer.pivot = UIWidget.Pivot.Bottom;
			_sprkTopSpacer.width = 2;
		}
		else if (_sprkTopSpacer.transform.parent != first.transform.parent)
		{
			_sprkTopSpacer.transform.SetParent(first.transform.parent, worldPositionStays: false);
		}
		_sprkTopSpacer.gameObject.SetActive(value: true);
		_sprkTopSpacer.height = Mathf.CeilToInt(overflow);
		Vector3 localPosition = first.transform.localPosition;
		_sprkTopSpacer.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
	}

	// How far the button's visible widgets reach above its own origin (its top edge).
	// Measured per widget: a root widget that covers its children reports only its own rect.
	private static float TopOverflow(MonoBehaviour mono)
	{
		float top = 0f;
		foreach (UIWidget widget in mono.GetComponentsInChildren<UIWidget>())
		{
			if (widget.enabled)
			{
				top = Mathf.Max(top, NGUIMath.CalculateRelativeWidgetBounds(mono.transform, widget.transform).max.y);
			}
		}
		return top;
	}

	private float CalcHeight(MonoBehaviour mono)
	{
		return CalcHeight(mono.transform);
	}

	private float CalcHeight(Transform t)
	{
		return NGUIMath.CalculateRelativeWidgetBounds(t).size.y;
	}

	private void ScrollTo(Component comp, float amount, float strength)
	{
		if (comp == null || ScrollView == null)
		{
			return;
		}
		UIPanel panel = ScrollView.panel;
		UIScrollView scrollView = ScrollView;
		if (!(panel == null) && !(scrollView == null))
		{
			Vector3 vector = -panel.cachedTransform.InverseTransformPoint(comp.transform.position);
			if (!scrollView.canMoveHorizontally)
			{
				vector.x = panel.cachedTransform.localPosition.x;
			}
			if (!scrollView.canMoveVertically)
			{
				vector.y = panel.cachedTransform.localPosition.y;
			}
			if (!Math.Approximately(amount, 0f))
			{
				Vector3 vector2 = ((ScrollView.movement == UIScrollView.Movement.Horizontal) ? Vector3.right : ((ScrollView.movement == UIScrollView.Movement.Vertical) ? Vector3.up : new Vector3(ScrollView.customMovement.x, ScrollView.customMovement.y)));
				vector += vector2 * amount;
			}
			if (Math.Approximately(strength, 0f))
			{
				Vector3 localPosition = panel.cachedGameObject.transform.localPosition;
				Vector3 relative = vector - localPosition;
				ScrollView.MoveRelative(relative);
				ScrollView.RestrictWithinBounds(instant: true);
				ScrollView.UpdatePosition();
			}
			else
			{
				SpringPanel.Begin(panel.cachedGameObject, vector, strength);
			}
		}
	}

	public void Clear()
	{
		ButtonInfos.Clear();
		_prevSelectSubKey = "";
		_prevSelectMainKey = "";
	}
}
