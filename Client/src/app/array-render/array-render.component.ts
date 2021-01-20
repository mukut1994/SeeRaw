import { CONTEXT_NAME } from '@angular/compiler/src/render3/view/util';
import { Component, OnInit, Input, OnDestroy } from "@angular/core";
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { pipe, Subscription } from 'rxjs';
import { Metadata, RenderOption, RenderContext } from './../data.model';
import { OptionsService } from './../options.service';

@Component({
  selector: "app-array-render",
  templateUrl: "./array-render.component.html",
  styleUrls: ["./array-render.component.scss"],
})
export class ArrayRenderComponent implements OnInit {
  @Input() context: RenderContext;
  @Input() value: any[];
  @Input() metadata: ArrayMetadata;

  options: ArrayRenderOptions;

  constructor(private modalService: NgbModal,
    private optionsService: OptionsService) {}

  ngOnInit(): void {
    this.options = this.metadata.renderOptions?.array ?? this.optionOrDefault();
  }

  optionOrDefault() {
      return {
        collapsed: this.context.currentPath.split(/[\.\[]/).length < 3,
        showIndex: true,
        showLength: true
      }
  }

  open(content) {
    this.modalService.open(content, { backdrop: false }).result.then(r => {
      this.optionsService.set("array", "$..*", this.options);
    });
  }
}

class ArrayMetadata extends Metadata {
  children: Metadata[];
}

class ArrayRenderOptions {
  collapsed: boolean;
  showIndex: boolean;
  showLength: boolean;
}
