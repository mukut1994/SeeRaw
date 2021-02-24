import { Component, Input, OnInit, ViewChildren, QueryList, ElementRef, Directive, ViewRef, ViewContainerRef, OnDestroy } from '@angular/core';
import { OptionComponent, TableOption } from './option/option.component';
import { RenderComponent } from './../../render.service';
import { Metadata, RenderContext, Option } from '@data/data.model';
import { OptionsService } from '@service/options.service';
import { HighlightDirective } from './../../directives/highlight.directive';
import * as jp from 'jsonpath-faster'

@Component({
  selector: 'app-table-render',
  templateUrl: './table-render.component.html',
  styleUrls: ['./table-render.component.css']
})
export class TableRenderComponent implements RenderComponent, OnInit {

  public static option = OptionComponent;

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;
  @Input() options: TableOption;

  @ViewChildren(HighlightDirective) rows: QueryList<HighlightDirective>;

  sessionOptions: SessionOptions;
  keys: any;

  header: any;

  constructor(private optionsService: OptionsService) { }

  ngOnInit() {
    this.keys = Object.keys(this.value);
    this.sessionOptions = this.optionsService.getSessionOptions(this.context, "table");

    if(this.options.headerPath)
      this.header = jp.value(this.value, this.options.headerPath);

    if(this.context.visibleDepth === 0)
      this.sessionOptions.collapsed = false;
  }

  expand(path: string) {
    if(this.collapsed()) this.collapse();

    var props = path.substr(this.context.currentPath.length + 1).split('.');

    return HighlightDirective.findWithKey(this.rows, props[0]);
  }

  collapsed() {
    return this.sessionOptions.collapsed ?? this.context.visibleDepth > 2;
  }

  collapse() {
    if(this.context.visibleDepth === 0)
      return;

    this.sessionOptions.collapsed = !this.collapsed();
  }
}

class SessionOptions {
  collapsed: boolean;
}
