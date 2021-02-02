import { Component, OnInit, Input } from '@angular/core';
import { Metadata, RenderContext } from '@data/data.model';
import { BackendService } from '@service/backend.service';

@Component({
  selector: 'app-form-render',
  templateUrl: './form-render.component.html',
  styleUrls: ['./form-render.component.css'],
})
export class FormRenderComponent implements OnInit {

  @Input() metadata: FormMetadata;
  @Input() value: any;
  @Input() context: RenderContext;

  constructor(readonly backendService: BackendService) {}

  ngOnInit() {}

  send() {

    const collapsed = this.value.inputs.map((x) =>
      this.removeSystemPropsAndCopy(x)
    );

    const message = {
      id: this.value.id,
      type: 'form',
      args: collapsed
    };

    this.backendService.sendMessage(JSON.stringify(message));
  }

  removeSystemPropsAndCopy(instance: any) {

    if (instance.type === 'string' || instance.type === 'number' || instance.type === 'bool' || instance.type === 'enum') {
      return { key: instance.name, value: instance.target };
    }

    return 'obj';
  }
}

class FormMetadata extends Metadata {
  inputs: FormInput[]
}

class FormInput extends Metadata {
  name: string;
}