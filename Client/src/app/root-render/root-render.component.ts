import { Component, OnInit, Input, ɵɵqueryRefresh, ChangeDetectorRef, ApplicationRef, ChangeDetectionStrategy } from '@angular/core';
import { Message, RenderContext, RenderRoot } from './../data.model';
import { BackendService } from '../backend.service';
import { OptionsService } from './../options.service';
import { RenderComponent } from './../render.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-root-render',
  templateUrl: './root-render.component.html',
  styleUrls: ['./root-render.component.css']
})
export class RootRenderComponent implements OnInit {

  renderRoot: RenderRoot;
  children: RenderComponent[] = [];

  @Input() goto: Observable<string[]>;

  constructor(private backend: BackendService, private options: OptionsService, private changeDetector: ChangeDetectorRef, private applicationRef: ApplicationRef) { }

  ngOnInit() {
    this.renderRoot = this.backend.renderRoot;

    this.backend.messageHandler.subscribe(x => {
      this.changeDetector.markForCheck();
      this.changeDetector.detectChanges();
      this.renderRoot = x;
    });
    this.backend.disconnected.subscribe(x => {
      this.renderRoot = null;
    });

    this.goto.subscribe(x => this.children[0].select(x));
  }

  context(index: number) {
    return new RenderContext('$', index);
  }
}
