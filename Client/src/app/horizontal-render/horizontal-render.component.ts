import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-horizontal-render',
  templateUrl: './horizontal-render.component.html',
  styleUrls: ['./horizontal-render.component.css']
})
export class HorizontalRenderComponent {

  @Input() editable: boolean;
  @Input() target: any;
}
