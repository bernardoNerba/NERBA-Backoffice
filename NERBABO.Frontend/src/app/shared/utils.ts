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

/**
 * Converts decimal hours to "HH:mm" format
 * @param hours - The decimal hours (e.g., 1.5 for 1 hour and 30 minutes)
 * @returns Time string in "HH:mm" format
 */
export function hoursToTimeFormat(hours: number): string {
  if (hours < 0) return '00:00';

  const wholeHours = Math.floor(hours);
  const minutes = Math.round((hours - wholeHours) * 60);

  const paddedHours = wholeHours.toString().padStart(2, '0');
  const paddedMinutes = minutes.toString().padStart(2, '0');

  return `${paddedHours}:${paddedMinutes}`;
}

export function convertHoursMinutesToDecimal(
  hours: number,
  minutes: number
): number {
  return hours + minutes / 60;
}

/**
 * Splits a full name into first name and last name.
 * Last word becomes last name, everything else becomes first name.
 *
 * @param fullName - The complete name to split
 * @returns Object with firstName and lastName
 * @throws Error if fullName has less than 2 words
 *
 * Examples:
 * - "João Silva" → { firstName: "João", lastName: "Silva" }
 * - "João Silva Santos" → { firstName: "João Silva", lastName: "Santos" }
 * - "José María García" → { firstName: "José María", lastName: "García" }
 */
export function splitFullName(fullName: string): {
  firstName: string;
  lastName: string;
} {
  const trimmed = fullName.trim();

  if (!trimmed) {
    throw new Error('Nome completo não pode estar vazio');
  }

  // Split by one or more whitespace characters
  const parts = trimmed.split(/\s+/);

  if (parts.length < 2) {
    throw new Error('Nome completo deve conter pelo menos duas palavras');
  }

  const lastName = parts[parts.length - 1];
  const firstName = parts.slice(0, -1).join(' ');

  return { firstName, lastName };
}

/**
 * Validator for full name format.
 * Ensures the name contains at least 2 words separated by space.
 * This validator is intended for use with Angular reactive forms.
 *
 * @param control - The form control to validate
 * @returns ValidationErrors object if invalid, null if valid
 */
export function fullNameValidator(
  control: any
): { [key: string]: any } | null {
  if (!control.value) {
    return null; // Let required validator handle empty values
  }

  const trimmed = control.value.trim();
  const parts = trimmed.split(/\s+/);

  if (parts.length < 2) {
    return {
      fullNameFormat: {
        message:
          'Nome completo deve conter pelo menos nome próprio e apelido separados por espaço',
      },
    };
  }

  return null;
}
