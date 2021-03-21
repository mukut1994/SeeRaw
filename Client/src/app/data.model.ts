export class RenderContext {
  editMode: boolean;
  currentPath: string;
  index: number;
  visibleDepth: number;

  constructor(currentPath: string = null, index: number = null) {
    this.currentPath = currentPath;
    this.index = index;
    this.visibleDepth = 0;
    this.editMode = false;
  }

  public child(name: string, incrementVisible: VisibleIncrement | Number = 1) {
    const ret = new RenderContext();
    ret.editMode = this.editMode;
    ret.currentPath = this.currentPath + '.' + name;
    ret.index = this.index;
    ret.visibleDepth = this.visibleDepth;

    if(typeof(incrementVisible) == "number")
      ret.visibleDepth += incrementVisible;
    else if(incrementVisible === VisibleIncrement.Reset)
      ret.visibleDepth = 0;

    return ret;
  }

  public WithEditMode() {
    this.editMode = true;
    return this;
  }
}

export enum VisibleIncrement {
  None = "None",
  Reset = "Reset"
}

export class Metadata {
  type: string;
  extendedType: string | undefined;
  children: Metadata | Metadata[] | undefined;
}

export class Option {
  renderer: string;
}

export class RenderRoot {
  targets: RenderTarget[];
}

export class RenderOption {
  public jsonPath: string;
  public typeOptions: Map<string, any> = new Map<string, any>();
}

export class Message {
  public kind: Kind;
}

export class RenderTarget extends Message {
  public id: string;
  public value: any;
  public meta: { key: string, value: Metadata };
  public links: { path: string, type: string };
}
export class OptionsMessage extends Message {
  public options: string;
}
export class ParsedRenderOption {
  renderer: string;
  options: { [typeName: string]: any } = {};
}

export enum Kind {
  Full = 0,
  RemoveTarget = 1,
  Delta = 2,
  Download = 3,
  Options
}
