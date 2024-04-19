import { Fact } from '../../interfaces/fact';
import {
  Component,
  OnInit,
  ElementRef,
  ViewChild,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import language from 'src/app/config/language';
import { AddSourceDialogComponent } from '../dialogs/add-source-dialog/add-source-dialog.component';
import { KnowledgeService } from 'src/app/services/knowledge.service';
import { MatDialog } from '@angular/material/dialog';
import { FormBuilder, Validators } from '@angular/forms';
import { SnackbarService } from 'src/app/services/snackbar.service';
import { FactDialogComponent } from '../dialogs/fact-dialog/fact-dialog.component';

interface SearchPayload {
  query: string;
  distance: number;
  limit: number;
  factTypeFilter?: string[];
  categoryFilter?: string[];
}

@Component({
  selector: 'app-knowledge-search',
  templateUrl: './knowledge-search.component.html',
  styleUrls: ['./knowledge-search.component.scss'],
})
export class KnowledgeSearchComponent implements OnInit {
  language = language;
  facts: Fact[] = [];
  searchInput: string = '';
  loading = false;
  categoryFilter: string[] = [];
  results!: Fact[];

  constructor(
    private knowledgeService: KnowledgeService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private snackbarService: SnackbarService
  ) {}
  ngOnInit() {
    this.knowledgeService.getSibyllaeConfigs().subscribe((response) => {
      const selectedSibylla = localStorage.getItem('selectedSibylla');
      var categoryFilter = response.find((c) => c.id == selectedSibylla)
        ?.memoryConfiguration.categoryFilter![0];
      if (categoryFilter) this.categoryFilter.push(categoryFilter);
    });
  }

  editFact(fact: Fact) {
    console.log('Edit fact', fact);
  }

  openFactDialog(fact?: Fact) {
    this.knowledgeService.getSibyllaeConfigs().subscribe((response) => {
      const selectedSibylla = localStorage.getItem('selectedSibylla');
      const categoryFilter = response.find((c) => c.id == selectedSibylla)
        ?.memoryConfiguration.categoryFilter![0];
      const dialogRef = this.dialog.open(FactDialogComponent, {
        width: '500px',
        data: fact
          ? fact
          : { category: categoryFilter ? categoryFilter : selectedSibylla },
      });

      dialogRef.afterClosed().subscribe((result) => {
        let fact!: Fact;
        // create new fact with result
        let formValue = result.formValue;
        if (formValue) {
          fact = {
            id: result.id ? result.id : '',
            title: formValue.title,
            factType: formValue.factType,
            category: formValue.category,
            expiration: formValue.expiration,
            tags: formValue.tags,
            content: formValue.content,
            citation: formValue.source,
            reference: '',
          };
        }
        if (result.action === 'add') {
          this.knowledgeService.postFact(fact).subscribe({
            next: (response) => {
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento aggiunto',
                'Chiudi',
                2000
              );
            },
            error: (err) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        } else if (result.action === 'edit') {
          this.knowledgeService.updateFact(fact).subscribe({
            next: (response: any) => {
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento modificato',
                'Chiudi',
                2000
              );
            },
            error: (err: any) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        } else if (result.action === 'delete') {
          this.knowledgeService.deleteFact(result.id).subscribe({
            next: (response: any) => {
              this.results = response;
              this.snackbarService.showSnackbar(
                'Elemento eliminato',
                'Chiudi',
                2000
              );
            },
            error: (err: any) => {
              console.error('Error:', err);
              this.snackbarService.showSnackbar('Errore', 'Chiudi', 2000);
            },
          });
        }
      });
    });
  }

  performSearch(): void {
    let payload: SearchPayload = {
      query: this.searchInput ?? '',
      distance: 1,
      limit: 50,
    };

    if (this.categoryFilter.length > 0) {
      payload.categoryFilter = this.categoryFilter;
    }

    this.knowledgeService.postQuery(payload).subscribe({
      next: (response) => {
        console.log(response);
        this.facts = response;
      },
      error: (err) => {
        console.error('Error:', err);
      },
    });
  }

  openAddSourceDialog(): void {
    const dialogRef = this.dialog.open(AddSourceDialogComponent, {
      minWidth: '50%',
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('Dialog closed with result:', result);

      this.knowledgeService.getSibyllaeConfigs().subscribe((response) => {
        const selectedSibylla = localStorage.getItem('selectedSibylla');
        const categoryFilter = response.find((c) => c.id == selectedSibylla)
          ?.memoryConfiguration.categoryFilter![0];

      if (result.type === 'application/pdf' || result.type.includes('word')){
        this.knowledgeService.ingestDocument(result.content, categoryFilter!).subscribe({
        });
      }
      if (result.type === 'audio/mp3' || result.type === 'video/mp4'){
        this.knowledgeService.ingestAudioVideo(result.content,categoryFilter!).subscribe({
        });
      }
      if (result.type === 'url'){
        this.knowledgeService.ingestWebpage(result.content,categoryFilter!).subscribe({
        });
      }
      if (result.type === 'text'){
        let cont = result.content
        this.knowledgeService.ingestText(cont,categoryFilter!).subscribe({
        });
      }
      
    });
  });
  }

  openRecognitionModal() {}

  onKey(event: any) {
    if (event.key === 'Enter' && !this.loading && this.searchInput !== '') {
      this.performSearch();
    }
  }
}
