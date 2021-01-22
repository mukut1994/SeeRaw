import { Metadata } from 'src/app/data.model';
import { RenderContext } from './../../data.model';

export class NavigationEvent {
  value: any;
  metadata: Metadata;
  context: RenderContext;
}

export class NavBarData extends NavigationEvent {
  title: string;
  children: NavBarData[];
}
