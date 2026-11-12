using System;
using System.Collections.Generic;
using NNguiExtension;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class EmoticonChattingPreview : BaseWindow
{
	[Serializable]
	public class EmoticonChattingPreviewComponent
	{
		public Transform ComponentRoot;

		public UILabel MessageLabel;

		public UILabelAligner Aligner;

		public UISprite BG;

		public UISprite Emoticon;

		public UISpriteSelector emoticonSelector;
	}

	public class EmoticonChattingPreviewContainer
	{
		public string senderName;

		public string message;

		public EmotiType emoticon;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Chatting/EmoticonChattingPreview";

	private static WindowPrefab<EmoticonChattingPreview> _windowPrefab = new WindowPrefab<EmoticonChattingPreview>("Resources/Prefab2/Window/Chatting/EmoticonChattingPreview", GameUIRoot.ELayerType.UI);

	public UIPanel Panel;

	public float ShowDuration = 2f;

	public int MessageWidth = 600;

	public int BGMargin = 100;

	private float _openedTime;

	private float _currentAlpha = 1f;

	public EmoticonChattingPreviewComponent[] PreviewComponents;

	private List<EmoticonChattingPreviewContainer> _EmoticonChattingPreviewContainer = new List<EmoticonChattingPreviewContainer>();

	public static void Open(ChatInfo chatInfo, EmotiType emoticon)
	{
		if (chatInfo != null)
		{
			_windowPrefab.instance.OpenInternal(chatInfo, emoticon);
		}
	}

	private void OpenComponent(EmoticonChattingPreviewContainer container, EmoticonChattingPreviewComponent component)
	{
		Color white = Color.white;
		Color black = Color.black;
		SetMessage(container, component, black, white);
		SetEmoticon(container, component);
		if (component.Aligner != null)
		{
			component.Aligner.Align();
		}
		if (component.BG != null)
		{
			component.BG.width = component.Aligner.TotalWidth + BGMargin;
		}
		if (component.Emoticon != null)
		{
			float i_value = (float)(-(component.Aligner.TotalWidth + component.Emoticon.width)) / 2f;
			component.Emoticon.transform.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceX(component.Emoticon.transform.localPosition, i_value);
		}
	}

	private void SetMessage(EmoticonChattingPreviewContainer container, EmoticonChattingPreviewComponent component, Color outlineColor, Color chatColor)
	{
		string fitEllipsisString = UIHelper.GetFitEllipsisString(container.message, MessageWidth, component.MessageLabel, component.MessageLabel.fontSize);
		UIHelper.SetLabelText(component.MessageLabel, fitEllipsisString);
	}

	private void SetEmoticon(EmoticonChattingPreviewContainer container, EmoticonChattingPreviewComponent component)
	{
		if (!(component.emoticonSelector == null))
		{
			component.emoticonSelector.Select(container.emoticon.ToString());
		}
	}

	protected void OpenInternal(ChatInfo chatInfo, EmotiType emoticonType)
	{
		base.Open();
		bool isSystem = chatInfo.IsSystem;
		string senderName = chatInfo.SenderName;
		string text = chatInfo.Chat;
		if (!isSystem)
		{
			text = NGUIText.StripSymbols(text);
		}
		_EmoticonChattingPreviewContainer.Add(new EmoticonChattingPreviewContainer
		{
			senderName = senderName,
			message = text,
			emoticon = emoticonType
		});
		while (_EmoticonChattingPreviewContainer.Count > PreviewComponents.Length)
		{
			_EmoticonChattingPreviewContainer.RemoveAt(0);
		}
		for (int i = 0; i < _EmoticonChattingPreviewContainer.Count; i++)
		{
			UIHelper.EnableGameObject(PreviewComponents[i].ComponentRoot, enable: true);
			OpenComponent(_EmoticonChattingPreviewContainer[i], PreviewComponents[i]);
		}
		for (int j = _EmoticonChattingPreviewContainer.Count; j < PreviewComponents.Length; j++)
		{
			UIHelper.EnableGameObject(PreviewComponents[j].ComponentRoot, enable: false);
		}
		_openedTime = Time.time;
		if ((bool)Panel)
		{
			Panel.ResetAndUpdateAnchors();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateTimeEnded();
	}

	private void UpdateTimeEnded()
	{
		if (Time.time - _openedTime >= ShowDuration)
		{
			Close();
		}
	}
}
