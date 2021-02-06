import { Component, OnInit, Input } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { RenderComponent } from './../../render.service';
import { highlightAnimation } from './../../animations';

@Component({
  selector: 'app-log-render',
  templateUrl: './log-render.component.html',
  styleUrls: ['./log-render.component.css'],
  animations: [ highlightAnimation ]
})
export class LogRenderComponent implements RenderComponent {
  @Input() context: RenderContext;
  @Input() value: Log;
  @Input() metadata: Metadata;

  highlight: boolean;

  select(childPath: string[]): void {
    if(childPath.length != 0)
      return;

    this.highlight = true;
    setTimeout(() => this.highlight = false, 3.5);
  }
}

class Log {
  message:string;
  children:Log[]
}
