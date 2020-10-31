import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-enum-render',
  templateUrl: './enum-render.component.html',
  styleUrls: ['./enum-render.component.css']
})
export class EnumRenderComponent {

  @Input() editable: boolean;
  @Input() target: any;

  getValues() {
    return (this.target["enum-values"] as string).split(',').map(a => a.trim());
  }

}
