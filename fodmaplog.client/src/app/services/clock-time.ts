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

/** Local wall-clock ISO without Z, so SQL and the date pipe keep the same hour. */
export function combineLocalDateTime(date: string, time: string): string {
  return `${date}T${normalizeClockTime(time)}:00`;
}
