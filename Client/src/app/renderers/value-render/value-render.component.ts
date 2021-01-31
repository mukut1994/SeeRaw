import { Component, OnInit, Input } from '@angular/core';
import { OptionComponent } from './option/option.component';
import { RenderContext } from '@data/data.model';
import { Metadata } from 'src/app/data.model';
import { RenderComponent } from './../../render.service';

@Component({
  selector: 'app-value-render',
  templateUrl: './value-render.component.html',
  styleUrls: ['./value-render.component.css']
})
export class ValueRenderComponent implements RenderComponent {

  public static option = OptionComponent;

  @Input() context: RenderContext;
  @Input() value: any;
  @Input() metadata: Metadata;

  style: string;

}
