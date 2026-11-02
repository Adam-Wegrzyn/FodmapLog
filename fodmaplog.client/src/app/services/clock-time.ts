/** Value for `<input type="time">` (HH:mm, 24-hour, local). */
export function formatClockTime(date: Date = new Date()): string {
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
}
