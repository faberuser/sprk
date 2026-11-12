using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class CountryDataContainer : IJitDataContainer, IGetData_for_GMS<string, CountryData>, IGetAllData_for_GMS<CountryData>
{
	public bool IsLoaded => GetDataContainer_Base().IsLoaded;

	void IJitDataContainer.Load()
	{
	}

	private CountryDataContainer_Base GetDataContainer_Base()
	{
		CountryDataContainer_FromPatch dataContainer = TableRoot.Instance.GetDataContainer<CountryDataContainer_FromPatch>();
		if (dataContainer != null)
		{
			return dataContainer;
		}
		return TableRoot.Instance.GetDataContainer<CountryDataContainer_InApp>();
	}

	public CountryData GetData(string key)
	{
		return GetDataContainer_Base().GetData(key);
	}

	public CountryData GetFirstData()
	{
		return GetDataContainer_Base().GetFirstData();
	}

	public IList<CountryData> GetAllData()
	{
		return GetDataContainer_Base().GetAllData();
	}

	public string[] GetDefaultAgreement()
	{
		return GetDataContainer_Base().GetDefaultAgreement();
	}
}
