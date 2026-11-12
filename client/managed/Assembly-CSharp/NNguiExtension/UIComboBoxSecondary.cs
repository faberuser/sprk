using NGame2.NUI;

namespace NNguiExtension;

public class UIComboBoxSecondary : UIComboBox
{
	public UIComboBox primaryComboBox;

	public int primaryComboBoxItemKey = -1;

	public string primaryComboBoxItemLabel = string.Empty;

	public override void OnClick()
	{
		Close();
	}

	public void Close()
	{
		UIHelper.EnableGameObject(primaryComboBox, enable: true);
		primaryComboBox.OnClick();
		UIHelper.EnableGameObject(this, enable: false);
	}

	public void Open()
	{
		UIHelper.EnableGameObject(primaryComboBox, enable: false);
		UIHelper.EnableGameObject(this, enable: true);
		SelectedLabel.text = primaryComboBoxItemLabel;
		RootWindow.ShowPopup();
	}
}
