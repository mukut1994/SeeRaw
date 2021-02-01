import { Component, Input, Output, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { RenderOptionComponent } from '@data/render.service';

@Component({
  selector: 'app-no-render-options',
  templateUrl: './no-render-options.component.html',
  styleUrls: ['./no-render-options.component.css']
})
export class NoRenderOptionsComponent implements RenderOptionComponent, OnInit {

  @Input() options: any;

  @Output() form: FormGroup;

  constructor(private formBuilder:FormBuilder) {}

  ngOnInit(): void {
    this.form = this.formBuilder.group({});
  }

}
