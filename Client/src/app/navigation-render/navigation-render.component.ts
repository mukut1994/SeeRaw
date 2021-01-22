import { Component, Input, OnInit, Output } from '@angular/core';
import { Metadata, RenderContext } from './../data.model';
import { NavBarData, NavigationEvent } from './nav-bar/data';
import { EventEmitter } from '@angular/core';
import * as jp from 'jsonpath'

@Component({
  selector: 'app-navigation-render',
  templateUrl: './navigation-render.component.html',
  styleUrls: ['./navigation-render.component.css']
})
export class NavigationRenderComponent implements OnInit {

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;

  selected: NavigationEvent;
  navbar: NavBarData;

  @Output() select: EventEmitter<string> = new EventEmitter<string>();

  ngOnInit() {
    console.log("init");
    this.navbar = this.convertToNavData(0, null, this.value, this.metadata, this.context);
  }

  convertToNavData(currentDepth: number, title: string, value:any, metadata: Metadata, context: RenderContext) {
    const ret = new NavBarData();
    ret.title = title;
    ret.children = [];
    ret.value = value;
    ret.metadata = metadata;
    ret.context = context;

    if(!this.selected && Object.keys(value).length == 0) {
      this.selected = ret;
    }

    if(currentDepth > 2) {
      if(!this.selected) {
        this.selected = ret;
      }

      return ret;
    }

    for(let key in value) {
      ret.children.push(this.convertToNavData(currentDepth + 1, key, value[key], metadata.children[key], context.child(key)));
    }

    return ret;
  }

  click(event: NavigationEvent) {
    this.selected = event;
  }

}
