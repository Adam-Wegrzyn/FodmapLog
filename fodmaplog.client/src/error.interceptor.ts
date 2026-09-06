import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

/** Logs HTTP failures but preserves the original HttpErrorResponse for callers. */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error) => {
      console.error('HTTP error:', req.method, req.url, error?.status, error);
      return throwError(() => error);
    })
  );
};
