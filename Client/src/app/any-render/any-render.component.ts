import { Component, OnInit, Input } from '@angular/core';
import { BackendService } from './../backend.service';

@Component({
  selector: 'app-any-render',
  templateUrl: './any-render.component.html',
  styleUrls: ['./any-render.component.css'],
})
export class AnyRenderComponent implements OnInit {
  @Input() editable: boolean;
  @Input() target: any;

  constructor(private backend: BackendService) {}

  ngOnInit() {}

  linkClick(id: string) {
    this.backend.sendMessage(
      JSON.stringify({
        type: 'link',
        id,
      })
    );
  }
}
