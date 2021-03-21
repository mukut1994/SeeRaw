import { Directive, AfterContentInit, DoCheck, OnInit, OnDestroy, Input, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { Subscription } from 'rxjs';
import { RenderService } from '@data/render.service';
import { OptionsService } from '@service/options.service';
import { GotoService } from './../goto.service';
import { BackendService } from './../backend.service';
import { MetadataService } from '@data/metadata.service';

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
    private metadataService: MetadataService,
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

    ngAfterContentInit() {
      this.render();
    }

    render() {
      this.viewContainer.clear();

      if(this.prevContext)
        this.gotoService.unregister(this.prevContext.currentPath);

      this.prevContext = this.context;

      if(!this.value)
        return;

      const metadata = this.metadataService.getMetadataFor(this.context) ?? this.defaultMetadata();
      const options = this.optionsService.get(this.context, metadata);

      if(!options) return;

      const type = this.renderService.getComponentFor(metadata.type, options.renderer);

      if(!type) return;

      const factory = this.componentFactoryResolver.resolveComponentFactory(type);
      const component = this.viewContainer.createComponent(factory);

      component.instance.value = this.value;
      component.instance.context = this.context;
      component.instance.metadata = metadata;
      component.instance.options = options;

      GotoService.InProg++;
      component.changeDetectorRef.detectChanges();
      GotoService.InProg--;

      this.gotoService.registerComponent(this.context.currentPath, component.instance);
    }

    defaultMetadata() : Metadata {
      return { type: typeof(this.value), extendedType: null, children: null };
    }
}
