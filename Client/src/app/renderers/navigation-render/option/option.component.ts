import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Option } from '@data/data.model';
import { RenderOptionComponent } from '@data/render.service';

@Component({
  selector: 'app-option',
  templateUrl: './option.component.html',
  styleUrls: ['./option.component.css']
})
export class OptionComponent implements RenderOptionComponent, OnInit {

  options: NavigationOption;
  form: FormGroup;

  constructor(private fb: FormBuilder) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      mergeWithParent: [ this.options?.mergeWithParent ?? true ]
    });
  }

}

export class NavigationOption extends Option {
  mergeWithParent: boolean;
}
