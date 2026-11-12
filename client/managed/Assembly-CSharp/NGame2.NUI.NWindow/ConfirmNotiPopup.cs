using System.Collections;
using NGame2.NQuest;
using NGame2.NScene;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.Mission;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ConfirmNotiPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/ConfirmNotiPopup";

	public UILabel TitleLabel;

	public UILabel DescLabel;

	public UIBundleLoader BackgroundBundleLoader;

	public float DelayTime;

	private static WindowPrefab<ConfirmNotiPopup> _windowPrefab;

	private Coroutine _coAutoClosing;

	private int _questIndex;

	public static bool IsOpenWindow()
	{
		if (_windowPrefab != null)
		{
			return _windowPrefab.IsOpen();
		}
		return false;
	}

	public static void Open(string title, string desc, string path = "", int questIndex = 0)
	{
	}

	private void OpenInternal(string title, string desc, int questIndex = 0)
	{
		Open();
		Init(title, desc, questIndex);
	}

	private void Init(string title, string desc, int questIndex = 0)
	{
		SetTitle(title);
		SetDesc(desc);
		_coAutoClosing = VespaCoroutine.Start(ClosePopup());
		_questIndex = questIndex;
	}

	public void SetBackground(string path)
	{
		if (!(BackgroundBundleLoader == null))
		{
			if (!String_Extension_Method.IsNullOrEmpty(path))
			{
				UIHelper.SetBundleTexture(BackgroundBundleLoader, path);
			}
			UIHelper.EnableGameObject(BackgroundBundleLoader, !String_Extension_Method.IsNullOrEmpty(path));
		}
	}

	private void SetTitle(string text)
	{
		if (!(TitleLabel == null))
		{
			UIHelper.SetLabelText(TitleLabel, text);
		}
	}

	private void SetDesc(string text)
	{
		if (!(DescLabel == null))
		{
			UIHelper.SetLabelText(DescLabel, text);
		}
	}

	private IEnumerator ClosePopup()
	{
		yield return new WaitForSeconds(DelayTime);
		if (!(this == null) && _windowPrefab != null)
		{
			Close();
			_windowPrefab = null;
		}
	}

	public void OnClick()
	{
		CloseImmediately();
		SceneAutoBehaviour<LobbyPopupQueue>.instance.RemoveAllClearedPopup("SubQuest");
		if (!SingletonUpdater<QuestManager>.isValidInstance || GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType != LoadingManager.ESceneType.Lobby || (SceneAutoBehaviour<LobbyPopupQueue>.isValidInstance && !SceneAutoBehaviour<LobbyPopupQueue>.instance.Empty()) || GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<MissionMain>())
		{
			return;
		}
		MissionVisualData missionVisualDataByQuestIndex = GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataByQuestIndex(_questIndex);
		MissionCategoryData missionCategoryData = null;
		if (missionVisualDataByQuestIndex != null)
		{
			missionCategoryData = GameRoot.Instance.MissionCategoryDataContainer.GetData(missionVisualDataByQuestIndex.MainCategoryIndex, missionVisualDataByQuestIndex.SubCategoryIndex);
		}
		if (missionCategoryData == null)
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
			return;
		}
		if (missionCategoryData.IsHeroMission() || missionCategoryData.IsWebEvent())
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(missionCategoryData.MainCategoryIndex);
			return;
		}
		SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(missionCategoryData.MainCategoryIndex, missionCategoryData.SubCategoryIndex, forceOpen: true);
		if (_questIndex > 0)
		{
			MissionComponentData elementData = new MissionComponentData(GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(_questIndex), missionVisualDataByQuestIndex);
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionPopup(elementData);
		}
	}

	private void CloseImmediately()
	{
		if (_coAutoClosing != null)
		{
			VespaCoroutine.Stop(_coAutoClosing);
			_coAutoClosing = null;
		}
		Close();
		_windowPrefab = null;
	}
}
