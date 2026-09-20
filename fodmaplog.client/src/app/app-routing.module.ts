import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { addMealLogComponent } from './add-meal-log/add-meal-log.component';
import { DailyLogComponent } from './daily-log/daily-log.component';
import { addSymptomsLogComponent } from './add-symptoms-log/add-symptoms-log.component';
import { LogoutComponent } from './logout/logout.component';
import { AudioRecorderComponent } from './audio-recorder/audio-recorder.component';
import { LoginComponent } from './login/login.component';
import { SignupComponent } from './signup/signup.component';
import { AuthGuard } from './AuthGuard';
import { LoginCallbackComponent } from './login-callback/login-callback.component';
import { ExportLogsComponent } from './export-logs/export-logs.component';
import { AccountComponent } from './account/account.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';

const routes: Routes = [
  {
    path: '', redirectTo: '/login', pathMatch: 'full'
  },
  {
    path: 'add-meal-log', component: addMealLogComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'add-meal-log/:id', component: addMealLogComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'daily-log', component: DailyLogComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'daily-log/:date', component: DailyLogComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'add-symptoms-log', component: addSymptomsLogComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'add-symptoms-log/:id', component: addSymptomsLogComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'audio-rec', component: AudioRecorderComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'export-logs', component: ExportLogsComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'account', component: AccountComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'login', component: LoginComponent
  },
  {
    path: 'signup', component: SignupComponent
  },
  {
    path: 'forgot-password', component: ForgotPasswordComponent
  },
  {
    path: 'reset-password', component: ResetPasswordComponent
  },
  {
    path: 'logout', component: LogoutComponent
  },
  {
    path: 'login-callback', component: LoginCallbackComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
