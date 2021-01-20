import { Component } from '@angular/core';
import { BackendService } from './backend.service';
import { RenderContext } from './data.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'SeeRaw';
  state: string;

  constructor(backendService: BackendService) {
    backendService.connected.subscribe(() => this.state = 'Connected');
    backendService.disconnected.subscribe(this.updateState.bind(this));
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
