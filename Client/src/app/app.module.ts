import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { RootRenderComponent } from './root-render/root-render.component';
import { FormRenderComponent } from './renderers/form-render/form-render.component';
import { ProgressRenderComponent } from './renderers/progress-render/progress-render.component';
import { EnumRenderComponent } from './renderers/enum-render/enum-render.component';
import { NavigationRenderComponent } from './renderers/navigation-render/navigation-render.component';
import { LogRenderComponent } from './renderers/log-render/log-render.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NavBarComponent } from './renderers/navigation-render/nav-bar/nav-bar.component';
import { CommonModule } from '@angular/common';
import { RenderDirective } from './render.service';
import { NavigationRenderOptionComponent } from './renderers/navigation-render/option/options.component';
import { OptionDirective, OptionEditComponent } from './option-edit/option-edit.component';
import { TableRenderComponent } from './renderers/table-render/table-render.component';
import { OptionComponent } from './renderers/table-render/option/option.component';
import { ValueRenderComponent } from './renderers/value-render/value-render.component';
import { OptionButtonComponent } from './option-button/option-button.component';
import { LinkRenderComponent } from './renderers/link-render/link-render.component';

@NgModule({
  declarations: [
    AppComponent,
    RootRenderComponent,
    FormRenderComponent,
    ProgressRenderComponent,
    EnumRenderComponent,
    NavigationRenderComponent,
    LogRenderComponent,
    NavBarComponent,
    RenderDirective,
    NavigationRenderOptionComponent,
    OptionEditComponent,
    OptionDirective,
    TableRenderComponent,
    OptionComponent,
    ValueRenderComponent,
    OptionButtonComponent,
    LinkRenderComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    NgbModule,
    CommonModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
