import { Injectable, Output, OnInit, ViewContainerRef } from '@angular/core';
import { Metadata, RenderOption, RenderContext, Message, RenderTarget, RenderRoot, ParsedRenderOption } from './data.model';
import { EventEmitter } from '@angular/core';
import { BehaviorSubject, of, pipe, scheduled, Subject } from 'rxjs';
import { concatAll, filter, map, startWith } from 'rxjs/operators';
import * as jp from 'jsonpath'
import { BackendService } from './backend.service';

@Injectable({
  providedIn: 'root'
})
export class OptionsService {

  private readonly storageKey = 'seeraw_settings';
  private readonly storage = window.localStorage;

  options: RenderOption[] = [];
  renderRoot: RenderRoot;

  @Output() optionsObserveable: BehaviorSubject<RenderOption[]> = new BehaviorSubject(this.options);

  constructor(private backend: BackendService) {
    this.renderRoot = this.backend.renderRoot;

    this.options = JSON.parse(this.storage.getItem(this.storageKey), this.reviver) ?? [];

    // TODO optimize, update options shouldn't be called so much
    this.backend.messageHandler.subscribe(() => this.updateOptions());
    this.backend.messageHandler.subscribe(x => this.renderRoot = x);
    this.backend.disconnected.subscribe(() => this.renderRoot = null);
  }

  reset() {
    this.storage.setItem(this.storageKey, null);
    this.options = [];
    this.updateOptions();
    this.backend.refresh();
    this.optionsObserveable.next(null);
  }

  set(type: string, path: string, options: any) {
    const existingOptions = this.options.filter(x => x.jsonPath === path);
    let update: RenderOption;

    if(existingOptions.length > 0)
      update = existingOptions[0];
    else
    {
      update = { jsonPath: path, typeOptions: new Map() };
      this.options.push(update);
    }

    update.typeOptions.set(type, options);

    this.storage.setItem(this.storageKey, JSON.stringify(this.options, this.replacer));
    this.updateOptions();
    this.backend.refresh();
    this.optionsObserveable.next(null);
  }

  private replacer(key, value) {
    if(value instanceof Map) {
      return {
        dataType: 'Map',
        value: Array.from(value.entries()), // or with spread: value: [...value]
      };
    } else {
      return value;
    }
  }

  private reviver(key, value) {
    if(typeof value === 'object' && value !== null) {
      if (value.dataType === 'Map') {
        return new Map(value.value);
      }
    }
    return value;
  }

  private updateOptions() {
    this.renderRoot.targets[0].metadata.renderOptions = new Map();

    this.options.forEach(element => {
      const matches = jp.paths(this.renderRoot.targets[0].value, element.jsonPath);

      matches.forEach(x => {
        const m = this.GetMetadataOfPath(this.renderRoot.targets[0].metadata, x);

        m.renderOptions = element.typeOptions;

        return;
        console.log(this.GetMetadataPath(x));
        console.log(jp.query(this.renderRoot.targets[0].metadata, this.GetMetadataPath(x)));
        jp.apply(this.renderRoot.targets[0].metadata, this.GetMetadataPath(x), r => r.renderOptions = element.typeOptions);
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
