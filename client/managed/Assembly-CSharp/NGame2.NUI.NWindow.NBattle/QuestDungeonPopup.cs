using System;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class QuestDungeonPopup : BaseWindow
{
	public enum EventType
	{
		None,
		NewQuestDungeon,
		EndQuestDungeon
	}

	[Serializable]
	public class EventInfo
	{
		public EventType Type;

		public Transform Root;

		public UILabel CustomLabel;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/QuestDungeonPopup";

	public EventInfo[] Events;

	public void Open(EventType eventType, string customText = null)
	{
		Open(delegate
		{
			Init(eventType, customText);
		}, null);
	}

	public void Init(EventType eventType, string customText = null)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Events); i++)
		{
			UIHelper.EnableGameObject(Events[i].Root, enable: false);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(Events); j++)
		{
			if (Events[j].Type == eventType)
			{
				InitEvent(Events[j], customText);
				break;
			}
		}
	}

	private void InitEvent(EventInfo eventInfo, string customText)
	{
		UIHelper.EnableGameObject(eventInfo.Root, enable: true);
		UIHelper.SetLabelText(eventInfo.CustomLabel, customText);
	}
}
