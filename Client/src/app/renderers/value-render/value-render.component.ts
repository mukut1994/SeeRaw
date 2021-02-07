import { Component, OnInit, Input, ViewChild, ViewChildren, QueryList, OnDestroy } from '@angular/core';
import { OptionComponent } from './option/option.component';
import { RenderContext } from '@data/data.model';
import { Metadata } from 'src/app/data.model';
import { RenderComponent } from './../../render.service';
import { HighlightDirective } from './../../directives/highlight.directive';
import { of } from 'rxjs';
import { GotoService } from './../../goto.service';

@Component({
  selector: 'app-value-render',
  templateUrl: './value-render.component.html',
  styleUrls: ['./value-render.component.css']
})
export class ValueRenderComponent implements RenderComponent, OnInit, OnDestroy {

  public static option = OptionComponent;

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;

  style: string;

  constructor(private goto: GotoService) { }

  ngOnDestroy(): void {
    this.goto.unregister(this.context.currentPath + "." + this.value);
  }

  ngOnInit(): void {
    this.goto.registerComponent(this.context.currentPath + "." + this.value, this);
  }

  expand(path) { return null; }
}
