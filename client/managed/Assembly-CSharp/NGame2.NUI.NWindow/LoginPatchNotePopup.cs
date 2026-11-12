using System;
using System.Collections.Generic;
using NGame2.NLogin;
using NShared;
using NShared.PatchNote;
using NVespa;
using NVespa.NGlobal;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginPatchNotePopup : BaseWindow
{
	[Serializable]
	public class NoticeUiData
	{
		public Transform Root;

		public UILabel TextLabel;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginPatchNotePopup";

	public NoticeUiData NoticeUi;

	public UIButton ConfirmButton;

	public UILabel LabelTitle;

	public UIButton Button_Prev;

	public UIButton Button_Next;

	public UIScrollView Scroll_PatchNote;

	private Action _onConfirm;

	private List<GameObject> _labelList = new List<GameObject>();

	private int _curPatchNoteIndex;

	private string _curLanguage = "";

	public void Open(bool accepted, Action onConfirm)
	{
		Open(delegate
		{
			Init(accepted, onConfirm);
		}, null);
	}

	public void Init(bool accepted, Action onConfirm)
	{
		_onConfirm = onConfirm;
		InitPatchNoteText();
		if (accepted || NoticeUi == null)
		{
			Vespa_System_Action_Extension_Method.fire(_onConfirm);
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickConfirm();
	}

	public override void Reset()
	{
		base.Reset();
		_onConfirm = null;
		ResetMessage();
	}

	private void ResetMessage()
	{
		_labelList.ForEach(delegate(GameObject e)
		{
			VespaObject.DestroyGo(e);
		});
		_labelList.Clear();
	}

	private void InitPatchNoteText()
	{
		if (NoticeUi != null)
		{
			_curPatchNoteIndex = LoginData.LastPatchNoteIndex;
			InitLastestPatchNote();
		}
	}

	private void SetPatchNoteText(string title, string message)
	{
		if (NoticeUi == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(title))
		{
			UIHelper.SetLabelText(LabelTitle, LocalizationManager.GetStringDirect("PATCH_NOTE_NOTICE"));
		}
		else
		{
			UIHelper.SetLabelText(LabelTitle, title);
		}
		if (string.IsNullOrEmpty(message))
		{
			UIHelper.EnableGameObject(NoticeUi.TextLabel, enable: true);
			UIHelper.SetLabelText(NoticeUi.TextLabel, "");
			return;
		}
		UILabel textLabel = NoticeUi.TextLabel;
		UIHelper.EnableGameObject(textLabel, enable: false);
		Vector3 localPosition = textLabel.transform.localPosition;
		int num = 0;
		string[] array = message.Split(new char[1] { '\n' });
		foreach (string text in array)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(textLabel.gameObject);
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, textLabel.transform.parent);
			gameObject.SetActive(value: true);
			gameObject.transform.localPosition = localPosition + Vector3.down * num;
			UILabel component = gameObject.GetComponent<UILabel>();
			UIHelper.SetLabelText(component, text);
			num += (int)component.printedSize.y;
			_labelList.Add(gameObject);
		}
		Scroll_PatchNote.ResetPosition();
	}

	private void SetPatchNote(PatchNoteInfo patchNote, bool isReset = false)
	{
		if (patchNote != null)
		{
			_curPatchNoteIndex = patchNote.Index;
			SetPatchNoteText(patchNote.Title, patchNote.Message);
			if (isReset || LoginData.LastPatchNoteIndex < patchNote.Index)
			{
				LoginData.LastPatchNoteIndex = patchNote.Index;
				LoginData.LastPatchNoteTitle = patchNote.Title;
				LoginData.LastPatchNoteMessage = patchNote.Message;
				LoginData.LastPatchNoteEndTime = patchNote.EndTime;
			}
		}
	}

	public void OnClickConfirm()
	{
		Vespa_System_Action_Extension_Method.fire(_onConfirm);
	}

	public void InitLastestPatchNote()
	{
		string text = SingletonAuto<LocalizationManager>.instance.Language.ToString();
		bool isRead = ((_curLanguage != text) ? true : false);
		_curLanguage = text;
		NShared.PatchNote.Request request = new NShared.PatchNote.Request
		{
			PatchNoteIndex = _curPatchNoteIndex,
			Language = _curLanguage,
			BeforeNext = 0,
			IsRead = true
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.PatchNote.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success && response.PatchNote != null)
			{
				ResetMessage();
				if (_curPatchNoteIndex == response.PatchNote.Index && !isRead)
				{
					SetPatchNoteText(LoginData.LastPatchNoteTitle, LoginData.LastPatchNoteMessage);
				}
				else
				{
					SetPatchNote(response.PatchNote, isReset: true);
				}
				UIHelper.EnableButton(Button_Prev, response.IsMore, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(Button_Prev, response.IsMore);
				UIHelper.EnableGameObject(Button_Next, enable: false);
			}
		});
	}

	public void OnClickPrevButton()
	{
		string language = SingletonAuto<LocalizationManager>.instance.Language.ToString();
		NShared.PatchNote.Request request = new NShared.PatchNote.Request
		{
			PatchNoteIndex = _curPatchNoteIndex,
			Language = language,
			BeforeNext = 1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.PatchNote.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
			{
				ResetMessage();
				UIHelper.EnableButton(Button_Prev, response.IsMore, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(Button_Prev, response.IsMore);
				UIHelper.EnableButton(Button_Next, enable: true, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(Button_Next, enable: true);
				SetPatchNote(response.PatchNote);
			}
		});
	}

	public void OnClickNextButton()
	{
		string language = SingletonAuto<LocalizationManager>.instance.Language.ToString();
		NShared.PatchNote.Request request = new NShared.PatchNote.Request
		{
			PatchNoteIndex = _curPatchNoteIndex,
			Language = language,
			BeforeNext = 2
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.PatchNote.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
			{
				ResetMessage();
				UIHelper.EnableButton(Button_Prev, enable: true, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(Button_Prev, enable: true);
				UIHelper.EnableButton(Button_Next, response.IsMore, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(Button_Next, response.IsMore);
				SetPatchNote(response.PatchNote);
			}
		});
	}
}
