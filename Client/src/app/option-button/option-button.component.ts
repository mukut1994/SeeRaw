import { Component, Input, OnInit } from '@angular/core';
import { RenderService } from '@data/render.service';
import { Metadata, RenderContext } from '@data/data.model';

@Component({
  selector: 'app-option-button',
  templateUrl: './option-button.component.html',
  styleUrls: ['./option-button.component.css']
})
export class OptionButtonComponent implements OnInit {

  @Input() renderer: string;
  @Input() metadata: Metadata;
  @Input() context: RenderContext;
  @Input() options: any;
  @Input() type: string;
  @Input() path: string;

  constructor(private renderService: RenderService) { }

  ngOnInit(): void {
  }

  openOptions() {
    this.renderService.openOptionModal(this.renderer ?? this.options.renderer, this.metadata?.type ?? this.type, this.context?.currentPath ?? this.path, this.options);
  }

}
