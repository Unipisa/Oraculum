/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { ImportDialog } from './import-dialog.component';

describe('DialogConfirmComponent', () => {
  let component: ImportDialog;
  let fixture: ComponentFixture<ImportDialog>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImportDialog ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
