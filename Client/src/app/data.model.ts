export class RenderContext {
  public editMode: boolean;
  public currentPath: string;

  constructor(currentPath: string = null) {
    this.currentPath = currentPath;
  }

  public child(name: string) {
    var ret = new RenderContext();
    ret.currentPath = this.currentPath + ".target." + name;

    return ret;
  }

  public array(index: number) {
    var ret = new RenderContext();
    ret.currentPath = this.currentPath + ".target[" + index + "]";

    return ret;
  }

  public WithEditMode() {
    this.editMode = true;
    return this;
  }
}

export class Metadata {
  type: string;
  extendedType: string | undefined;
  children: Metadata | Metadata[] | undefined;

  renderOptions: { [typeName: string]: any } | undefined;
}

export class RenderRoot {
  targets: RenderTarget[];
}

export class RenderOption {
  public jsonPath: string;
  public typeOptions: { [typeName: string]: any } = {};
}

export class Message {
  public kind: Kind;
}

export class RenderTarget extends Message {
  public id: string;
  public value: any;
  public metadata: Metadata;
}

export enum Kind {
  Full = 0,
  RemoveTarget = 1,
  Delta = 2,
  Download = 3
}
