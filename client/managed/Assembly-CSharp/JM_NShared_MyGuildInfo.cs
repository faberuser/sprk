using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MyGuildInfo
{
	public static MyGuildInfo Parse(string s)
	{
		return Parse(s, new MyGuildInfo());
	}

	public static MyGuildInfo Parse(string s, MyGuildInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MyGuildInfo Parse(IDictionary d)
	{
		return Parse(d, new MyGuildInfo());
	}

	public static MyGuildInfo Parse(IDictionary d, MyGuildInfo o)
	{
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Level = BaseJsonMarshaler.ParseValueByte(d, "Level");
		o.Logo = BaseJsonMarshaler.ParseValueByte(d, "Logo");
		o.Back = BaseJsonMarshaler.ParseValueByte(d, "Back");
		o.Flag = BaseJsonMarshaler.ParseValueByte(d, "Flag");
		o.Notice = BaseJsonMarshaler.ParseValueString(d, "Notice");
		o.ReqTeamLevel = BaseJsonMarshaler.ParseValueInt(d, "ReqTeamLevel");
		o.JoinWay = BaseJsonMarshaler.ParseValueByte(d, "JoinWay");
		o.ActivityPoint = BaseJsonMarshaler.ParseValueLong(d, "ActivityPoint");
		o.SuppressPoint = BaseJsonMarshaler.ParseValueLong(d, "SuppressPoint");
		o.CurMember = BaseJsonMarshaler.ParseValueInt(d, "CurMember");
		o.Introduce = BaseJsonMarshaler.ParseValueString(d, "Introduce");
		o.Skill1Level = BaseJsonMarshaler.ParseValueByte(d, "Skill1Level");
		o.Skill2Level = BaseJsonMarshaler.ParseValueByte(d, "Skill2Level");
		o.Skill3Level = BaseJsonMarshaler.ParseValueByte(d, "Skill3Level");
		o.Skill4Level = BaseJsonMarshaler.ParseValueByte(d, "Skill4Level");
		o.Skill5Level = BaseJsonMarshaler.ParseValueByte(d, "Skill5Level");
		o.Skill6Level = BaseJsonMarshaler.ParseValueByte(d, "Skill6Level");
		o.SkillInitTime = BaseJsonMarshaler.ParseValueString(d, "SkillInitTime");
		o.ContributedGem = BaseJsonMarshaler.ParseValueInt(d, "ContributedGem");
		o.Skill1Req = BaseJsonMarshaler.ParseValueByte(d, "Skill1Req");
		o.Skill2Req = BaseJsonMarshaler.ParseValueByte(d, "Skill2Req");
		o.Skill3Req = BaseJsonMarshaler.ParseValueByte(d, "Skill3Req");
		o.Skill4Req = BaseJsonMarshaler.ParseValueByte(d, "Skill4Req");
		o.Skill5Req = BaseJsonMarshaler.ParseValueByte(d, "Skill5Req");
		o.Skill6Req = BaseJsonMarshaler.ParseValueByte(d, "Skill6Req");
		o.Wood = BaseJsonMarshaler.ParseValueInt(d, "Wood");
		o.Stone = BaseJsonMarshaler.ParseValueInt(d, "Stone");
		o.Metal = BaseJsonMarshaler.ParseValueInt(d, "Metal");
		o.AutoApplySuppress = BaseJsonMarshaler.ParseValueInt(d, "AutoApplySuppress");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.GuildBuildingInfos = BaseJsonMarshaler.ParseArray(d, "GuildBuildingInfos", JM_NShared_GuildBuildingInfo.Parse);
		o.SkillInfos = BaseJsonMarshaler.ParseArray(d, "SkillInfos", JM_NShared_GuildSkillInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(MyGuildInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MyGuildInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("Logo", o.Logo.ToString());
		dictionary.Add("Back", o.Back.ToString());
		dictionary.Add("Flag", o.Flag.ToString());
		if (o.Notice != null)
		{
			dictionary.Add("Notice", o.Notice);
		}
		dictionary.Add("ReqTeamLevel", o.ReqTeamLevel.ToString());
		dictionary.Add("JoinWay", o.JoinWay.ToString());
		dictionary.Add("ActivityPoint", o.ActivityPoint.ToString());
		dictionary.Add("SuppressPoint", o.SuppressPoint.ToString());
		dictionary.Add("CurMember", o.CurMember.ToString());
		if (o.Introduce != null)
		{
			dictionary.Add("Introduce", o.Introduce);
		}
		dictionary.Add("Skill1Level", o.Skill1Level.ToString());
		dictionary.Add("Skill2Level", o.Skill2Level.ToString());
		dictionary.Add("Skill3Level", o.Skill3Level.ToString());
		dictionary.Add("Skill4Level", o.Skill4Level.ToString());
		dictionary.Add("Skill5Level", o.Skill5Level.ToString());
		dictionary.Add("Skill6Level", o.Skill6Level.ToString());
		if (o.SkillInitTime != null)
		{
			dictionary.Add("SkillInitTime", o.SkillInitTime);
		}
		dictionary.Add("ContributedGem", o.ContributedGem.ToString());
		dictionary.Add("Skill1Req", o.Skill1Req.ToString());
		dictionary.Add("Skill2Req", o.Skill2Req.ToString());
		dictionary.Add("Skill3Req", o.Skill3Req.ToString());
		dictionary.Add("Skill4Req", o.Skill4Req.ToString());
		dictionary.Add("Skill5Req", o.Skill5Req.ToString());
		dictionary.Add("Skill6Req", o.Skill6Req.ToString());
		dictionary.Add("Wood", o.Wood.ToString());
		dictionary.Add("Stone", o.Stone.ToString());
		dictionary.Add("Metal", o.Metal.ToString());
		dictionary.Add("AutoApplySuppress", o.AutoApplySuppress.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		dictionary.Add("GuildBuildingInfos", BaseJsonMarshaler.EncodeArray(o.GuildBuildingInfos, JM_NShared_GuildBuildingInfo.EncodeJsonObject));
		dictionary.Add("SkillInfos", BaseJsonMarshaler.EncodeArray(o.SkillInfos, JM_NShared_GuildSkillInfo.EncodeJsonObject));
		return dictionary;
	}
}
