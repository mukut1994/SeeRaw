import { Component, Input, OnInit, Output } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { NavBarData, NavigationEvent } from './nav-bar/data';
import { EventEmitter } from '@angular/core';
import * as jp from 'jsonpath'
import { OptionsService } from '@service/options.service';
import { RenderService } from '@data/render.service';
import { RenderComponent } from './../../render.service';

@Component({
  selector: 'app-navigation-render',
  templateUrl: './navigation-render.component.html',
  styleUrls: ['./navigation-render.component.css']
})
export class NavigationRenderComponent implements OnInit, RenderComponent {

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;

  selected: NavigationEvent;
  navbar: NavBarData;
  options: any;

  constructor(private optionsService: OptionsService) { }

  @Output() select: EventEmitter<string> = new EventEmitter<string>();

  ngOnInit() {
    this.navbar = this.convertToNavData(0, null, this.value, this.metadata, this.context);
    this.selected = this.firstSelectable(this.navbar);
  }

  private firstSelectable(navbar: NavBarData) {
    for(const x of navbar.children) {
      if(x.children.length === 0)
        return x;

      const subSelectable = this.firstSelectable(x);

      if(subSelectable)
        return subSelectable;
    }
  }

  private convertToNavData(currentDepth: number, title: string, value:any, metadata: Metadata, context: RenderContext) {
    const ret = new NavBarData();
    ret.title = title;
    ret.children = [];
    ret.value = value;
    ret.metadata = metadata;
    ret.context = context;

    if(!this.selected && Object.keys(value).length === 0) {
      return ret;
    }

    const childIsNav = this.optionsService.get(context, metadata)?.renderer == "navigation";

    if(!childIsNav) {
      return ret;
    }

    if(typeof(value) === 'string')
      return ret;

    for(const key in value) {
      ret.children.push(this.convertToNavData(currentDepth + 1, key, value[key], metadata.children[key], context.child(key)));
    }

    return ret;
  }

  click(event: NavigationEvent) {
    this.selected = event;
  }
}
