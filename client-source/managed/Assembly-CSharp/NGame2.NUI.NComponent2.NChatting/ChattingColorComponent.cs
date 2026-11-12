using System;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NChatting;

public class ChattingColorComponent : MonoBehaviour
{
	public enum ColoredChatType
	{
		None,
		System,
		Admin,
		Guild,
		GuildMaster,
		Whisper,
		Party
	}

	[Serializable]
	public class ColorInfo
	{
		public ColoredChatType ChatType;

		public Color ChatColor;
	}

	public const string PrefabPath = "Prefab2/Component/Chatting/ChattingColorComponent";

	public const string BattlePrefabPath = "Prefab2/Component/BattleChatting/BattleChattingColorComponent";

	public ColorInfo[] PreviewColorInfos;

	public ColorInfo[] ColorInfos;
}
