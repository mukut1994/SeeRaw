import { Directive, AfterContentInit, DoCheck, OnInit, OnDestroy, Input, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { Subscription } from 'rxjs';
import { RenderService } from '@data/render.service';
import { OptionsService } from '@service/options.service';
import { GotoService } from './../goto.service';

@Directive({ selector: '[appRender]' })
export class RenderDirective implements AfterContentInit, DoCheck, OnInit, OnDestroy {

  @Input() value: any;
  @Input() metadata: Metadata;
  @Input() context: RenderContext;

  @Input() dirty = false;
  sub: Subscription;
  oldValue: any;
  prevContext: RenderContext;

  constructor(
    private renderService: RenderService,
    private viewContainer: ViewContainerRef,
    private componentFactoryResolver: ComponentFactoryResolver,
    private optionsService: OptionsService,
    private gotoService: GotoService) { }

    ngOnInit() {
      this.sub = this.optionsService.optionsObserveable.subscribe(() => this.dirty = true);
    }

    ngOnDestroy() {
      this.sub?.unsubscribe();
      this.gotoService.unregister(this.context.currentPath);
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

      if(this.prevContext)
        this.gotoService.unregister(this.prevContext.currentPath);

      this.prevContext = this.context;

      const options = this.optionsService.get(this.context, this.metadata);

      if(!options) return;

      const type = this.renderService.getComponentFor(this.metadata.type, options.renderer);

      if(!type) return;

      const factory = this.componentFactoryResolver.resolveComponentFactory(type);
      const component = this.viewContainer.createComponent(factory);

      component.instance.value = this.value;
      component.instance.context = this.context;
      component.instance.metadata = this.metadata;
      component.instance.options = options;

      GotoService.InProg++;
      component.changeDetectorRef.detectChanges();
      GotoService.InProg--;

      this.gotoService.registerComponent(this.context.currentPath, component.instance);
    }
}
