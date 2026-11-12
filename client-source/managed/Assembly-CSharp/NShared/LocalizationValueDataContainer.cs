using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NShared.NJit;
using NShared.NLocalization;

namespace NShared;

public class LocalizationValueDataContainer : JitDataContainerDefault<LanguageType, LocalizationValueData>, IData_from_PreloadInAppBytes
{
	public const string TABLENAME = "Basic/Localization_Language";

	private JitRef<List<LanguageType>> _languageList = new JitRef<List<LanguageType>>();

	private CultureInfo _localiztionCultureInfo;

	public override TablePathDefine PathDefine => new TablePathDefine("Basic/Localization_Language", TablePathDefine.ELoadFrom.LocalizationRepo);

	public LocalizationValueDataContainer()
	{
		base.Packer.RegisterAndBind(_languageList);
	}

	protected override void OnEndLoadFromJsonData()
	{
		List<LanguageType> value = (from v in base.BaseList.JsonLoad_GetOrNew()
			select v.Language).ToList();
		_languageList.JsonLoad_Set(value);
	}

	public bool IsContain(LanguageType language)
	{
		return GetData(language) != null;
	}

	public CultureInfo GetCultureInfo()
	{
		return _localiztionCultureInfo;
	}

	public CultureInfo CreateLocalizationCultureInfo(LanguageType language)
	{
		LocalizationValueData data = GetData(language);
		_localiztionCultureInfo = CultureInfo.CreateSpecificCulture(data.CultureInfoName);
		return _localiztionCultureInfo;
	}

	public IEnumerable<LanguageType> GetAllLanguages()
	{
		return _languageList.Eval();
	}
}
