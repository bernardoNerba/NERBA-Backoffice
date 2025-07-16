export function matchDateOnly(dateString: string): string {
  const date = new Date(dateString);
  return date.toISOString().split('T')[0];
}

export function convertDateOnlyToPtDate(dateString: string): string {
  if (!dateString) return '';
  const [year, month, day] = dateString.split('-');
  return `${day}/${month}/${year}`;
}
