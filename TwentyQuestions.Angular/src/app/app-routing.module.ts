import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponentModule, HomeComponent } from '@views';

const routes: Routes = [
    { path: '', component: HomeComponent, },
    { path: 'home', component: HomeComponent }
];

@NgModule({
    imports: [
        HomeComponentModule,
        RouterModule.forRoot(routes)
    ],
    exports: [RouterModule]
})
export class AppRoutingModule { }
