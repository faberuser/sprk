using System;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BattleEmoti : BaseWindow
{
	public interface Listener
	{
		void SendEmoti(EmotiType emotiType, long receiverId, string receiverName);
	}

	[Serializable]
	public class ButtonInfo
	{
		public EmotiType Emoti;

		public Transform Root;

		public Transform EnabledRoot;

		public Transform DisabledRoot;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BattleEmoti";

	public ButtonInfo[] Buttons;

	public float Cooltime = 2f;

	private Listener _listener;

	private float _usedTime;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Buttons); i++)
		{
			ButtonInfo button = Buttons[i];
			UpdateButton(button);
		}
	}

	private void UpdateButton(ButtonInfo button)
	{
		bool flag = IsInCooltime();
		UIHelper.EnableGameObject(button.EnabledRoot, !flag);
		UIHelper.EnableGameObject(button.DisabledRoot, flag);
	}

	public void StartCooltime()
	{
		_usedTime = Time.time;
	}

	public bool IsInCooltime()
	{
		return Time.time - _usedTime < Cooltime;
	}

	public void OnClickEomotiButton(GameObject senderObject)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Buttons); i++)
		{
			ButtonInfo buttonInfo = Buttons[i];
			if ((bool)buttonInfo.Root && buttonInfo.Root.gameObject == senderObject && !IsInCooltime())
			{
				SendEmoti(buttonInfo);
				break;
			}
		}
	}

	private void SendEmoti(ButtonInfo button)
	{
		if (button != null && _listener != null)
		{
			StartCooltime();
			_listener.SendEmoti(button.Emoti, 0L, null);
		}
	}
}
