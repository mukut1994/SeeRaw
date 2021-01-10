import { Injectable } from '@angular/core';
import { RenderContext } from './data.model';

@Injectable({
  providedIn: 'root'
})
export class OptionsService {

  get<T>(context: RenderContext, type: string) {
    for(let i = 0; i < context.options.length; i++) {
      let item = context.options[i];

      return item.typeOptions[type] as T;
    }
  }

  set(context: RenderContext, path: string, options: any) {
    context.options
  }

}
