/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { ChatExplainComponent } from './chatExplain.component';

describe('ChatExplainComponent', () => {
  let component: ChatExplainComponent;
  let fixture: ComponentFixture<ChatExplainComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChatExplainComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChatExplainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
