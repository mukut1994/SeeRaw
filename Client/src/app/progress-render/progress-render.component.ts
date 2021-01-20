import { Component, OnInit, Input } from "@angular/core";
import { Metadata } from "../data.model";
import { BackendService } from "./../backend.service";
import { RenderContext } from './../data.model';

@Component({
  selector: "app-progress-render",
  templateUrl: "./progress-render.component.html",
  styleUrls: ["./progress-render.component.css"],
})
export class ProgressRenderComponent {

  @Input() context: RenderContext;
  @Input() value: Progress;
  @Input() metadata: ProgressMetadata;

  constructor(private readonly backendService: BackendService) {}

  setSpeed() {
    const speed = prompt("Enter speed (0 for unlimited): ", "0");

    this.backendService.sendMessage(
      JSON.stringify({
        type: "form",
        id: this.metadata.setSpeed,
        args: [ parseInt(speed) ]
      })
    );
  }

  pause() {
    this.backendService.sendMessage(
      JSON.stringify({ type: "link", id: this.metadata.pause })
    );
  }

  cancel() {
    this.backendService.sendMessage(
      JSON.stringify({ type: "link", id: this.metadata.cancel })
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
