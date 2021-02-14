import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { Metadata, RenderContext, Option } from '@data/data.model';
import { HighlightDirective } from './../../directives/highlight.directive';
import { of } from 'rxjs';
import { RenderComponent } from './../../render.service';

@Component({
  selector: 'app-log-render',
  templateUrl: './log-render.component.html',
  styleUrls: ['./log-render.component.css']
})
export class LogRenderComponent implements RenderComponent {
  @Input() context: RenderContext;
  @Input() value: Log;
  @Input() metadata: Metadata;
  @Input() options: Option;

  @ViewChild(HighlightDirective) view;

  expand(path) { return of(this.view) }
}

class Log {
  message:string;
  children:Log[]
}
