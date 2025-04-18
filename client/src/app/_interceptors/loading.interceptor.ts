import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BusyService } from '../_services/busy.service';
import { delay, finalize } from 'rxjs';
import { environment } from '../../environments/environment';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);
  busyService.busy();

  return next(req).pipe(
    (environment.production ? delay(0) : delay(1000)), // Simulate a delay in development
    finalize(() => {
      busyService.idle();
    })
  );
};
