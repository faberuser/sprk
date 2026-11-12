using System;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow.NMonthlyHero;

public class CalendalDate
{
	private const string LocaleKey = "BIRTH_MONTH_{0}";

	private DateTime _minDate = DateTime.MinValue;

	private DateTime _maxDate = DateTime.MaxValue;

	private DateTime _dateTime = DateTime.UtcNow;

	private Action _onChangeDate;

	public CalendalDate(DateTime initDate, Action onChangeMonth)
	{
		_dateTime = initDate;
		_onChangeDate = onChangeMonth;
	}

	public void SetMinimum(DateTime minDate)
	{
		_minDate = new DateTime(minDate.Year, minDate.Month, 1, 0, 0, 0);
		if (_dateTime < _minDate)
		{
			SetDate(_minDate);
		}
	}

	public void SetMaximum(DateTime maxDate)
	{
		_maxDate = new DateTime(maxDate.Year, maxDate.Month, DateTime.DaysInMonth(maxDate.Year, maxDate.Month), 23, 59, 59);
		if (_maxDate < _dateTime)
		{
			SetDate(_maxDate);
		}
	}

	public void AddMonth(int add)
	{
		DateTime dateTime = _dateTime.AddMonths(add);
		if (IsValidDate(dateTime))
		{
			SetDate(dateTime);
		}
	}

	private bool IsValidDate(DateTime newDate)
	{
		if (newDate < _minDate || _maxDate < newDate)
		{
			return false;
		}
		return true;
	}

	public void SetDate(DateTime newDate)
	{
		if (!(_dateTime == newDate))
		{
			_dateTime = newDate;
			Vespa_System_Action_Extension_Method.fire(_onChangeDate);
		}
	}

	public DateTime GetDateTime()
	{
		return _dateTime;
	}

	public string GetLocale()
	{
		return LocalizationManager.GetStringDirect($"BIRTH_MONTH_{_dateTime.Month}");
	}
}
