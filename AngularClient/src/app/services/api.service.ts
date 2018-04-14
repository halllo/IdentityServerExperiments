import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpErrorResponse } from '@angular/common/http/src/response';
import { map, mergeMap, filter, scan } from 'rxjs/operators';
import { Observable } from 'rxjs/Observable';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export class EinwilligungenDto {
  time: string;
  value: number;
  you_are: string;
}

@Injectable()
export class Api {

  constructor(private http: HttpClient, private auth: AuthService) { }

  public getEinwilligungen(id?: string): Observable<EinwilligungenDto> {
    return this.auth.getAccessToken().pipe(
      mergeMap(token => {
        return this.http.get(environment.backend_api + '/api/einwilligungen/' + id, {
          headers: new HttpHeaders({
            'Authorization': `Bearer ${token}`
          })
        }).pipe(
          map(v => {
            return <EinwilligungenDto> v;
          })
        );
      })
    );
  }
}
