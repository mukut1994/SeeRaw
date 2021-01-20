import { Injectable, Output, OnInit } from '@angular/core';
import { Metadata, RenderOption, RenderContext, Message, RenderTarget, RenderRoot } from './data.model';
import { EventEmitter } from '@angular/core';
import { BehaviorSubject, of, pipe, scheduled, Subject } from 'rxjs';
import { concatAll, filter, map, startWith } from 'rxjs/operators';
import * as jp from 'jsonpath'
import { BackendService } from './backend.service';

@Injectable({
  providedIn: 'root'
})
export class OptionsService {

  options: RenderOption[] = [];
  renderRoot: RenderRoot;
  parsedOptions: { [path: string]: RenderOption } = {};

  @Output() optionsObserveable: BehaviorSubject<RenderOption[]> = new BehaviorSubject(this.options);

  constructor(private backend: BackendService) {
    this.renderRoot = this.backend.renderRoot;
    this.backend.messageHandler.subscribe(x => this.renderRoot = x);
    this.backend.disconnected.subscribe(x => this.renderRoot = null);
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
    const existingOptions = this.options.filter(x => x.jsonPath === path);
    let update: RenderOption;

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
      const matches = jp.paths(this.renderRoot.targets[0].value, element.jsonPath);

      matches.forEach(x => {
        /*const m = this.GetMetadataOfPath(this.renderRoot.targets[0].metadata, x);

        m.renderOptions = element.typeOptions;
        console.log(m);

        return;
        console.log(this.GetMetadataPath(x));
        console.log(jp.query(this.renderRoot.targets[0].metadata, this.GetMetadataPath(x)));
        jp.apply(this.renderRoot.targets[0].metadata, this.GetMetadataPath(x), r => r.renderOptions = element.typeOptions);*/
      })
    });

    this.optionsObserveable.next(this.options);
  }

  private GetMetadataOfPath(metadata: Metadata, path: (string | number)[]) {

    for(let i = 1; i < path.length; i++) {
      metadata = jp.value(metadata.children, [ '$', path[i] ]);
    }

    return metadata;
  }

  private GetMetadataPath(path: (string | number)[]) {
    const ret = [];

    for(const p of path) {
      ret.push(p);
      ret.push('children');
    }

    return jp.stringify(ret);
  }
}
