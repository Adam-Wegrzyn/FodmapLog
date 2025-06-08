import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req)
  .pipe(
    catchError((error) => {
      console.log('Error intercepted:', error);
      if(error.error && error.error.message) {
        // Handle the error message from the server
        console.error('Error message from server:', error.error.message);
      }
      else {
        // Handle generic error message
        console.error('An error occurred:', error.message);
      }
      return throwError(() => {
        // Create a new error with a custom message
        console.log(error)
        return new Error('An error occurred while processing your request. Please try again later.');
      }
    
  )    })
  );
};
