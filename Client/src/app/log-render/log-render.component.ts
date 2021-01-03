import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-log-render',
  templateUrl: './log-render.component.html',
  styleUrls: ['./log-render.component.css']
})
export class LogRenderComponent {
  @Input() target: Log;
}

class Log
{
  type:string;
  message:string;
  children:Log[]
}
