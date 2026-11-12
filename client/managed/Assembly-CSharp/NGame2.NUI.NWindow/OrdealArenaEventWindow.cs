using System.Collections;
using NGame2.NOrdeal;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NShared;
using NShared.EndOrdealArena;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OrdealArenaEventWindow : BaseWindow
{
	public interface IListener
	{
		void RequestReward();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/OrdealArena/OrdealArenaEventWindow";

	public UIBundleLoader BundleLoder;

	public GameObject RootDesc;

	public UILabel LabelDesc;

	private int _sceneCount;

	private int _curScenceIndex;

	private OrdealArenaEventData _data;

	private NShared.EndOrdealArena.Response _res;

	private IListener _listener;

	public void Open(OrdealNodeInfo nodeInfo, NShared.EndOrdealArena.Response res, IListener listener)
	{
		if (nodeInfo != null && res != null)
		{
			_listener = listener;
			_data = GameRoot.Instance.OrdealArenaEventDataContainer.GetData(nodeInfo.EventIndex);
			_res = res;
			if (_data == null)
			{
				Close();
				return;
			}
			_curScenceIndex = 0;
			_sceneCount = __Extension_Method_ICollection__.COUNT(_data.DescKeys);
			UIHelper.EnableGameObject(base.gameObject, enable: true);
			UIHelper.EnableGameObject(WindowRoot, enable: false);
			VespaCoroutine.Start(CoOpen(), null, null);
		}
	}

	private IEnumerator CoOpen()
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(3f, GlobalTopmostWindow.ESceneChangeType.Cloud);
		if (sceneChanger != null)
		{
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return null;
			}
		}
		sceneChanger.Close();
		Open(delegate
		{
			SetVolume(_data.Volume);
			SetTexture(_data.TexturePath);
			Set();
		}, null);
	}

	private IEnumerator CoClose()
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(3f, GlobalTopmostWindow.ESceneChangeType.Cloud);
		if (sceneChanger != null)
		{
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return null;
			}
		}
		sceneChanger.Close();
		base.Close();
	}

	public void Set()
	{
		string valueString = ArgParser.GetValueString(_data.DescKeys, _curScenceIndex);
		SetDesc(valueString);
	}

	public void SetTexture(string texturePath)
	{
		UIHelper.SetBundleTexture(BundleLoder, texturePath);
	}

	public void SetVolume(int volume)
	{
		if (volume == 0)
		{
			volume = 100;
		}
		float volume2 = (float)volume / 100f;
		SingletonAuto<BgmManager2>.instance.SetVolume(volume2);
	}

	public void SetDesc(string descKey)
	{
		bool flag = String_Extension_Method.IsNullOrEmpty(descKey);
		UIHelper.EnableGameObject(RootDesc, !flag);
		if (!flag)
		{
			string stringDirect = LocalizationManager.GetStringDirect(descKey);
			UIHelper.SetLabelText(LabelDesc, stringDirect);
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		SetVolume(100);
		_listener = null;
		_data = null;
		_res = null;
		_sceneCount = 0;
		_curScenceIndex = 0;
	}

	public override void OnSystemBackButton()
	{
		OnClickNext();
	}

	public override void Close()
	{
		VespaCoroutine.Start(CoClose(), null, null);
	}

	public void OnClickNext()
	{
		if (_curScenceIndex + 1 >= _sceneCount)
		{
			switch (_data.EventType)
			{
			case EOrdealEventType.GetOpponentChange:
			{
				string stringDirect2 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_EVENT_TITLE");
				string stringDirect3 = LocalizationManager.GetStringDirect("ORDEALARENA_MESSAGE_DESC_CHANGEOPPONENT");
				MessagePopup.Open(stringDirect2, stringDirect3, delegate
				{
					Close();
				});
				break;
			}
			case EOrdealEventType.GetPoint:
			{
				string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_MESSAGE_GAINPOINT");
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).SetCurrencyResultInfo3Single(_res.CurrencyResult), isGlobal: false, delegate
				{
					Close();
				});
				break;
			}
			case EOrdealEventType.ReviveAllCharacter:
			{
				string stringDirect4 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_EVENT_TITLE");
				string stringDirect5 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_EVENT_DESC_RESTORE");
				MessagePopup.Open(stringDirect4, stringDirect5, delegate
				{
					Close();
				});
				break;
			}
			case EOrdealEventType.SelectClearBuff:
				if (!GlobalAutoBehaviour<OrdealArenaManager>.instance.HasSelectableBuff())
				{
					Close();
				}
				else
				{
					SceneAutoBehaviour<OrdealArenaManagement>.instance.OpenBuffSelectPopup(isLobbyType: true);
				}
				break;
			default:
				MessagePopup.Open("Error", "UnDefined EventType : " + _data.EventType, delegate
				{
					Close();
				});
				break;
			}
		}
		else
		{
			_curScenceIndex++;
			Set();
		}
	}
}
