import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'formatDateRange',
  standalone: true,
})
export class FormatDateRangePipe implements PipeTransform {
  transform(dates: [string | Date, string | Date]): string {
    const [start, end] = dates.map((d) => new Date(d));

    const startDay = start.getDate();
    const startMonth = start.toLocaleString('pt-PT', { month: 'short' });
    const startYear = start.getFullYear();

    const endDay = end.getDate();
    const endMonth = end.toLocaleString('pt-PT', { month: 'short' });
    const endYear = end.getFullYear();

    // Removes the final dot on month abreviation (ex: "set." -> "set")
    const cleanStartMonth = startMonth.replace('.', '');
    const cleanEndMonth = endMonth.replace('.', '');

    if (startYear !== endYear) {
      // Diferent Years: 10 dez 2025 a 15 fev 2026
      return `De ${startDay} ${cleanStartMonth} ${startYear} a ${endDay} ${cleanEndMonth} ${endYear}`;
    }

    if (cleanStartMonth !== cleanEndMonth) {
      // Same year, diferent month: 5 jul a 20 set 2025
      return `De ${startDay} ${cleanStartMonth} a ${endDay} ${cleanEndMonth} de ${startYear}`;
    }

    // Same month and year: 5 a 10 de jul de 2025
    return `De ${startDay} a ${endDay} de ${cleanEndMonth} de ${startYear}`;
  }
}
