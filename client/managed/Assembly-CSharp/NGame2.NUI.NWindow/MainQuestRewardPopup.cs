using NShared;

namespace NGame2.NUI.NWindow;

public class MainQuestRewardPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Lobby/MainQuestRewardPopup";

	public UILabel TitleLabel;

	public void Open(int step, int progress)
	{
		Open();
		Init(step, progress);
	}

	private void Init(int step, int progress)
	{
		MainQuestData dataByStep = GameRoot.Instance.MainQuestDataContainer.GetDataByStep(step);
		if (dataByStep != null)
		{
			SetTitleLabel(dataByStep.Title);
		}
	}

	private void SetTitleLabel(string title)
	{
		if (!(TitleLabel == null))
		{
			UIHelper.SetLabelText(TitleLabel, title);
		}
	}

	public void OnClickView()
	{
		Close();
	}
}
