import { Component, OnInit, Input } from "@angular/core";
import { Progress } from "../data.model";
import { BackendService } from "./../backend.service";

@Component({
  selector: "app-progress-render",
  templateUrl: "./progress-render.component.html",
  styleUrls: ["./progress-render.component.css"],
})
export class ProgressRenderComponent {
  @Input() target: Progress;

  constructor(private readonly backendService: BackendService) {}

  setSpeed() {
    const speed = prompt("Enter speed (0 for unlimited): ", "0");

    this.backendService.sendMessage(
      JSON.stringify({
        type: "form",
        id: this.target.setSpeed,
        args: [ parseInt(speed) ]
      })
    );
  }

  pause() {
    this.backendService.sendMessage(
      JSON.stringify({ type: "link", id: this.target.pause })
    );
  }

  cancel() {
    this.backendService.sendMessage(
      JSON.stringify({ type: "link", id: this.target.cancel })
    );
  }
}
