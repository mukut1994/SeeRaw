import { Component, OnInit, Input, ɵɵqueryRefresh, ChangeDetectorRef, ApplicationRef, ChangeDetectionStrategy } from '@angular/core';
import { Message, RenderContext, RenderRoot } from './../data.model';
import { BackendService } from '../backend.service';
import { OptionsService } from './../options.service';

@Component({
  selector: 'app-root-render',
  templateUrl: './root-render.component.html',
  styleUrls: ['./root-render.component.css']
})
export class RootRenderComponent implements OnInit {

  renderRoot: RenderRoot;

  context = new RenderContext('$');

  constructor(private backend: BackendService, private options: OptionsService, private changeDetector: ChangeDetectorRef, private applicationRef: ApplicationRef) { }

  ngOnInit() {
    this.renderRoot = this.backend.renderRoot;
    // this.options.optionsObserveable.subscribe(() => this.ApplicationRef.tick());
    this.backend.messageHandler.subscribe(x => {
      this.changeDetector.markForCheck();
      this.changeDetector.detectChanges();
      this.renderRoot = x;
    });
    this.backend.disconnected.subscribe(x => {
      this.renderRoot = null;
    })
  }
}
