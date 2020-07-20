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
