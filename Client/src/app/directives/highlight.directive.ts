import { Directive, Input, ViewContainerRef, QueryList } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';


@Directive({ selector: '[appHighlight]' })
export class HighlightDirective {

  @Input() key;

  constructor(public view: ViewContainerRef){}

  @Input() set appHighlight(key: string) {
    this.key = key;
  }

  static findWithKey(directive: QueryList<HighlightDirective>, prop) {
    const ret = directive.find(d => d.key === prop);

    if(ret)
      return of(ret);

    return directive.changes.pipe(map((x: QueryList<HighlightDirective>) => x.find(d => d.key === prop)));
  }

}
