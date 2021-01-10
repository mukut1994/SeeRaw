import { Component, OnInit, Input } from "@angular/core";
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Options, RenderContext } from './../data.model';
import { OptionsService } from './../options.service';

@Component({
  selector: "app-array-render",
  templateUrl: "./array-render.component.html",
  styleUrls: ["./array-render.component.css"],
})
export class ArrayRenderComponent implements OnInit {
  @Input() editable: boolean;
  @Input() context: RenderContext;
  @Input() value: any[];

  options: ArrayRenderOptions;

  optionPath: string = "OK";

  constructor(private modalService: NgbModal,
    private optionsService: OptionsService) {}

  ngOnInit(): void {

    let options = this.optionsService.get<ArrayRenderOptions>(this.context, "array");

    if(options?.collapsed == undefined) {
      this.options = {
        collapsed: this.context.currentPath.split(/[\.\[]/).length < 3,
        showIndex: true,
        showLength: true
      }
    }

  }

  open(content) {
    this.modalService.open(content, { backdrop: false }).result.then(r => {

    });
  }

}

class ArrayRenderOptions {
  collapsed: boolean;
  showIndex: boolean;
  showLength: boolean;
}
