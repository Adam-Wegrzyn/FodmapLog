/** YYYY-MM-DD in the user's local calendar (not UTC). */
export function formatDateInput(date: Date = new Date()): string {
  const year = date.getFullYear();
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/** HH:mm for `<input type="time">` (local, 24-hour). */
export function formatClockTime(date: Date = new Date()): string {
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
}

/** Normalize picker/locale strings to HH:mm. */
export function normalizeClockTime(time: string | null | undefined): string {
  const raw = (time || '').trim();
  const ampm = raw.match(/^(\d{1,2}):(\d{2})(?::\d{2})?\s*([AaPp][Mm])?$/);
  if (ampm) {
    let hours = Number(ampm[1]);
    const minutes = ampm[2];
    const mer = ampm[3]?.toLowerCase();
    if (mer === 'pm' && hours < 12) {
      hours += 12;
    }
    if (mer === 'am' && hours === 12) {
      hours = 0;
    }
    return `${hours.toString().padStart(2, '0')}:${minutes}`;
  }
  return formatClockTime();
}

/** Local wall-clock ISO without Z. The API converts this to UTC when it knows the zone. */
export function combineLocalDateTime(date: string, time: string): string {
  return `${date}T${normalizeClockTime(time)}:00`;
}

/** YYYY-MM-DDTHH:mm:ss in the user's local zone, no Z. */
export function formatLocalDateTime(date: Date = new Date()): string {
  return combineLocalDateTime(formatDateInput(date), formatClockTime(date));
}

/** IANA zone from the browser, for example Asia/Bangkok. */
export function ianaTimeZone(): string {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC';
  } catch {
    return 'UTC';
  }
}

/** Minutes east of UTC. Asia/Bangkok is 420. */
export function utcOffsetMinutes(date: Date = new Date()): number {
  return -date.getTimezoneOffset();
}

/**
 * Keep a civil local timestamp. Strings without a zone are already wall-clock.
 * Instants with Z or an offset are converted to this browser's local wall clock.
 */
export function asCivilDateTime(value: string | Date | null | undefined): string {
  if (value == null || value === '') {
    return formatLocalDateTime();
  }
  if (typeof value === 'string') {
    const trimmed = value.trim();
    const zoned = /[zZ]$/.test(trimmed) || /[+-]\d{2}:\d{2}$/.test(trimmed);
    if (!zoned) {
      const match = trimmed.match(/^(\d{4}-\d{2}-\d{2})[T ](\d{2}:\d{2})(?::(\d{2}))?/);
      if (match) {
        return `${match[1]}T${match[2]}:${match[3] ?? '00'}`;
      }
    }
  }
  const parsed = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return formatLocalDateTime();
  }
  return combineLocalDateTime(formatDateInput(parsed), formatClockTime(parsed));
}
