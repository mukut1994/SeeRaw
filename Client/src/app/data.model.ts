export class RenderContext {
  public currentPath: string;
  public options: Options[] = [];

  constructor(currentPath: string = null) {
    this.currentPath = currentPath;
  }

  public child(name: string) {
    var ret = new RenderContext();
    ret.currentPath = this.currentPath + "." + name;
    ret.options = this.options;

    return ret;
  }

  public array(index: number) {
    var ret = new RenderContext();
    ret.currentPath = this.currentPath + "[" + index + "]";
    ret.options = this.options;

    return ret;
  }
}

export class Options {
  public jsonPath: string;
  public typeOptions: { [typeName: string]: any };
}

export class RenderRoot {
  public targets: RenderTarget[];
}

export class RenderTarget {
  public type: string;
  public target: any;
}

export class Link {
  public id: string;
  public text: string;
}

export class Form {
  public id: string;
  public text: string;
  public inputs: FormInput[];
}

export class FormInput {
  public type: string;
  public name: string;
  public target: any;
}

export class Progress {
  public percent: number;
  public value: string;
  public min: string;
  public max: string;
  public speed: string;
  public pause: string;
  public paused: boolean;
  public setSpeed: string;
  public cancel: string;
}
