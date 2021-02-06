import { Component, Input, OnInit, Output, Type, ViewChildren, QueryList } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { NavBarData, NavigationEvent } from './nav-bar/data';
import { EventEmitter } from '@angular/core';
import { OptionsService } from '@service/options.service';
import { RenderDirective, RenderService } from '@data/render.service';
import { RenderComponent } from './../../render.service';
import { NavigationOption, OptionComponent } from './option/option.component';
import { Observable, pipe } from 'rxjs';
import { first, map, tap } from 'rxjs/operators';


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
  selectChild: string[];
  child: RenderComponent;

  constructor(private optionsService: OptionsService) { }

  ngOnInit() {
    this.navbar = this.convertToNavData(0, null, this.value, this.metadata, this.context);
    this.selected = this.firstSelectable(this.navbar);
  }

  select(childPath: string[]): void {
    if(childPath.length === 0)
      return;

    let x = this.navbar.children.find(n => n.title == childPath[0]);

    if(!x)
      return;

    this.selectChild = childPath.slice(1);

    if(x != this.selected)
      this.selected = x;
    else
      this.onChild(this.child);
  }

  onChild(child: RenderComponent) {
    this.child = child;

    if(!this.selectChild)
      return;

    child.select(this.selectChild);
    this.selectChild = null;
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

    const childOpt = this.optionsService.get(context, metadata);
    const childMerge = childOpt?.renderer == "navigation" && ((childOpt as NavigationOption)?.mergeWithParent ?? true);

    if(currentDepth > 0 && !childMerge)
      return ret;

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
