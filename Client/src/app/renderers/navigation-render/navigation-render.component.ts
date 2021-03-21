import { Component, Input, OnInit, Output, Type } from '@angular/core';
import { Metadata, RenderContext, VisibleIncrement } from '@data/data.model';
import { NavBarData, NavigationEvent } from './nav-bar/data';
import { EventEmitter } from '@angular/core';
import { OptionsService } from '@service/options.service';
import { RenderComponent } from './../../render.service';
import { NavigationOption, OptionComponent } from './option/option.component';
import * as jp from 'jsonpath-faster'
import { MetadataService } from '@data/metadata.service';

@Component({
  selector: 'app-navigation-render',
  templateUrl: './navigation-render.component.html',
  styleUrls: ['./navigation-render.component.css']
})
export class NavigationRenderComponent implements OnInit, RenderComponent {

  public static option = OptionComponent;

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;

  selected: NavigationEvent;
  navbar: NavBarData;
  options: any;

  constructor(private optionsService: OptionsService, private metadataService: MetadataService) { }

  @Output() select: EventEmitter<string> = new EventEmitter<string>();

  ngOnInit() {
    this.navbar = this.convertToNavData(0, null, this.value, this.metadata, this.context);
    this.selected = this.firstSelectable(this.navbar);
  }

  expand(path: string) {
    if(path.length === 0)
      return null;

    let x = this.findWithPath(path, this.navbar.children);

    if(!x)
      return null;

    if(x != this.selected)
      this.selected = x;
  }

  private findWithPath(path:string, nav: NavBarData[]): NavBarData {
    for(const x of nav) {
      if(x.children.length == 0 && path.startsWith(x.context.currentPath))
        return x;

      const ret = this.findWithPath(path, x.children);

      if(ret) return ret;
    }

    return null;
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

    if(!value)
      return ret;

    if(!this.selected && Object.keys(value).length === 0)
      return ret;

    const childOpt = this.optionsService.get(context, metadata) as NavigationOption;
    const childMerge = childOpt?.renderer == "navigation" && (childOpt?.mergeWithParent ?? true);

    if(currentDepth > 0 && !childMerge)
      return ret;

    if(typeof(value) === 'string')
      return ret;

    for(const key in value) {
      let childKey = key;
      if(childOpt.keyPath) childKey = this.key(value[key], childOpt.keyPath) ?? "null";

      const childContext = context.child(key, VisibleIncrement.Reset);
      ret.children.push(this.convertToNavData(currentDepth + 1, childKey, value[key], this.metadataService.getMetadataFor(childContext), childContext));
    }

    return ret;
  }

  key(value: any, path: string) {
    return jp.value(value, path);
  }

  click(event: NavigationEvent) {
    this.selected = event;
  }
}
