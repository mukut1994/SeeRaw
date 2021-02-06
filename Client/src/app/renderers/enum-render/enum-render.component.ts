import { Component, Input, OnInit } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { RenderComponent } from './../../render.service';
import { highlightAnimation } from './../../animations';

@Component({
  selector: 'app-enum-render',
  templateUrl: './enum-render.component.html',
  styleUrls: ['./enum-render.component.css'],
  animations: [ highlightAnimation ]
})
export class EnumRenderComponent implements RenderComponent {

  @Input() metadata: EnumMetadata;
  @Input() value: any;
  @Input() context: RenderContext;

  highlight: boolean;

  select(childPath: string[]): void {
    if(childPath.length != 0)
      return;

    this.highlight = true;
    setTimeout(() => this.highlight = false, 3.5);
  }

}

class EnumMetadata extends Metadata {
  values: any[]
}
