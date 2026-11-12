using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class BaseHelpComponent : LinkFactoryComponent
{
	public UIScrollView.Movement Movement;

	public UILabel LabelTitle;

	public UILabel LabelDesc;

	public virtual void Open(HelpListPageElement elementData)
	{
		SetLabel(elementData.TitleKey, elementData.DescKey);
	}

	public virtual void Close()
	{
		UIHelper.SetLabelText(LabelTitle, "");
		UIHelper.SetLabelText(LabelDesc, "");
	}

	protected void SetLabel(string titleKey, string descKey)
	{
		string stringDirect = LocalizationManager.GetStringDirect(titleKey);
		UIHelper.SetLabelText(LabelTitle, stringDirect);
		string stringDirect2 = LocalizationManager.GetStringDirect(descKey);
		UIHelper.SetLabelText(LabelDesc, stringDirect2);
	}
}
