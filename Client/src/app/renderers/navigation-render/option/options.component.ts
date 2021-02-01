import { Component, Input, OnInit, Output } from '@angular/core';
import { RenderComponentOption } from '@service/render.service'
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { RenderOptionComponent } from '@data/render.service';
import { Metadata } from 'src/app/data.model';
import { RenderOption } from '@data/data.model';

@Component({
  selector: 'app-options',
  templateUrl: './options.component.html',
  styleUrls: ['./options.component.css']
})
export class NavigationRenderOptionComponent implements RenderOptionComponent, OnInit {

  @Input() options: NavigationOptions;

  @Output() form: FormGroup;

  constructor(private formBuilder: FormBuilder) { }

  ngOnInit(): void {
    this.form = this.formBuilder.group({
      depth: [ this.options?.depth ?? 3, Validators.required ]
    })
  }

}

export class NavigationOptions {

  depth: number;

}
