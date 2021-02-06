import { Component, OnInit, Input } from '@angular/core';
import { BackendService } from '@service/backend.service';
import { RenderContext, Metadata } from '@data/data.model';
import { RenderComponent } from './../../render.service';
import { highlightAnimation } from './../../animations';

@Component({
  selector: 'app-progress-render',
  templateUrl: './progress-render.component.html',
  styleUrls: ['./progress-render.component.css'],
  animations: [ highlightAnimation ]
})
export class ProgressRenderComponent implements RenderComponent {

  @Input() context: RenderContext;
  @Input() value: Progress;
  @Input() metadata: ProgressMetadata;

  highlight: boolean = false;

  constructor(private readonly backendService: BackendService) {}

  select(childPath: string[]): void {
    if(childPath.length != 0)
      return;

    this.highlight = true;

    setTimeout(() => this.highlight = false, 3.5);
  }

  setSpeed() {
    const speed = prompt('Enter speed (0 for unlimited): ', '0');

    this.backendService.sendMessage(
      JSON.stringify({
        type: 'form',
        id: this.metadata.setSpeed,
        args: [ parseInt(speed, 10) ]
      })
    );
  }

  pause() {
    this.backendService.sendMessage(
      JSON.stringify({ type: 'link', id: this.metadata.pause })
    );
  }

  cancel() {
    this.backendService.sendMessage(
      JSON.stringify({ type: 'link', id: this.metadata.cancel })
    );
  }
}

class Progress {
  percent: number;
  value: string | null;
  min: string;
  max: string;
  speed: string | null;
  paused: boolean
}

class ProgressMetadata extends Metadata {
  pause: string;
  setSpeed: string;
  cancel: string;
}
