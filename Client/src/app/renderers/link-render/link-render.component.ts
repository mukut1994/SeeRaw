import { Component, Input, OnInit } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { RenderComponent } from './../../render.service';

@Component({
  selector: 'app-link-render',
  templateUrl: './link-render.component.html',
  styleUrls: ['./link-render.component.css']
})
export class LinkRenderComponent implements RenderComponent, OnInit {

  @Input() value: any;
  @Input() metadata: Metadata;
  @Input() context: RenderContext;

  constructor() { }

  ngOnInit(): void {
  }

}
