import { Component, Input } from '@angular/core';
import { TruncatePipe } from '../../pipes/truncate.pipe';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-icon-anchor',
  imports: [TooltipModule],
  template: ` @if (field) {
    <a
      style="text-decoration: none"
      class="p-button p-button-text p-button-secondary text-truncate"
      [href]="link"
      tooltipPosition="right"
      [pTooltip]="tooltipContent"
      [autoHide]="false"
    >
      <i [class]="icon"></i>
      {{ display }}
    </a>
    <ng-template #tooltipContent>
      <small
        ><b class="text-break">{{ field }}</b></small
      >
    </ng-template>
    } @else { N/A }`,
})
export class IconAnchorComponent {
  @Input({ required: true }) field?: string | null;
  @Input({ required: true }) link?: string | null;
  @Input({ required: true }) display?: string | null;
  @Input({ required: true }) icon?: string | null;
}
