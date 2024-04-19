/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { ChatModularComponent } from './chat-modular.component';

describe('ChatModularComponent', () => {
  let component: ChatModularComponent;
  let fixture: ComponentFixture<ChatModularComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChatModularComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChatModularComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
