import {NgModule} from "@angular/core";
import {MatButtonModule, MatToolbarModule} from "@angular/material";

const modules = [
  MatButtonModule,
  MatToolbarModule,
];

@NgModule({
  imports: modules,
  exports: modules
})
export class MaterialsModule
{

}
