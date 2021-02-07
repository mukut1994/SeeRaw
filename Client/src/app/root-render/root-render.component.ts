import { Component, OnInit, Input, ɵɵqueryRefresh, ChangeDetectorRef, ApplicationRef, ChangeDetectionStrategy, ViewChildren, QueryList } from '@angular/core';
import { Message, RenderContext, RenderRoot } from './../data.model';
import { BackendService } from '../backend.service';
import { OptionsService } from './../options.service';
import { GotoService } from './../goto.service';
import { RenderDirective } from './../directives/render.directive';

@Component({
  selector: 'app-root-render',
  templateUrl: './root-render.component.html',
  styleUrls: ['./root-render.component.css']
})
export class RootRenderComponent implements OnInit {

  renderRoot: RenderRoot;

  @ViewChildren(RenderDirective) children: QueryList<RenderDirective>;

  constructor(private backend: BackendService, private gotoService: GotoService, private changeDetector: ChangeDetectorRef, private applicationRef: ApplicationRef) { }

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
    });
  }

  context(index: number) {
    return new RenderContext('$', index);
  }

  goto(i: number, path: string) {
    this.gotoService.navigateTo(path)
  }
}
