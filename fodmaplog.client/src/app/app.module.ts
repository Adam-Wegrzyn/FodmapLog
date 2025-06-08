import { HttpClientModule, provideHttpClient, withInterceptors } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { addMealLogComponent } from './add-meal-log/add-meal-log.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TotalKcalConverterPipe } from './pipes/total-kcal-converter.pipe';
import { DailyLogComponent } from './daily-log/daily-log.component';
import { addSymptomsLogComponent } from './add-symptoms-log/add-symptoms-log.component';
import { AddLogBaseComponent } from './add-log-base/add-log-base.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { CommonModule } from '@angular/common';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { DateTimeInputComponent } from './date-time-input/date-time-input.component';
import { LogoutComponent } from './logout/logout.component';
import { AudioRecorderComponent } from './audio-recorder/audio-recorder.component';
import { LoginComponent } from './login/login.component';
import { SignupComponent } from './signup/signup.component';
import { authInterceptor } from './auth.interceptor';
import { LoginCallbackComponent } from './login-callback/login-callback.component';
import { errorInterceptor } from '../error.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    addMealLogComponent,
    TotalKcalConverterPipe,
    DailyLogComponent,
    addSymptomsLogComponent,
    AddLogBaseComponent,
    LogoutComponent,
    AudioRecorderComponent,
    LoginComponent,
    SignupComponent,
    LoginCallbackComponent,
    //DateTimeInputComponent,
  ],
  imports: [
    BrowserModule,
     HttpClientModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    FontAwesomeModule,
    NgxMaterialTimepickerModule,
    DateTimeInputComponent,
  //   MsalModule.forRoot(
  //     new PublicClientApplication({
  //       auth: {
  //         clientId: '5fc4f8ff-fbac-4258-8d19-c5da09bffda4', // Replace with your Angular app's client ID
  //         authority: 'https://login.microsoftonline.com/49754875-8b65-4c47-afc3-ecb6e859ac5e/B2X_1_BasicUserFlow', // Replace with your Azure AD tenant ID
  //         redirectUri: 'http://localhost:4200', // Replace with your redirect URI
  //         postLogoutRedirectUri: 'http://localhost:4200/logout', // Replace with your post-logout redirect URI
  //       },
  //       cache: {
  //         cacheLocation: 'localStorage',
  //         storeAuthStateInCookie: false,
  //       },
  //       system: {
  //         loggerOptions: {
  //           loggerCallback: (level, message, containsPii) => {
  //             if (!containsPii) {
  //               console.log(message);
  //             }
  //           },
  //           piiLoggingEnabled: false,
  //           logLevel: LogLevel.Verbose,
  //         },
  //       },
  //     }),
  //     {
  //       interactionType: InteractionType.Redirect,
  //       authRequest: {
  //         scopes: ['openid', 'profile', 'email'], // Replace with your API scope
  //       },
  //     },
  //     {
  //       interactionType: InteractionType.Redirect,
  //       protectedResourceMap: new Map([]),
  //     }
  //   ),
   ],
  providers: [
    provideAnimationsAsync('noop'),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
