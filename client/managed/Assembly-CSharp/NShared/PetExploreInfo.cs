using System.Collections.Generic;
using NVespa;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class PetExploreInfo : IAccountAttrKey<PetExploreInfoKey>
{
	[NonDbSchema]
	private PetExploreInfoKey _key;

	[NonDbSchema]
	private List<int> _petIndexList;

	public int ExploreIndex { get; set; }

	public int DeckIndex { get; set; }

	public string PetIndices { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public int RewardCount { get; set; }

	public int RewardRate { get; set; }

	public int RewardGrade { get; set; }

	PetExploreInfoKey IAccountAttrKey<PetExploreInfoKey>.Key
	{
		get
		{
			if (_key == null)
			{
				_key = new PetExploreInfoKey(ExploreIndex, DeckIndex);
			}
			return _key;
		}
	}

	public List<int> GetPetIndexList()
	{
		if (PetIndices == null || PetIndices.Length == 0)
		{
			return null;
		}
		if (_petIndexList != null && _petIndexList.Count > 0)
		{
			return _petIndexList;
		}
		string[] array = PetIndices.Split(',');
		foreach (string text in array)
		{
			int result = 0;
			if (string.IsNullOrEmpty(text) || !int.TryParse(text, out result))
			{
				VespaDebug.LogError("Invalid PetIndex");
			}
			else if (result != 0)
			{
				if (_petIndexList == null)
				{
					_petIndexList = new List<int>(array.Length);
				}
				_petIndexList.Add(result);
			}
		}
		return _petIndexList;
	}
}
