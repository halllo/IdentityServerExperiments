import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpErrorResponse } from '@angular/common/http/src/response';
import { map, mergeMap, filter, scan } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable()
export class Api {

  constructor(private http: HttpClient, private auth: AuthService) { }

  public get(): Observable<Object> {
    return this.http.get(environment.backend_api + '/api/books/', {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${this.auth.accessToken}`
      })
    }).pipe(
      map(v => {
        return v;
      })
    );
  }

  public post(): Observable<Object> {
    return this.http.post(environment.backend_api + '/api/books/', {}, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${this.auth.accessToken}`
      })
    }).pipe(
      map(v => {
        return v;
      })
    );
  }
}
