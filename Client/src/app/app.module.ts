import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { RootRenderComponent } from './root-render/root-render.component';
import { ArrayRenderComponent } from './array-render/array-render.component';
import { AnyRenderComponent } from './any-render/any-render.component';
import { ObjectRenderComponent } from './object-render/object-render.component';
import { FormRenderComponent } from './form-render/form-render.component';

@NgModule({
  declarations: [
    AppComponent,
    RootRenderComponent,
    ArrayRenderComponent,
    AnyRenderComponent,
    ObjectRenderComponent,
    FormRenderComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
