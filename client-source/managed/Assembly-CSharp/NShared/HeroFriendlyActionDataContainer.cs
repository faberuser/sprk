using System.Collections;
using NShared.NJit;

namespace NShared;

public class HeroFriendlyActionDataContainer : JitDataContainerDefault<int, HeroFriendlyActionData>
{
	public const string TABLENAME = "HeroFriendlyActionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HeroFriendlyActionTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, HeroFriendlyActionData value)
	{
		TrimUnnecessaryChars_in_JsonLoad(value.ActionResponse1);
		TrimUnnecessaryChars_in_JsonLoad(value.ActionResponse2);
		TrimUnnecessaryChars_in_JsonLoad(value.ActionResponse3);
		TrimUnnecessaryChars_in_JsonLoad(value.ActionResponse4);
		TrimUnnecessaryChars_in_JsonLoad(value.ActionResponse5);
		TrimUnnecessaryChars_in_JsonLoad(value.ActionResponse6);
		TrimUnnecessaryChars_in_JsonLoad(value.MaxResponse1);
		TrimUnnecessaryChars_in_JsonLoad(value.MaxResponse2);
		TrimUnnecessaryChars_in_JsonLoad(value.MaxResponse3);
		TrimUnnecessaryChars_in_JsonLoad(value.Response1_MAX);
	}

	private void TrimUnnecessaryChars_in_JsonLoad(string[] values)
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
