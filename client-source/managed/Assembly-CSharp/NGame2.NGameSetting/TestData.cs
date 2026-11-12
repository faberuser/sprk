using System.Collections.Generic;
using LitJson;
using NShared;
using UnityEngine;

namespace NGame2.NGameSetting;

public class TestData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class AssignNickname : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public TeamType TeamType;

		public int CreatureIndex;

		public string Nickname = string.Empty;

		bool ILoadJson.Load(JsonData json)
		{
			TeamType = json["TeamType"].GETENUM(TeamType);
			CreatureIndex = json["CreatureIndex"].GETINT(CreatureIndex);
			Nickname = json["Nickname"].GETSTRING(Nickname);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldEnum("TeamType", TeamType);
			jsonData.AddFieldLong("CreatureIndex", CreatureIndex);
			jsonData.AddFieldString("Nickname", Nickname);
			return jsonData;
		}
	}

	public bool ShowObjectBaseAxis;

	public bool MotionColorVisualize;

	public Color MotionColor = Color.magenta;

	public List<AssignNickname> AssignNicknameList = new List<AssignNickname>();

	public string GetNickname(TeamType teamType, int creatureIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AssignNicknameList); i++)
		{
			AssignNickname assignNickname = AssignNicknameList[i];
			if (teamType == assignNickname.TeamType && creatureIndex == assignNickname.CreatureIndex)
			{
				return assignNickname.Nickname;
			}
		}
		return string.Empty;
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		ShowObjectBaseAxis = json["ShowObjectBaseAxis"].GETBOOLEAN(ShowObjectBaseAxis);
		MotionColorVisualize = json["MotionColorVisualize"].GETBOOLEAN(MotionColorVisualize);
		MotionColor = JsonTools.ToColor(json["MotionColor"], MotionColor);
		AssignNicknameList.LoadFromJson(json["AssignNicknameList"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("ShowObjectBaseAxis", ShowObjectBaseAxis);
		jsonData.AddFieldBool("MotionColorVisualize", MotionColorVisualize);
		jsonData.AddField("MotionColor", JsonTools.FromColor(MotionColor));
		jsonData.AddFieldList("AssignNicknameList", AssignNicknameList);
		return jsonData;
	}
}
