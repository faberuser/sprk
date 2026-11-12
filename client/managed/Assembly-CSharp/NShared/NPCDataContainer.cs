using NShared.NJit;

namespace NShared;

public class NPCDataContainer : JitDataContainerDefault<string, NPCData>
{
	public const string TABLENAME = "NPCTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NPCTable");

	protected override void OnEndLoadFromJsonData()
	{
		foreach (NPCData item in base.BaseList.JsonLoad_GetOrNew())
		{
			TrimStringArray_in_JsonLoad(item.OnOpenEntryMenu);
			TrimStringArray_in_JsonLoad(item.OnTouch);
			TrimStringArray_in_JsonLoad(item.OnGetMissionReward);
			TrimStringArray_in_JsonLoad(item.OnOpenMissionPopup);
			TrimStringArray_in_JsonLoad(item.QuizCorrectAnswer);
			TrimStringArray_in_JsonLoad(item.QuizWrongAnswer);
		}
	}

	private void TrimStringArray_in_JsonLoad(string[] valueArray)
	{
		if (valueArray != null)
		{
			for (int i = 0; i < valueArray.Length; i++)
			{
				valueArray[i] = valueArray[i].Trim('\n', '\r', '\t', ' ');
			}
		}
	}

	public bool EntryMenuExist(string NPCCode)
	{
		if (!string.IsNullOrEmpty(NPCCode))
		{
			return false;
		}
		return GetData(NPCCode)?.EntryMenuExist() ?? false;
	}
}
