import { Component, Input, OnInit } from '@angular/core';
import { OptionComponent } from './option/option.component';
import { RenderComponent } from './../../render.service';
import { Metadata, RenderContext } from '@data/data.model';
import { OptionsService } from '@service/options.service';
import { highlightAnimation } from './../../animations';

@Component({
  selector: 'app-table-render',
  templateUrl: './table-render.component.html',
  styleUrls: ['./table-render.component.css'],
  animations: [ highlightAnimation ]
})
export class TableRenderComponent implements RenderComponent, OnInit {

  public static option = OptionComponent;

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;

  sessionOptions: SessionOptions;
  keys: any;
  highlight: boolean;
  selectChild: string[];
  children: { [prop: string]: RenderComponent } = {};

  constructor(private optionService: OptionsService) { }

  ngOnInit() {
    this.keys = Object.keys(this.value);
    this.sessionOptions = this.optionService.getSessionOptions(this.context, "table");
  }

  select(childPath: string[]): void {
    this.uncollapse();

    if(childPath.length != 0) {
      if(this.children[childPath[0]])
        this.children[childPath[0]].select(childPath.slice(1));
      else
        this.selectChild = childPath;

      return;
    }

    this.highlight = true;
    setTimeout(() => this.highlight = false, 3.5);
  }

  onChild(prop: string, renderer: RenderComponent) {
    this.children[prop] = renderer;

    if(this.selectChild && prop === this.selectChild[0]) {
      renderer.select(this.selectChild.slice(1));
    }
  }

  collapsed() {
    return this.sessionOptions.collapsed ?? this.context.currentPath.split(".").length > 2;
  }

  uncollapse() {
    if(this.collapsed()) this.collapse();
  }

  collapse() {
    this.sessionOptions.collapsed = !this.collapsed();
  }
}

class SessionOptions {
  collapsed: boolean;
}
