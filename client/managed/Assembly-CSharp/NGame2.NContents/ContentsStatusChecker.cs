using NGame2.NAccount;
using NGame2.NUI;
using NShared;
using NVespa;
using UnityEngine;
using UnityEngine.Events;

namespace NGame2.NContents;

public class ContentsStatusChecker : MonoBehaviour, IUserObserver<ContentsStatusInfo>, IUserObserver
{
	[SerializeField]
	private string StatusTypeString;

	[SerializeField]
	private string SubTypeString;

	[SerializeField]
	private GameObject Root;

	[SerializeField]
	private UIButton Button;

	[SerializeField]
	private UnityEvent OnUpdateEvent;

	private ContentsStatusType StatusType;

	private ContentsStatusSubType SubType;

	private ContentsStatus Status = ContentsStatus.On;

	private void OnEnable()
	{
		if (!String_Extension_Method.IsNullOrEmpty(StatusTypeString))
		{
			Vespa_String_Extension_Method.TryToEnum<ContentsStatusType>(StatusTypeString, out StatusType);
		}
		if (!String_Extension_Method.IsNullOrEmpty(SubTypeString))
		{
			Vespa_String_Extension_Method.TryToEnum<ContentsStatusSubType>(SubTypeString, out SubType);
		}
		if (StatusType != ContentsStatusType.None)
		{
			if (Root == null)
			{
				Root = base.gameObject;
			}
			ContentsStatus contentsStatus = SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(StatusType, SubType);
			SetContentsState(contentsStatus);
			SingletonAuto<ContentsStatusManager>.instance.ObserverManager.AddObserver(this);
		}
	}

	public void Init(ContentsStatusType statusType, ContentsStatusSubType subType, UIButton button, UnityAction onUpdate = null)
	{
		StatusType = statusType;
		SubType = subType;
		Button = button;
		if (onUpdate != null)
		{
			if (OnUpdateEvent == null)
			{
				OnUpdateEvent = new UnityEvent();
			}
			OnUpdateEvent.AddListener(onUpdate);
		}
	}

	private void OnDestroy()
	{
		SingletonAuto<ContentsStatusManager>.instance.ObserverManager.RemoveObserver(this);
	}

	private void SetContentsState(ContentsStatus contentsStatus)
	{
		if (Status != contentsStatus)
		{
			switch (contentsStatus)
			{
			case ContentsStatus.Hide:
				UIHelper.EnableGameObject(Root, enable: false);
				break;
			case ContentsStatus.Disable:
				UIHelper.EnableGameObject(Root, enable: true);
				UIHelper.EnableButton(Button, enable: false, grayscaleIfDisable: true);
				break;
			case ContentsStatus.On:
				UIHelper.EnableGameObject(Root, enable: true);
				UIHelper.EnableButton(Button, enable: true, grayscaleIfDisable: true);
				break;
			default:
				UIHelper.EnableGameObject(Root, enable: false);
				break;
			}
			if (OnUpdateEvent != null)
			{
				OnUpdateEvent.Invoke();
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ContentsStatusInfo>.OnChanged(ContentsStatusInfo changedInfo)
	{
		ContentsStatusType o_output = ContentsStatusType.None;
		ContentsStatusSubType o_output2 = ContentsStatusSubType.None;
		Vespa_String_Extension_Method.TryToEnum<ContentsStatusType>(changedInfo.ContentsType, out o_output);
		Vespa_String_Extension_Method.TryToEnum<ContentsStatusSubType>(changedInfo.ContentsSubType, out o_output2);
		if (o_output == StatusType && o_output2 == SubType)
		{
			ContentsStatus o_output3 = ContentsStatus.On;
			Vespa_String_Extension_Method.TryToEnum<ContentsStatus>(changedInfo.Status, out o_output3);
			SetContentsState(o_output3);
		}
	}
}
