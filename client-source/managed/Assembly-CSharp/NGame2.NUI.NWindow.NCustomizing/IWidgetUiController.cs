namespace NGame2.NUI.NWindow.NCustomizing;

public interface IWidgetUiController
{
	void SwitchToggle(ToggleType toggleType);

	HeroCostumeView.CostumeButtonInfo[] GetCostumeButtons();

	HeroCostumeView.CostumeInfoUI GetCostumeInfoUI();

	WidgetBase GetCostumeViewWidget(ToggleType toggleType);
}
