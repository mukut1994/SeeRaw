import { CONTEXT_NAME } from '@angular/compiler/src/render3/view/util';
import { Component, OnInit, Input, OnDestroy } from "@angular/core";
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { pipe, Subscription } from 'rxjs';
import { Options, RenderContext } from './../data.model';
import { OptionsService } from './../options.service';

@Component({
  selector: "app-array-render",
  templateUrl: "./array-render.component.html",
  styleUrls: ["./array-render.component.scss"],
})
export class ArrayRenderComponent implements OnInit {
  @Input() editable: boolean;
  @Input() context: RenderContext;
  @Input() value: any[];
  @Input() options: ArrayRenderOptions;

  optionPath: string;

  constructor(private modalService: NgbModal,
    private optionsService: OptionsService) {}

  ngOnInit(): void {
    this.options = this.optionOrDefault(this.options);
    this.optionPath = this.context.currentPath;
  }

  optionOrDefault(options:any) {
    if(options?.collapsed == null) {
      return {
        visible: true,
        collapsed: true, //this.context.currentPath.split(/[\.\[]/).length < 3,
        showIndex: true,
        showLength: true
      }
    }
  }

  open(content) {
    this.modalService.open(content, { backdrop: false }).result.then(r => {
      this.optionsService.set("array", this.optionPath, this.options);
    });
  }

}

class ArrayRenderOptions {
  visible: boolean;
  collapsed: boolean;
  showIndex: boolean;
  showLength: boolean;
}
