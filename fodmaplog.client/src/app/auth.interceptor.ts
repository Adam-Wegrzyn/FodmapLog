import { HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ianaTimeZone, utcOffsetMinutes } from './services/clock-time';


export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  const token = localStorage.getItem('token');
  const headers: Record<string, string> = {
    'X-Time-Zone': ianaTimeZone(),
    'X-Utc-Offset-Minutes': String(utcOffsetMinutes())
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  return next(req.clone({ setHeaders: headers }));
};

