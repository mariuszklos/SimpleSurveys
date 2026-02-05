import { Routes } from '@angular/router';
import { SurveyViewComponent } from './components/survey-view/survey-view.component';
import { AdminLoginComponent } from './components/admin-login/admin-login.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { SurveyEditorComponent } from './components/survey-editor/survey-editor.component';
import { adminGuard } from './guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/admin/login', pathMatch: 'full' },
  { path: 'survey/:id', component: SurveyViewComponent },
  { path: 'admin/login', component: AdminLoginComponent },
  { path: 'admin', component: AdminDashboardComponent, canActivate: [adminGuard] },
  { path: 'admin/survey/new', component: SurveyEditorComponent, canActivate: [adminGuard] },
  { path: 'admin/survey/:id', component: SurveyEditorComponent, canActivate: [adminGuard] },
  { path: '**', redirectTo: '/admin/login' }
];
