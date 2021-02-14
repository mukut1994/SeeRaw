import { ComponentFactoryResolver, Directive, Injectable, Type, ViewContainerRef, Input, AfterContentInit, OnInit, DoCheck, OnDestroy, EventEmitter } from '@angular/core';
import { RenderContext } from './data.model';
import { Metadata } from 'src/app/data.model';
import { FormGroup } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { OptionEditComponent } from './option-edit/option-edit.component';
import { Subscription, Observable } from 'rxjs';
import { HighlightDirective } from './directives/highlight.directive';
import { Option } from '@data/data.model';

export class RendererSet {

  components: { [name: string]: RendererOptionTuple } = {};

}

export class RendererOptionTuple {
  renderer: Type<RenderComponent>;
  options: Type<any>;
}

export interface RenderComponent {
  value: any;
  metadata: Metadata;
  context: RenderContext;
  options: Option;

  expand(path: string): Observable<HighlightDirective> | null;
}

export interface RenderOptionComponent {
  options: any;
  form: FormGroup;
}

export class RenderComponentOption {
  jpath: string;
}

@Injectable({
  providedIn: 'root'
})
export class RenderService {

  private availableComponents: { [type: string]: RendererSet } = {}

  constructor(private modalService: NgbModal) { }

  registerComponent(renderName: string, typeName: string, renderer: Type<RenderComponent>, options: Type<any>) {

    let available = this.availableComponents[typeName];

    if(!available) {
      available = new RendererSet();
      this.availableComponents[typeName] = available;
    }

    available.components._default = { renderer, options };
    available.components[renderName] = { renderer, options };
  }

  getComponentFor(type: string, renderer: string | null) {
    return this.availableComponents[type]?.components[renderer ?? '_default'].renderer;
  }

  openOptionModal(renderName: string, typeName: string, path: string, options: any) {
    const ref = this.modalService.open(OptionEditComponent, { size: 'lg' });
    const instance = ref.componentInstance as OptionEditComponent;

    instance.modal = ref;
    instance.renderers = this.availableComponents;
    instance.rendererName = renderName;
    instance.typeName = typeName;
    instance.path = path;
    instance.options = options;
  }

  getOptionType(rendererName: string, typeName: string) {
    return this.availableComponents[typeName].components[rendererName].options;
  }
}
