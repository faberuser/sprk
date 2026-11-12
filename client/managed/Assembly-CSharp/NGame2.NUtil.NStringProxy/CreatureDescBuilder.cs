using NShared;
using NVespa.NGlobal;

namespace NGame2.NUtil.NStringProxy;

public class CreatureDescBuilder : IBuilder
{
	private CreatureData _creatureData;

	private string _visualInfo;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public CreatureDescBuilder(CreatureData creatureData, string visualInfo, bool appendLine)
	{
		_creatureData = creatureData;
		_visualInfo = visualInfo;
		_appendLine = appendLine;
	}

	public string Build()
	{
		return GetCreatureDesc(_creatureData, _visualInfo);
	}

	public static string GetCreatureDesc(CreatureData creatureData, string visualInfo)
	{
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureDesc(creatureData, visualInfo, LocalizationManager.GetStringDirect, out var creatureDesc);
		return creatureDesc;
	}
}
