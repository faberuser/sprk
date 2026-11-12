using NGame2.NUI;
using UnityEngine;

namespace NNguiExtension;

public class UIComboBoxItem : MonoBehaviour
{
	public interface Listener
	{
		void OnClickItem(UIComboBoxItem clickItem);
	}

	public UISpriteSelector SpriteSelector;

	public UISpriteSelector IconSpriteSelector;

	public UILabel TitleLabel;

	public UILabelSelector LabelSelector;

	private Listener _Listener;

	public int Key { get; private set; }

	public string Title { get; private set; }

	public void SetData(int key, string title, string spriteSelectorKey, Listener listener)
	{
		_Listener = listener;
		Key = key;
		Title = title;
		UIHelper.SetLabelText(TitleLabel, title);
		if (SpriteSelector != null && !String_Extension_Method.IsNullOrEmpty(spriteSelectorKey))
		{
			SpriteSelector.Select(spriteSelectorKey);
		}
		if (IconSpriteSelector != null)
		{
			IconSpriteSelector.Select(key.ToString());
		}
		if (LabelSelector != null)
		{
			LabelSelector.Select(key.ToString());
			UILabel targetLabel = LabelSelector.GetTargetLabel();
			if (targetLabel != null)
			{
				Title = targetLabel.text;
			}
		}
	}

	public void OnClick()
	{
		if (_Listener != null)
		{
			_Listener.OnClickItem(this);
		}
	}
}
