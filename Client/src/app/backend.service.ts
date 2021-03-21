import { Injectable, Output, EventEmitter } from '@angular/core';
import { Observable, timer } from 'rxjs';
import { webSocket, WebSocketSubject } from 'rxjs/webSocket';
import { Kind, Message, OptionsMessage, RenderRoot, RenderTarget, Metadata } from './data.model';
import { processMetadata } from './metadata.service';

@Injectable({
  providedIn: 'root',
})
export class BackendService {
  private socketLink = `ws://${window.location.hostname}:3054`;
  private backoffTimeSeconds = [ 5, 5, 10, 10, 30, 30, 60, 60 ];
  private backoffIndex = 0;
  private socket : WebSocket;

  renderRoot: RenderRoot;
  metadata: Metadata;

  @Output() messageHandler = new EventEmitter<RenderRoot>();
  @Output() disconnected = new EventEmitter<number>();
  @Output() connected = new EventEmitter();
  @Output() serverOptions = new EventEmitter<string>();

  constructor() {
    this.renderRoot = new RenderRoot();
    this.renderRoot.targets = [];

    this.reconnect();
  }

  onMessage(message: MessageEvent<any>) {
    if(message == null)
      return;

    const data = JSON.parse(message.data);

    if (data.download) {
      this.download(new URL(data.download, window.location.href).href);
      return;
    }

    const m = data as Message;

    switch(m.kind)
    {
      case Kind.Full:
        const renderTarget = processMetadata(m as RenderTarget);
        const index = this.renderRoot.targets.findIndex(x => x.id === renderTarget.id);
        if(index > -1)
          this.renderRoot.targets[index] = renderTarget;
        else
          this.renderRoot.targets.push(renderTarget);
          break;
      case Kind.Options:
        this.serverOptions.emit((m as OptionsMessage).options);
        return;
    }

    this.messageHandler.emit(this.renderRoot);
  }

  refresh() {
    this.messageHandler.emit(this.renderRoot);
  }

  sendMessage(message: any) {
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

  reconnect() {
    this.socket = new WebSocket(this.socketLink);

    this.socket.onopen = () => { this.backoffIndex = 0; this.connected.emit(); }
    this.socket.onmessage = this.onMessage.bind(this);
    this.socket.onclose = this.onSocketClose.bind(this);
  }

  async onSocketClose() {
    this.onMessage(null);

    if(this.backoffIndex > this.backoffTimeSeconds.length)
    {
      this.disconnected.emit(-1);
      return;
    }

    const sleepTime = this.backoffTimeSeconds[this.backoffIndex];
    for(let i = 0; i < sleepTime; i++)
    {
      await this.sleep(1000);
      this.disconnected.emit(sleepTime - i);
    }

    this.disconnected.emit(0);
    this.backoffIndex++;
    this.reconnect();
  }

  sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}
