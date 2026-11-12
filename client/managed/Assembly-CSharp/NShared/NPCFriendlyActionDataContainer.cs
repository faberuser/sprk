using System.Collections;
using NShared.NJit;

namespace NShared;

public class NPCFriendlyActionDataContainer : JitDataContainerDefault<NPCFriendlyActionKey2, NPCFriendlyActionData>
{
	public const string TABLENAME = "NPCFriendlyActionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NPCFriendlyActionTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, NPCFriendlyActionData value)
	{
		TrimUnnecessaryChars(value.ActionResponse);
	}

	public NPCFriendlyActionData GetData(int HeroIndex, int ItemGrade)
	{
		return GetData(new NPCFriendlyActionKey2(HeroIndex, ItemGrade));
	}

	private void TrimUnnecessaryChars(string[] values)
	{
		char[] trimChars = new char[3] { '\n', ' ', '\t' };
		if (values == null)
		{
			return;
		}
		for (int i = 0; i < values.Length; i++)
		{
			if (!string.IsNullOrEmpty(values[i]))
			{
				values[i] = values[i].Trim(trimChars);
			}
		}
	}
}
