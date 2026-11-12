using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NWindow;
using NVespa.NNGUIExtension;
using UnityEngine;

public sealed class SprkLoginPresentationMenu : MonoBehaviour
{
	private sealed class Choice
	{
		public string id;

		public UILabel name;

		public GameObject select;

		public GameObject inUse;
	}

	private static readonly Color Gold = new Color(1f, 0.84f, 0.38f, 1f);

	private GameOptionWindow window;

	private GameObject picker;

	private bool pickerMusic;

	private readonly List<Choice> choices = new List<Choice>();

	private Transform Part(string path)
	{
		return window.transform.Find(path);
	}

	private static void Prepare(GameObject go)
	{
		UILocalize[] componentsInChildren = go.GetComponentsInChildren<UILocalize>(includeInactive: true);
		foreach (UILocalize obj in componentsInChildren)
		{
			obj.enabled = false;
			UnityEngine.Object.Destroy(obj);
		}
		UILocalizeEx[] componentsInChildren2 = go.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
		foreach (UILocalizeEx obj2 in componentsInChildren2)
		{
			obj2.enabled = false;
			UnityEngine.Object.Destroy(obj2);
		}
		UIRect[] componentsInChildren3 = go.GetComponentsInChildren<UIRect>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].SetAnchor((Transform)null);
		}
		UIEventListener[] componentsInChildren4 = go.GetComponentsInChildren<UIEventListener>(includeInactive: true);
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren4[i]);
		}
		UIButton[] componentsInChildren5 = go.GetComponentsInChildren<UIButton>(includeInactive: true);
		for (int i = 0; i < componentsInChildren5.Length; i++)
		{
			componentsInChildren5[i].onClick.Clear();
		}
	}

	private static UISprite NewSprite(string name, Transform parent, UISprite source, int width, int height, int depth)
	{
		GameObject obj = new GameObject(name);
		obj.layer = parent.gameObject.layer;
		obj.transform.SetParent(parent, worldPositionStays: false);
		UISprite uISprite = obj.AddComponent<UISprite>();
		uISprite.atlas = source.atlas;
		uISprite.spriteName = source.spriteName;
		uISprite.type = source.type;
		uISprite.color = source.color;
		uISprite.width = width;
		uISprite.height = height;
		uISprite.depth = depth;
		return uISprite;
	}

	private static void Size(GameObject go, int width, int height)
	{
		UIWidget component = go.GetComponent<UIWidget>();
		if (component != null)
		{
			component.width = width;
			component.height = height;
		}
		BoxCollider component2 = go.GetComponent<BoxCollider>();
		if (component2 != null)
		{
			component2.size = new Vector3(width, height, 1f);
			component2.center = Vector3.zero;
		}
	}

	private static void OnClick(UIButton button, Action click)
	{
		button.onClick.Clear();
		EventDelegate.Add(button.onClick, delegate
		{
			click();
		});
	}

	private UIButton Row(string rowName, string buttonName, string title, string value, float y, Action click)
	{
		Transform transform = Part("Panel/Root/EtcOption/EtcInfoGrid/WidgetAccountId");
		Transform transform2 = base.transform.Find(rowName);
		GameObject gameObject;
		UILabel[] componentsInChildren;
		if (transform2 != null)
		{
			gameObject = transform2.gameObject;
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(transform.gameObject, base.transform, worldPositionStays: false);
			gameObject.name = rowName;
			Prepare(gameObject);
			componentsInChildren = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
			foreach (UILabel uILabel in componentsInChildren)
			{
				if (uILabel != null && uILabel.name.EndsWith("Login"))
				{
					UnityEngine.Object.DestroyImmediate(uILabel.gameObject);
				}
			}
			gameObject.transform.Find("Sprite_Back/AccountIdCopyButton").name = buttonName;
		}
		gameObject.transform.localPosition = new Vector3(0f, y, 0f);
		Transform obj = gameObject.transform.Find("Sprite_Back");
		UILabel component = obj.Find("Label_AccountId").GetComponent<UILabel>();
		component.text = title;
		component.overflowMethod = UILabel.Overflow.ResizeFreely;
		UILabel component2 = obj.Find("Label_AccountIdNum").GetComponent<UILabel>();
		component2.text = value;
		component2.overflowMethod = UILabel.Overflow.ShrinkContent;
		component2.width = 720;
		component2.height = 40;
		Transform transform3 = obj.Find(buttonName);
		transform3.localPosition = new Vector3(570f, 0f, 0f);
		componentsInChildren = transform3.GetComponentsInChildren<UILabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].text = "Change";
		}
		UIButton component3 = transform3.GetComponent<UIButton>();
		OnClick(component3, click);
		gameObject.SetActive(value: true);
		return component3;
	}

	public void Setup(GameOptionWindow view)
	{
		window = view;
		if (!(Part("Panel/Root/EtcOption/EtcInfoGrid/WidgetAccountId") == null))
		{
			Row("WidgetLoginBackground", "Button_LoginBackground", "Login Background", SprkLoginPresentation.BackgroundCaption, 40f, delegate
			{
				OpenPicker(isMusic: false);
			});
			Row("WidgetLoginMusic", "Button_LoginMusic", "Login Music", SprkLoginPresentation.MusicCaption, -68f, delegate
			{
				OpenPicker(isMusic: true);
			});
		}
	}

	private void ClosePicker()
	{
		if (picker != null)
		{
			UnityEngine.Object.DestroyImmediate(picker);
		}
		picker = null;
		choices.Clear();
	}

	private void OpenPicker(bool isMusic)
	{
		ClosePicker();
		pickerMusic = isMusic;
		picker = new GameObject(isMusic ? "SPRK Login Music Picker" : "SPRK Login Background Picker");
		picker.layer = base.gameObject.layer;
		picker.transform.SetParent(Part("Panel/Root"), worldPositionStays: false);
		UIPanel uIPanel = picker.AddComponent<UIPanel>();
		uIPanel.depth = window.GetComponentsInChildren<UIPanel>(includeInactive: true).Max((UIPanel p) => p.depth) + 20;
		UISprite component = Part("Panel/Outside_ClickClose").GetComponent<UISprite>();
		UISprite uISprite = NewSprite("Dim", picker.transform, component, 4000, 4000, 0);
		uISprite.color = new Color(0f, 0f, 0f, 0.55f);
		NGUITools.AddWidgetCollider(uISprite.gameObject);
		UIEventListener.Get(uISprite.gameObject).onClick = delegate
		{
			ClosePicker();
		};
		NGUITools.AddWidgetCollider(NewSprite("Body", picker.transform, Part("Panel/Root/Sprite_Frame_a").GetComponent<UISprite>(), 1140, 840, 1).gameObject);
		NewSprite("Frame", picker.transform, Part("Panel/Root/Sprite_Frame").GetComponent<UISprite>(), 1140, 840, 40);
		GameObject obj = UnityEngine.Object.Instantiate(Part("Panel/Root/Sprite_Title").gameObject, picker.transform, worldPositionStays: false);
		obj.name = "Title";
		Prepare(obj);
		obj.transform.localPosition = new Vector3(0f, 355f, 0f);
		obj.GetComponent<UISprite>().width = 920;
		obj.GetComponent<UISprite>().depth = 41;
		UILabel component2 = obj.transform.Find("Label_Title").GetComponent<UILabel>();
		component2.text = (isMusic ? "Login Music" : "Login Background");
		component2.depth = 42;
		Transform obj2 = obj.transform.Find("Button_Exit");
		obj2.name = "Close";
		obj2.localPosition = new Vector3(510f, 0f, 0f);
		obj2.GetComponent<UISprite>().depth = 43;
		OnClick(obj2.GetComponent<UIButton>(), ClosePicker);
		GameObject gameObject = new GameObject("Choices");
		gameObject.layer = base.gameObject.layer;
		gameObject.transform.SetParent(picker.transform, worldPositionStays: false);
		gameObject.transform.localPosition = new Vector3(0f, -60f, 0f);
		UIPanel uIPanel2 = gameObject.AddComponent<UIPanel>();
		uIPanel2.depth = uIPanel.depth + 1;
		uIPanel2.clipping = UIDrawCall.Clipping.SoftClip;
		uIPanel2.baseClipRegion = new Vector4(0f, 0f, 1040f, 590f);
		uIPanel2.clipSoftness = new Vector2(0f, 16f);
		UIScrollView uIScrollView = gameObject.AddComponent<UIScrollView>();
		uIScrollView.movement = UIScrollView.Movement.Vertical;
		uIScrollView.dragEffect = UIScrollView.DragEffect.MomentumAndSpring;
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		if (isMusic)
		{
			list.AddRange(SprkLoginPresentation.Options.music.Select((SprkLoginPresentation.Music x) => new KeyValuePair<string, string>(x.id, x.label)));
		}
		else
		{
			list.AddRange(SprkLoginPresentation.Options.backgrounds.Select((SprkLoginPresentation.Background x) => new KeyValuePair<string, string>(x.id, x.label)));
		}
		Transform transform = Part("Panel/Root/EtcOption/EtcInfoGrid/WidgetVersion");
		Transform transform2 = Part("Panel/Root/EtcOption/EtcInfoGrid/WidgetAccountId/Sprite_Back/AccountIdCopyButton");
		float num = 245f;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			KeyValuePair<string, string> option = list[num2];
			Action click = delegate
			{
				Pick(option.Key);
			};
			GameObject obj3 = UnityEngine.Object.Instantiate(transform.gameObject, gameObject.transform, worldPositionStays: false);
			obj3.name = "Choice_" + num2;
			Prepare(obj3);
			obj3.transform.localPosition = new Vector3(0f, num - (float)(num2 * 100), 0f);
			Size(obj3, 1000, 88);
			Transform transform3 = obj3.transform.Find("Sprite_Back");
			transform3.localPosition = Vector3.zero;
			Size(transform3.gameObject, 1000, 88);
			UILabel component3 = transform3.Find("Label_Version").GetComponent<UILabel>();
			component3.name = "Label_Name";
			component3.text = option.Value;
			component3.transform.localPosition = new Vector3(-464f, 0f, 0f);
			component3.overflowMethod = UILabel.Overflow.ShrinkContent;
			component3.width = 620;
			component3.height = 44;
			transform3.Find("Label_VersionNum").gameObject.SetActive(value: false);
			NGUITools.AddWidgetCollider(obj3);
			UIButton uIButton = obj3.AddComponent<UIButton>();
			uIButton.tweenTarget = null;
			OnClick(uIButton, click);
			obj3.AddComponent<UIDragScrollView>().scrollView = uIScrollView;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(transform2.gameObject, transform3, worldPositionStays: false);
			gameObject2.name = "Select";
			Prepare(gameObject2);
			gameObject2.transform.localPosition = new Vector3(350f, 0f, 0f);
			Size(gameObject2, 240, 72);
			UILabel[] componentsInChildren = gameObject2.GetComponentsInChildren<UILabel>(includeInactive: true);
			for (int num3 = 0; num3 < componentsInChildren.Length; num3++)
			{
				componentsInChildren[num3].text = "Select";
			}
			OnClick(gameObject2.GetComponent<UIButton>(), click);
			gameObject2.AddComponent<UIDragScrollView>().scrollView = uIScrollView;
			GameObject gameObject3 = UnityEngine.Object.Instantiate(component3.gameObject, transform3, worldPositionStays: false);
			gameObject3.name = "Label_InUse";
			UILabel component4 = gameObject3.GetComponent<UILabel>();
			component4.text = "In Use";
			component4.pivot = UIWidget.Pivot.Center;
			component4.width = 240;
			gameObject3.transform.localPosition = new Vector3(350f, 0f, 0f);
			choices.Add(new Choice
			{
				id = option.Key,
				name = component3,
				select = gameObject2,
				inUse = gameObject3
			});
		}
		RefreshChoices();
		uIScrollView.ResetPosition();
		int num4 = choices.FindIndex((Choice c) => c.inUse.activeSelf);
		int num5 = 5;
		if (num4 >= num5 && list.Count > num5)
		{
			uIScrollView.SetDragAmount(0f, Mathf.Clamp01(((float)num4 - (float)num5 / 2f + 0.5f) / (float)(list.Count - num5)), updateScrollbars: false);
		}
	}

	private void RefreshChoices()
	{
		string text = (pickerMusic ? SprkLoginPresentation.SelectedMusic : SprkLoginPresentation.SelectedBackground);
		foreach (Choice choice in choices)
		{
			bool flag = choice.id == text;
			choice.select.SetActive(!flag);
			choice.inUse.SetActive(flag);
			choice.name.color = (flag ? Gold : Color.white);
			choice.inUse.GetComponent<UILabel>().color = Gold;
		}
	}

	private void Pick(string id)
	{
		if (pickerMusic)
		{
			SprkLoginPresentation.SelectMusic(id);
		}
		else
		{
			SprkLoginPresentation.SelectBackground(id);
		}
		RefreshChoices();
		Setup(window);
	}

	private void OnDisable()
	{
		ClosePicker();
	}

	private void OnDestroy()
	{
		ClosePicker();
	}
}
