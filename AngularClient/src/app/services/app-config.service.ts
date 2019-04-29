import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable()
export class AppConfigService {
  private _basePath: string;
  
  constructor(private http: HttpClient) { }

  public loadAppConfig() {
    this._basePath = (<any>document.getElementById('myBase')).href;
  }

  public prefixOrigin(uri: string): string {
    if (uri.startsWith('~/')) {
      return location.origin + uri.substring(1);
    } else if (uri.startsWith('/')) {
      return this._basePath + (this._basePath.endsWith('/') ? uri.substring(1) : uri);
    } else {
      return uri;
    }
  }

  public origin(url: string): string {
    const absoluteUrl = this.prefixOrigin(url);
    const pathArray = absoluteUrl.split( '/' );
    const protocol = pathArray[0];
    const host = pathArray[2];
    return protocol + '//' + host;
  }

  public isMyOrigin(url: string): boolean {
    const myOrigin = this.origin(location.origin);
    const otherOrigin = this.origin(url);
    return myOrigin === otherOrigin;
  }

  public get basePath(): string {
    return this._basePath;
  }

}

export const appInitializerFn = (appConfig: AppConfigService) => {
  return () => {
    return appConfig.loadAppConfig();
  };
};
