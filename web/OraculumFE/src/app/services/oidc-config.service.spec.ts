/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { OidcConfigService } from './oidc-config.service';

describe('Service: OidcConfig', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OidcConfigService],
    });
  });

  it('should ...', inject([OidcConfigService], (service: OidcConfigService) => {
    expect(service).toBeTruthy();
  }));
});
