import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-vertical-render',
  templateUrl: './vertical-render.component.html',
  styleUrls: ['./vertical-render.component.css']
})
export class VerticalRenderComponent  {
  @Input() editable: boolean;
  @Input() target: any;
}
