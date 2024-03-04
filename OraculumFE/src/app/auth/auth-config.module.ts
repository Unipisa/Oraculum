import { HttpClient } from '@angular/common/http';
import { NgModule } from '@angular/core';
import {
  AuthModule,
  LogLevel,
  StsConfigHttpLoader,
  StsConfigLoader,
} from 'angular-auth-oidc-client';
import { map } from 'rxjs';

export const httpLoaderFactory = (httpClient: HttpClient) => {
  const config$ = httpClient.get<any>('/api/oraculum/v1/User/oidc-info').pipe(
    map((customConfig: any) => {
      console.log('customConfig', customConfig);
      return {
        authority: customConfig.authority,
        clientId: customConfig.clientId,
        redirectUrl: window.location.origin + '/login',
        postLogoutRedirectUri: window.location.origin,
        scope: 'openid profile offline_access',
        responseType: 'code',
        silentRenew: true,
        useRefreshToken: true,
        renewTimeBeforeTokenExpiresInSeconds: 30,
        logLevel: LogLevel.Warn,
        secureRoutes: ['/api/oraculum'],
      };
    })
  );

  return new StsConfigHttpLoader(config$);
};
@NgModule({
  imports: [
    AuthModule.forRoot({
      loader: {
        provide: StsConfigLoader,
        useFactory: httpLoaderFactory,
        deps: [HttpClient],
      },
    }),
  ],
  exports: [AuthModule],
})
export class AuthConfigModule {}
