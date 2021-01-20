import { Component, OnInit, Input } from '@angular/core';
import { Metadata, RenderContext } from './../data.model';

@Component({
  selector: 'app-object-render',
  templateUrl: './object-render.component.html',
  styleUrls: ['./object-render.component.css']
})
export class ObjectRenderComponent implements OnInit {

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;

  keys: any;

  constructor() { }

  ngOnInit() {
    this.keys = Object.keys(this.value);
  }

}
