using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MiniGuidePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tutorial/MiniGuidePopup";

	public int WidgetCount = 6;

	public UIScrollView ScrollView;

	public UILabel[] Lbl_Title;

	public UILabel[] Lbl_Desc;

	public UIWidget[] WidgetUi;

	public UITable Table_Root;

	public Transform MoveGuideButtonTranform;

	private const int maxWidth = 1609;

	private const int minWidth = 1500;

	private GameGuideData _gameGuideData;

	private int _gameGuideIndex;

	public override void Reset()
	{
		base.Reset();
		_gameGuideData = null;
		_gameGuideIndex = 0;
		if (ScrollView != null)
		{
			ScrollView.ResetPosition();
		}
	}

	public void Open(int[] openHelpIndex)
	{
		WindowPrefab<MiniGuidePopup> windowPrefab = new WindowPrefab<MiniGuidePopup>("Resources/Prefab2/Window/Tutorial/MiniGuidePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(openHelpIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(int[] openHelpIndex)
	{
		base.Open();
		Init(openHelpIndex);
	}

	private void Init(int[] openHelpIndex)
	{
		_gameGuideIndex = ArgParser.GetValueInt(openHelpIndex, 0);
		_gameGuideData = GameRoot.Instance.GameGuideDataContainer.GetData(_gameGuideIndex);
		if (_gameGuideData == null)
		{
			for (int i = 0; i < WidgetCount; i++)
			{
				UIHelper.EnableGameObject(WidgetUi[i], enable: false);
			}
			return;
		}
		UIHelper.EnableGameObject(MoveGuideButtonTranform, _gameGuideData.GuideType != 0);
		int num = 0;
		for (int j = 0; j < WidgetCount; j++)
		{
			int valueInt = ArgParser.GetValueInt(openHelpIndex, j + 1, -1);
			if (!_gameGuideData.GetQandA(valueInt, out var q, out var a))
			{
				UIHelper.EnableGameObject(WidgetUi[j], enable: false);
				continue;
			}
			UIHelper.EnableGameObject(WidgetUi[j], enable: true);
			UIHelper.SetLabelText(Lbl_Title[j], LocalizationManager.GetStringDirect(q));
			UIHelper.SetLabelText(Lbl_Desc[j], LocalizationManager.GetStringDirect(a));
			num++;
		}
		for (int k = 0; k < num; k++)
		{
			if (!(Lbl_Desc[k] == null))
			{
				if (k == num - 1)
				{
					Lbl_Desc[k].width = 1500;
				}
				else
				{
					Lbl_Desc[k].width = 1609;
				}
			}
		}
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, RepostionNextUpdate());
	}

	private IEnumerator RepostionNextUpdate()
	{
		yield return null;
		if ((bool)Table_Root)
		{
			Table_Root.repositionNow = true;
		}
	}

	public void OnClickGuideDetail()
	{
		if (_gameGuideIndex != 0 && _gameGuideData != null)
		{
			base.Close();
			SceneAutoBehaviour<LobbyManagement>.instance.OpenGuidebyIndex(_gameGuideIndex);
		}
	}
}
