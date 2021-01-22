import { Component } from '@angular/core';
import { BackendService } from './backend.service';
import { RenderService } from './render.service';
import { ArrayRenderComponent } from './array-render/array-render.component';
import { EnumRenderComponent } from './enum-render/enum-render.component';
import { FormRenderComponent } from './form-render/form-render.component';
import { LogRenderComponent } from './log-render/log-render.component';
import { NavigationRenderComponent } from './navigation-render/navigation-render.component';
import { ObjectRenderComponent } from './object-render/object-render.component';
import { ProgressRenderComponent } from './progress-render/progress-render.component';

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
    private renderService: RenderService) {
    backendService.connected.subscribe(() => this.state = 'Connected');
    backendService.disconnected.subscribe(this.updateState.bind(this));

    this.initOptionsForms();
    this.initRenderComponents();
  }

  // TODO this should go in the module initialiser so it can be extended
  initOptionsForms() {

  }

  initRenderComponents() {
    this.renderService.registerComponent("array", ArrayRenderComponent);
    this.renderService.registerComponent("object", ObjectRenderComponent);
    this.renderService.registerComponent("enum", EnumRenderComponent);
    this.renderService.registerComponent("form", FormRenderComponent);
    this.renderService.registerComponent("log", LogRenderComponent);
    this.renderService.registerComponent("progress", ProgressRenderComponent);

    this.renderService.registerComponent("array", NavigationRenderComponent);
    this.renderService.registerComponent("object", NavigationRenderComponent);
  }

  updateState(reconnectingIn: number) {
    if(reconnectingIn > 0)
      this.state = 'Reconnecting in ' + reconnectingIn;

    else if (reconnectingIn === -1)
      this.state = 'Automatic reconnection failed';

    else
      this.state = 'Connecting....';
  }
}

class OptionConfigurator {
  type: string;
  form: Component;
}
