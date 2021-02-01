import { Component, Input, OnInit } from '@angular/core';
import { NavBarData, NavigationEvent } from './data';
import { Output } from '@angular/core';
import { EventEmitter } from '@angular/core';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.css']
})
export class NavBarComponent implements OnInit {

  @Input() selected: NavigationEvent;
  @Input() navbar: NavBarData[];

  @Output() select: EventEmitter<string> = new EventEmitter<string>();

  constructor() { }

  ngOnInit(): void {
  }

}
