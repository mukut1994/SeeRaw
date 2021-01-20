import { Component, Input, OnInit } from '@angular/core';
import { Metadata } from '../data.model';
import { RenderContext } from './../data.model';

@Component({
  selector: 'app-enum-render',
  templateUrl: './enum-render.component.html',
  styleUrls: ['./enum-render.component.css']
})
export class EnumRenderComponent {

  @Input() metadata: EnumMetadata;
  @Input() value: any;
  @Input() context: RenderContext;

}

class EnumMetadata {
  values: any[]
}
