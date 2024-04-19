import { Fact } from '../../interfaces/fact';
import { Component, OnInit, ElementRef, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import language from 'src/app/config/language';

@Component({
  selector: 'app-fact-list',
  templateUrl: './fact-list.component.html',
  styleUrls: ['./fact-list.component.scss'],
})
export class FactListComponent implements OnInit {
  language = language;
  @Input() facts: Fact[] = [];
  @Input() mode: "explain" | "search" = 'explain';
  @Input() hideVoting: boolean = false;
  @Output() onEditFact = new EventEmitter<Fact>();
  @Output() onUpvoteFact = new EventEmitter<Fact>();
  @Output() onDownvoteFact = new EventEmitter<Fact>();
  usedFacts: Fact[] = this.facts.filter(fact => fact.outOfLimit == false);
  unusedFacts: Fact[] = this.facts.filter(fact => fact.outOfLimit == true);

  listTitle = "";
  unusedFactsChipTitle = ""
  otherRetrivedSourcesTitle = ""

  constructor() {}
  ngOnInit() {
    this.changeLabels();
  }

  ngOnChanges() {
    this.facts.forEach(fact => {
      if(fact.score == undefined && fact.distance) fact.score = Math.round((1 - fact.distance!) * 100)
    })
    this.usedFacts = this.facts.filter(fact => fact.outOfLimit == false || fact.outOfLimit == undefined);
    this.unusedFacts = this.facts.filter(fact => fact.outOfLimit == true);

    this.changeLabels();
  } 

  changeLabels(){
    if(this.facts){
      this.listTitle = this.mode == "explain" 
      ? `${language.SourcesUsed} (${this.facts.filter(fact => fact.outOfLimit == false).length})` 
      : `${language.RelevantSources} (${this.facts.length})`;

      this.unusedFactsChipTitle = `${language.ViewSourcesUnused} (${this.facts.filter(fact => fact.outOfLimit == true).length})`;

      this.otherRetrivedSourcesTitle = `${language.OtherRetrivedSources} (${this.facts.filter(fact => fact.outOfLimit == true).length})`;
    }else{
      this.listTitle = this.mode == "explain" 
      ? `${language.SourcesUsed} (0)` 
      : `${language.RelevantSources} (0)`;

      this.unusedFactsChipTitle = `${language.ViewSourcesUnused} (0)`;

      this.otherRetrivedSourcesTitle = `${language.OtherRetrivedSources} (0)`;
    }
  }

  //TODO: Doesn't work on Chrome
  scroll(el: HTMLElement) {
    el.scrollIntoView({behavior: "smooth"});
  }

  editFact(fact: Fact) {
    this.onEditFact.emit(fact);
  }

  upvoteFact(fact: Fact) {
    this.onUpvoteFact.emit(fact);
  }

  downvoteFact(fact: Fact) {
    this.onDownvoteFact.emit(fact);
  }
}
