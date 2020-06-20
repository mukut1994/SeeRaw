import { Injectable, Output, EventEmitter } from "@angular/core";
import { RenderRoot } from "./data.model";

@Injectable({
  providedIn: "root",
})
export class BackendService {
  private socket: WebSocket;

  renderRoot: RenderRoot;

  @Output() messageHandler = new EventEmitter<RenderRoot>();

  constructor() {
    this.socket = new WebSocket("ws:localhost:3054");

    this.socket.onmessage = this.onMessage.bind(this);
    this.socket.onclose = (x) => console.log(x);
  }

  onMessage(message: MessageEvent) {
    this.renderRoot = JSON.parse(message.data);

    this.messageHandler.emit(this.renderRoot);
  }

  sendMessage(message: string) {
    this.socket.send(message);
  }
}
