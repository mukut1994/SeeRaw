import { Component, OnInit } from '@angular/core';
import { OptionsService } from '@service/options.service';
import { NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-option-list',
  templateUrl: './option-list.component.html',
  styleUrls: ['./option-list.component.css']
})
export class OptionListComponent implements OnInit {

  modal: NgbModalRef;

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
}
