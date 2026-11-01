import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Location } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private readonly http = inject(HttpClient);

  /** Reads the rows saved in the Location_Details table. */
  getLocations(): Observable<Location[]> {
    return this.http.get<Location[]>('/api/locations');
  }
}
