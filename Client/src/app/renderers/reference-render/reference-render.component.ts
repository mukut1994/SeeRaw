import { Component, OnInit, Input } from '@angular/core';
import { Metadata, RenderContext, Option } from '@data/data.model';
import { HighlightDirective } from '@data/directives/highlight.directive';
import { Observable } from 'rxjs';
import { RenderComponent } from './../../render.service';
import { GotoService } from './../../goto.service';

@Component({
  selector: 'app-reference-render',
  templateUrl: './reference-render.component.html',
  styleUrls: ['./reference-render.component.css']
})
export class ReferenceRenderComponent implements RenderComponent {

  @Input() value: any;
  @Input() metadata: Metadata;
  @Input() context: RenderContext;
  @Input() options: Option;

  constructor(private gotoService: GotoService) { }

  expand(path: string) {
     return null;
  }

  click() {
    this.gotoService.navigateTo(this.value);
  }

}
