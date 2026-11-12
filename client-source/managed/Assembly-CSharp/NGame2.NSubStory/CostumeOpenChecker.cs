using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NSubStory;

public class CostumeOpenChecker : GlobalAutoBehaviour<CostumeOpenChecker>, IUserObserver<ChapterDungeonInfo>, IUserObserver
{
	private Queue<CostumeData> _queueUnlockCostumeData = new Queue<CostumeData>();

	private List<ICostumeOpenChecker> _openCheckerList = new List<ICostumeOpenChecker>();

	protected override void Awake()
	{
		base.Awake();
		_openCheckerList.Add(new SubStoryCostumeOpenChecker(this));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public bool ExistUnlockCostume()
	{
		return _queueUnlockCostumeData.Count != 0;
	}

	public void OpenUnlockPopup()
	{
		if (ExistUnlockCostume() && GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
		{
			CostumeData costumeData = DequeueUnlockCostumeInQ();
			SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<CostumeOpenPopup>(), delegate(CostumeOpenPopup window)
			{
				window.Open(costumeData, null);
			}, CostumeOpenPopup.CanOpen);
		}
	}

	public CostumeData DequeueUnlockCostumeInQ()
	{
		if (!ExistUnlockCostume())
		{
			return null;
		}
		return _queueUnlockCostumeData.Dequeue();
	}

	public void Enqueue(CostumeData costumeData)
	{
		if (costumeData != null)
		{
			_queueUnlockCostumeData.Enqueue(costumeData);
		}
	}

	void IUserObserver<ChapterDungeonInfo>.OnChanged(ChapterDungeonInfo changedInfo)
	{
		if (changedInfo != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_openCheckerList); i++)
			{
				_openCheckerList[i].OnChanged(changedInfo);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}
}
