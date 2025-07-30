/**
    2  * Converts a date string from "dd/MM/yyyy" format to "yyyy-MM-dd"
      format,
    3  * which is compatible with C# DateOnly when deserializing from JSON.
    4  *
    5  * @param dateString - The date string to convert, expecting
      "dd/MM/yyyy".
    6  * @returns The formatted date string "yyyy-MM-dd".
    7  */
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

export function convertDateOnlyToPtDate(dateString: string): string {
  if (!dateString) return '';
  const [year, month, day] = dateString.split('-');
  return `${day}/${month}/${year}`;
}
