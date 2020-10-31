import { Component, Input, OnInit } from '@angular/core';
import { BackendService } from './../backend.service';

@Component({
  selector: 'app-navigation-render',
  templateUrl: './navigation-render.component.html',
  styleUrls: ['./navigation-render.component.css']
})
export class NavigationRenderComponent {

  @Input() target: any;

  constructor(private backendService: BackendService) { }

  click(id: string) {
    this.backendService.sendMessage(
      JSON.stringify({
        type: 'link',
        id,
      })
    );
  }
}
