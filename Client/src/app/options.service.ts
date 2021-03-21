import { Injectable, Output } from '@angular/core';
import { Metadata, Option, RenderOption, RenderRoot } from './data.model';
import { BehaviorSubject } from 'rxjs';
import * as jp from 'jsonpath-faster'
import { BackendService } from './backend.service';
import { RenderContext } from '@data/data.model';
import { referenceTypeName } from './metadata.service';

@Injectable({
  providedIn: 'root'
})
export class OptionsService {

  private readonly storageKey = 'seeraw_settings';
  private readonly storage = window.localStorage;

  defaultOptions: RenderOption[] = [ {
      jsonPath: "$..*",
      typeOptions: new Map<string, any>([
        [ "string", { renderer: "value" } ],
        [ "bool", { renderer: "value" } ],
        [ "number", { renderer: "value" } ],
        [ "enum", { renderer: "value" } ],
        [ "datetime", { renderer: "value" } ],

        [ "object", { renderer: "table" } ],
        [ "array", { renderer: "table" } ],

        [ "link", { renderer: "link" } ],
        [ "progress", { renderer: "progress" } ],

        [ referenceTypeName, { renderer: referenceTypeName } ]
        /*
        [ "form", { renderer: "table" } ],
        [ "log", { renderer: "table" } ],
        */
      ])
    }
  ]

  options: RenderOption[] = [];
  renderRoot: RenderRoot;
  sessionOptions: Map<string, any> = new Map();
  parsedRenderOptions: Map<string, Map<string, Option>> = new Map();

  @Output() optionsObserveable: BehaviorSubject<RenderOption[]> = new BehaviorSubject(this.options);

  constructor(private backend: BackendService) {
    this.renderRoot = this.backend.renderRoot;

    this.options = JSON.parse(this.storage.getItem(this.storageKey), this.reviver) ?? this.getDefaultOptions();

    // TODO optimize, update options shouldn't be called so much
    this.backend.serverOptions.subscribe(x => this.loadServerOptions(x));
    this.backend.messageHandler.subscribe(x => { this.renderRoot = x; this.updateOptions() });
    this.backend.disconnected.subscribe(() => this.renderRoot = null);
  }

  serializedOptions() {
    return JSON.stringify(this.options, this.replacer);
  }

  loadServerOptions(opt: string) {
    if(!opt) return;

    this.defaultOptions = JSON.parse(opt, this.reviver);
  }

  loadOptions(opt: string) {
    if(!opt) return;

    this.options = JSON.parse(opt, this.reviver);
  }

  remove(renderOption: RenderOption, type: string) {
    renderOption.typeOptions.delete(type);

    this.storage.setItem(this.storageKey, this.serializedOptions());
    this.updateOptions();
    this.backend.refresh();
    this.optionsObserveable.next(null);
  }

  reset() {
    this.storage.setItem(this.storageKey, null);
    this.options = this.getDefaultOptions();
    this.updateOptions();
    this.backend.refresh();
    this.optionsObserveable.next(null);
  }

  private getDefaultOptions() {
    return JSON.parse(JSON.stringify(this.defaultOptions, this.replacer), this.reviver)
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

    this.storage.setItem(this.storageKey, this.serializedOptions());
    this.updateOptions();
    this.backend.refresh();
    this.optionsObserveable.next(null);
  }

  getSessionOptions(context: RenderContext, renderer: string) {
    let ret = this.sessionOptions.get(context.currentPath + "|" + renderer);

    if(ret)
      return ret;

    ret = {};
    this.sessionOptions.set(context.currentPath + "|" + renderer, ret);

    return ret;
  }

  get(context: RenderContext, metadata: Metadata) {
    if(!metadata)
      return null;

    const renderOptions = this.parsedRenderOptions.get(context.currentPath);

    return renderOptions.get(metadata.type);
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
    this.options.forEach(element => {
      if(!this.renderRoot.targets[0])
        return;

      let value = this.renderRoot.targets[0].value;

      let matches;

      if(typeof(value) != "object") {
        matches = [ [ "$" ] ];
      }
      else {
        matches = jp.nodes(value, element.jsonPath);

        if(element.jsonPath === "$..*")
          matches = (<[]>jp.nodes(this.renderRoot.targets[0].value, "$")).concat(matches);
      }

      matches.forEach(x => {
        this.parsedRenderOptions.set(x.path.join('.'), element.typeOptions);

        return;
        const m = this.GetMetadataOfPath(x.path, x.value);

        if(!m)
          return;

        m.renderOptions = m.renderOptions ?? [];
        m.renderOptions.push(element.typeOptions);

        return;
      })
    });

    this.optionsObserveable.next(this.options);
  }

  private GetMetadataOfPath(path: (string | number)[], value: any) {

    let metaId;

    if(!value)
      return;

    if(value.$id)
      metaId = this.renderRoot.targets[0].links[`$${value.$id}`];
    else
      metaId = this.renderRoot.targets[0].links[path.slice(1).join('.')];

    const meta = this.renderRoot.targets[0].meta[metaId];

    return meta;
/*
    for(let i = 1; i < path.length; i++) {
      if(!metadata.children)
        return null;

      metadata = jp.value(metadata.children, jp.stringify([ '$', path[i] ]));
    }

    return metadata;*/
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

interface jpath {
  expression: Expression;
  operation: Operation;
  scope: Scope;
}

interface Expression {
  type: ExpressionType;
  value: string;
}

enum ExpressionType {
  root = "root",
  identifier = "identifier",
  numeric_literal = "numeric_literal",
  filter_expression = "filter_expression"
}

enum Operation {
  member = "member",
  subscript = "subscript"
}

enum Scope {
  child = "child",
  descendant = "descendant"
}
