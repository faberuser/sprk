using System.Collections.Generic;
using JsonBin;
using UnityEngine;

namespace NGameAsset.NSound;

public class SoundData : ILoadJson
{
	private class ClipFieldName
	{
		public string Name;

		public string Volume;

		public string Type;

		public string BeginTickMs;
	}

	private static bool _clipFieldNameInitialized;

	private static List<ClipFieldName> _clipFieldNameList = new List<ClipFieldName>();

	public string Code = string.Empty;

	public string CreatureCode = string.Empty;

	public SystemLanguage Language = SystemLanguage.Unknown;

	public bool ContainsLoop;

	public List<ClipData> ClipList = new List<ClipData>();

	private static void InitializeFieldName()
	{
		if (!_clipFieldNameInitialized)
		{
			_clipFieldNameInitialized = true;
			_clipFieldNameList.Clear();
			for (int i = 0; i < 100; i++)
			{
				string text = $"Clip{i}";
				ClipFieldName clipFieldName = new ClipFieldName();
				clipFieldName.Name = text + "Name";
				clipFieldName.Volume = text + "Volume";
				clipFieldName.Type = text + "Type";
				clipFieldName.BeginTickMs = text + "BeginTickMs";
				_clipFieldNameList.Add(clipFieldName);
			}
		}
	}

	bool ILoadJson.Load(JsonRead json)
	{
		InitializeFieldName();
		Code = json["Code"].GETSTRING(string.Empty);
		CreatureCode = json["CreatureCode"].GETSTRING(CreatureCode);
		Language = json["Language"].GETENUM(Language);
		for (int i = 1; i < 100; i++)
		{
			ClipFieldName clipFieldName = _clipFieldNameList[i];
			string sTRING = json[clipFieldName.Name].GETSTRING(string.Empty);
			if (String_Extension_Method.IsNullOrEmpty(sTRING))
			{
				break;
			}
			int iNT = json[clipFieldName.Volume].GETINT(1000);
			ClipData.EPlayType eNUM = json[clipFieldName.Type].GETENUM(ClipData.EPlayType.Once);
			JsonRead jsonRead = json[clipFieldName.BeginTickMs];
			int[] array;
			if (!jsonRead.IsArray())
			{
				array = new int[1] { 0 };
			}
			else
			{
				array = new int[jsonRead.Count];
				for (int j = 0; j < jsonRead.Count; j++)
				{
					string sTRING2 = jsonRead.GetFromArray(j).GETSTRING(string.Empty);
					array[j] = Vespa_String_Extension_Method.TOINT(sTRING2, 0);
				}
			}
			if (iNT > 0)
			{
				ClipData clipData = new ClipData();
				clipData.Open(sTRING, iNT, array, eNUM);
				ClipList.Add(clipData);
				if (clipData.PlayType == ClipData.EPlayType.Loop)
				{
					ContainsLoop = true;
				}
			}
		}
		return true;
	}
}
