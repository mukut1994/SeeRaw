import { Injectable, Output, OnInit } from '@angular/core';
import { Options, RenderContext, RenderRoot } from './data.model';
import { EventEmitter } from '@angular/core';
import { BehaviorSubject, of, pipe, scheduled, Subject } from 'rxjs';
import { concatAll, filter, map, startWith } from 'rxjs/operators';
import * as jp from 'jsonpath'
import { BackendService } from './backend.service';

@Injectable({
  providedIn: 'root'
})
export class OptionsService {

  options: Options[] = [];
  renderRoot: RenderRoot;
  parsedOptions: { [path: string]: Options } = {};

  constructor(private backend: BackendService) {
    this.renderRoot = this.backend.renderRoot;
    this.backend.messageHandler.subscribe(x => this.renderRoot = x);
    this.backend.onDisconnected.subscribe(x => this.renderRoot = null);
  }

  get(path: string, type: string) {

    const match = this.parsedOptions[path];

    return match?.typeOptions[type];
  }

  log(object: any) {
    console.log(object);
    return true;
  }

  set(type: string, path: string, options: any) {
    //  '$..target[?(@.$type=="number")]'
    //   $..*[?(@.$type)]
    //   $..*[?(@.$type && @.$type=="array")]
    const existingOptions = this.options.filter(x => x.jsonPath == path);
    let update: Options;

    if(existingOptions.length > 0)
      update = existingOptions[0];
    else
    {
      update = { jsonPath: path, typeOptions: [] };
      this.options.push(update);
    }

    update.typeOptions[type] = options;

    this.updateOptions();
  }

  private updateOptions() {
    this.parsedOptions = {};

    this.options.forEach(element => {
      const matches = jp.paths(this.renderRoot.targets[0], element.jsonPath);

      matches.forEach(x => {
        const path = jp.stringify(x);// jp.stringify(x.slice(0, x.length - 2));
        this.parsedOptions[path] = element;
      })
    });

  }
}
