import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { RenderComponent } from './../../render.service';
import { BackendService } from '@service/backend.service';
import { HighlightDirective } from './../../directives/highlight.directive';
import { of } from 'rxjs';

@Component({
  selector: 'app-link-render',
  templateUrl: './link-render.component.html',
  styleUrls: ['./link-render.component.css']
})
export class LinkRenderComponent implements RenderComponent, OnInit {

  @Input() value: any;
  @Input() metadata: LinkMetadata;
  @Input() context: RenderContext;

  @ViewChild(HighlightDirective) btn;

  constructor(private backendService: BackendService) { }

  ngOnInit(): void {
  }

  expand(path) { return of(this.btn) }

  send() {
    const message = {
      id: this.metadata.id,
      type: 'execute'
    };

    this.backendService.sendMessage(JSON.stringify(message));
  }
}

class LinkMetadata extends Metadata {
  id: string;
}
