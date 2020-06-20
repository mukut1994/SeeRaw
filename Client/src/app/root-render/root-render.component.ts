import { Component, OnInit, Input } from '@angular/core';
import { RenderRoot } from './../data.model';
import { BackendService } from '../backend.service';

@Component({
  selector: 'app-root-render',
  templateUrl: './root-render.component.html',
  styleUrls: ['./root-render.component.css']
})
export class RootRenderComponent implements OnInit {

  renderRoot: RenderRoot;

  constructor(private backend: BackendService) { }

  ngOnInit() {
    this.renderRoot = this.backend.renderRoot;
    this.backend.messageHandler.subscribe(x => {
      this.renderRoot = x;
    });
  }
}
