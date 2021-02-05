import { Component, Input, OnInit } from '@angular/core';
import { OptionComponent } from './option/option.component';
import { RenderComponent } from './../../render.service';
import { Metadata, RenderContext } from '@data/data.model';
import { OptionsService } from '@service/options.service';

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

  sessionOptions: SessionOptions;
  keys: any;

  constructor(private optionService: OptionsService) { }

  ngOnInit() {
    this.keys = Object.keys(this.value);
    this.sessionOptions = this.optionService.getSessionOptions(this.context, "table");
  }

  collapsed() {
    return this.sessionOptions.collapsed ?? this.context.currentPath.split(".").length > 2;
  }

  collapse() {
    this.sessionOptions.collapsed = !this.collapsed();
  }
}

class SessionOptions {
  collapsed: boolean;
}
