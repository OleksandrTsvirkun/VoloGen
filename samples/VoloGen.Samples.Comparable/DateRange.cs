using VoloGen;

namespace VoloGen.Samples.Comparable;

/// <summary>
/// A date range value object with comparison by start date, then by end date.
/// Demonstrates multi-field comparison ordering logic.
/// </summary>
[AutoComparable]
public partial struct DateRange
{
    private readonly DateOnly _start;
    private readonly DateOnly _end;

    public DateRange(DateOnly start, DateOnly end)
    {
        _start = start;
        _end = end;
    }

    public DateOnly Start
    {
        get { return _start; }
    }

    public DateOnly End
    {
        get { return _end; }
    }

    public int Days
    {
        get { return _end.DayNumber - _start.DayNumber; }
    }

    /// <summary>
    /// Compare first by start date, then by end date.
    /// Earlier start dates sort first; for same start, shorter ranges sort first.
    /// </summary>
    public static int Compare(DateRange left, DateRange right)
    {
        int startCompare = left._start.CompareTo(right._start);
        if (startCompare != 0)
        {
            return startCompare;
        }
        return left._end.CompareTo(right._end);
    }

    public override bool Equals(object? obj)
    {
        return obj is DateRange other && _start == other._start && _end == other._end;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return _start.GetHashCode() * 397 ^ _end.GetHashCode();
        }
    }

    public override string ToString()
    {
        return $"{_start:yyyy-MM-dd} to {_end:yyyy-MM-dd} ({Days} days)";
    }
}
