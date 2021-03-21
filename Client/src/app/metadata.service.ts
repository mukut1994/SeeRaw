import { Injectable } from '@angular/core';
import { RenderContext, RenderRoot, RenderTarget } from './data.model';
import { BackendService } from '@service/backend.service';
import * as jp from 'jsonpath-faster'
import { Metadata } from '@data/data.model';

export const referenceTypeName = "$reference";

export function processMetadata(target: RenderTarget) {

  let refs: Map<string, string> = new Map();

  target.meta[referenceTypeName] = <Metadata>{ type: referenceTypeName, extendedType: null, children: null };

  jp.apply(target, "$..[?(@ && @.$id)]", (value: any, path: string[]) => {
    const fullPath = [ "$" ].concat(path.slice(2)).join('.');
    refs.set(value.$id, fullPath);
    target.links[fullPath] = target.links[`$${value.$id}`];
    delete target.links[value.$id];

    delete value.$id;

    return value;
  });

  jp.apply(target, "$..[?(@ && @.$ref)]", (value: any, path: string[]) => {
    const fullPath = [ "$" ].concat(path.slice(2)).join('.');
    target.links[fullPath] = referenceTypeName;
    return refs.get(value.$ref);
  });

  return target;
}

@Injectable({
  providedIn: 'root'
})
export class MetadataService {

  constructor(private backendService: BackendService) { }

  public getMetadataFor(context: RenderContext) {
    const metaId = this.backendService.renderRoot.targets[context.index].links[context.currentPath];
    const meta = this.backendService.renderRoot.targets[context.index].meta[metaId];

    return meta;
  }
}
