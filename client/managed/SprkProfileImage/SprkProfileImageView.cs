using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

/// <summary>
/// The "Profile" tab of My Info, below Common and Buff. Common keeps the leader
/// hero and Lil' Raider cards; this tab previews the profile picture and
/// uploads or removes the custom one. Built once per window from the window's
/// own widgets, so it uses the native atlases, fonts and button behaviour.
/// </summary>
public sealed class SprkProfileImageView : MonoBehaviour
{
	public const string TabName = "Button_SprkProfile";

	public const string PanelName = "SprkProfilePanel";

	private const string SelectedSprite = "Mission_Button_02";

	private const string NormalSprite = "Mission_Button_03";

	private static readonly Color SelectedLabel = Color.white;

	private static readonly Color NormalLabel = new Color(0.2745098f, 0.21568628f, 0.13725491f, 1f);

	public UIButton CommonTab;

	public UIButton ProfileTab;

	public GameObject[] CommonContent;

	public GameObject Panel;

	public UIBundleLoader Preview;

	public UILabel Status;

	public UIButton UploadButton;

	public UIButton RemoveButton;

	public static SprkProfileImageView Setup(MyInfoWindow window)
	{
		if (window == null)
		{
			return null;
		}
		Transform transform = window.transform.Find("Panel/Root");
		if (transform == null)
		{
			return null;
		}
		SprkProfileImageView sprkProfileImageView = transform.GetComponent<SprkProfileImageView>();
		if (sprkProfileImageView == null)
		{
			sprkProfileImageView = Build(transform);
			if (sprkProfileImageView == null)
			{
				return null;
			}
		}
		sprkProfileImageView.ShowCommon();
		return sprkProfileImageView;
	}

	private static SprkProfileImageView Build(Transform root)
	{
		Transform transform = root.Find("CategoryRoot");
		Transform transform2 = transform?.Find("Button_Normal");
		Transform transform3 = transform?.Find("Button_Buff");
		Transform transform4 = root.Find("WidgetMyInfo");
		Transform transform5 = root.Find("WidgetAvatar");
		Transform transform6 = root.Find("WidgetMyInfo/Sprite_StatBg");
		Transform transform7 = root.Find("WidgetMyInfo/WidgetMyinfoBox/NickBox/Label_Nick");
		Transform transform8 = root.Find("WidgetAvatar/MainHero/Button_ChangeAvatar");
		Transform transform9 = root.Find("WidgetAvatar/MainHero/Texture_AvatarHero");
		if (transform2 == null || transform3 == null || transform4 == null || transform5 == null || transform6 == null || transform7 == null || transform8 == null || transform9 == null)
		{
			return null;
		}
		SprkProfileImageView view = root.gameObject.AddComponent<SprkProfileImageView>();
		view.CommonTab = transform2.GetComponent<UIButton>();
		view.CommonContent = new GameObject[2] { transform4.gameObject, transform5.gameObject };
		// The tab: a copy of Buff, one row further down.
		GameObject gameObject = Clone(transform3.gameObject, transform.gameObject, TabName);
		gameObject.transform.localPosition = transform3.localPosition * 2f - transform2.localPosition;
		gameObject.SetActive(value: true);
		view.ProfileTab = gameObject.GetComponent<UIButton>();
		SetText(gameObject.transform.Find("Label"), SprkText.Get("Profile"));
		EventDelegate.Set(view.ProfileTab.onClick, view.ShowProfile);
		EventDelegate.Set(view.CommonTab.onClick, view.ShowCommon);
		// The panel, inside the window frame (x -713..709, y -488..368).
		GameObject gameObject2 = NGUITools.AddChild(root.gameObject);
		gameObject2.name = PanelName;
		view.Panel = gameObject2;
		UISprite component = Clone(transform6.gameObject, gameObject2, "Sprite_PreviewBg").GetComponent<UISprite>();
		component.width = 520;
		component.height = 520;
		component.transform.localPosition = new Vector3(-386f, -60f, 0f);
		UITexture component2 = transform9.GetComponent<UITexture>();
		UITexture uITexture = NGUITools.AddWidget<UITexture>(gameObject2, component.depth + 2);
		uITexture.name = "Texture_Preview";
		uITexture.shader = component2.shader;
		uITexture.width = 440;
		uITexture.height = 440;
		uITexture.transform.localPosition = new Vector3(-386f, -60f, 0f);
		view.Preview = uITexture.gameObject.AddComponent<UIBundleLoader>();
		view.Preview.SetUITexture(uITexture);
		Label(transform7, gameObject2, "Label_ProfileTitle", SprkText.Get("Profile Picture"), 36, new Vector3(-40f, 200f, 0f), 700);
		UILabel uILabel = Label(transform7, gameObject2, "Label_ProfileHelp", SprkText.Get("Upload a PNG or JPG image to use as your profile picture. It is cropped to a square. Your leader hero does not change."), 26, Vector3.zero, 700);
		uILabel.pivot = UIWidget.Pivot.TopLeft;
		uILabel.overflowMethod = UILabel.Overflow.ResizeHeight;
		uILabel.transform.localPosition = new Vector3(-40f, 150f, 0f);
		view.Status = Label(transform7, gameObject2, "Label_ProfileStatus", string.Empty, 26, new Vector3(-40f, 30f, 0f), 700);
		view.Status.overflowMethod = UILabel.Overflow.ResizeHeight;
		view.UploadButton = Button(transform8, gameObject2, "Button_UploadPicture", SprkText.Get("Upload Picture"), new Vector3(110f, -250f, 0f), view.OnUpload);
		view.RemoveButton = Button(transform8, gameObject2, "Button_RemovePicture", SprkText.Get("Remove Picture"), new Vector3(420f, -250f, 0f), view.OnRemove);
		gameObject2.SetActive(value: false);
		SprkProfileImage.WatchPreview(view.Preview);
		return view;
	}

