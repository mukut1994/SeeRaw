import { ComponentFactoryResolver, Directive, Injectable, TemplateRef, Type, ViewContainerRef, Input, AfterContentInit } from '@angular/core';
import { RenderContext } from './data.model';
import { Metadata } from 'src/app/data.model';
import { NavigationRenderComponent } from './navigation-render/navigation-render.component';
import { EnumRenderComponent } from './enum-render/enum-render.component';
import { VerticalRenderComponent } from './vertical-render/vertical-render.component';


@Directive({ selector: '[appRender]'})
export class RenderDirective implements AfterContentInit {

  value: any;
  metadata: Metadata;
  context: RenderContext;

  constructor(
    private renderService: RenderService,
    private viewContainer: ViewContainerRef,
    private componentFactoryResolver: ComponentFactoryResolver) { }

    @Input() set appRender(value: any) {
      this.value = value;
    }

    @Input() set appRenderContext(context: RenderContext) {
      this.context = context;
    }

    @Input() set appRenderMetadata(metadata: Metadata) {
      this.metadata = metadata;
    }

    ngAfterContentInit(): void {
      const type = this.renderService.getComponentFor("array"); //this.metadata.type);
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

  private components: { [type:string]: Type<RenderComponent> } = {};

  constructor() {
    this.registerComponent("array", NavigationRenderComponent);
    this.registerComponent("array", NavigationRenderComponent);
  }

  registerComponent(name: string, component: Type<RenderComponent>) {
    this.components[name] = component;
  }

  getComponentFor(type: string) {
    return this.components[type];
  }

}

export interface RenderComponent {
  value: any;
  metadata: Metadata;
  context: RenderContext;
}
