import { Component, OnInit, Input } from '@angular/core';
import { OptionComponent } from './option/option.component';
import { RenderContext } from '@data/data.model';
import { Metadata } from 'src/app/data.model';
import { RenderComponent } from './../../render.service';
import { highlightAnimation } from './../../animations';

@Component({
  selector: 'app-value-render',
  templateUrl: './value-render.component.html',
  styleUrls: ['./value-render.component.css'],
  animations: [ highlightAnimation ]
})
export class ValueRenderComponent implements RenderComponent {

  public static option = OptionComponent;

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;

  style: string;
  highlight: boolean;

  select(childPath: string[]): void {
    if(childPath.length != 0)
      return;

    this.highlight = true;
    setTimeout(() => this.highlight = false, 3.5);
  }

}
