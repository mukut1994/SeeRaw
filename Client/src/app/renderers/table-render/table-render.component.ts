import { Component, Input, OnInit } from '@angular/core';
import { OptionComponent } from './option/option.component';
import { RenderComponent } from './../../render.service';
import { Metadata, RenderContext } from '@data/data.model';
import { RenderService } from '@data/render.service';

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

  keys: any;

  constructor(private renderService: RenderService) { }

  ngOnInit() {
    this.keys = Object.keys(this.value);
  }

}
