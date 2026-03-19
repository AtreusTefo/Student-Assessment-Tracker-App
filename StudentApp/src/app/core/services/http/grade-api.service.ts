import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GradeDto } from '../../models';

/**
 * DATA ACCESS LAYER - Grade HTTP API Service
 * Read-only — fetches the seeded grade lookup table for populating dropdowns.
 */
@Injectable({
  providedIn: 'root'
})
export class GradeApiService {
  private readonly apiUrl = '/api/grades';

  constructor(private http: HttpClient) { }

  getAll(): Observable<GradeDto[]> {
    return this.http.get<GradeDto[]>(this.apiUrl);
  }
}
