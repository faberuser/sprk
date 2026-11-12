using NShared;

namespace NGame2.NUI.NComponent2;

public class ArchiveItemInfoNormal : ArchiveItemInfoBase
{
	public UILabel LabelName;

	protected override void Init(ItemData itemData)
	{
		UIHelper.SetLabelText(LabelName, itemData.Name);
	}
}
