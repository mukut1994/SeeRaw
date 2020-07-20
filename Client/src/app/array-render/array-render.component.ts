import { Component, OnInit, Input } from "@angular/core";

@Component({
  selector: "app-array-render",
  templateUrl: "./array-render.component.html",
  styleUrls: ["./array-render.component.css"],
})
export class ArrayRenderComponent implements OnInit {
  @Input() editable: boolean;
  @Input() value: any[];

  constructor() {}

  ngOnInit() {
  }
}
