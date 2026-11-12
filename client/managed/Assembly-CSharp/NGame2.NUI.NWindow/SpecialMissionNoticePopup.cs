using NGame2.NCtr;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NShared;
using NShared.SendClickThroughRate;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class SpecialMissionNoticePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/QuestIntegration/SpecialMissionNoticePopup";

	private static WindowPrefab<SpecialMissionNoticePopup> _windowPrefab;

	public UILabel LabelTitle;

	public UILabel LabelDesc;

	public UIBundleLoader BundleTexture;

	private MissionCategoryData _missionCategoryData;

	public static void OpenPopup(MissionCategoryData data)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new WindowPrefab<SpecialMissionNoticePopup>("Resources/Prefab2/Window/QuestIntegration/SpecialMissionNoticePopup", GameUIRoot.ELayerType.UI);
		}
		if (_windowPrefab != null && !_windowPrefab.IsOpen())
		{
			_windowPrefab.instance.OpenInternal(data);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
		}
	}

	public void OpenInternal(MissionCategoryData data)
	{
		_missionCategoryData = data;
		Open(delegate
		{
			InitPopup(data);
		}, null);
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickNextMidNight()
	{
		if (_missionCategoryData == null)
		{
			Close();
			return;
		}
		SendCtr(EClickTag.FailType1);
		SceneAutoBehaviour<MissionManagement>.instance.SetSpecialMissionPopupNextOpenTime(isNextMidnight: true, _missionCategoryData.MainCategoryIndex, _missionCategoryData.SubCategoryIndex);
		Close();
	}

	public void OnClickClose()
	{
		if (_missionCategoryData == null)
		{
			Close();
			return;
		}
		SendCtr(EClickTag.FailType2);
		SceneAutoBehaviour<MissionManagement>.instance.SetSpecialMissionPopupNextOpenTime(isNextMidnight: false, _missionCategoryData.MainCategoryIndex, _missionCategoryData.SubCategoryIndex);
		Close();
	}

	public void OnClickShowMission()
	{
		if (_missionCategoryData == null)
		{
			Close();
			return;
		}
		SendCtr(EClickTag.Success);
		SceneAutoBehaviour<MissionManagement>.instance.SetSpecialMissionPopupNextOpenTime(isNextMidnight: false, _missionCategoryData.MainCategoryIndex, _missionCategoryData.SubCategoryIndex);
		Close();
		SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(_missionCategoryData.MainCategoryIndex, _missionCategoryData.SubCategoryIndex, forceOpen: true);
	}

	private void InitPopup(MissionCategoryData data)
	{
		if (data != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(data.PopupTitleLocalKey);
			UIHelper.SetLabelText(LabelTitle, stringDirect);
			string stringDirect2 = LocalizationManager.GetStringDirect(data.PopupDescLocalKey);
			UIHelper.SetLabelText(LabelDesc, stringDirect2);
			if (!String_Extension_Method.IsNullOrEmpty(data.PopupImageAssetPath))
			{
				BundleTexture.SetAssetPath(data.PopupImageAssetPath);
			}
		}
	}

	private void SendCtr(EClickTag clickTag)
	{
		if (_missionCategoryData != null && !String_Extension_Method.IsNullOrEmpty(_missionCategoryData.CtrAdid))
		{
			SingletonAuto<CtrManager>.instance.Send(_missionCategoryData.CtrAdid, clickTag);
		}
	}
}
