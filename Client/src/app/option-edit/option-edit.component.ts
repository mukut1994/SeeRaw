import { Component, OnInit, Directive, ViewContainerRef, ComponentFactoryResolver, Type, AfterContentInit, Input,
   OnChanges, SimpleChanges, Output, EventEmitter, DoCheck } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RenderOptionComponent, RenderService } from '@data/render.service';
import { NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { RendererSet } from './../render.service';
import { OptionsService } from '@service/options.service';

@Component({
  selector: 'app-option-edit',
  templateUrl: './option-edit.component.html',
  styleUrls: ['./option-edit.component.css']
})
export class OptionEditComponent implements OnInit, DoCheck {

  @Input() typeName: string;
  @Input() rendererName: string;
  @Input() modal: NgbModalRef;
  @Input() path: string;
  @Input() options: any;

  @Input() renderers: { [type: string]: RendererSet };

  formGroup: FormGroup;
  childForm: RenderOptionComponent;
  childType: Type<RenderOptionComponent>;

  oldTypeName: string;

  constructor(private formBuilder: FormBuilder,
    private renderService: RenderService,
    private optionsService: OptionsService) { }

  ngDoCheck(): void {
    if(this.oldTypeName === this.typeName) {
      this.getChildType();
      return;
    }

    this.rendererName = this.renders()[0];

    this.getChildType();
  }

  ngOnInit(): void {
    this.oldTypeName = this.typeName;

    this.formGroup = this.formBuilder.group({
      path: [ this.path, Validators.required ]
    });

    this.getChildType();
  }

  types() {
    return Object.keys(this.renderers);
  }

  renders() {
    return Object.keys(this.renderers[this.typeName].components).filter(x => x !== '_default');
  }

  getChildType() {
    this.childType = this.renderService.getOptionType(this.rendererName, this.typeName);
  }

  save() {
    this.optionsService.set(this.typeName, this.formGroup.value.path, { renderer: this.rendererName, ...this.childForm.form.value });

    this.modal.close();
  }
}

@Directive({ selector: '[appOption]'})
export class OptionDirective implements AfterContentInit, OnChanges {

  @Input() type:Type<RenderOptionComponent>;
  @Input() options: any;

  @Output() onComponent: EventEmitter<RenderOptionComponent> = new EventEmitter<RenderOptionComponent>();

  constructor(
    private viewContainerRef: ViewContainerRef,
    private componentFactory: ComponentFactoryResolver) { }

  @Input() set appOption(type:Type<RenderOptionComponent>) {
    this.type = type;
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.viewContainerRef.clear();

    const factory = this.componentFactory.resolveComponentFactory(this.type);
    const component = this.viewContainerRef.createComponent(factory, 0);

    component.instance.options = this.options;

    this.onComponent.emit(component.instance);
  }

  ngAfterContentInit(): void {

  }
}
