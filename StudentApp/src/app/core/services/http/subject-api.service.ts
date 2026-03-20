import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SubjectDto } from '../../models';

@Injectable({ providedIn: 'root' })
export class SubjectApiService {
  private readonly url = '/api/subjects';

  constructor(private http: HttpClient) {}

  getAll(): Observable<SubjectDto[]> {
    return this.http.get<SubjectDto[]>(this.url);
  }
}
