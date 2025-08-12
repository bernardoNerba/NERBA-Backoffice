/**
 * Converts a date string from "dd/MM/yyyy" format to "yyyy-MM-dd" format,
 * which is compatible with C# DateOnly when deserializing from JSON.
 *
 * @param dateString - The date string to convert, expecting "dd/MM/yyyy".
 * @returns The formatted date string "yyyy-MM-dd".
 */
export function matchDateOnly(dateString: string): string {
  if (!dateString) return '';

  const parts = dateString.split('/');
  if (parts.length !== 3) {
    console.error("Invalid date format: Expected 'dd/MM/yyyy'");
    return '';
  }

  const [day, month, year] = parts;

  // Pad day and month with a leading zero if they are single-digit
  const paddedDay = day.padStart(2, '0');
  const paddedMonth = month.padStart(2, '0');
  return `${year}-${paddedMonth}-${paddedDay}`;
}

/**
 * Converts a date to "yyyy-MM-dd" format,
 * which is compatible with C# DateOnly when deserializing from JSON.
 *
 * @param date - The date to convert, expecting "dd/MM/yyyy".
 * @returns The formatted date string "yyyy-MM-dd".
 */
export function formatDateForApi(date: Date): string {
  const year = date.getFullYear();
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/**
 * Converts a date string from "yyyy-MM-dd" format to "dd/MM/yyyy" format,
 * which is the standart for portuguese culture.
 *
 * @param dateString - The date string to convert, expecting "yyyy-MM-dd".
 * @returns The formatted date string "dd/MM/yyyy".
 */
export function convertDateOnlyToPtDate(dateString: string): string {
  if (!dateString) return '';
  const [year, month, day] = dateString.split('-');
  return `${day}/${month}/${year}`;
}

/**
 * Converts a 24-hour time string (e.g., "14:30", "09:15") to a Date object
 * @param timeString - Time in format "HH:mm" or "HH:mm:ss"
 * @returns Date object with today's date and the specified time
 */
export function stringToTimeObject(timeString: string): Date {
  const [hours, minutes, seconds = '0'] = timeString.split(':');

  const timeObject = new Date();
  timeObject.setHours(parseInt(hours, 10));
  timeObject.setMinutes(parseInt(minutes, 10));
  timeObject.setSeconds(parseInt(seconds, 10));
  timeObject.setMilliseconds(0);

  return timeObject;
}

/**
 * Converts a date into a portuguese week day
 * @param date - The date to convert
 * @returns The portuguese week day as a string
 */
export function getWeekDayPT(date: Date): string {
  const days = [
    'domingo',
    'segunda-feira',
    'terça-feira',
    'quarta-feira',
    'quinta-feira',
    'sexta-feira',
    'sábado',
  ];
  return days[date.getDay()];
}
