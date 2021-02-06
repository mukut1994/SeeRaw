import { Component, Input, OnInit } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { RenderComponent } from './../../render.service';
import { BackendService } from '@service/backend.service';
import { highlightAnimation } from './../../animations';

@Component({
  selector: 'app-link-render',
  templateUrl: './link-render.component.html',
  styleUrls: ['./link-render.component.css'],
  animations: [ highlightAnimation ]
})
export class LinkRenderComponent implements RenderComponent {

  @Input() value: any;
  @Input() metadata: LinkMetadata;
  @Input() context: RenderContext;

  highlight: boolean;

  constructor(private backendService: BackendService) { }

  select(childPath: string[]): void {
    if(childPath.length != 0)
      return;

    this.highlight = true;
    setTimeout(() => this.highlight = false, 3.5);
  }


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