	private static GameObject Clone(GameObject source, GameObject parent, string name)
	{
		GameObject gameObject = NGUITools.AddChild(parent, source);
		gameObject.name = name;
		// Copies keep the source's anchors, localization keys and SPRK
		// watchers; drop them so nothing rewrites the copy's text, position or texture.
		UILocalizeEx[] componentsInChildren = gameObject.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate(componentsInChildren[i]);
		}
		MonoBehaviour[] componentsInChildren2 = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren2)
		{
			if (monoBehaviour != null && monoBehaviour.GetType().Name == "SprkProfilePortraitState")
			{
				Object.DestroyImmediate(monoBehaviour);
			}
		}
		UIRect[] componentsInChildren3 = gameObject.GetComponentsInChildren<UIRect>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			if (!(componentsInChildren3[i] is UIPanel))
			{
				componentsInChildren3[i].SetAnchor((Transform)null);
			}
		}
		return gameObject;
	}

	private static void SetText(Transform label, string text)
	{
		UILabel uILabel = ((label != null) ? label.GetComponent<UILabel>() : null);
		if (uILabel != null)
		{
			uILabel.text = text;
		}
	}

	private static UILabel Label(Transform source, GameObject parent, string name, string text, int size, Vector3 position, int width)
	{
		UILabel component = Clone(source.gameObject, parent, name).GetComponent<UILabel>();
		component.pivot = UIWidget.Pivot.Left;
		component.alignment = NGUIText.Alignment.Left;
		component.overflowMethod = UILabel.Overflow.ShrinkContent;
		component.multiLine = true;
		component.maxLineCount = 0;
		component.fontSize = size;
		component.width = width;
		component.height = size + 12;
		component.text = text;
		component.transform.localPosition = position;
		return component;
	}

	private static UIButton Button(Transform source, GameObject parent, string name, string text, Vector3 position, EventDelegate.Callback onClick)
	{
		GameObject gameObject = Clone(source.gameObject, parent, name);
		gameObject.transform.localPosition = position;
		foreach (UILabel componentsInChild in gameObject.GetComponentsInChildren<UILabel>(includeInactive: true))
		{
			componentsInChild.text = text;
		}
		UIButton component = gameObject.GetComponent<UIButton>();
		EventDelegate.Set(component.onClick, onClick);
		return component;
	}

	private void OnEnable()
	{
		SprkProfileImage.Changed += Refresh;
	}

	private void OnDisable()
	{
		SprkProfileImage.Changed -= Refresh;
	}

	private void OnDestroy()
	{
		SprkProfileImage.Unwatch(Preview);
	}

	private static void SelectTab(UIButton tab, bool selected)
	{
		if (tab == null)
		{
			return;
		}
		tab.normalSprite = (selected ? SelectedSprite : NormalSprite);
		UISprite component = tab.GetComponent<UISprite>();
		if (component != null)
		{
			component.spriteName = tab.normalSprite;
		}
		Transform transform = tab.transform.Find("Label");
		UILabel uILabel = ((transform != null) ? transform.GetComponent<UILabel>() : null);
		if (uILabel != null)
		{
			uILabel.color = (selected ? SelectedLabel : NormalLabel);
		}
	}

	public void ShowCommon()
	{
		Show(profile: false);
	}

	public void ShowProfile()
	{
		Show(profile: true);
	}

	public bool ProfileShown
	{
		get
		{
			if (Panel != null)
			{
				return Panel.activeSelf;
			}
			return false;
		}
	}

	private void Show(bool profile)
	{
		GameObject[] commonContent = CommonContent;
		foreach (GameObject gameObject in commonContent)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(!profile);
			}
		}
		if (Panel != null)
		{
			Panel.SetActive(profile);
		}
		SelectTab(CommonTab, !profile);
		SelectTab(ProfileTab, profile);
		Refresh();
	}

	public void Refresh()
	{
		if (Preview != null)
		{
			string text = HeroIconPath();
			if (!string.IsNullOrEmpty(text) && Preview.AssetPath != text)
			{
				Preview.SetAssetPath(text, UIHelper.EAssetTextureAssign.ClipCenter);
			}
		}
		bool busy = SprkProfileImage.Busy;
		bool flag = SprkProfileImage.Picture != null;
		if (Status != null)
		{
			Status.text = (busy ? SprkText.Get(SprkProfileImage.Loaded ? "Uploading..." : "Loading...") : (flag ? SprkText.Get("Your custom picture is shown as your profile picture.") : SprkText.Get("No custom picture: your leader hero's icon is shown.")));
		}
		UIHelper.EnableButton(UploadButton, !busy, grayscaleIfDisable: true);
		UIHelper.EnableButton(RemoveButton, !busy && flag, grayscaleIfDisable: true);
	}

	private static string HeroIconPath()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.UserManager == null)
		{
			return null;
		}
		return UIAvatarHelper.GetIconPath(GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex);
	}

	private void OnUpload()
	{
		if (SprkProfileImage.Busy)
		{
			return;
		}
		if (!SprkProfileImage.CanPickFiles)
		{
			MessagePopup.Open(SprkText.Get("Profile Picture"), SprkText.Get("Uploading pictures is only available in the Windows client. Pictures uploaded there are shown here too."));
			return;
		}
		SprkProfileImage.Upload(Report);
	}

	private void OnRemove()
	{
		if (!SprkProfileImage.Busy && !(SprkProfileImage.Picture == null))
		{
			MessagePopup.Open(SprkText.Get("Profile Picture"), SprkText.Get("Remove your custom profile picture? Your leader hero's icon will be shown again."), delegate
			{
				SprkProfileImage.Remove(Report);
			}, null);
		}
	}

	private static void Report(SprkProfileImage.Outcome outcome)
	{
		switch (outcome)
		{
		case SprkProfileImage.Outcome.Unreadable:
			MessagePopup.Open(SprkText.Get("Profile Picture"), SprkText.Get("Choose a PNG or JPG image (up to 20 MB) at least 16 pixels wide and tall."));
			break;
		case SprkProfileImage.Outcome.Rejected:
			MessagePopup.Open(SprkText.Get("Profile Picture"), SprkText.Get("The server did not confirm the change. Try again later."));
			break;
		}
	}
}
