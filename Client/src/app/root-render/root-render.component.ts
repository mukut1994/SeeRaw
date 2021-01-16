import { Component, OnInit, Input, ɵɵqueryRefresh, ChangeDetectorRef } from '@angular/core';
import { RenderRoot, RenderContext } from './../data.model';
import { BackendService } from '../backend.service';
import { OptionsService } from './../options.service';

@Component({
  selector: 'app-root-render',
  templateUrl: './root-render.component.html',
  styleUrls: ['./root-render.component.css']
})
export class RootRenderComponent implements OnInit {

  renderRoot: RenderRoot;

  context = new RenderContext("$");

  constructor(private backend: BackendService, private options: OptionsService, private changeDetector: ChangeDetectorRef) { }

  ngOnInit() {
    this.renderRoot = this.backend.renderRoot;
    this.options.optionsObserveable.subscribe(() => this.changeDetector.detectChanges());
    this.backend.messageHandler.subscribe(x => {
      this.renderRoot = x;
    });
    this.backend.onDisconnected.subscribe(x => {
      this.renderRoot = null;
    })
  }
}
