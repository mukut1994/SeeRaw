import { Component } from '@angular/core';
import { BackendService } from './backend.service';
import { RenderService } from './render.service';
import { EnumRenderComponent } from '@renderer/enum-render/enum-render.component';
import { FormRenderComponent } from '@renderer/form-render/form-render.component';
import { LogRenderComponent } from '@renderer/log-render/log-render.component';
import { NavigationRenderComponent } from '@renderer/navigation-render/navigation-render.component';
import { ProgressRenderComponent } from '@renderer/progress-render/progress-render.component';
import { TableRenderComponent } from './renderers/table-render/table-render.component';
import { ValueRenderComponent } from './renderers/value-render/value-render.component';
import { OptionsService } from '@service/options.service';
import { NoRenderOptionsComponent } from './no-render-options/no-render-options.component';
import { LinkRenderComponent } from './renderers/link-render/link-render.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { OptionListComponent } from './option-list/option-list.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'SeeRaw';
  state: string;

  opt: OptionConfigurator[];

  constructor(private backendService: BackendService,
    private ngbModal: NgbModal,
    private optionsService: OptionsService,
    private renderService: RenderService) {
    backendService.connected.subscribe(() => this.state = 'Connected');
    backendService.disconnected.subscribe(this.updateState.bind(this));

    this.initRenderComponents();
  }

  initRenderComponents() {
    this.renderService.registerComponent('navigation', 'array', NavigationRenderComponent, NoRenderOptionsComponent);
    this.renderService.registerComponent('navigation', 'object', NavigationRenderComponent,  NoRenderOptionsComponent);

    this.renderService.registerComponent('table', 'array', TableRenderComponent, TableRenderComponent.option);
    this.renderService.registerComponent('table', 'object', TableRenderComponent, TableRenderComponent.option);

    this.renderService.registerComponent('value', 'string', ValueRenderComponent, ValueRenderComponent.option);
    this.renderService.registerComponent('value', 'number', ValueRenderComponent, ValueRenderComponent.option);
    this.renderService.registerComponent('value', 'bool', ValueRenderComponent, ValueRenderComponent.option);
    this.renderService.registerComponent('value', 'enum', ValueRenderComponent, ValueRenderComponent.option);
    this.renderService.registerComponent('value', 'datetime', ValueRenderComponent, ValueRenderComponent.option);

    this.renderService.registerComponent('enum', 'enum', EnumRenderComponent, NoRenderOptionsComponent);

    this.renderService.registerComponent('link', 'link', LinkRenderComponent, NoRenderOptionsComponent);
    this.renderService.registerComponent('form', 'form', FormRenderComponent, NoRenderOptionsComponent);

    this.renderService.registerComponent('log', 'log', LogRenderComponent, NoRenderOptionsComponent);
    this.renderService.registerComponent('progress', 'progress', ProgressRenderComponent, NoRenderOptionsComponent);
  }

  updateState(reconnectingIn: number) {
    if(reconnectingIn > 0)
      this.state = 'Reconnecting in ' + reconnectingIn;

    else if (reconnectingIn === -1)
      this.state = 'Automatic reconnection failed';

    else
      this.state = 'Connecting....';
  }

  resetOptions() {
    let x = this.ngbModal.open(OptionListComponent, { size: "lg" });

    (<OptionListComponent>x.componentInstance).modal = x;
  }
}

class OptionConfigurator {
  type: string;
  form: Component;
}
