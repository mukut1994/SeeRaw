import { Component, Input, OnInit } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { RenderComponent } from './../../render.service';
import { BackendService } from '@service/backend.service';

@Component({
  selector: 'app-link-render',
  templateUrl: './link-render.component.html',
  styleUrls: ['./link-render.component.css']
})
export class LinkRenderComponent implements RenderComponent, OnInit {

  @Input() value: any;
  @Input() metadata: LinkMetadata;
  @Input() context: RenderContext;

  constructor(private backendService: BackendService) { }

  ngOnInit(): void {
  }

  expand(path) {}

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
