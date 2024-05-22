import { Fact } from '../../interfaces/fact';
import { Component, OnInit, ElementRef, ViewChild, Input, EventEmitter, Output } from '@angular/core';
import language from 'src/app/config/language';

@Component({
  selector: 'app-fact-card',
  templateUrl: './fact-card.component.html',
  styleUrls: ['./fact-card.component.scss'],
})
export class FactCardComponent implements OnInit {
  language = language;
  expanded = false;
  @Input() fact!: Fact;
  @Input() hideVoting = false;
  @Input() voted: 'up' | 'down' | null = null;
  @Input() autoVote = true; // True: do not wait for voted to be set, directly set voted status on upvote/downvote click
  @ViewChild('cardBody') cardBody!: ElementRef;
  @Output() editFact = new EventEmitter<Fact>();
  @Output() upvoteFact = new EventEmitter<Fact>();
  @Output() downvoteFact = new EventEmitter<Fact>();

  constructor() {}
  ngOnInit() {
  }

  toggleExpand() {
    this.expanded = !this.expanded;
    if (this.expanded) {
      this.cardBody.nativeElement.style.maxHeight = this.cardBody.nativeElement.scrollHeight + 'px';
    } else {
      this.cardBody.nativeElement.style.maxHeight = '158px';
    }
  }

  onEditFact() {
    this.editFact.emit(this.fact);
  }

  onUpvoteFact() {
    this.upvoteFact.emit(this.fact);
    if (this.autoVote) {
      this.voted = 'up';
    }
  }

  onDownvoteFact() {
    this.downvoteFact.emit(this.fact);
    if (this.autoVote) {
      this.voted = 'down';
    }
  }
}
