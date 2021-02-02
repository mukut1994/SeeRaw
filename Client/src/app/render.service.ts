import { ComponentFactoryResolver, Directive, Injectable, Type, ViewContainerRef, Input, AfterContentInit, OnInit, DoCheck, OnDestroy } from '@angular/core';
import { RenderContext } from './data.model';
import { Metadata } from 'src/app/data.model';
import { FormGroup } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { OptionEditComponent } from './option-edit/option-edit.component';
import { OptionsService } from '@service/options.service';
import { Subscription } from 'rxjs';

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
}

export interface RenderOptionComponent {
  options: any;
  form: FormGroup;
}

export class RenderComponentOption {
  jpath: string;
}

@Directive({ selector: '[appRender]' })
export class RenderDirective implements AfterContentInit, DoCheck, OnInit, OnDestroy {

  @Input() value: any;
  @Input() metadata: Metadata;
  @Input() context: RenderContext;

  @Input() dirty = false;
  sub: Subscription;
  oldValue: any;


  constructor(
    private renderService: RenderService,
    private viewContainer: ViewContainerRef,
    private componentFactoryResolver: ComponentFactoryResolver,
    private optionsService: OptionsService) { }

    ngOnInit() {
      this.sub = this.optionsService.optionsObserveable.subscribe(() => this.dirty = true);
    }

    ngOnDestroy() {
      this.sub?.unsubscribe();
    }

    ngDoCheck(): void {
      if(!this.dirty && this.oldValue === this.value)
        return;

      this.oldValue = this.value;
      this.dirty = false;

      this.render();
    }

    @Input() set appRender(value: any) {
      this.value = value;
    }

    @Input() set appRenderContext(context: RenderContext) {
      this.context = context;
    }

    @Input() set appRenderMetadata(metadata: Metadata) {
      this.metadata = metadata;
    }

    ngAfterContentInit() {
      this.render();
    }

    render() {
      this.viewContainer.clear();

      const options = this.optionsService.get(this.context, this.metadata);

      if(!options) return;

      const type = this.renderService.getComponentFor(this.metadata.type, options.renderer);

      if(!type) return;

      const factory = this.componentFactoryResolver.resolveComponentFactory(type);
      const component = this.viewContainer.createComponent(factory);

      component.instance.value = this.value;
      component.instance.context = this.context;
      component.instance.metadata = this.metadata;

      component.changeDetectorRef.detectChanges();
    }
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
