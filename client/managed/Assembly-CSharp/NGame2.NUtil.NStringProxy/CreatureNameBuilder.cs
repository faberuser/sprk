using NShared;
using NVespa.NGlobal;

namespace NGame2.NUtil.NStringProxy;

public class CreatureNameBuilder : IBuilder
{
	private CreatureData _creatureData;

	private string _visualInfo;

	private int _level;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public CreatureNameBuilder(CreatureData creatureData, string visualInfo, int level, bool appendLine)
	{
		_creatureData = creatureData;
		_visualInfo = visualInfo;
		_level = level;
		_appendLine = appendLine;
	}

	public string Build()
	{
		return GetCreatureName(_creatureData, _visualInfo, _level);
	}

	public static string GetCreatureName(CreatureData creatureData, string visualInfo, int level)
	{
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(creatureData, visualInfo, LocalizationManager.GetStringDirect, out var creatureName, out var _);
		string text = creatureName;
		if (level > 0)
		{
			text = $"Lv{level} {text}";
		}
		return text;
	}
}
