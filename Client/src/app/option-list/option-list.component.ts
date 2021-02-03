import { Component, OnInit, ViewChild } from '@angular/core';
import { OptionsService } from '@service/options.service';
import { NgbAlert, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-option-list',
  templateUrl: './option-list.component.html',
  styleUrls: ['./option-list.component.css']
})
export class OptionListComponent implements OnInit {

  @ViewChild('selfClosingAlert', {static: false}) selfClosingAlert: NgbAlert;

  modal: NgbModalRef;
  exportAlert = false;

  constructor(public optionService: OptionsService) { }

  ngOnInit(): void {
  }

  types(map: Map<string, any>) {
    return Array.from(map.keys());
  }

  replacer(key, value) {
    if(value instanceof Map) {
      return {
        dataType: 'Map',
        value: Array.from(value.entries()), // or with spread: value: [...value]
      };
    } else {
      return value;
    }
  }

  export() {
    navigator.clipboard.writeText(this.optionService.serializedOptions());
    this.exportAlert = true;
    setTimeout(() => {
      this.exportAlert = false;
    }, 5000);
  }

  import(value: string) {
    console.log(value);
  }
}
