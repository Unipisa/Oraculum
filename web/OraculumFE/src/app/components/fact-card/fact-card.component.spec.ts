/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { FactCardComponent } from './fact-card.component';

describe('ChatExplainComponent', () => {
  let component: FactCardComponent;
  let fixture: ComponentFixture<FactCardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FactCardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FactCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
