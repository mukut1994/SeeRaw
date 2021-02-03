import { Injectable, Output } from '@angular/core';
import { Metadata, RenderOption, RenderRoot } from './data.model';
import { BehaviorSubject } from 'rxjs';
import * as jp from 'jsonpath'
import { BackendService } from './backend.service';
import { RenderContext } from '@data/data.model';

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
        /*
        [ "form", { renderer: "table" } ],
        [ "log", { renderer: "table" } ],
        [ "progress", { renderer: "table" } ],
        */
      ])
    }
  ]

  options: RenderOption[] = [];
  renderRoot: RenderRoot;

  @Output() optionsObserveable: BehaviorSubject<RenderOption[]> = new BehaviorSubject(this.options);

  constructor(private backend: BackendService) {
    this.renderRoot = this.backend.renderRoot;

    this.options = JSON.parse(this.storage.getItem(this.storageKey), this.reviver) ?? this.getDefaultOptions();

    // TODO optimize, update options shouldn't be called so much
    this.backend.messageHandler.subscribe(x => this.renderRoot = x);
    this.backend.messageHandler.subscribe(() => this.updateOptions());
    this.backend.disconnected.subscribe(() => this.renderRoot = null);
  }

  remove(renderOption: RenderOption, type: string) {
    renderOption.typeOptions.delete(type);

    this.storage.setItem(this.storageKey, JSON.stringify(this.options, this.replacer));
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

    this.storage.setItem(this.storageKey, JSON.stringify(this.options, this.replacer));
    this.updateOptions();
    this.backend.refresh();
    this.optionsObserveable.next(null);
  }

  get(context: RenderContext, metadata: Metadata) {

    if(!metadata.renderOptions) return;

    for(let i = 0; i < metadata.renderOptions.length; i++) {
      let opt = metadata.renderOptions[metadata.renderOptions.length - i - 1].get(metadata.type);

      if(opt)
        return opt;
    }

    // TODO write a jpath reverse parser here
    /*
    for(const option of this.options) {
      jp.paths(this.renderRoot.targets[context.index])
      const jpath = <jpath[]>jp.parse(option.jsonPath); // TODO cache this;
      const target = this.renderRoot.targets[context.index];
      const path = context.currentPath.split('.');

      for(const jp of jpath) {
        if(jp.expression.type == ExpressionType.root) continue;


      }
    }*/
  }
/*
  private getPathAfterScope(path: string[], jp: jpath) {

    if(jp.scope == Scope.child)
      return path;


  }
*/
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
    this.renderRoot.targets[0].metadata.renderOptions = [];

    this.options.forEach(element => {
      let value = this.renderRoot.targets[0].value;

      let matches;

      if(typeof(value) != "object") {
        matches = [ [ "$" ] ];
      }
      else {
        matches = jp.paths(value, element.jsonPath);

        if(element.jsonPath === "$..*")
          matches = (<[]>jp.paths(this.renderRoot.targets[0].value, "$")).concat(matches);
      }

      matches.forEach(x => {
        const m = this.GetMetadataOfPath(this.renderRoot.targets[0].metadata, x);

        if(!m)
          return;

        m.renderOptions = m.renderOptions ?? [];
        m.renderOptions.push(element.typeOptions);

        return;
      })
    });

    this.optionsObserveable.next(this.options);
  }

  private GetMetadataOfPath(metadata: Metadata, path: (string | number)[]) {

    for(let i = 1; i < path.length; i++) {
      if(!metadata.children)
        return null;

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
