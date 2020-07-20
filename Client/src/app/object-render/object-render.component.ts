import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-object-render',
  templateUrl: './object-render.component.html',
  styleUrls: ['./object-render.component.css']
})
export class ObjectRenderComponent implements OnInit {

  @Input() editable: boolean;
  @Input() target: any;

  keys: any;

  constructor() { }

  ngOnInit() {
    this.keys = Object.keys(this.target);
  }

}
