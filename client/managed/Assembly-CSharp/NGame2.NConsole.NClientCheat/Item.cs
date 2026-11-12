using NShared;

namespace NGame2.NConsole.NClientCheat;

public class Item : ICheatCommand
{
	string ICheatCommand.Help()
	{
		return "item [name or key]";
	}

	void ICheatCommand.Process(string[] tokens, out string output)
	{
		output = string.Empty;
		int valueInt = ArgParser.GetValueInt(tokens, 1);
		if (valueInt != 0)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(valueInt);
			output = GetItemDesc(data);
			return;
		}
		string valueString = ArgParser.GetValueString(tokens, 1);
		foreach (ItemData allDatum in GameRoot.Instance.ItemDataContainer.GetAllData())
		{
			if (allDatum.Name.Contains(valueString))
			{
				output += (string.IsNullOrEmpty(output) ? "" : "\n");
				output += GetItemDesc(allDatum);
			}
		}
	}

	private string GetItemDesc(ItemData itemData)
	{
		if (itemData == null)
		{
			return string.Empty;
		}
		string text = "";
		text += itemData.Name;
		text = text + " (Key=" + itemData.Index + " Code=" + itemData.Code + ")";
		text = text + " Type=" + itemData.Type;
		return text + " Grade=" + (int)itemData.Grade;
	}
}
