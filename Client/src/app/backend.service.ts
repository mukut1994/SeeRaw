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
    const deserialized = JSON.parse(message.data);

    if (deserialized.download) {
      this.download(new URL(deserialized.download, window.location.href).href);
      return;
    }

    this.renderRoot = deserialized;

    this.messageHandler.emit(this.renderRoot);
  }

  sendMessage(message: string) {
    this.socket.send(message);
  }

  download(sUrl) {

    const isChrome = navigator.userAgent.toLowerCase().indexOf('chrome') > -1;
    const isSafari = navigator.userAgent.toLowerCase().indexOf('safari') > -1;

    // iOS devices do not support downloading. We have to inform user about this.
    if (/(iP)/g.test(navigator.userAgent)) {
        alert('Your device does not support files downloading. Please try again in desktop browser.');
        return false;
    }

    // If in Chrome or Safari - download via virtual link click
    if (isChrome || isSafari) {        // Creating new link node.
        const link = document.createElement('a');
        link.href = sUrl;

        if (link.download !== undefined) {
            // Set HTML5 download attribute. This will prevent file from opening if supported.
            const fileName = sUrl.substring(sUrl.lastIndexOf('/') + 1, sUrl.length);
            link.download = fileName;
        }

        // Dispatching click event.
        if (document.createEvent) {
            const e = document.createEvent('MouseEvents');
            e.initEvent('click', true, true);
            link.dispatchEvent(e);
            return true;
        }
    }

    window.open(sUrl, '_self');
    return true;
}

}
