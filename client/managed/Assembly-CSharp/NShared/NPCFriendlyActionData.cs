using System;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NPCFriendlyActionData : IDataKeySelector<NPCFriendlyActionKey2>
{
	public const int ActionResponse_SpeechIndex = 0;

	public const int ActionResponse_SoundIndex = 1;

	public const int ActionResponse_MotionIndex = 2;

	public const int EventGiftStepReactionLength = 3;

	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public int ItemGrade { get; set; }

	[Key(2)]
	public string[] ActionResponse { get; set; }

	NPCFriendlyActionKey2 IDataKeySelector<NPCFriendlyActionKey2>.Key => new NPCFriendlyActionKey2(HeroIndex, ItemGrade);

	public override string ToString()
	{
		return "NPCFriendlyActionData HeroIndex=" + HeroIndex;
	}

	public string GetSpeech()
	{
		return GetAction(0);
	}

	public string GetSound()
	{
		return GetAction(1);
	}

	public string GetMotion()
	{
		return GetAction(2);
	}

	public string[] GetEventRandom()
	{
		string[] array = new string[3];
		int num = ActionResponse.Length;
		int num2 = new Random().Next(0, num / 3);
		array[0] = GetAction(num2 * 3);
		array[1] = GetAction(1 + num2 * 3);
		array[2] = GetAction(2 + num2 * 3);
		return array;
	}

	private string GetAction(int index)
	{
		if (ActionResponse == null)
		{
			return "";
		}
		if (ActionResponse.Length <= index)
		{
			return "";
		}
		return ActionResponse[index];
	}
}
