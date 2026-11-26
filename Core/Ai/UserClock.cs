namespace Core.Ai
{
    /// <summary>
    /// The viewer's timezone. Used to turn a civil local wall time into a UTC instant and back.
    /// Unknown clocks leave stored values untouched (legacy rows are civil local).
    /// </summary>
    public interface IUserClock
    {
        bool IsKnown { get; }
        TimeSpan Offset { get; }
        TimeZoneInfo? Zone { get; }
        DateTime ToUtc(DateTime civilLocal);
        DateTime ToCivil(DateTime utcStored);
        (DateTime UtcStart, DateTime UtcEnd) UtcWindow(DateTime civilDay);
    }

    public sealed class UserClock : IUserClock
    {
        public static readonly UserClock Unknown = new(false, TimeSpan.Zero, null);

        private UserClock(bool isKnown, TimeSpan offset, TimeZoneInfo? zone)
        {
            IsKnown = isKnown;
            Offset = offset;
            Zone = zone;
        }

        public bool IsKnown { get; }
        public TimeSpan Offset { get; }
        public TimeZoneInfo? Zone { get; }

        public static UserClock FromClient(string? iana, int? offsetMinutes)
        {
            if (offsetMinutes is < -14 * 60 or > 14 * 60)
            {
                offsetMinutes = null;
            }

            TimeZoneInfo? zone = null;
            if (!string.IsNullOrWhiteSpace(iana))
            {
                try
                {
                    zone = TimeZoneInfo.FindSystemTimeZoneById(iana.Trim());
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            if (zone == null && offsetMinutes == null)
            {
                return Unknown;
            }

            var offset = offsetMinutes.HasValue
                ? TimeSpan.FromMinutes(offsetMinutes.Value)
                : zone!.GetUtcOffset(DateTime.UtcNow);
            return new UserClock(true, offset, zone);
        }

        public DateTime ToUtc(DateTime civilLocal)
        {
            var local = DateTime.SpecifyKind(civilLocal, DateTimeKind.Unspecified);
            if (!IsKnown)
            {
                return local;
            }

            if (Zone != null)
            {
                if (Zone.IsInvalidTime(local))
                {
                    local = local.AddHours(1);
                }

                var utc = TimeZoneInfo.ConvertTimeToUtc(local, Zone);
                return DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            }

            return new DateTimeOffset(local, Offset).UtcDateTime;
        }

        public DateTime ToCivil(DateTime utcStored)
        {
            if (!IsKnown)
            {
                return DateTime.SpecifyKind(utcStored, DateTimeKind.Unspecified);
            }

            var utc = DateTime.SpecifyKind(utcStored, DateTimeKind.Utc);
            var civil = Zone != null
                ? TimeZoneInfo.ConvertTimeFromUtc(utc, Zone)
                : utc.Add(Offset);
            return DateTime.SpecifyKind(civil, DateTimeKind.Unspecified);
        }

        public (DateTime UtcStart, DateTime UtcEnd) UtcWindow(DateTime civilDay)
        {
            var start = DateTime.SpecifyKind(civilDay.Date, DateTimeKind.Unspecified);
            return (ToUtc(start), ToUtc(start.AddDays(1)));
        }
    }

    /// <summary>
    /// Request-scoped clock filled from <c>X-Time-Zone</c> and <c>X-Utc-Offset-Minutes</c>.
    /// </summary>
    public sealed class UserClockAccessor : IUserClock
    {
        private IUserClock _clock = UserClock.Unknown;

        public void Use(IUserClock clock) => _clock = clock ?? UserClock.Unknown;

        public bool IsKnown => _clock.IsKnown;
        public TimeSpan Offset => _clock.Offset;
        public TimeZoneInfo? Zone => _clock.Zone;
        public DateTime ToUtc(DateTime civilLocal) => _clock.ToUtc(civilLocal);
        public DateTime ToCivil(DateTime utcStored) => _clock.ToCivil(utcStored);
        public (DateTime UtcStart, DateTime UtcEnd) UtcWindow(DateTime civilDay) => _clock.UtcWindow(civilDay);
    }

    /// <summary>
    /// Local "now" at the moment of a voice note, in the speaker's zone.
    /// </summary>
    public sealed class RecordingClock
    {
        public RecordingClock(DateTime localNow, IUserClock zone)
        {
            LocalNow = DateTime.SpecifyKind(localNow, DateTimeKind.Unspecified);
            Zone = zone ?? UserClock.Unknown;
        }

        public DateTime LocalNow { get; }
        public IUserClock Zone { get; }

        public DateTime ToUtc(DateTime civilLocal) => Zone.ToUtc(civilLocal);

        public static RecordingClock FromClient(string? clientNow, string? timeZone, int? utcOffsetMinutes)
        {
            var zone = UserClock.FromClient(timeZone, utcOffsetMinutes);
            return new RecordingClock(ParseLocalNow(clientNow, zone), zone);
        }

        private static DateTime ParseLocalNow(string? text, IUserClock zone)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return zone.IsKnown ? zone.ToCivil(DateTime.UtcNow) : DateTime.Now;
            }

            var trimmed = text.Trim();
            if (!DateTimeOffset.TryParse(
                    trimmed,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                return zone.IsKnown ? zone.ToCivil(DateTime.UtcNow) : DateTime.Now;
            }

            var hasZone = trimmed.EndsWith('Z')
                || trimmed.EndsWith('z')
                || System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"[+-]\d{2}:\d{2}$");
            if (!hasZone)
            {
                return DateTime.SpecifyKind(parsed.DateTime, DateTimeKind.Unspecified);
            }

            return zone.IsKnown
                ? zone.ToCivil(parsed.UtcDateTime)
                : DateTime.SpecifyKind(parsed.DateTime, DateTimeKind.Unspecified);
        }
    }
}
