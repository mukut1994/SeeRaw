import { Component, OnInit, Input } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';

@Component({
  selector: 'app-log-render',
  templateUrl: './log-render.component.html',
  styleUrls: ['./log-render.component.css']
})
export class LogRenderComponent {
  @Input() context: RenderContext;
  @Input() value: Log;
  @Input() metadata: Metadata;

  expand(path) {}
}

class Log {
  message:string;
  children:Log[]
}
