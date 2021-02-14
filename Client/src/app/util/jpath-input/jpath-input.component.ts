import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { AbstractControl, ControlContainer, FormGroup } from '@angular/forms';
import { Subscription } from 'rxjs';
import * as jp from 'jsonpath-faster'

@Component({
  selector: 'app-jpath-input',
  templateUrl: './jpath-input.component.html',
  styleUrls: ['./jpath-input.component.css']
})
export class JpathInputComponent implements OnInit, OnDestroy {

  public form: FormGroup;

  @Input() label: string;
  @Input() controlName: string;
  @Input() placeholder: string;

  valid: boolean = true;
  id = JpathInputComponent.id++;

  private sub: Subscription;

  static id = 0;

  // Let Angular inject the control container
  constructor(private controlContainer: ControlContainer) { }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  ngOnInit() {
    this.form = <FormGroup>this.controlContainer.control;

    this.sub = this.form.get(this.controlName).statusChanges
      .subscribe(x => this.valid = x === "VALID");
  }
}

export function JPathValidator(control: AbstractControl) {
  if(control.value == null || control.value === '')
    return null;

  try
  {
    jp.parse(control.value);

    return null;
  }
  catch
  {
    return { jpath: false };
  }
}
