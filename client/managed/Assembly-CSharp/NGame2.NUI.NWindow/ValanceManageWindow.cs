using System.Collections;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceManageWindow : BaseCinemaWindow
{
	public interface IListener
	{
		void OnClickAwaken();

		void OnClickOptionRenew();

		void OnClickTierUpgrade();

		void OnClickEnchant();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceManageWindow";

	public GameObject HideRoot;

	public GameObject GoAwakenNotice;

	public GameObject GoOptionRenewNotice;

	public GameObject GoTierUpgradeNotice;

	public GameObject GoEnchantNotice;

	public GameObject[] HideButtons;

	private IListener _listener;

	public Animator Animator;

	public void Open(IListener listener)
	{
		_listener = listener;
		OnOffHideRoot(isOn: false);
		OnOffAwakenNotice(isOn: false);
		OnOffOptionRenewNotice(isOn: false);
		OnOffTierUpgradeNotice(isOn: false);
		OnOffEnchantNotice(isOn: false);
		Open();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		OnOffHideRoot(isOn: false);
		OnOffAwakenNotice(isOn: false);
		OnOffOptionRenewNotice(isOn: false);
		OnOffTierUpgradeNotice(isOn: false);
		OnOffEnchantNotice(isOn: false);
		StopAllCoroutines();
	}

	private void OnOffHideRoot(bool isOn)
	{
		if (!isOn)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HideButtons); i++)
			{
				UIHelper.EnableGameObject(HideButtons[i], isOn);
			}
			StopAllCoroutines();
		}
		else
		{
			VespaCoroutine.Start(StartAnimator());
		}
	}

	private IEnumerator StartAnimator()
	{
		Animator.Play("Enter");
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HideButtons); i++)
		{
			UIHelper.EnableGameObject(HideButtons[i], enable: true);
		}
	}

	private void OnOffAwakenNotice(bool isOn)
	{
		UIHelper.EnableGameObject(GoAwakenNotice, isOn);
	}

	private void OnOffTierUpgradeNotice(bool isOn)
	{
		UIHelper.EnableGameObject(GoTierUpgradeNotice, isOn);
	}

	private void OnOffOptionRenewNotice(bool isOn)
	{
		UIHelper.EnableGameObject(GoOptionRenewNotice, isOn);
	}

	private void OnOffEnchantNotice(bool isOn)
	{
		UIHelper.EnableGameObject(GoEnchantNotice, isOn);
	}

	protected override void NodeTreeOpenOnEnd()
	{
		base.NodeTreeOpenOnEnd();
		OnOffHideRoot(isOn: true);
	}

	public void HideButtonRoot()
	{
		OnOffHideRoot(isOn: false);
	}

	public void ShowButtonRoot()
	{
		OnOffHideRoot(isOn: true);
	}

	public void OpenCustomCinema(IListener listener)
	{
		_listener = listener;
		OnOffHideRoot(isOn: false);
		OnOffAwakenNotice(isOn: false);
		OnOffOptionRenewNotice(isOn: false);
		OnOffTierUpgradeNotice(isOn: false);
		OnOffEnchantNotice(isOn: false);
		OpenCustomCinema("ValanceManageWindowInven", null, null);
	}

	public void OnClickAwaken()
	{
		if (_listener != null)
		{
			_listener.OnClickAwaken();
		}
	}

	public void OnClickOptionRenew()
	{
		if (_listener != null)
		{
			_listener.OnClickOptionRenew();
		}
	}

	public void OnClickTierUpgrade()
	{
		if (_listener != null)
		{
			_listener.OnClickTierUpgrade();
		}
	}

	public void OnClickEnchant()
	{
		if (_listener != null)
		{
			_listener.OnClickEnchant();
		}
	}
}
