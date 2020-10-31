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
