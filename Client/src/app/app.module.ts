import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { RootRenderComponent } from './root-render/root-render.component';
import { ArrayRenderComponent } from './array-render/array-render.component';
import { AnyRenderComponent } from './any-render/any-render.component';
import { ObjectRenderComponent } from './object-render/object-render.component';

@NgModule({
  declarations: [
    AppComponent,
    RootRenderComponent,
    ArrayRenderComponent,
    AnyRenderComponent,
    ObjectRenderComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
