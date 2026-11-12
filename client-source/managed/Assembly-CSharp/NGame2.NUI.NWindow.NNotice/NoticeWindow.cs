using System;
using System.Linq;
using NGame2.NScene.NPreloader;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NNotice;

public class NoticeWindow : MonoBehaviour, IGlobalWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Notice/NoticeWindow";

	private static GlobalWindowPrefab<NoticeWindow> _prefabInstance = new GlobalWindowPrefab<NoticeWindow>("Resources/Prefab2/Window/Notice/NoticeWindow", GlobalUIRoot.ELayerType.GlobalUI);

	public Transform Root;

	public UILabel MessageLabel;

	public int Speed = 200;

	private Vector2 _messageSize;

	private Vector3 _messageStartPosition;

	private float _rollingTime;

	private Notice _currentNotice;

	private bool _currentNoticeRemoved;

	public static NoticeWindow instance => _prefabInstance.instance;

	public static bool isValidInstance => _prefabInstance.IsValidInstance;

	public static void ReleaseInstance()
	{
		_prefabInstance.ReleaseWindow();
	}

	private void Awake()
	{
		_messageStartPosition = GetRollingStartPosition();
	}

	public void Open()
	{
		base.gameObject.SetActive(value: true);
		_rollingTime = 0f;
		_currentNoticeRemoved = false;
	}

	public void Close()
	{
		Clear();
		base.gameObject.SetActive(value: false);
	}

	public bool IsOpen()
	{
		return base.gameObject.activeInHierarchy;
	}

	public void ReserveCache(UiPreloader preloader)
	{
	}

	private void Clear()
	{
		_currentNotice = null;
		_currentNoticeRemoved = false;
	}

	public void AddNotice(Notice notice)
	{
		_currentNotice = notice;
		_currentNoticeRemoved = false;
		if (_currentNotice == null)
		{
			return;
		}
		_rollingTime = 0f;
		SystemLanguage currentLanguage = SingletonAuto<LocalizationManager>.instance.Language;
		NoticeLocalInfo noticeLocalInfo = ((_currentNotice.LocalInfo != null) ? _currentNotice.LocalInfo.FirstOrDefault((NoticeLocalInfo e) => string.Equals(e.Language, Vespa_String_Extension_Method.GetEnumString(currentLanguage), StringComparison.InvariantCultureIgnoreCase)) : null);
		if (noticeLocalInfo == null)
		{
			noticeLocalInfo = _currentNotice.LocalInfo.FirstOrDefault((NoticeLocalInfo e) => string.Equals(e.Language, "English", StringComparison.InvariantCultureIgnoreCase));
		}
		if (noticeLocalInfo == null)
		{
			noticeLocalInfo = _currentNotice.LocalInfo.FirstOrDefault();
		}
		string text = ((noticeLocalInfo != null) ? noticeLocalInfo.Message : _currentNotice.Message);
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			VespaDebug.LogError("Empty notice message.");
		}
		UIHelper.SetLabelText(MessageLabel, text);
		_messageSize = MessageLabel.printedSize;
	}

	public void RemoveNotice(int noticeId)
	{
		if (_currentNotice != null && _currentNotice.Id == noticeId)
		{
			_currentNoticeRemoved = true;
		}
	}

	public void RemoveCurrentNotice()
	{
		_currentNotice = null;
		_rollingTime = 0f;
	}

	private Vector3 GetRollingStartPosition()
	{
		return new Vector3(1080 * Screen.width / Screen.height / 2, MessageLabel.transform.localPosition.y, 0f);
	}

	private void Update()
	{
		if (_currentNotice == null)
		{
			Close();
			return;
		}
		_rollingTime += Time.deltaTime;
		if (_rollingTime <= 0f)
		{
			UIHelper.EnableGameObject(Root, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Root, enable: true);
		_messageSize = MessageLabel.printedSize;
		Vector3 localPosition = _messageStartPosition + Vector3.left * _rollingTime * Speed;
		MessageLabel.transform.localPosition = localPosition;
		if (localPosition.x + _messageSize.x < 0f - _messageStartPosition.x)
		{
			if (_currentNotice.Oneshot > 0 || _currentNoticeRemoved)
			{
				RemoveCurrentNotice();
				_currentNoticeRemoved = false;
			}
			else
			{
				_rollingTime = -_currentNotice.Interval;
			}
		}
	}
}
