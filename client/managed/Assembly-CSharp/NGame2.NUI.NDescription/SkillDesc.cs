using LitJson;
using NGame2.NNodeTree.NTypeValue;
using NShared;
using NVespa;

namespace NGame2.NUI.NDescription;

public class SkillDesc : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public int SkillIndex;

	private ArgumentSingleGeneric<string> _skillDesc;

	public string SkillName
	{
		get
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(SkillIndex);
			if (data != null)
			{
				return SkillDescContainer.GetSkillName(data);
			}
			return "(noname)";
		}
	}

	public SkillDesc()
	{
		_skillDesc = new ArgumentSingleGeneric<string>(string.Empty);
	}

	public string GetDescription(StatData statData, int creatureIndex, int creatureStar, int creatureTranscended, int skillLevel)
	{
		SkillDescVariable skillDescVariable = SmallPooling<SkillDescVariable>.CreateInstance();
		skillDescVariable.Open(statData, creatureIndex, creatureStar, creatureTranscended, SkillIndex, skillLevel);
		string value = _skillDesc.GetValue(skillDescVariable);
		SmallPooling<SkillDescVariable>.ReleaseInstance(skillDescVariable);
		return value;
	}

	bool ILoadJson.Load(JsonData json)
	{
		SkillIndex = json["SkillIndex"].GETINT(SkillIndex);
		_skillDesc.LoadFromJson(json["SkillDesc"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("SkillIndex", SkillIndex);
		jsonData.AddField("SkillDesc", _skillDesc.MakeJson());
		return jsonData;
	}
}
