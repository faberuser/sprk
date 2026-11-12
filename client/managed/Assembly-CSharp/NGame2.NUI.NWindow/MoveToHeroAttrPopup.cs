using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MoveToHeroAttrPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MoveToHeroAttrPopup";

	private static WindowPrefab<MoveToHeroAttrPopup> _windowPrefab;

	public UILabel TranscendSkillPointLabel;

	private Action _onOk;

	public static MoveToHeroAttrPopup instance
	{
		get
		{
			if (_windowPrefab == null)
			{
				_windowPrefab = new WindowPrefab<MoveToHeroAttrPopup>("Resources/Prefab2/Window/MessagePopup/MoveToHeroAttrPopup", GameUIRoot.ELayerType.UI);
			}
			_windowPrefab.instance.gameObject.SetActive(value: false);
			return _windowPrefab.instance;
		}
	}

	public static void Open(int heroIndex, int transcended, Action onOk)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new WindowPrefab<MoveToHeroAttrPopup>("Resources/Prefab2/Window/MessagePopup/MoveToHeroAttrPopup", GameUIRoot.ELayerType.UI);
		}
		_windowPrefab.instance.OpenInternal(heroIndex, transcended, onOk);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
	}

	private void OpenInternal(int heroIndex, int transcended, Action onOk)
	{
		int num = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex).GetTranscendSkillPoint[transcended - 1];
		string stringDirect = LocalizationManager.GetStringDirect("ADDED_TRANSCEND_SKILL_POINT", num);
		UIHelper.SetLabelText(TranscendSkillPointLabel, stringDirect);
		base.Open();
		_onOk = onOk;
	}

	public void OnClickClose()
	{
		Close();
		_onOk = null;
		_windowPrefab = null;
	}

	public void OnClickMoveHeroInfo()
	{
		Vespa_System_Action_Extension_Method.fire(_onOk);
		OnClickClose();
	}
}
