/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { FactListComponent } from './fact-list.component';

describe('ChatExplainComponent', () => {
  let component: FactListComponent;
  let fixture: ComponentFixture<FactListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FactListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FactListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
