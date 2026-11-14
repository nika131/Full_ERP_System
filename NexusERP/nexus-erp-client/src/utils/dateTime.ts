/** UTC event instants are different from date-only business fields. */
export function localCalendarDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
export function localCalendarDisplay(value: string): string {
  const date = value.slice(0, 10);
  return new Date(`${date}T00:00:00Z`).toLocaleDateString(undefined, { timeZone: 'UTC' });
}
export function deviceTimeZone(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone;
}
export function localInstantDisplay(value: string): string {
  return new Date(value).toLocaleString();
}
