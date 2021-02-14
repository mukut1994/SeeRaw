import { Component, Input, OnInit, Output } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { RenderOptionComponent } from '@data/render.service';
import { JPathValidator } from '@data/util/jpath-input/jpath-input.component';
import { Option } from '@data/data.model';

@Component({
  selector: 'app-option',
  templateUrl: './option.component.html',
  styleUrls: ['./option.component.css']
})
export class OptionComponent implements RenderOptionComponent, OnInit {

  options: TableOption;
  form: FormGroup;

  constructor(private fb: FormBuilder) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      headerPath: [ this.options?.headerPath, JPathValidator ]
    });
  }

}

export class TableOption extends Option {
  headerPath: string | null;
}
