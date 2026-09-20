import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClient, HttpClientModule, provideHttpClient, withInterceptors } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, registerLocaleData } from '@angular/common';
import localePl from '@angular/common/locales/pl';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { addMealLogComponent } from './add-meal-log/add-meal-log.component';
import { TotalKcalConverterPipe } from './pipes/total-kcal-converter.pipe';
import { DailyLogComponent } from './daily-log/daily-log.component';
import { addSymptomsLogComponent } from './add-symptoms-log/add-symptoms-log.component';
import { AddLogBaseComponent } from './add-log-base/add-log-base.component';
import { DateTimeInputComponent } from './date-time-input/date-time-input.component';
import { LogoutComponent } from './logout/logout.component';
import { AudioRecorderComponent } from './audio-recorder/audio-recorder.component';
import { LoginComponent } from './login/login.component';
import { SignupComponent } from './signup/signup.component';
import { authInterceptor } from './auth.interceptor';
import { LoginCallbackComponent } from './login-callback/login-callback.component';
import { errorInterceptor } from '../error.interceptor';
import { ExportLogsComponent } from './export-logs/export-logs.component';
import { DayTimelineComponent } from './day-timeline/day-timeline.component';
import { AccountComponent } from './account/account.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { LanguageService } from './services/language.service';

registerLocaleData(localePl);

export function HttpLoaderFactory(http: HttpClient): TranslateHttpLoader {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

export function initLanguage(language: LanguageService): () => Promise<unknown> {
  return () => language.init();
}

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
    ExportLogsComponent,
    DayTimelineComponent,
    AccountComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
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
    TranslateModule.forRoot({
      defaultLanguage: 'en',
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    }),
  ],
  providers: [
    provideAnimationsAsync('noop'),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: initLanguage,
      deps: [LanguageService],
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
