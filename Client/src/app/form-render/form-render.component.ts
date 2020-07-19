import { Component, OnInit, Input } from "@angular/core";
import { Form } from "../data.model";
import { BackendService } from "./../backend.service";
import { FormInput } from "./../data.model";

@Component({
  selector: "app-form-render",
  templateUrl: "./form-render.component.html",
  styleUrls: ["./form-render.component.css"],
})
export class FormRenderComponent implements OnInit {
  @Input() value: Form;

  constructor(readonly backendService: BackendService) {}

  ngOnInit() {}

  send() {
    let collapsed = this.value.inputs.map((x) =>
      this.removeSystemPropsAndCopy(x)
    );

    console.log(collapsed);
    //this.backendService.sendMessage(JSON.stringify(this.value));
  }

  removeSystemPropsAndCopy(instance: any) {
    let ret = {};

    if (instance.target !== undefined) {
      if (instance.target != null && instance.target.target === undefined) {
        return instance.target;
      }

      for (const prop in instance.target.target) {
        if (instance.target.hasOwnProperty(prop)) {
          ret[prop] = this.removeSystemPropsAndCopy(instance.target[prop]);
        }
      }
    }

    return ret;
  }
}
