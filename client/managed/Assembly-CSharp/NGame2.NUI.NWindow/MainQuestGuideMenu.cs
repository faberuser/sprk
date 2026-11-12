using NGame2.NAccount;
using NGame2.NQuest;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MainQuestGuideMenu : BaseWindow, IUserObserver<MainQuestInfo>, IUserObserver
{
	public interface IListener
	{
		void Move(string moveTaskType, string[] moveTaskValue);
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/MainQuestGuideMenu";

	public UILabel TitleLabel;

	public UILabel DescLabel;

	public UILabel RequireLabel;

	public int Width;

	private IListener _iListener;

	private MainQuestData _mainQuestData;

	public void Open(IListener iListener)
	{
		base.Open();
		Init(iListener);
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		SetQuestInfo();
	}

	public void SetQuestInfo()
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		MainQuestInfo mainQuestInfo = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.MainQuestInfo;
		if (mainQuestInfo != null)
		{
			MainQuestData dataByStep = GameRoot.Instance.MainQuestDataContainer.GetDataByStep(mainQuestInfo.Step);
			if (dataByStep != null)
			{
				_mainQuestData = dataByStep;
				SetInfoLabel();
				SetRequireLabel();
			}
		}
	}

	private void SetInfoLabel()
	{
		if (_mainQuestData != null)
		{
			if (TitleLabel != null)
			{
				string fitEllipsisString = UIHelper.GetFitEllipsisString(SingletonUpdater<QuestManager>.instance.GetLocalizeQuestTitle(_mainQuestData), Width, TitleLabel, TitleLabel.fontSize, 3);
				UIHelper.SetLabelText(TitleLabel, fitEllipsisString);
			}
			string localizeQuestDesc = SingletonUpdater<QuestManager>.instance.GetLocalizeQuestDesc(_mainQuestData);
			UIHelper.SetLabelText(DescLabel, localizeQuestDesc);
		}
	}

	private void SetRequireLabel()
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		MainQuestInfo mainQuestInfo = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.MainQuestInfo;
		if (mainQuestInfo != null)
		{
			MainQuestData dataByStep = GameRoot.Instance.MainQuestDataContainer.GetDataByStep(mainQuestInfo.Step);
			if (dataByStep != null)
			{
				string text = $"({mainQuestInfo.Progress}/{dataByStep.GetReqProgress()})";
				UIHelper.SetLabelText(RequireLabel, text);
			}
		}
	}

	public void OnClickMove()
	{
		if (_iListener != null && _mainQuestData != null)
		{
			_iListener.Move(_mainQuestData.MoveTaskType, _mainQuestData.MoveTaskValue);
		}
	}

	void IUserObserver<MainQuestInfo>.OnChanged(MainQuestInfo changedInfo)
	{
		SetQuestInfo();
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
