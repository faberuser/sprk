using UnityEngine;

namespace NGame2.NUI.NWindow.NCutScene;

public class CutSceneWindow : FrameControlWindow
{
	public interface Listener
	{
		void OnClickWindow();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/CutScene/CutSceneWindow";

	public UIWidget RootWidget;

	public UIWidget FrameRoot;

	public UILabel TitleLabel;

	public UILabel TextLabel;

	public GameObject ClickTweenGo;

	public GameObject TitleBox;

	private Listener _listener;

	public void SetListener(Listener listener)
	{
		_listener = listener;
	}

	public void OpenWindow()
	{
		Open();
		UIHelper.EnableGameObject(FrameRoot, enable: false);
		FrameRoot.alpha = 1f;
	}

	public void BeginDisplay()
	{
		if (ClickTweenGo != null)
		{
			UITweener.PlayReset(ClickTweenGo, forward: true);
		}
	}

	public void SetWindowAlpha(float alpha)
	{
		FrameRoot.alpha = alpha;
	}

	public void SetText(string title, string text, string origText)
	{
		if (String_Extension_Method.IsNullOrEmpty(title) && String_Extension_Method.IsNullOrEmpty(text))
		{
			UIHelper.EnableGameObject(FrameRoot, enable: false);
			return;
		}
		UIHelper.EnableGameObject(FrameRoot, enable: true);
		UIHelper.SetLabelText(TitleLabel, title);
		TextLabel.UpdateNGUIText();
		Vector2 vector = NGUIText.CalculatePrintedSize(origText);
		TextLabel.transform.localPosition = Vector3.left * vector.x * 0.5f;
		UIHelper.SetLabelText(TextLabel, text);
	}

	public void SetText(string title, string text)
	{
		SetText(title, text, text);
	}

	public void HideTitle(bool hideTitle)
	{
		UIHelper.EnableGameObject(TitleBox, !hideTitle);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public void OnClickWindow()
	{
		if (_listener != null)
		{
			_listener.OnClickWindow();
		}
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(RootWidget, show);
	}
}
