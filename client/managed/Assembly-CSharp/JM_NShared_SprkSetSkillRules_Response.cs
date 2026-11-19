using System.Collections;
using MiniJSON;
using NShared;
using NShared.SprkSetSkillRules;

public static class JM_NShared_SprkSetSkillRules_Response
{
	public static NShared.SprkSetSkillRules.Response Parse(string s)
	{
		return Parse(s, new NShared.SprkSetSkillRules.Response());
	}

	public static NShared.SprkSetSkillRules.Response Parse(string s, NShared.SprkSetSkillRules.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SprkSetSkillRules.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.SprkSetSkillRules.Response());
	}

	public static NShared.SprkSetSkillRules.Response Parse(IDictionary d, NShared.SprkSetSkillRules.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.BaseResult = BaseJsonMarshaler.ParseEnum<BaseResultType>(d, "BaseResult");
		o.InternalErrorMessage = BaseJsonMarshaler.ParseValueString(d, "InternalErrorMessage");
		return o;
	}
}
