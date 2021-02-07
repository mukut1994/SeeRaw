import { Injectable } from '@angular/core';
import { RenderComponent } from './render.service';
import { Observable } from 'rxjs';
import { first, map } from 'rxjs/operators';
import { HighlightDirective } from './directives/highlight.directive';

@Injectable({
  providedIn: 'root'
})
export class GotoService {

  components: Map<string, RenderComponent> = new Map();
  path: string;

  registerComponent(path: string, component: RenderComponent) {
    this.components.set(path, component);

    if(this.path && this.path.startsWith(path))
      this.navigateTo(this.path);
  }

  unregister(path: string) {
    this.components.delete(path);
  }

  navigateTo(path: string) {
    this.path = path;
    let parts = path.split(".");
    let highlight: Observable<HighlightDirective>;

    for(let i = 1; i < parts.length; i++) {
      const subpath = parts.slice(0, i).join('.');

      if(!this.components.has(subpath)) continue;

      const component = this.components.get(subpath);

      highlight = component.expand(path) ?? highlight;

      if(i + 1 == parts.length) {
        this.path = null;

        highlight?.pipe(
          first(),
          map(this.animate))
        .subscribe();
      }
    }
  }

  animate(d: HighlightDirective) {
    d.view?.element?.nativeElement.animate(
      [
        { background: "" },
        { background: "var(--info)" },
        { background: "" }
      ],
      { easing: "cubic-bezier(.03,.35,.03,.99)", duration: 3000 } );
  }

}
